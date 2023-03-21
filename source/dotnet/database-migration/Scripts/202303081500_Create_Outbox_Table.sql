CREATE TABLE [OutboxMessage]
(
    [Id]       [uniqueidentifier]   NOT NULL,
    [MessageType]     [nvarchar](max)      NOT NULL,
    [Data]     [varbinary](max)            NOT NULL,
    [CreationDate]  [datetime2](7)       NOT NULL,
    [ProcessedDate] [datetime2](7)       NULL,
    CONSTRAINT [PK_OutboxMessage] PRIMARY KEY NONCLUSTERED ([Id] ASC)
)
