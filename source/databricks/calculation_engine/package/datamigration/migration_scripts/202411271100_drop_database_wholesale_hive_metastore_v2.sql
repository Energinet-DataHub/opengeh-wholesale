-- This script is used to drop the hive_metastore database if it exists.
-- The CASCADE option is used to drop all objects in the database before dropping the database.
USE default
DROP DATABASE IF EXISTS basis_data CASCADE
