-- Add nullable fields as nullable to support existing rows
ALTER TABLE integrationevents.CompletedCalculation
    ADD
        [OrchestrationInstanceId] NVARCHAR(100) NULL,
        [Version]                 BIGINT NULL;
GO

-- Set values on existing rows
UPDATE integrationevents.CompletedCalculation
    SET [OrchestrationInstanceId] = '00000000-0000-0000-0000-000000000001'
    WHERE [OrchestrationInstanceId] IS NULL

UPDATE integrationevents.CompletedCalculation
    SET [Version] = 1
    WHERE [Version] IS NULL
GO

-- Remove nullable from new fields
ALTER TABLE integrationevents.CompletedCalculation
    ALTER COLUMN [OrchestrationInstanceId] NVARCHAR(100) NOT NULL

ALTER TABLE integrationevents.CompletedCalculation
    ALTER COLUMN [Version] BIGINT NOT NULL;
GO