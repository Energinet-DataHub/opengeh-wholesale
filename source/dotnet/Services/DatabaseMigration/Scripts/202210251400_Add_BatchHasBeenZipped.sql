ALTER TABLE Batch
    ADD BatchHasBeenZipped BIT;
GO

UPDATE Batch SET BatchHasBeenZipped=1 WHERE BatchHasBeenZipped IS NULL
ALTER TABLE Batch
ALTER COLUMN BatchHasBeenZipped BIT NOT NULL;
GO
