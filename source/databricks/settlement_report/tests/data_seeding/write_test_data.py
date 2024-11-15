from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.types import StructType

from settlement_report_job.infrastructure.wholesale import (
    database_definitions,
)
from settlement_report_job.infrastructure.wholesale.schemas import (
    charge_link_periods_v1,
    metering_point_periods_v1,
)
from settlement_report_job.infrastructure.wholesale.schemas import (
    charge_price_information_periods_v1,
)
from settlement_report_job.infrastructure.wholesale.schemas import (
    metering_point_time_series_v1,
)
from settlement_report_job.infrastructure.wholesale.schemas.amounts_per_charge_v1 import (
    amounts_per_charge_v1,
)
from settlement_report_job.infrastructure.wholesale.schemas.charge_price_points_v1 import (
    charge_price_points_v1,
)
from settlement_report_job.infrastructure.wholesale.schemas.energy_per_es_v1 import (
    energy_per_es_v1,
)
from settlement_report_job.infrastructure.wholesale.schemas.energy_v1 import (
    energy_v1,
)
from settlement_report_job.infrastructure.wholesale.schemas.latest_calculations_by_day_v1 import (
    latest_calculations_by_day_v1,
)
from settlement_report_job.infrastructure.wholesale.schemas import (
    monthly_amounts_per_charge_v1,
)
from settlement_report_job.infrastructure.wholesale.schemas.total_monthly_amounts_v1 import (
    total_monthly_amounts_v1,
)


def write_latest_calculations_by_day_to_delta_table(
    spark: SparkSession,
    df: DataFrame,
    table_location: str,
) -> None:
    write_dataframe_to_table(
        spark,
        df=df,
        database_name=database_definitions.WholesaleResultsDatabase.DATABASE_NAME,
        table_name=database_definitions.WholesaleResultsDatabase.LATEST_CALCULATIONS_BY_DAY_VIEW_NAME,
        table_location=f"{table_location}/{database_definitions.WholesaleResultsDatabase.LATEST_CALCULATIONS_BY_DAY_VIEW_NAME}",
        schema=latest_calculations_by_day_v1,
    )


def write_amounts_per_charge_to_delta_table(
    spark: SparkSession,
    df: DataFrame,
    table_location: str,
) -> None:
    write_dataframe_to_table(
        spark,
        df=df,
        database_name=database_definitions.WholesaleResultsDatabase.DATABASE_NAME,
        table_name=database_definitions.WholesaleResultsDatabase.AMOUNTS_PER_CHARGE_VIEW_NAME,
        table_location=f"{table_location}/{database_definitions.WholesaleResultsDatabase.AMOUNTS_PER_CHARGE_VIEW_NAME}",
        schema=amounts_per_charge_v1,
    )


def write_monthly_amounts_per_charge_to_delta_table(
    spark: SparkSession,
    df: DataFrame,
    table_location: str,
) -> None:
    write_dataframe_to_table(
        spark,
        df=df,
        database_name=database_definitions.WholesaleResultsDatabase.DATABASE_NAME,
        table_name=database_definitions.WholesaleResultsDatabase.MONTHLY_AMOUNTS_PER_CHARGE_VIEW_NAME,
        table_location=f"{table_location}/{database_definitions.WholesaleResultsDatabase.MONTHLY_AMOUNTS_PER_CHARGE_VIEW_NAME}",
        schema=monthly_amounts_per_charge_v1,
    )


def write_total_monthly_amounts_to_delta_table(
    spark: SparkSession,
    df: DataFrame,
    table_location: str,
) -> None:
    write_dataframe_to_table(
        spark,
        df=df,
        database_name=database_definitions.WholesaleResultsDatabase.DATABASE_NAME,
        table_name=database_definitions.WholesaleResultsDatabase.TOTAL_MONTHLY_AMOUNTS_VIEW_NAME,
        table_location=f"{table_location}/{database_definitions.WholesaleResultsDatabase.TOTAL_MONTHLY_AMOUNTS_VIEW_NAME}",
        schema=total_monthly_amounts_v1,
    )


def write_energy_to_delta_table(
    spark: SparkSession,
    df: DataFrame,
    table_location: str,
) -> None:
    write_dataframe_to_table(
        spark,
        df=df,
        database_name=database_definitions.WholesaleResultsDatabase.DATABASE_NAME,
        table_name=database_definitions.WholesaleResultsDatabase.ENERGY_V1_VIEW_NAME,
        table_location=f"{table_location}/{database_definitions.WholesaleResultsDatabase.ENERGY_V1_VIEW_NAME}",
        schema=energy_v1,
    )


