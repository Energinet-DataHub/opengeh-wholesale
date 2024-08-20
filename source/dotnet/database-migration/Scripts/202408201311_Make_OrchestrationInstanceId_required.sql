UPDATE [calculations].[Calculation]
    SET [OrchestrationInstanceId] = NEWID()
    WHERE [OrchestrationInstanceId] IS NULL
GO

ALTER TABLE [calculations].[Calculation]
    ALTER COLUMN [OrchestrationInstanceId] NVARCHAR(256) NOT NULL
GO
