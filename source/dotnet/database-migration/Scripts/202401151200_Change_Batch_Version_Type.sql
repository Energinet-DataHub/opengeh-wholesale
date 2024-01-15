DECLARE @ObjectName NVARCHAR(100)
SELECT @ObjectName = OBJECT_NAME([default_object_id]) FROM SYS.COLUMNS
WHERE [object_id] = OBJECT_ID('[batches].[Batch]') AND [name] = 'Version';
EXEC('ALTER TABLE [batches].[Batch] DROP CONSTRAINT ' + @ObjectName)
GO

ALTER TABLE batches.Batch
ALTER COLUMN Version bigint NOT NULL 
GO