-- This script is used to drop the hive_metastore database if it exists.
-- The CASCADE option is used to drop all objects in the database before dropping the database.
DROP DATABASE IF EXISTS hive_metastore CASCADE;
GO
