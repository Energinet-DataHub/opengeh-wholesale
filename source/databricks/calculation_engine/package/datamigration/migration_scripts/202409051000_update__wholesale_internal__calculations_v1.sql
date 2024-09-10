ALTER TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations_v1 SET TBLPROPERTIES (
    'delta.columnMapping.mode' = 'name',
    'delta.minReaderVersion' = '2',
    'delta.minWriterVersion' = '5')
GO

ALTER TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations_v1
ADD COLUMN is_internal_calculation BOOLEAN
GO

MERGE INTO {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations_v1 AS target
USING {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations AS source
ON target.calculation_id = source.calculation_id
WHEN MATCHED THEN
  UPDATE SET target.is_internal_calculation = source.is_internal_calculation;
GO

UPDATE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations_v1
SET is_internal_calculation = FALSE
WHERE is_internal_calculation IS NULL;
GO

ALTER TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations_v1
    ADD CONSTRAINT is_internal_calculation_chk
    CHECK (is_internal_calculation IS NOT NULL)
