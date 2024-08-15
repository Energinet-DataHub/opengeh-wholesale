ALTER TABLE [calculations].[Calculation]
    ADD [ScheduledAt] DATETIME2 NULL
GO

UPDATE [calculations].[Calculation]
    SET [ScheduledAt] = [ExecutionTimeStart]
    WHERE [ScheduledAt] IS NULL
GO

ALTER TABLE [calculations].[Calculation]
    ALTER COLUMN [ScheduledAt] DATETIME2 NOT NULL
GO
