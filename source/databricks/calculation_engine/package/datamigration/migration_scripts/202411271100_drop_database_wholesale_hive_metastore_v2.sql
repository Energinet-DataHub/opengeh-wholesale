-- This script is used to drop the hive_metastore database if it exists.
-- The CASCADE option is used to drop all objects in the database before dropping the database.
USE basis_data
DROP DATABASE IF EXISTS basis_data CASCADE
GO

USE schema_migration
DROP DATABASE IF EXISTS schema_migration CASCADE
GO

USE settlement_report
DROP DATABASE IF EXISTS settlement_report CASCADE
GO

USE wholesale_calculation_results
DROP DATABASE IF EXISTS wholesale_calculation_results CASCADE
GO

USE wholesale_input
DROP DATABASE IF EXISTS wholesale_input CASCADE
GO

USE wholesale_internal
DROP DATABASE IF EXISTS wholesale_internal CASCADE
GO

USE wholesale_output
DROP DATABASE IF EXISTS wholesale_output CASCADE
GO

USE wholesale_output_anonymised
DROP DATABASE IF EXISTS wholesale_output_anonymised CASCADE
GO
