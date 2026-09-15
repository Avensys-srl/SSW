# Automatic DataCentral catalog update

On normal startup, before `CLEnvironment` opens SQL Server Compact, SSW reads
the manifest configured by `DataCentralCatalogManifestUrl`.

The updater:

1. shows a startup status window;
2. validates manifest version, schema, customer and minimum SSW version;
3. skips a catalog version already recorded in `catalog-update.state.json`;
4. downloads to `DataCentral.sdf.download`;
5. verifies byte length and SHA-256;
6. validates the downloaded SDF using `CLDatabaseCompatibilityReader`;
7. replaces the live file and keeps `DataCentral.sdf.previous`;
8. records the applied catalog version only after successful replacement.

Manifest/network/download failures are logged to `data/catalog-update.log` and
do not prevent SSW from opening with the previous valid catalog. Automated
smoke and screenshot commands do not contact the catalog endpoint.

The publishing side is documented in csvexporter's `EXPORT_SSW.md`. A catalog
is published only after the existing exporter has generated and validated it.
