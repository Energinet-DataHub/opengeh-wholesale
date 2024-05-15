ALTER TABLE [settlementreports].[SettlementReport]
DROP COLUMN [CreatedDateTime]
GO

ALTER TABLE [settlementreports].[SettlementReport]
ADD [CreatedDateTime] DATETIME2 NOT NULL
GO
