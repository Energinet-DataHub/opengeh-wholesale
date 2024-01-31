-- Changes to 'integrationevents'
-- Table
EXEC sp_rename 'integrationevents.CompletedBatch', 'CompletedCalculation';
GO
-- Primary keys
EXEC sp_rename 'integrationevents.PK_CompletedBatch', 'PK_CompletedCalculation';
GO

-- Changes to 'batches'
-- Schema
CREATE SCHEMA calculations;
GO
ALTER SCHEMA calculations TRANSFER batches.Batch;
GO
ALTER SCHEMA calculations TRANSFER batches.GridAreaOwner;
GO
ALTER SCHEMA calculations TRANSFER batches.ReceivedIntegrationEvent
GO
DROP SCHEMA batches;
GO
-- Table
EXEC sp_rename 'calculations.Batch', 'Calculation';
GO
-- Primary keys
EXEC sp_rename 'calculations.PK_Batch', 'PK_Calculation';
GO
