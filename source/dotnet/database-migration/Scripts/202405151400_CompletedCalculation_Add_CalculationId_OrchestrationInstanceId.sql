ALTER TABLE integrationevents.CompletedCalculation
    ADD OrchestrationInstanceId [varchar](100) NULL,
    Version [bigint] NOT NULL;
GO
