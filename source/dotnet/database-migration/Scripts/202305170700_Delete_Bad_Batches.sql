-- Perhaps these are old batches created before introducing execution time.
-- We, however, don't know for sure as we don't timestamp when batches are created.
-- Create them as they block publishing of integration events moving to solution not using domain events.
-- This will only affect development and test environments.
delete from batches.Batch
where executionstate = 2
    and (executiontimestart is null or executiontimeend is null)
GO

-- Results of old batches where actors file isn't created can't be (re)published using the current logic.
-- So we delete them (and probably also some that could have been published).
-- The exact time isn't important as this will only affect development and test environments.
delete from batches.Batch
where executiontimestart < DATEADD(day, -50, GETDATE())
GO
delete from integrationevents.CompletedBatch
where CompletedTime < DATEADD(day, -50, GETDATE())
