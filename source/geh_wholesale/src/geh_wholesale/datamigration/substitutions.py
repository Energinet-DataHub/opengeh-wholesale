from geh_wholesale.infrastructure import paths


def get_substitutions(catalog_name: str, is_testing: bool) -> dict[str, str]:
    """Get the list of substitutions for the migration scripts.

    This is the authoritative list of substitutions that can be used in all migration scripts.
    """
    return {
        "{CATALOG_NAME}": catalog_name,
        "{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}": paths.WholesaleResultsInternalDatabase().DATABASE_WHOLESALE_RESULTS_INTERNAL,
        "{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}": paths.WholesaleBasisDataInternalDatabase().DATABASE_WHOLESALE_BASIS_DATA_INTERNAL,
        "{WHOLESALE_BASIS_DATA_DATABASE_NAME}": paths.WholesaleBasisDataDatabase().DATABASE_WHOLESALE_BASIS_DATA,
        "{WHOLESALE_INTERNAL_DATABASE_NAME}": paths.WholesaleInternalDatabase().DATABASE_WHOLESALE_INTERNAL,
        "{WHOLESALE_RESULTS_DATABASE_NAME}": paths.WholesaleResultsDatabase().DATABASE_WHOLESALE_RESULTS,
        "{WHOLESALE_SAP_DATABASE_NAME}": paths.WholesaleSapDatabase().DATABASE_WHOLESALE_SAP,
        "{SHARED_WHOLESALE_INPUT}": paths.MigrationsWholesaleDatabase().DATABASE_WHOLESALE_MIGRATION,
        # Flags
        "{DATABRICKS-ONLY}": (
            "--" if is_testing else ""
        ),  # Comment out script lines when running in test as it is not using Databricks
        "{TEST-ONLY}": (
            "--" if not is_testing else ""
        ),  # Comment out script lines when running in test as it is not using Databricks
    }
