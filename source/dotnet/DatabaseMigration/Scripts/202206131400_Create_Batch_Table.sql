CREATE TABLE [dbo].[Batch]
(
    [Id] [uniqueidentifier] NOT NULL,
    [GridAreaIds] [varchar](MAX) NOT NULL, -- JSON array
    [ExecutionState] [int] NOT NULL,
    CONSTRAINT [PK_Batch] PRIMARY KEY NONCLUSTERED
(
[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    GO
