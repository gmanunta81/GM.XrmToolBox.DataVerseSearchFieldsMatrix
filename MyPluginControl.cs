using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using XrmToolBox.Extensibility;

namespace GM.XrmToolBox.DataVerseSearchFieldsMatrix
{
    /// <summary>
    /// Main user-control for the Dataverse Search Fields Matrix plugin.
    /// 
    /// This plugin analyses every entity whose Dataverse Search
    /// (Relevance Search / External Search Index) flag is turned on,
    /// retrieves its Quick Find view (querytype = 4), and builds a
    /// flat matrix of all indexed fields with:
    ///   - Entity display name and logical name
    ///   - Field display name, schema (logical) name, and type
    ///   - Weighted index cost (Lookup = 3, OptionSet = 2, others = 1)
    ///   - The Quick Find view name the field belongs to
    ///
    /// Network optimisation strategy (only 3 + N round-trips):
    ///   1. One call  → organisation setting (isexternalsearchindexenabled)
    ///   2. One call  → RetrieveAllEntitiesRequest (Entity filter only)
    ///   3. One call  → all Quick Find views via paged RetrieveMultiple
    ///   4. N calls   → RetrieveEntityRequest per enabled entity (Attributes)
    ///      where N is typically 20–50, far less than the total entity count.
    /// </summary>
    public partial class MyPluginControl : PluginControlBase
    {
        // ──────────────────────────────────────────────────────────────
        //  Fields
        // ──────────────────────────────────────────────────────────────

        /// <summary>Persisted plugin settings (e.g. last used org URL).</summary>
        private Settings mySettings;

        /// <summary>
        /// The backing DataTable for the grid.
        /// Filtering is done through <see cref="DataTable.DefaultView"/>.
        /// </summary>
        private DataTable matrixTable;

        /// <summary>
        /// Cached list of string column names used by the search filter.
        /// Built once when the DataTable is created to avoid rebuilding
        /// the LIKE expression on every keystroke.
        /// </summary>
        private string[] searchableColumnNames;


        /// <summary>
        /// Holds rows that explain why some fields are not counted (or are counted once)
        /// in the PPAC-like total (dedup/common fields, ignored find types, etc.).
        /// </summary>
        private DataTable diagnosticsTable;
        // ──────────────────────────────────────────────────────────────
        //  Constructor / Lifecycle
        // ──────────────────────────────────────────────────────────────

        public MyPluginControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Fired when the control is first shown.
        /// Loads (or creates) the persisted settings file.
        /// </summary>
        private void MyPluginControl_Load(object sender, EventArgs e)
        {
            if (!SettingsManager.Instance.TryLoad(GetType(), out mySettings))
            {
                mySettings = new Settings();
                LogWarning("Settings not found => a new settings file has been created!");
            }
            else
            {
                LogInfo("Settings found and loaded");
            }
        }

        /// <summary>Closes the plugin tab inside XrmToolBox.</summary>
        private void tsbClose_Click(object sender, EventArgs e)
        {
            CloseTool();
        }

