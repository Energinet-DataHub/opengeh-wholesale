-- Changes to 'integrationevents'
-- Column
EXEC sp_rename 'integrationevents.CompletedCalculation.ProcessType', 'CalculationType', 'COLUMN';
GO

-- Changes to 'calculations'
-- Column
EXEC sp_rename 'calculations.Calculation.ProcessType', 'CalculationType', 'COLUMN';
GO
