from package.infrastructure import paths


def get_substitutions(catalog_name: str) -> dict[str, str]:
    """
    Get the list of substitutions for the migration scripts.
    This is the authoritative list of substitutions that can be used in all migration scripts.
    """

    return {
        "{CATALOG_NAME}": catalog_name,
        "{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}": paths.WholesaleResultsInternalDatabase.DATABASE_NAME,
        "{WHOLESALE_BASIS_DATA_DATABASE_NAME}": paths.WholesaleBasisDataInternalDatabase.DATABASE_NAME,
        "{WHOLESALE_INTERNAL_DATABASE_NAME}": paths.WholesaleInternalDatabase.DATABASE_NAME,
        "{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}": paths.WholesaleSettlementReportsDatabase.DATABASE_NAME,
        # Remove this substitution when the old hive migration is removed
        "{INPUT_DATABASE_NAME}": paths.InputDatabase.DATABASE_NAME,
    }
