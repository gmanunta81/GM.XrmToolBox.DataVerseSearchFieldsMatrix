using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace GM.XrmToolBox.DataVerseSearchFieldsMatrix
{
    [Export(typeof(IXrmToolBoxPlugin)),
        ExportMetadata("Name", "Dataverse Search Fields Matrix"),
        ExportMetadata("Description", "Displays a matrix of all fields indexed for Dataverse Search (Relevance Search) from Quick Find views across all enabled entities."),
        ExportMetadata("SmallImageBase64", null),
        ExportMetadata("BigImageBase64", null),
        ExportMetadata("BackgroundColor", "Lavender"),
        ExportMetadata("PrimaryFontColor", "Black"),
        ExportMetadata("SecondaryFontColor", "Gray")]
    public class MyPlugin : PluginBase, IAboutPlugin
    {
        public override IXrmToolBoxPluginControl GetControl()
        {
            return new MyPluginControl();
        }

        public MyPlugin()
        {
            // If you have external assemblies that you need to load, uncomment the following to
            // hook into the event that will fire when an Assembly fails to resolve
            // AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolveEventHandler);
        }

        public void ShowAboutDialog()
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
                var lblTitle = new Label();
                lblTitle.Text = "Dataverse Search Fields Matrix";
                lblTitle.Font = new Font(lblTitle.Font.FontFamily, 14, FontStyle.Bold);
                lblTitle.AutoSize = true;
                lblTitle.Location = new Point(80, 125);
                dlg.Controls.Add(lblTitle);

                // Version
                var lblVersion = new Label();
                lblVersion.Text = "Version " + version;
                lblVersion.AutoSize = true;
                lblVersion.ForeColor = Color.Gray;
                lblVersion.Location = new Point(175, 155);
                dlg.Controls.Add(lblVersion);

                // Author
                var lblAuthor = new Label();
                lblAuthor.Text = "by Giovanni Manunta";
                lblAuthor.AutoSize = true;
                lblAuthor.Location = new Point(155, 180);
                dlg.Controls.Add(lblAuthor);

                // Description
                var lblDesc = new Label();
                lblDesc.Text = "Displays all fields indexed for Dataverse Search\n"
                             + "(Relevance Search) from Quick Find views.";
                lblDesc.AutoSize = true;
                lblDesc.ForeColor = Color.DimGray;
                lblDesc.Location = new Point(80, 210);
                dlg.Controls.Add(lblDesc);

                // Accuracy note
                var lblNote = new Label();
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
                    try
                    {
                        Process.Start(new ProcessStartInfo("https://github.com/gmanunta81") { UseShellExecute = true });
                    }
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
                    try
                    {
                        Process.Start(new ProcessStartInfo(
                            "https://learn.microsoft.com/en-us/power-platform/admin/configure-relevance-search-organization")
                        { UseShellExecute = true });
                    }
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

                dlg.ShowDialog();
            }
        }

        private Assembly AssemblyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            Assembly loadAssembly = null;
            Assembly currAssembly = Assembly.GetExecutingAssembly();

            var argName = args.Name.Substring(0, args.Name.IndexOf(","));

            List<AssemblyName> refAssemblies = currAssembly.GetReferencedAssemblies().ToList();
            var refAssembly = refAssemblies.Where(a => a.Name == argName).FirstOrDefault();

            if (refAssembly != null)
            {
                string dir = Path.GetDirectoryName(currAssembly.Location).ToLower();
                string folder = Path.GetFileNameWithoutExtension(currAssembly.Location);
                dir = Path.Combine(dir, folder);

                var assmbPath = Path.Combine(dir, $"{argName}.dll");

                if (File.Exists(assmbPath))
                {
                    loadAssembly = Assembly.LoadFrom(assmbPath);
                }
                else
                {
                    throw new FileNotFoundException($"Unable to locate dependency: {assmbPath}");
                }
            }

            return loadAssembly;
        }
    }
}