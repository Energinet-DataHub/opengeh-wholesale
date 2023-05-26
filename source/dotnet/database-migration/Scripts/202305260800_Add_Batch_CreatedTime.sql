ALTER TABLE batches.Batch
    ADD CreatedTime DATETIME2 NOT NULL DEFAULT GETDATE();
GO

UPDATE batches.Batch
    SET CreatedTime = ExecutionTimeStart;
GO
