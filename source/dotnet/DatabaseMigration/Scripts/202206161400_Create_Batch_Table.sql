drop table if exists dbo.BatchGridArea
drop table if exists dbo.[Batch]
go

CREATE TABLE [dbo].[Batch]
(
    [Id] [uniqueidentifier] NOT NULL,
    [GridAreaCodes] [varchar](MAX) NOT NULL, -- JSON array of grid area code strings, e.g. ["004","805"]
    [ExecutionState] [int] NOT NULL,
    CONSTRAINT [PK_Batch] PRIMARY KEY NONCLUSTERED
(
[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    GO
