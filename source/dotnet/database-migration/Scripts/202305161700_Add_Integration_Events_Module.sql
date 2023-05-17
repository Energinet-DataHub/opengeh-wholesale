CREATE SCHEMA [integrationevents]
GO

CREATE TABLE [integrationevents].[CompletedBatch]
(
    Id UNIQUEIDENTIFIER NOT NULL,
    ProcessType INT NOT NULL,
    GridAreaCodes [varchar](MAX) NOT NULL, -- JSON array of grid area code strings, e.g. ["004","805"]
    PeriodStart DATETIME2 NOT NULL,
    PeriodEnd DATETIME2 NOT NULL,
    CompletedTime DATETIME2 NOT NULL,
    IsPublished BIT NOT NULL,
    CONSTRAINT [PK_CompletedBatch] PRIMARY KEY NONCLUSTERED
(
[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    GO

CREATE SCHEMA batches
GO

-- move table dbo.Batch to schema batches
ALTER SCHEMA batches TRANSFER dbo.Batch
GO

-- move table dbo.OutboxMessage to schema integrationevents
ALTER SCHEMA integrationevents TRANSFER dbo.OutboxMessage
GO