        /// <summary>
        /// Shows the About dialog with logo, version, author, and links.
        /// Same visual style used in the FlowRunHistoryViewer plugin.
        /// </summary>
        private void tsbAbout_Click(object sender, EventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;

            using (var dlg = new Form())
            {
                dlg.Text = "About - Dataverse Search Fields Matrix";
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.Size = new Size(460, 380);

                // Logo
                var picBox = new PictureBox();
                picBox.SizeMode = PictureBoxSizeMode.Zoom;
                picBox.Size = new Size(100, 100);
                picBox.Location = new Point(175, 15);
                try
                {
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var iconPath = Path.Combine(baseDir, "assets", "icon.png");
                    if (!File.Exists(iconPath))
                        iconPath = Path.Combine(Path.GetDirectoryName(assembly.Location), "assets", "icon.png");
                    if (!File.Exists(iconPath))
                        iconPath = Path.Combine(Application.StartupPath, "assets", "icon.png");
                    if (!File.Exists(iconPath))
                        iconPath = Path.Combine("assets", "icon.png");
                    if (File.Exists(iconPath))
                        picBox.Image = Image.FromFile(iconPath);
                }
                catch { }
                dlg.Controls.Add(picBox);

                // Title
                var lblTitle = new System.Windows.Forms.Label();
                lblTitle.Text = "Dataverse Search Fields Matrix";
                lblTitle.Font = new Font(lblTitle.Font.FontFamily, 14, FontStyle.Bold);
                lblTitle.AutoSize = true;
                lblTitle.Location = new Point(80, 125);
                dlg.Controls.Add(lblTitle);

                // Version
                var lblVersion = new System.Windows.Forms.Label();
                lblVersion.Text = "Version " + version;
                lblVersion.AutoSize = true;
                lblVersion.ForeColor = Color.Gray;
                lblVersion.Location = new Point(175, 155);
                dlg.Controls.Add(lblVersion);

                // Author
                var lblAuthor = new System.Windows.Forms.Label();
                lblAuthor.Text = "by Giovanni Manunta";
                lblAuthor.AutoSize = true;
                lblAuthor.Location = new Point(155, 180);
                dlg.Controls.Add(lblAuthor);

                // Description
                var lblDesc = new System.Windows.Forms.Label();
                lblDesc.Text = "Displays all fields indexed for Dataverse Search\n"
                             + "(Relevance Search) from Quick Find views.";
                lblDesc.AutoSize = true;
                lblDesc.ForeColor = Color.DimGray;
                lblDesc.Location = new Point(80, 210);
                dlg.Controls.Add(lblDesc);

                // Accuracy note
                var lblNote = new System.Windows.Forms.Label();
                lblNote.Text = "Note: counts may differ from PPAC due to default indexed\n"
                             + "columns, internal deduplication, or non-indexable types.";
                lblNote.AutoSize = true;
                lblNote.ForeColor = Color.Gray;
                lblNote.Location = new Point(40, 235);
                dlg.Controls.Add(lblNote);

                // GitHub link
                var lnk = new LinkLabel();
                lnk.Text = "GitHub Repository";
                lnk.AutoSize = true;
                lnk.Location = new Point(165, 270);
                lnk.LinkClicked += (s2, e2) =>
                {
                    try { Process.Start(new ProcessStartInfo("https://github.com/gmanunta81") { UseShellExecute = true }); }
                    catch { }
                };
                dlg.Controls.Add(lnk);

                // MS Docs link
                var lnkDocs = new LinkLabel();
                lnkDocs.Text = "Microsoft Docs - Configure Dataverse Search";
                lnkDocs.AutoSize = true;
                lnkDocs.Location = new Point(95, 295);
                lnkDocs.LinkClicked += (s2, e2) =>
                {
                    try { Process.Start(new ProcessStartInfo("https://learn.microsoft.com/en-us/power-platform/admin/configure-relevance-search-organization") { UseShellExecute = true }); }
                    catch { }
                };
                dlg.Controls.Add(lnkDocs);

                // OK button
                var btnOk = new Button();
                btnOk.Text = "OK";
                btnOk.Size = new Size(80, 30);
                btnOk.Location = new Point(185, 320);
                btnOk.DialogResult = DialogResult.OK;
                dlg.AcceptButton = btnOk;
                dlg.Controls.Add(btnOk);

                dlg.ShowDialog(this);
            }
        }

        /// <summary>
        /// Persists plugin settings when the user closes the tool tab.
        /// </summary>
        private void MyPluginControl_OnCloseTool(object sender, EventArgs e)
        {
            SettingsManager.Instance.Save(GetType(), mySettings);
        }

        /// <summary>
        /// Called by XrmToolBox whenever the active connection changes.
        /// Stores the new org URL in settings and resets the UI so the
        /// user can reload data for the new environment.
        /// </summary>
        public override void UpdateConnection(IOrganizationService newService,
            ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            if (mySettings != null && detail != null)
            {
                mySettings.LastUsedOrganizationWebappUrl = detail.WebApplicationUrl;
                LogInfo("Connection has changed to: {0}", detail.WebApplicationUrl);
            }

            // Clear previous results so stale data from another org is never shown
            if (matrixTable != null)
            {
                matrixTable.Clear();
                dgvMatrix.DataSource = null;
            }

            lblSearchStatus.Text =
                "Connect and click 'Load Data' to check Dataverse Search configuration.";
            lblSearchStatus.ForeColor = Color.Gray;
            lblRecordCount.Text = string.Empty;
            txtSearch.Text = string.Empty;
        }

        // ──────────────────────────────────────────────────────────────
        //  Main data-loading pipeline
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Entry point wired to the "Load Data" toolbar button.
        /// <see cref="ExecuteMethod"/> ensures a valid connection
        /// exists before calling <see cref="LoadDataverseSearchMatrix"/>.
        /// </summary>
        private void tsbLoadData_Click(object sender, EventArgs e)
        {
            ExecuteMethod(LoadDataverseSearchMatrix);
        }

