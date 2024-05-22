-- Add nullable fields as nullable to support existing rows
ALTER TABLE calculations.Calculation
    ADD
        [OrchestrationState] INT NULL
GO

-- Set values on existing rows
UPDATE calculations.Calculation -- Map from (Created || Submitted || Pending) to Scheduled 
    SET [OrchestrationState] = 1            
    WHERE [OrchestrationState] IS NULL AND [ExecutionState] IN (-2, -1, 0)   

UPDATE calculations.Calculation -- Map from Executing to Calculating
    SET [OrchestrationState] = 2
    WHERE [OrchestrationState] IS NULL AND [ExecutionState] = 1

UPDATE calculations.Calculation -- Map from Completed to Completed
    SET [OrchestrationState] = 8
    WHERE [OrchestrationState] IS NULL AND [ExecutionState] = 2

UPDATE calculations.Calculation -- Map from Failed to CalculationFailed
    SET [OrchestrationState] = 4
    WHERE [OrchestrationState] IS NULL AND [ExecutionState] = 3

UPDATE calculations.Calculation -- Map from Canceled to CalculationFailed
    SET [OrchestrationState] = 4
    WHERE [OrchestrationState] IS NULL AND [ExecutionState] = 4
GO

-- Remove nullable from new fields
ALTER TABLE calculations.Calculation
    ALTER COLUMN [OrchestrationState] INT NOT NULL
GO