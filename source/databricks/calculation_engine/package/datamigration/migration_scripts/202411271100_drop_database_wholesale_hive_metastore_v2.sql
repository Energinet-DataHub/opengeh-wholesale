-- This script is used to drop the hive_metastore database if it exists.
-- The CASCADE option is used to drop all objects in the database before dropping the database.
DROP DATABASE IF EXISTS hive_metastore.basis_data CASCADE
GO

DROP DATABASE IF EXISTS hive_metastore.schema_migration CASCADE
GO

DROP DATABASE IF EXISTS hive_metastore.settlement_report CASCADE
GO

DROP DATABASE IF EXISTS hive_metastore.wholesale_calculation_results CASCADE
GO

DROP DATABASE IF EXISTS hive_metastore.wholesale_input CASCADE
GO
     
DROP DATABASE IF EXISTS hive_metastore.wholesale_internal CASCADE
GO

DROP DATABASE IF EXISTS hive_metastore.wholesale_output CASCADE
GO

DROP DATABASE IF EXISTS hive_metastore.wholesale_output_anonymised CASCADE
GO