        /// <summary>
        /// Orchestrates the full data-loading pipeline on a background
        /// thread via <see cref="PluginControlBase.WorkAsync"/>.
        ///
        /// Steps:
        ///   1. Read the organisation-level "isexternalsearchindexenabled" flag.
        ///   2. Retrieve lightweight entity metadata for every table; keep
        ///      only those with <c>SyncToExternalSearchIndex == true</c>.
        ///   3. Fetch all active Quick Find views (querytype 4) in a single
        ///      paged query and filter to the enabled entities.
        ///   4. For each enabled entity, retrieve its attribute metadata and
        ///      build a Dictionary&lt;logicalName, AttributeMetadata&gt; for
        ///      O(1) lookups when populating the matrix rows.
        ///   5. Parse each view's fetchxml + layoutxml, resolve attribute
        ///      display names / types, compute the weighted index cost,
        ///      and emit one DataRow per field.
        /// </summary>
        private void LoadDataverseSearchMatrix()
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Loading Dataverse Search configuration...",
                Work = (worker, args) =>
                {
                    // ── Step 1: Organisation-level Dataverse Search flag ──────
                    worker.ReportProgress(0, "Checking Dataverse Search status...");

                    bool isSearchEnabled = RetrieveOrgSearchEnabled();

                    // ── Step 2: Entity metadata (Entity filter only — fast) ───
                    worker.ReportProgress(10, "Retrieving entity metadata...");

                    var allEntityMeta = RetrieveAllEntityMetadata();

                    // Keep only entities that are indexed for Dataverse Search
                    var enabledEntities = allEntityMeta
                        .Where(em => em.SyncToExternalSearchIndex == true)
                        .OrderBy(em => GetEntityDisplayName(em))
                        .ToList();

                    // O(1) lookup: logicalName → display name
                    var entityDisplayNames = enabledEntities.ToDictionary(
                        em => em.LogicalName,
                        em => GetEntityDisplayName(em),
                        StringComparer.OrdinalIgnoreCase);

                    // ── Step 3: All Quick Find views in one paged query ───────
                    worker.ReportProgress(25, "Retrieving Quick Find views...");

                    var allQuickFindViews = RetrieveAllQuickFindViews();

                    // Pre-filter to only enabled entities
                    var enabledNames = new HashSet<string>(
                        enabledEntities.Select(em => em.LogicalName),
                        StringComparer.OrdinalIgnoreCase);

                    var relevantViews = allQuickFindViews
                        .Where(v => enabledNames.Contains(
                            v.GetAttributeValue<string>("returnedtypecode") ?? string.Empty))
                        .ToList();

                    // ── Step 4: Attribute metadata per enabled entity ─────────
                    //    Builds a Dictionary<logicalName, AttributeMetadata>
                    //    per entity so field lookups later are O(1) instead
                    //    of O(n) FirstOrDefault scans.
                    worker.ReportProgress(35, "Retrieving attribute metadata...");

                    var attrCache = new Dictionary<string, Dictionary<string, AttributeMetadata>>(
                        StringComparer.OrdinalIgnoreCase);

                    int idx = 0;
                    int total = enabledEntities.Count;

                    foreach (var entity in enabledEntities)
                    {
                        idx++;
                        int pct = 35 + (int)(50.0 * idx / Math.Max(total, 1));
                        worker.ReportProgress(pct,
                            "Loading attributes for " + GetEntityDisplayName(entity) +
                            " (" + idx + "/" + total + ")...");

                        try
                        {
                            var resp = (RetrieveEntityResponse)Service.Execute(
                                new RetrieveEntityRequest
                                {
                                    LogicalName = entity.LogicalName,
                                    EntityFilters = EntityFilters.Attributes,
                                    RetrieveAsIfPublished = true
                                });

                            // Build O(1) dictionary keyed by attribute logical name
                            var dict = new Dictionary<string, AttributeMetadata>(
                                StringComparer.OrdinalIgnoreCase);
                            foreach (var a in resp.EntityMetadata.Attributes)
                                dict[a.LogicalName] = a;

                            attrCache[entity.LogicalName] = dict;
                        }
                        catch (Exception ex)
                        {
                            LogWarning("Could not retrieve attributes for "
                                + entity.LogicalName + ": " + ex.Message);
                        }
                    }

                    // ── Step 5: Assemble the flat matrix DataTable ────────────
                    worker.ReportProgress(90, "Building matrix...");

                    DataTable diagDt;
                    var dt = BuildMatrixDataTable(
                        relevantViews, entityDisplayNames, attrCache, out diagDt);

                    args.Result = new object[] { isSearchEnabled, dt, diagDt };
                },
                ProgressChanged = (progressArgs) =>
                {
                    // Update the XrmToolBox spinner overlay text
                    SetWorkingMessage(
                        progressArgs.UserState?.ToString() ?? "Loading...");
                },
                PostWorkCallBack = (resultArgs) =>
                {
                    if (resultArgs.Error != null)
                    {
                        MessageBox.Show(resultArgs.Error.ToString(), "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var result = resultArgs.Result as object[];
                    if (result == null) return;

                    bool isEnabled = (bool)result[0];
                    matrixTable = (DataTable)result[1];
                    diagnosticsTable = (DataTable)result[2];
                    // Cache the list of searchable (string) column names once
                    searchableColumnNames = matrixTable.Columns.Cast<DataColumn>()
                        .Where(c => c.DataType == typeof(string))
                        .Select(c => c.ColumnName)
                        .ToArray();

                    // Update the status banner
                    UpdateSearchStatusBanner(isEnabled);

                    // Bind data to the grid with layout suspension for speed
                    dgvMatrix.SuspendLayout();
                    dgvMatrix.DataSource = matrixTable;
                    FormatGrid();
                    dgvMatrix.ResumeLayout(true);

                    // Show totals and clear any previous search text
                    RefreshFooterAndDiagnostics();
                    txtSearch.Text = string.Empty;
                }
            });
        }

