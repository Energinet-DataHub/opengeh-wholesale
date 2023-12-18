CREATE TABLE [batches].[GridAreaOwner] 
(
    [Id] [uniqueidentifier] NOT NULL,
    [GridAreaCode] [nvarchar](3) NOT NULL,
    [OwnerActorNumber] [nvarchar](16) NOT NULL,
    [SequenceNumber] [int] NOT NULL,
    [ValidFrom] [datetime2] NOT NULL,
    CONSTRAINT [UC_GridAreaOwner_GridAreaCode_OwnerActorNumber_ValidFrom_SequenceNumber] UNIQUE ([GridAreaCode], [OwnerActorNumber], [ValidFrom], [SequenceNumber]),
)
