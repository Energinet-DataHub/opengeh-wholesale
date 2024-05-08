CREATE SCHEMA [settlementreports]
GO

CREATE TABLE [settlementreports].[SettlementReportRequest]
(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [uniqueidentifier] NOT NULL,
    [ActorId] [uniqueidentifier] NOT NULL,
    [RequestId] [nvarchar](512) NOT NULL,
    [CreatedDateTime] [datetimeoffset] NOT NULL,
    [Status] int NOT NULL,
    [BlobFilename] [nvarchar](2048),
    CONSTRAINT [PK_SettlementReportRequest] PRIMARY KEY ([Id])
)