        // ──────────────────────────────────────────────────────────────
        //  Dataverse service helpers
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Reads the <c>isexternalsearchindexenabled</c> flag from the
        /// Organisation entity.  Returns false when it cannot be read.
        /// </summary>
        private bool RetrieveOrgSearchEnabled()
        {
            var query = new QueryExpression("organization")
            {
                ColumnSet = new ColumnSet("isexternalsearchindexenabled")
            };
            var results = Service.RetrieveMultiple(query);

            return results.Entities.Count > 0
                && results.Entities[0].GetAttributeValue<bool>("isexternalsearchindexenabled");
        }

        /// <summary>
        /// Retrieves all entity metadata using the lightweight
        /// <see cref="EntityFilters.Entity"/> filter (no attributes,
        /// no relationships) so the call completes in a few seconds.
        /// </summary>
        private EntityMetadata[] RetrieveAllEntityMetadata()
        {
            var resp = (RetrieveAllEntitiesResponse)Service.Execute(
                new RetrieveAllEntitiesRequest
                {
                    EntityFilters = EntityFilters.Entity,
                    RetrieveAsIfPublished = true
                });
            return resp.EntityMetadata;
        }

        /// <summary>
        /// Retrieves every active **default** Quick Find view
        /// (querytype = 4, isdefault = true) across all entities.
        /// Uses server-side paging (5 000 per page) to handle very
        /// large organisations.
        ///
        /// Only the default Quick Find view per entity is used by
        /// Dataverse Search for indexing.  Non-default Quick Find
        /// views are ignored by the search engine and must be
        /// excluded to match the Admin Center field count.
        /// </summary>
        private List<Entity> RetrieveAllQuickFindViews()
        {
            var allViews = new List<Entity>();

            var query = new QueryExpression("savedquery")
            {
                ColumnSet = new ColumnSet(
                    "name", "returnedtypecode", "fetchxml", "layoutxml"),
                Criteria = new FilterExpression(LogicalOperator.And),
                PageInfo = new PagingInfo { Count = 5000, PageNumber = 1 }
            };
            query.Criteria.AddCondition("querytype", ConditionOperator.Equal, 4);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            query.Criteria.AddCondition("isdefault", ConditionOperator.Equal, true);

            EntityCollection page;
            do
            {
                page = Service.RetrieveMultiple(query);
                allViews.AddRange(page.Entities);
                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = page.PagingCookie;
            }
            while (page.MoreRecords);

            return allViews;
        }

        // ──────────────────────────────────────────────────────────────
        //  Matrix builder
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Iterates the relevant Quick Find views, extracts all indexed
        /// fields from fetchxml, resolves display names and types from
        /// the pre-cached attribute dictionaries, and returns a fully
        /// populated <see cref="DataTable"/>.
        /// </summary>
        private DataTable BuildMatrixDataTable(
            List<Entity> views,
            Dictionary<string, string> entityDisplayNames,
            Dictionary<string, Dictionary<string, AttributeMetadata>> attrCache,
            out DataTable diagnosticsDt)
        {
            var dt = new DataTable();
            dt.Columns.Add("Entity Display Name", typeof(string));
            dt.Columns.Add("Entity Logical Name", typeof(string));
            dt.Columns.Add("Field Display Name", typeof(string));

            // (6) Split logical vs schema (today you were writing logical into "Schema Name")
            dt.Columns.Add("Field Logical Name", typeof(string));
            dt.Columns.Add("Field Schema Name", typeof(string));

            dt.Columns.Add("Field Type", typeof(string));
            dt.Columns.Add("Index Weight", typeof(int));

            // New PPAC columns
            dt.Columns.Add("IsCommonField", typeof(bool));
            dt.Columns.Add("CountedOnce", typeof(bool));
            dt.Columns.Add("PPAC Weight", typeof(int));
            dt.Columns.Add("PPAC Exclusion Reason", typeof(string));

            dt.Columns.Add("Quick Find View Name", typeof(string));
            diagnosticsDt = new DataTable();
            diagnosticsDt.Columns.Add("Entity Logical Name", typeof(string));
            diagnosticsDt.Columns.Add("Entity Display Name", typeof(string));
            diagnosticsDt.Columns.Add("Field Logical Name", typeof(string));
            diagnosticsDt.Columns.Add("Index Weight", typeof(int));
            diagnosticsDt.Columns.Add("PPAC Weight", typeof(int));
            diagnosticsDt.Columns.Add("Reason", typeof(string));
            diagnosticsDt.Columns.Add("Quick Find View Name", typeof(string));

            // Common fields counted once globally (PPAC-like)
            var commonFieldsCountOnce = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ownerid"
            };

