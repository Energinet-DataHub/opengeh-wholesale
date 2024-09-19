CREATE TABLE [dbo].[Outbox]
(
    [Id]                UNIQUEIDENTIFIER NOT NULL,
    [RecordId]          INT IDENTITY (1,1) NOT NULL,

    -- ROWVERSION makes Entity Framework throw an exception if trying to update a row which has already been updated (concurrency conflict)
    -- https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations
    [RowVersion]        ROWVERSION NOT NULL,

    [Type]              NVARCHAR(255) NOT NULL,
    [Payload]           NVARCHAR(MAX) NOT NULL,
    [CreatedAt]         DATETIME2 NOT NULL,
    [ProcessingAt]      DATETIME2 NULL,
    [PublishedAt]       DATETIME2 NULL,
    [FailedAt]          DATETIME2 NULL,
    [ErrorMessage]      NVARCHAR(MAX) NULL,
    [ErrorCount]        INT NOT NULL,

    CONSTRAINT [PK_Outbox]            PRIMARY KEY NONCLUSTERED ([Id]),

    -- A UNIQUE CLUSTERED constraint on an INT IDENTITY column optimizes the performance of the outbox table
    -- by ordering indexes by the sequential RecordId column instead of the UNIQUEIDENTIFIER  primary key (which is random).
    CONSTRAINT [UX_Outbox_RecordId]   UNIQUE CLUSTERED ([RecordId] ASC),
)
GO

-- The index used for finding up messages to process in the Outbox table
CREATE INDEX [IX_Outbox_PublishedAt_FailedAt_ProcessingAt_CreatedAt]
    ON [dbo].[Outbox] ([PublishedAt], [FailedAt], [ProcessingAt], [CreatedAt])
    INCLUDE ([Id])
GO
