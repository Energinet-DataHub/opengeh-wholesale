-- This guard was added as a workaround because apparently the migration was run without being registered
-- and thus failing when running subsequently (in t-001).
IF NOT EXISTS (
   SELECT 1
   FROM sys.columns
   WHERE Name = 'CreatedTime' AND Object_ID = Object_ID('batches.Batch')
)
BEGIN
    ALTER TABLE batches.Batch
        ADD CreatedTime DATETIME2 NOT NULL DEFAULT GETDATE();
    
    UPDATE batches.Batch
    SET CreatedTime = ExecutionTimeStart;
END
