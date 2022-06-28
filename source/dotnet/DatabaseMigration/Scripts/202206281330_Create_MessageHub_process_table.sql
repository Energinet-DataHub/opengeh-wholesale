IF NOT EXISTS ( SELECT  *
                FROM    sys.schemas
                WHERE   name = N'messagehub' )
    EXEC('CREATE SCHEMA [messagehub]');
go

drop table if exists messagehub.Process
    go

CREATE TABLE [messagehub].[Process]
(
    [Id] [uniqueidentifier] NOT NULL,
    [MessageHubReference] varchar(36) NOT NULL,
    [GridAreaCode] char(3) NOT NULL,
    CONSTRAINT [PK_Batch] PRIMARY KEY NONCLUSTERED
(
[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    GO
