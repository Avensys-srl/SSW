-- Additive: old clients and historical technical revisions have no offer state.
ALTER TABLE ssw_selection_revisions
    ADD COLUMN offer_status ENUM('Provisional', 'Definitive') NULL,
    ADD COLUMN offer_generated_at_utc DATETIME(6) NULL,
    ADD COLUMN offer_definitive_at_utc DATETIME(6) NULL;

INSERT INTO ssw_schema_migrations (version, description)
VALUES (10, 'Per-revision provisional and definitive offer status');
