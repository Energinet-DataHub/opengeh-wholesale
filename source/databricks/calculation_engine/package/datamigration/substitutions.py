from .migration_script_args import MigrationScriptArgs
from package.infrastructure.paths import OUTPUT_DATABASE_NAME, TEST
from package.infrastructure.paths import (
    OUTPUT_FOLDER,
    INPUT_DATABASE_NAME,
)


def substitutions(migration_args: MigrationScriptArgs) -> dict[str, str]:
    return {
        "{CONTAINER_PATH}": migration_args.storage_container_path,
        "{OUTPUT_DATABASE_NAME}": OUTPUT_DATABASE_NAME,
        "{INPUT_DATABASE_NAME}": INPUT_DATABASE_NAME,
        "{OUTPUT_FOLDER}": OUTPUT_FOLDER,
        "{INPUT_FOLDER}": migration_args.calculation_input_folder,
        "{TEST}": TEST,
    }