def write_energy_per_es_to_delta_table(
    spark: SparkSession,
    df: DataFrame,
    table_location: str,
) -> None:
    write_dataframe_to_table(
        spark,
        df=df,
        database_name=database_definitions.WholesaleResultsDatabase.DATABASE_NAME,
        table_name=database_definitions.WholesaleResultsDatabase.ENERGY_PER_ES_V1_VIEW_NAME,
        table_location=f"{table_location}/{database_definitions.WholesaleResultsDatabase.ENERGY_PER_ES_V1_VIEW_NAME}",
        schema=energy_per_es_v1,
    )


def write_charge_price_information_periods_to_delta_table(
    spark: SparkSession,
    df: DataFrame,
    table_location: str,
) -> None:
    write_dataframe_to_table(
        spark,
        df=df,
        database_name=database_definitions.WholesaleBasisDataDatabase.DATABASE_NAME,
        table_name=database_definitions.WholesaleBasisDataDatabase.CHARGE_PRICE_INFORMATION_PERIODS_VIEW_NAME,
        table_location=f"{table_location}/{database_definitions.WholesaleBasisDataDatabase.CHARGE_PRICE_INFORMATION_PERIODS_VIEW_NAME}",
        schema=charge_price_information_periods_v1,
    )


def write_charge_link_periods_to_delta_table(
    spark: SparkSession,
    df: DataFrame,
    table_location: str,
) -> None:
    write_dataframe_to_table(
        spark,
        df=df,
        database_name=database_definitions.WholesaleBasisDataDatabase.DATABASE_NAME,
        table_name=database_definitions.WholesaleBasisDataDatabase.CHARGE_LINK_PERIODS_VIEW_NAME,
        table_location=f"{table_location}/{database_definitions.WholesaleBasisDataDatabase.CHARGE_LINK_PERIODS_VIEW_NAME}",
        schema=charge_link_periods_v1,
    )


def write_charge_price_points_to_delta_table(
    spark: SparkSession,
    df: DataFrame,
    table_location: str,
) -> None:
    write_dataframe_to_table(
        spark,
        df=df,
        database_name=database_definitions.WholesaleBasisDataDatabase.DATABASE_NAME,
        table_name=database_definitions.WholesaleBasisDataDatabase.CHARGE_PRICE_POINTS_VIEW_NAME,
        table_location=f"{table_location}/{database_definitions.WholesaleBasisDataDatabase.CHARGE_PRICE_POINTS_VIEW_NAME}",
        schema=charge_price_points_v1,
    )


def write_metering_point_time_series_to_delta_table(
    spark: SparkSession,
    df: DataFrame,
    table_location: str,
) -> None:
    write_dataframe_to_table(
        spark,
        df=df,
        database_name=database_definitions.WholesaleBasisDataDatabase.DATABASE_NAME,
        table_name=database_definitions.WholesaleBasisDataDatabase.TIME_SERIES_POINTS_VIEW_NAME,
        table_location=f"{table_location}/{database_definitions.WholesaleBasisDataDatabase.TIME_SERIES_POINTS_VIEW_NAME}",
        schema=metering_point_time_series_v1,
    )


def write_metering_point_periods_to_delta_table(
    spark: SparkSession,
    df: DataFrame,
    table_location: str,
) -> None:
    write_dataframe_to_table(
        spark,
        df=df,
        database_name=database_definitions.WholesaleBasisDataDatabase.DATABASE_NAME,
        table_name=database_definitions.WholesaleBasisDataDatabase.METERING_POINT_PERIODS_VIEW_NAME,
        table_location=f"{table_location}/{database_definitions.WholesaleBasisDataDatabase.METERING_POINT_PERIODS_VIEW_NAME}",
        schema=metering_point_periods_v1,
    )


def write_dataframe_to_table(
    spark: SparkSession,
    df: DataFrame,
    database_name: str,
    table_name: str,
    table_location: str,
    schema: StructType,
    mode: str = "append",  # Append because the tables are shared across tests
) -> None:
    spark.sql(f"CREATE DATABASE IF NOT EXISTS {database_name}")

    sql_schema = _struct_type_to_sql_schema(schema)

    # Creating table if not exists - note that the table is shared across tests, and should therefore not be deleted first.
    spark.sql(
        f"CREATE TABLE IF NOT EXISTS {database_name}.{table_name} ({sql_schema}) USING DELTA LOCATION '{table_location}'"
    )
    df.write.format("delta").option("overwriteSchema", "true").mode(mode).saveAsTable(
        f"{database_name}.{table_name}"
    )


def _struct_type_to_sql_schema(schema: StructType) -> str:
    schema_string = ""
    for field in schema.fields:
        field_name = field.name
        field_type = field.dataType.simpleString()

        if not field.nullable:
            field_type += " NOT NULL"

        schema_string += f"{field_name} {field_type}, "

    # Remove the trailing comma and space
    schema_string = schema_string.rstrip(", ")
    return schema_string
