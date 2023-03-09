CREATE TABLE [OutboxMessages]
(
    [Id]            [uniqueidentifier]   NOT NULL,
    [RecordId]      [int] IDENTITY (1,1) NOT NULL,
    [EventType]     [nvarchar](255)      NOT NULL,
    [EventData]     [nvarchar](max)      NOT NULL,
    [CreationDate]  [datetime2](7)       NOT NULL,
    [ProcessedDate] [datetime2](7)       NULL,
    CONSTRAINT [PK_OutboxMessages] PRIMARY KEY NONCLUSTERED ([Id] ASC)
)
