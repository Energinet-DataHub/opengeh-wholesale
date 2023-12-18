CREATE TABLE [batches].[ReceivedIntegrationEvent] 
(
    [Id] [uniqueidentifier] NOT NULL,
    [OccurredOn] [datetime2] NOT NULL,
    [EventType] [nvarchar](MAX) NOT NULL,
    CONSTRAINT [UC_ReceivedIntegrationEvents_Id] UNIQUE ([Id]),
    CONSTRAINT [PK_ReceivedIntegrationEvents] PRIMARY KEY NONCLUSTERED ([Id])
)
