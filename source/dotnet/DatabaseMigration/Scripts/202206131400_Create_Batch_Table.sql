CREATE TABLE [dbo].[Batch]
(
    [RowId] [uniqueidentifier] NOT NULL,
    [Id] [varchar](36) NOT NULL,
    [GridArea] [nvarchar](50) NOT NULL,
    [ExecutionState] [int] NOT NULL,
    CONSTRAINT [PK_Batch] PRIMARY KEY CLUSTERED
(
[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    GO

-- The grid area IDs in the batch
CREATE TABLE [dbo].[BatchGridArea]
(
    [RowId] [uniqueidentifier] NOT NULL,
    [BatchRowId] [uniqueidentifier] NOT NULL,
    [GridAreaId] [varchar](36) NOT NULL,
    CONSTRAINT [PK_BatchGridArea] PRIMARY KEY CLUSTERED
(
[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    GO

ALTER TABLE [dbo].[BatchGridArea]  WITH CHECK ADD  CONSTRAINT [FK_BatchGridArea_Batch] FOREIGN KEY([BatchRowId])
    REFERENCES [dbo].[Batch] ([RowId])
    GO
