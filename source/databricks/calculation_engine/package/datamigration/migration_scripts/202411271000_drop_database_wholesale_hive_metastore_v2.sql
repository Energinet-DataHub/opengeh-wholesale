-- This script is used to drop the hive_metastore database if it exists.
-- The CASCADE option is used to drop all objects in the database before dropping the database.
USE CATALOG hive_metastore;
GO

DROP CATALOG IF EXISTS hive_metastore CASCADE;
GO