            // Tracks which common fields were already counted once
            var commonAlreadyCounted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var view in views)
            {
                var entityLogicalName = view.GetAttributeValue<string>("returnedtypecode");
                if (string.IsNullOrEmpty(entityLogicalName)) continue;

                var viewName = view.GetAttributeValue<string>("name") ?? "(no name)";
                var fetchXml = view.GetAttributeValue<string>("fetchxml");
                var layoutXml = view.GetAttributeValue<string>("layoutxml");

                // Collect every field referenced by the view
                var fieldUsages = ExtractFieldUsagesFromView(fetchXml);
                if (fieldUsages.Count == 0) continue;

                // Resolve the entity display name (fall back to logical name)
                string entityDisplayName;
                if (!entityDisplayNames.TryGetValue(entityLogicalName, out entityDisplayName))
                    entityDisplayName = entityLogicalName;

                // Try to obtain the attribute dictionary for this entity
                Dictionary<string, AttributeMetadata> attrDict;
                attrCache.TryGetValue(entityLogicalName, out attrDict);

                foreach (var kvp in fieldUsages.OrderBy(k => k.Key))
                {
                    string fieldLogicalName = kvp.Key;
                    var usage = kvp.Value;

                    string fieldDisplayName = fieldLogicalName;
                    string fieldType = "Unknown";
                    string fieldSchemaName = string.Empty;
                    AttributeMetadata attrMeta = null;

                    // O(1) lookup in the pre-built dictionary
                    if (attrDict != null && attrDict.TryGetValue(fieldLogicalName, out attrMeta))
                    {
                        fieldDisplayName =
                            attrMeta.DisplayName?.UserLocalizedLabel?.Label ?? fieldLogicalName;
                        fieldType =
                            attrMeta.AttributeTypeName?.Value
                            ?? attrMeta.AttributeType?.ToString()
                            ?? "Unknown";

                        fieldSchemaName = attrMeta.SchemaName ?? string.Empty;
                    }

                    int indexWeight = GetIndexWeight(attrMeta);

                    // Is this a "common field" that PPAC-like counting treats as global?
                    bool isCommonField = commonFieldsCountOnce.Contains(fieldLogicalName);

                    // Count common fields only once globally (PPAC-like)
                    bool countedOnce = false;
                    int ppacWeight = indexWeight;
                    string exclusionReason = string.Empty;

                    if (isCommonField)
                    {
                        countedOnce = commonAlreadyCounted.Add(fieldLogicalName); // true only the first time
                        if (!countedOnce)
                        {
                            ppacWeight = 0;
                            exclusionReason = "Common field counted once globally (PPAC-like dedup)";
                        }
                    }

                    // (5.2) If it is ONLY a Find column (not a View column) and type is unsupported,
                    // it is ignored by Dataverse Search (PPAC-like weight = 0).
                    bool ignoredFindType =
                            usage.FromFindColumns && !usage.FromViewColumns && (attrMeta != null) && !IsSupportedFindColumnType(attrMeta);

                    if (ignoredFindType)
                    {
                        // IMPORTANT:
                        // PPAC appears to count these fields in the 950 budget in some environments,
                        // even if the search engine may ignore them for query matching.
                        // Therefore, to "Match PPAC counting" we DO NOT set ppacWeight = 0.
                        if (string.IsNullOrEmpty(exclusionReason))
                        {
                            exclusionReason = "Find column type may be ignored by search engine (still counted in PPAC)";
                        }
                    }

                    var row = dt.NewRow();
                    row["Entity Display Name"] = entityDisplayName;
                    row["Entity Logical Name"] = entityLogicalName;
                    row["Field Display Name"] = fieldDisplayName;

                    row["Field Logical Name"] = fieldLogicalName;
                    row["Field Schema Name"] = fieldSchemaName;

                    row["Field Type"] = fieldType;
                    row["Index Weight"] = indexWeight;

                    row["IsCommonField"] = isCommonField;
                    row["CountedOnce"] = countedOnce || !isCommonField; // for non-common fields treat as "counted"
                    row["PPAC Weight"] = ppacWeight;
                    row["PPAC Exclusion Reason"] = exclusionReason;

                    row["Quick Find View Name"] = viewName;

                    dt.Rows.Add(row);

                    // Diagnostics: show rows where PPAC weight differs from index weight
                    if (ppacWeight != indexWeight || !string.IsNullOrEmpty(exclusionReason))
                    {
                        var drow = diagnosticsDt.NewRow();
                        drow["Entity Logical Name"] = entityLogicalName;
                        drow["Entity Display Name"] = entityDisplayName;
                        drow["Field Logical Name"] = fieldLogicalName;
                        drow["Index Weight"] = indexWeight;
                        drow["PPAC Weight"] = ppacWeight;
                        drow["Reason"] = exclusionReason;
                        drow["Quick Find View Name"] = viewName;
                        diagnosticsDt.Rows.Add(drow);
                    }
                }
            }

            return dt;
        }

        // Tracks where a field is declared in the Quick Find view.
        // Needed to apply Dataverse Search rules:
        // - View columns are indexed
        // - Find columns are indexed only for supported types
        private sealed class FieldUsage
        {
            public bool FromViewColumns { get; set; }
            public bool FromFindColumns { get; set; }
        }
        // ──────────────────────────────────────────────────────────────
        //  FetchXml / LayoutXml parsing
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Extracts every unique field logical name referenced by a
        /// Quick Find view's fetchxml.  Two sources are combined:
        ///
        ///   fetchxml  → &lt;attribute name="xxx"/&gt;           (View columns)
        ///   fetchxml  → &lt;condition attribute="xxx"/&gt;      (Find columns only,
        ///               i.e. inside &lt;filter isquickfindfields="1"&gt;)
        ///
        /// Regular filter conditions (e.g. statecode = 0) are NOT
        /// included because they are not indexed by Dataverse Search.
        /// The layoutxml is intentionally NOT parsed because it can
        /// contain display-only cells that do not consume index slots,
        /// leading to an inflated count vs. the Admin Center.
        /// </summary>
        private Dictionary<string, FieldUsage> ExtractFieldUsagesFromView(string fetchXml)
        {
            var fields = new Dictionary<string, FieldUsage>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(fetchXml)) return fields;

            try
            {
                var doc = XDocument.Parse(fetchXml);
                var entity = doc.Root?.Element("entity");
                if (entity == null) return fields;

                // View columns declared as <attribute name="xxx" />
                foreach (var attr in entity.Elements("attribute"))
                {
                    var name = attr.Attribute("name")?.Value;
                    if (string.IsNullOrEmpty(name)) continue;

                    if (!fields.TryGetValue(name, out var usage))
                    {
                        usage = new FieldUsage();
                        fields[name] = usage;
                    }
                    usage.FromViewColumns = true;
                }

                // Find columns: <filter isquickfindfields="1"> ... <condition attribute="xxx" ... />
                foreach (var filter in entity.Descendants("filter"))
                {
                    if (filter.Attribute("isquickfindfields")?.Value != "1")
                        continue;

                    // Use Descendants("condition") to catch nested filters too.
                    foreach (var cond in filter.Descendants("condition"))
                    {
                        // (5.1) Related table fields are ignored by Dataverse Search.
                        // If entityname is present, this condition belongs to a linked entity.
                        if (!string.IsNullOrEmpty(cond.Attribute("entityname")?.Value))
                            continue;

                        var name = cond.Attribute("attribute")?.Value;
                        if (string.IsNullOrEmpty(name)) continue;

                        if (!fields.TryGetValue(name, out var usage))
                        {
                            usage = new FieldUsage();
                            fields[name] = usage;
                        }
                        usage.FromFindColumns = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogWarning("Error parsing fetchxml: " + ex.Message);
            }

            return fields;
        }

        // ──────────────────────────────────────────────────────────────
        //  Index weight calculation
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns how many index slots a single field consumes in the
        /// Dataverse Search index, as documented by Microsoft:
        ///
        ///   Lookup / Customer / Owner  → 3 slots
        ///   Picklist / State / Status  → 2 slots
        ///   All other types            → 1 slot
        ///
        /// See: https://learn.microsoft.com/en-us/power-platform/admin/
        ///      configure-relevance-search-organization
        /// </summary>
        private static int GetIndexWeight(AttributeMetadata attrMeta)
        {
            if (attrMeta?.AttributeType == null) return 1;

            switch (attrMeta.AttributeType.Value)
            {
                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Customer:
                case AttributeTypeCode.Owner:
                    return 3;
                case AttributeTypeCode.Picklist:
                case AttributeTypeCode.State:
                case AttributeTypeCode.Status:
                    return 2;
                default:
                    return 1;
            }
        }

        // (5.2) Find columns are searchable only for: Text, Lookups, Option Sets.
        // Other find column types are ignored by Dataverse Search.
        private static bool IsSupportedFindColumnType(AttributeMetadata attrMeta)
        {
            if (attrMeta?.AttributeType == null) return false;

            switch (attrMeta.AttributeType.Value)
            {
                case AttributeTypeCode.String:
                case AttributeTypeCode.Memo:
                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Customer:
                case AttributeTypeCode.Owner:
                case AttributeTypeCode.Picklist:
                case AttributeTypeCode.State:
                case AttributeTypeCode.Status:
                    return true;

                default:
                    return false;
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  UI helpers
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Updates the top-of-page banner that tells the user whether
        /// Dataverse Search is turned on or off at the organisation level.
        /// </summary>
        private void UpdateSearchStatusBanner(bool isEnabled)
        {
            if (isEnabled)
            {
                lblSearchStatus.Text =
                    "\u2705 Dataverse Search (Relevance Search) is ENABLED for this environment";
                lblSearchStatus.ForeColor = Color.DarkGreen;
            }
            else
            {
                lblSearchStatus.Text =
                    "\u274C Dataverse Search (Relevance Search) is set to DEFAULT (search bar hidden; Quick Find can be used) or is DISABLED or  for this environment";
                lblSearchStatus.ForeColor = Color.DarkRed;
            }
        }

        /// <summary>
        /// Applies consistent visual formatting to the DataGridView:
        /// alternating row colours, bold dark headers, sortable columns.
        /// </summary>
        private void FormatGrid()
        {
            if (dgvMatrix.Columns.Count == 0) return;

            dgvMatrix.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Alternating row style for readability
            dgvMatrix.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(240, 240, 255);

            // Bold dark-blue header row
            dgvMatrix.EnableHeadersVisualStyles = false;
            dgvMatrix.ColumnHeadersDefaultCellStyle.Font =
                new Font(dgvMatrix.Font, FontStyle.Bold);
            dgvMatrix.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(60, 60, 120);
            dgvMatrix.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            // Allow the user to click any column header to sort
            foreach (DataGridViewColumn col in dgvMatrix.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.Automatic;
            }
        }

        /// <summary>
        /// Builds the status-bar text showing both the raw field count
        /// (every field = 1) and the weighted Dataverse Search index
        /// usage (Lookup = 3, OptionSet = 2, others = 1) out of the
        /// 950 configurable-slot maximum.
        ///
        /// Note: the weighted total may differ slightly (~5%) from the
        /// Admin Center because the platform applies internal rules
        /// (e.g. default-indexed columns, unpublished changes) that
        /// are not fully exposed through the SDK.
        /// </summary>
        private static string BuildRecordCountText(DataView view, bool matchPpac)
        {
            int rawTotal = view.Count;
            int weightedTotal = 0;
            int ppacLikeTotal = 0;

            foreach (DataRowView drv in view)
            {
                weightedTotal += Convert.ToInt32(drv["Index Weight"]);

                // PPAC-like total uses the precomputed "PPAC Weight" per row
                // (0 for deduped/ignored fields).
                if (drv.Row.Table.Columns.Contains("PPAC Weight"))
                    ppacLikeTotal += Convert.ToInt32(drv["PPAC Weight"]);
            }

            int shownTotal = matchPpac ? ppacLikeTotal : weightedTotal;

            return "Rows: " + rawTotal
                 + " | Weighted index (per-table): ~" + weightedTotal + " / 950"
                 + " | PPAC-like: ~" + ppacLikeTotal + " / 950"
                 + " | Showing: ~" + shownTotal + " / 950";
        }

        /// <summary>
        /// Returns the best available display name for an entity,
        /// falling back to the logical name when no label is set.
        /// </summary>
        private static string GetEntityDisplayName(EntityMetadata em)
        {
            return em.DisplayName?.UserLocalizedLabel?.Label ?? em.LogicalName;
        }

        // ──────────────────────────────────────────────────────────────
        //  Search filter
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Triggered on every keystroke in the search box.
        /// Builds a DataView RowFilter that performs a case-insensitive
        /// LIKE across all string columns.  The column name list is
        /// pre-cached in <see cref="searchableColumnNames"/> to avoid
        /// re-enumerating the DataTable schema on every keystroke.
        /// </summary>
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (matrixTable == null || searchableColumnNames == null) return;

            // Explicit string variable avoids System.Memory extension
            // methods (Trim/Replace returning ReadOnlySpan<char> on .NET 4.8)
            string searchText = txtSearch.Text ?? string.Empty;
            searchText = searchText.Trim();

            if (searchText.Length == 0)
            {
                matrixTable.DefaultView.RowFilter = string.Empty;
            }
            else
            {
                // Escape characters that have special meaning in DataView
                // RowFilter LIKE expressions.
                // NOTE: separate statements avoid System.Memory extension
                // methods that return ReadOnlySpan<char> on .NET Framework 4.8.
                string escaped = searchText;
                escaped = escaped.Replace("'", "''");
                escaped = escaped.Replace("[", "[[]");
                escaped = escaped.Replace("%", "[%]");
                escaped = escaped.Replace("*", "[*]");

                // Build: [Col1] LIKE '%text%' OR [Col2] LIKE '%text%' ...
                var sb = new StringBuilder(searchableColumnNames.Length * 40);
                for (int i = 0; i < searchableColumnNames.Length; i++)
                {
                    if (i > 0) sb.Append(" OR ");
                    sb.Append('[').Append(searchableColumnNames[i])
                      .Append("] LIKE '%").Append(escaped).Append("%'");
                }
                matrixTable.DefaultView.RowFilter = sb.ToString();
            }

            RefreshFooterAndDiagnostics();
        }
        private void chkMatchPpac_CheckedChanged(object sender, EventArgs e)
        {
            // Show/hide diagnostics + splitter together
            bool show = chkMatchPpac.Checked;

            panelDiagnostics.Visible = show;
            splitterDiagnostics.Visible = show;

            // Optional: make the splitter easier to grab
            splitterDiagnostics.BringToFront();

            RefreshFooterAndDiagnostics();
        }

        private void RefreshFooterAndDiagnostics()
        {
            if (matrixTable == null) return;

            bool matchPpac = chkMatchPpac != null && chkMatchPpac.Checked;

            lblRecordCount.Text = BuildRecordCountText(matrixTable.DefaultView, matchPpac);

            // Show diagnostics panel only when PPAC mode is enabled
            
            if (panelDiagnostics != null)
                panelDiagnostics.Visible = matchPpac;
            // Keep the splitter visibility in sync with the diagnostics panel.
            splitterDiagnostics.Visible = panelDiagnostics.Visible;
            if (matchPpac && dgvDiagnostics != null && diagnosticsTable != null)
            {
                dgvDiagnostics.DataSource = diagnosticsTable;
                FormatDiagnosticsGrid();
            }
        }

        private void FormatDiagnosticsGrid()
        {
            if (dgvDiagnostics == null || dgvDiagnostics.Columns.Count == 0) return;

            dgvDiagnostics.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDiagnostics.ReadOnly = true;
            dgvDiagnostics.RowHeadersVisible = false;
            dgvDiagnostics.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void tsbReadme_Click(object sender, EventArgs e)
        {
            ShowReadmeDialog();
        }

        private void ShowReadmeDialog()
        {
            string text = LoadReadmeText();

            using (var dlg = new ReadmeDialog("Dataverse Search Fields Matrix - Readme", text))
            {
                dlg.ShowDialog(this);
            }
        }

        private string LoadReadmeText()
        {
            // 1) Try embedded resource first (recommended for XrmToolBox distribution).
            string embedded = TryLoadEmbeddedText("GM.XrmToolBox.DataVerseSearchFieldsMatrix.README.md");
            if (!string.IsNullOrWhiteSpace(embedded))
                return embedded;

            // 2) Fallback to README.md placed next to the plugin assembly.
            try
            {
                string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string filePath = Path.Combine(baseDir, "README.md");
                if (File.Exists(filePath))
                    return File.ReadAllText(filePath, Encoding.UTF8);
            }
            catch
            {
                // Ignore IO errors and fallback to default text.
            }

            return "README not found.\r\n\r\nAdd a README.md file to the project or embed it as a resource.";
        }

        private static string TryLoadEmbeddedText(string resourceName)
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                using (var stream = asm.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    using (var reader = new StreamReader(stream, Encoding.UTF8, true))
                        return reader.ReadToEnd();
                }
            }
            catch
            {
                return null;
            }
        }

        //private void splitterDiagnostics_SplitterMoved(object sender, SplitterEventArgs e)
        //{
        //    // Optional: persist user preference (requires a setting property)
        //    mySettings.DiagnosticsPanelHeight = panelDiagnostics.Height;
        //    SettingsManager.Instance.Save(GetType(), mySettings);
        //}

        // ──────────────────────────────────────────────────────────────
        //  CSV export
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Exports the currently visible (filtered) rows to a CSV file.
        /// Uses a pre-sized <see cref="StringBuilder"/> for efficiency
        /// and proper RFC 4180 quoting (double-quote escaping).
        /// </summary>
        private void tsbExportCsv_Click(object sender, EventArgs e)
        {
            if (matrixTable == null || matrixTable.Rows.Count == 0)
            {
                MessageBox.Show("No data to export. Please load data first.",
                    "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV files (*.csv)|*.csv";
                sfd.FileName = "DataverseSearchFieldsMatrix.csv";

                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    int colCount = matrixTable.Columns.Count;
                    int rowCount = matrixTable.DefaultView.Count;

                    // Rough pre-allocation: ~80 chars per cell
                    var sb = new StringBuilder(colCount * rowCount * 80);

                    // ── Header row ────────────────────────────────────
                    for (int c = 0; c < colCount; c++)
                    {
                        if (c > 0) sb.Append(',');
                        sb.Append('"').Append(matrixTable.Columns[c].ColumnName).Append('"');
                    }
                    sb.AppendLine();

                    // ── Data rows (respects the active search filter) ─
                    foreach (DataRowView drv in matrixTable.DefaultView)
                    {
                        for (int c = 0; c < colCount; c++)
                        {
                            if (c > 0) sb.Append(',');
                            var val = drv[c]?.ToString() ?? string.Empty;
                            sb.Append('"').Append(val.Replace("\"", "\"\"")).Append('"');
                        }
                        sb.AppendLine();
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);

                    MessageBox.Show(
                        "Exported " + rowCount + " rows to:" +
                        Environment.NewLine + sfd.FileName,
                        "Export Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error exporting: " + ex.Message,
                        "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
