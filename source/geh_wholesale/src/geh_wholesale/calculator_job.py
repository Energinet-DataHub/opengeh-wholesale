import geh_common.telemetry.logging_configuration as config
from featuremanagement import FeatureManager
from geh_common.telemetry.decorators import start_trace
from geh_common.telemetry.logger import Logger
from pyspark.sql import SparkSession

from geh_wholesale import calculation
from geh_wholesale.calculation import CalculationCore
from geh_wholesale.calculation.calculation_metadata_service import CalculationMetadataService
from geh_wholesale.calculation.calculation_output_service import CalculationOutputService
from geh_wholesale.calculation.calculator_args import CalculatorArgs
from geh_wholesale.container import create_and_configure_container
from geh_wholesale.databases import migrations_wholesale, wholesale_internal
from geh_wholesale.databases.feature_flag_manager import FeatureManagerFactory
from geh_wholesale.infrastructure import initialize_spark
from geh_wholesale.infrastructure.environment_variables import get_storage_account_credential
from geh_wholesale.infrastructure.infrastructure_settings import InfrastructureSettings


# The start() method should only have its name updated in correspondence with the
# wheels entry point for it. Further the method must remain parameterless because
# it will be called from the entry point when deployed.
def start() -> None:
    # Parse params for LoggingSettings

    # Parse params for CalculatorArgs and InfrastructureSettings
    args = CalculatorArgs()
    infrastructure_settings = InfrastructureSettings()

    config.configure_logging(
        cloud_role_name="dbr-calculation-engine",
        subsystem="wholesale-aggregations",  #  Will be used as trace_name
        extras=dict(calculation_id=args.calculation_id),
    )

    start_with_deps(args=args, infrastructure_settings=infrastructure_settings)


@start_trace()
def start_with_deps(
    *,
    args: CalculatorArgs,
    infrastructure_settings: InfrastructureSettings,
) -> None:
    """Start overload with explicit dependencies for easier testing."""
    logger = Logger(__name__)
    logger.info(f"Calculator arguments: {args}")
    logger.info(f"Infrastructure settings: {infrastructure_settings}")

    spark = initialize_spark()
    create_and_configure_container(spark, infrastructure_settings)

    feature_manager = FeatureManagerFactory(
        get_storage_account_credential(),
        infrastructure_settings.azure_app_configuration__endpoint,
    ).build()

    prepared_data_reader = create_prepared_data_reader(infrastructure_settings, spark, feature_manager)

    if not prepared_data_reader.is_calculation_id_unique(args.calculation_id):
        raise Exception(f"Calculation ID '{args.calculation_id}' is already used.")

    calculation.execute(
        args,
        prepared_data_reader,
        CalculationCore(),
        CalculationMetadataService(),
        CalculationOutputService(),
    )


def create_prepared_data_reader(
    settings: InfrastructureSettings,
    spark: SparkSession,
    feature_manager: FeatureManager,
) -> calculation.PreparedDataReader:
    """Create calculation execution dependencies."""
    migrations_wholesale_repository = migrations_wholesale.MigrationsWholesaleRepository(
        spark,
        feature_manager,
        settings.catalog_name,
        settings.calculation_input_database_name,
        settings.measurements_gold_database_name,
        settings.time_series_points_table_name,
        settings.metering_point_periods_table_name,
        settings.grid_loss_metering_point_ids_table_name,
        settings.measurements_gold_current_v1_view_name,
    )

    wholesale_internal_repository = wholesale_internal.WholesaleInternalRepository(
        spark,
        settings.catalog_name,
    )

    prepared_data_reader = calculation.PreparedDataReader(
        migrations_wholesale_repository, wholesale_internal_repository
    )
    return prepared_data_reader
