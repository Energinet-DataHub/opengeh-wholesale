ALTER TABLE [settlementreports].[SettlementReport]
ADD [CalculationType] [int] NOT NULL;
GO

ALTER TABLE [settlementreports].[SettlementReport]
ADD [PeriodStart] [datetime2] NOT NULL;
GO

ALTER TABLE [settlementreports].[SettlementReport]
ADD [PeriodEnd] [datetime2] NOT NULL;
GO

ALTER TABLE [settlementreports].[SettlementReport]
ADD [GridAreaCount] [int] NOT NULL;
GO

ALTER TABLE [settlementreports].[SettlementReport]
ADD [ContainsBasisData] [bit] NOT NULL;
GO
