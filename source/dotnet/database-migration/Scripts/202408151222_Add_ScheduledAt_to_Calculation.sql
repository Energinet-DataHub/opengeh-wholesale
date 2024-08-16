ALTER TABLE [calculations].[Calculation]
    ADD [ScheduledAt] DATETIME2 NULL
GO

UPDATE [calculations].[Calculation]
    SET [ScheduledAt] = [ExecutionTimeStart]
    WHERE [ScheduledAt] IS NULL AND [ExecutionTimeStart] IS NOT NULL
GO

UPDATE [calculations].[Calculation]
    SET [ScheduledAt] = '2000-01-01 00:00:00'
    WHERE [ScheduledAt] IS NULL AND [ExecutionTimeStart] IS NULL
GO

ALTER TABLE [calculations].[Calculation]
    ALTER COLUMN [ScheduledAt] DATETIME2 NOT NULL
GO
