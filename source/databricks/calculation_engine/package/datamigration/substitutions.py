from package.infrastructure import paths


def get_substitutions(catalog_name: str, is_testing: bool) -> dict[str, str]:
    """
    Get the list of substitutions for the migration scripts.
    This is the authoritative list of substitutions that can be used in all migration scripts.
    """

    return {
        "{CATALOG_NAME}": catalog_name,
        "{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}": paths.WholesaleResultsInternalDatabase.DATABASE_NAME,
        "{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}": paths.WholesaleBasisDataInternalDatabase.DATABASE_NAME,
        "{WHOLESALE_BASIS_DATA_DATABASE_NAME}": paths.WholesaleBasisDataDatabase.DATABASE_NAME,
        "{WHOLESALE_INTERNAL_DATABASE_NAME}": paths.WholesaleInternalDatabase.DATABASE_NAME,
        "{WHOLESALE_RESULTS_DATABASE_NAME}": paths.WholesaleResultsDatabase.DATABASE_NAME,
        "{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}": paths.WholesaleSettlementReportsDatabase.DATABASE_NAME,
        "{WHOLESALE_SAP_DATABASE_NAME}": paths.WholesaleSapDatabase.DATABASE_NAME,
        # Remove this substitution when the old hive migration is removed
        "{INPUT_DATABASE_NAME}": paths.InputDatabase.DATABASE_NAME,
        # Flags
        "{DATABRICKS-ONLY}": (
            "--" if is_testing else ""
        ),  # Comment out script lines when running in test as it is not using Databricks
        "{TEST-ONLY}": (
            "--" if not is_testing else ""
        ),  # Comment out script lines when running in test as it is not using Databricks
    }
