from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.types import StructType

from settlement_report_job.infrastructure import database_definitions
from settlement_report_job.infrastructure.schemas.energy_v1 import (
    energy_v1,
)
from settlement_report_job.infrastructure.schemas.charge_link_periods_v1 import (
    charge_link_periods_v1,
)
from settlement_report_job.infrastructure.schemas.charge_price_information_periods_v1 import (
    charge_price_information_periods_v1,
)
from settlement_report_job.infrastructure.schemas.metering_point_time_series_v1 import (
    metering_point_time_series_v1,
)


def write_energy_to_delta_table(
    spark: SparkSession,
    df: DataFrame,
    table_location: str,
) -> None:
    write_dataframe_to_table(
        spark,
        df=df,
        database_name=database_definitions.WholesaleWholesaleResultsDatabase.DATABASE_NAME,
        table_name=database_definitions.WholesaleWholesaleResultsDatabase.ENERGY_V1_VIEW_NAME,
        table_location=f"{table_location}/{database_definitions.WholesaleWholesaleResultsDatabase.ENERGY_V1_VIEW_NAME}",
        schema=energy_v1,
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
        table_name=database_definitions.WholesaleBasisDataDatabase.CHARGE_LINKS_VIEW_NAME,
        table_location=f"{table_location}/{database_definitions.WholesaleBasisDataDatabase.CHARGE_LINKS_VIEW_NAME}",
        schema=charge_link_periods_v1,
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


def write_dataframe_to_table(
    spark: SparkSession,
    df: DataFrame,
    database_name: str,
    table_name: str,
    table_location: str,
    schema: StructType,
    mode: str = "overwrite",
) -> None:
    spark.sql(f"CREATE DATABASE IF NOT EXISTS {database_name}")

    sql_schema = _struct_type_to_sql_schema(schema)
    spark.sql(
        f"CREATE OR REPLACE TABLE {database_name}.{table_name} ({sql_schema}) USING DELTA LOCATION '{table_location}'"
    )
    df.write.format("delta").mode(mode).saveAsTable(f"{database_name}.{table_name}")


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
