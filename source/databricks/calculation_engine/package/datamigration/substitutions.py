from package.infrastructure import paths


def get_substitutions(catalog_name: str) -> dict[str, str]:
    """
    Get the list of substitutions for the migration scripts.
    This is the authoritative list of substitutions that can be used in all migration scripts.
    """

    return {
        "{CATALOG_NAME}": catalog_name,
        "{OUTPUT_DATABASE_NAME}": paths.WholesaleResultsInternalDatabase.DATABASE_NAME,
    }
