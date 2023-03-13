CREATE TABLE [OutboxMessages]
(
    [Id]            [uniqueidentifier]   NOT NULL,
    [Type]     [nvarchar](max)      NOT NULL,
    [Data]     [nvarchar](max)      NOT NULL,
    [CreationDate]  [datetime2](7)       NOT NULL,
    [ProcessedDate] [datetime2](7)       NULL,
    CONSTRAINT [PK_OutboxMessages] PRIMARY KEY NONCLUSTERED ([Id] ASC)
)
