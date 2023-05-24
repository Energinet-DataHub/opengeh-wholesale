IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[integrationevents].[OutboxMessage]') AND type in (N'U'))
DROP TABLE [integrationevents].[OutboxMessage]
GO