# Dataverse Search Fields Matrix (XrmToolBox)

## Overview
**Dataverse Search Fields Matrix** is an XrmToolBox plugin that inspects your Dataverse environment and builds a matrix of fields involved in Dataverse Search / Relevance Search.

The tool is **read-only**: it does not modify views, fields, or settings.

## What the tool shows
For each table (entity) enabled for Dataverse Search, the matrix lists:
- Entity display name + logical name
- Quick Find view name (system Quick Find view)
- Field display name
- Field logical name
- Field schema name
- Field type
- **Index Weight** (per Microsoft rules: Lookup/Owner/Customer = 3, OptionSet/State/Status = 2, others = 1)

## Data sources used
The tool relies on:
- `organization.isexternalsearchindexenabled` (environment-level toggle)
- `EntityMetadata.SyncToExternalSearchIndex = true` (tables enabled for search)
- `savedquery` (system views) with:
  - `querytype = 4` (Quick Find)
  - `statecode = 0` (active)
  - `isdefault = true` (default)
- Quick Find `fetchxml` parsing:
  - View columns: `<attribute name="..."/>`
  - Find columns: `<filter isquickfindfields="1"> ... <condition attribute="..."/>`

## Counting modes
### Weighted index (per-table)
This is the sum of **Index Weight** across all rows in the matrix.
It represents a “per-table interpretation” of the configured index footprint.

### PPAC-like counting
Power Platform Admin Center (PPAC) shows “Columns indexed for search” as *X of 950*.
This value is not always a simple sum per-table, because some **common fields** can be treated as global defaults.

When **Match PPAC counting** is enabled:
- The tool computes a **PPAC Weight** per row
- Some fields can be **counted once globally** (deduplicated), producing PPAC-like totals

## Known exceptions and important notes
- A table may be enabled for Dataverse Search but still not appear in the matrix if it has no **active default** Quick Find view.
- Related-table fields (conditions with `entityname="..."`) are ignored by Dataverse Search.
- Some Find column types may be ignored by the search engine for query matching, but may still appear as counted in PPAC in certain environments. The tool flags these rows as warnings in Diagnostics.

## Diagnostics pane
When **Match PPAC counting** is enabled, a diagnostics pane shows:
- Which rows are excluded from PPAC-like counting
- Whether a field was deduplicated (CountedOnce)
- The reason / rule applied

The diagnostics pane can be resized using the splitter bar.

## Export
Use **Export CSV** to export the full matrix, including PPAC fields and diagnostics columns.

## Performance considerations
The tool optimizes network round-trips:
- One call for organization status
- One call for entity list (entity metadata only)
- One call for Quick Find views
- One call per enabled table to retrieve attributes (metadata)

Large environments may still take time; the tool runs the loading in XrmToolBox async worker to keep the UI responsive.

## Support / Contribution
- Repository: GM.XrmToolBox.DataVerseSearchFieldsMatrix
- Author: GM