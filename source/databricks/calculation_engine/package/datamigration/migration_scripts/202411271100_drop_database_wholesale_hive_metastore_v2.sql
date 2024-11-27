-- This script is used to drop the hive_metastore database if it exists.
-- The CASCADE option is used to drop all objects in the database before dropping the database.
USE basis_data
GO

DROP DATABASE IF EXISTS basis_data CASCADE
GO

USE schema_migration
GO

DROP DATABASE IF EXISTS schema_migration CASCADE
GO

USE settlement_report
GO

DROP DATABASE IF EXISTS settlement_report CASCADE
GO

USE wholesale_calculation_results
GO

DROP DATABASE IF EXISTS wholesale_calculation_results CASCADE
GO

USE wholesale_input
GO

DROP DATABASE IF EXISTS wholesale_input CASCADE
GO

USE wholesale_internal
GO

DROP DATABASE IF EXISTS wholesale_internal CASCADE
GO

USE wholesale_output
GO

DROP DATABASE IF EXISTS wholesale_output CASCADE
GO

USE wholesale_output_anonymised
GO

DROP DATABASE IF EXISTS wholesale_output_anonymised CASCADE
GO
