ALTER TABLE [integrationevents].[CompletedBatch]
    ADD PublishedTime DATETIME2;
GO

UPDATE [integrationevents].[CompletedBatch]
    SET PublishedTime = CompletedTime
    WHERE IsPublished = 1;
GO

ALTER TABLE [integrationevents].[CompletedBatch]
    DROP COLUMN IsPublished;
GO
