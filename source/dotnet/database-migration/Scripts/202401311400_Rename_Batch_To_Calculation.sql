-- Changes to 'integrationevents'
-- Table
EXEC sp_rename 'integrationevents.CompletedBatch', 'integrationevents.CompletedCalculation';
GO

-- Changes to 'batches'
-- Schema
EXEC sp_rename 'batches', 'calculations';
GO
-- Table
EXEC sp_rename 'calculations.Batch', 'calculations.Calculation';
GO
