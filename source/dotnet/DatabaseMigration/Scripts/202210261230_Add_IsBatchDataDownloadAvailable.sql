ALTER TABLE Batch
    ADD IsBasisDataDownloadAvailable BIT;
GO

UPDATE Batch SET IsBasisDataDownloadAvailable=1 WHERE IsBasisDataDownloadAvailable IS NULL
ALTER TABLE Batch
ALTER COLUMN IsBasisDataDownloadAvailable BIT NOT NULL;
GO
