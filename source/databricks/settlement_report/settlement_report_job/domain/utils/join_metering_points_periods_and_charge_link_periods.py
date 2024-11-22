from pyspark.sql import functions as F, DataFrame

from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)


def join_metering_points_periods_and_charge_link_periods(
    charge_link_periods: DataFrame,
    metering_point_periods: DataFrame,
) -> DataFrame:
    """
    Joins metering point periods and charge link periods and returns the joined DataFrame.
    Periods are joined on calculation_id and metering_point_id.
    The output DataFrame will contain the columns from_date and to_date, which are the intersection of the periods.
    """
    link_from_date = "link_from_date"
    link_to_date = "link_to_date"
    metering_point_from_date = "metering_point_from_date"
    metering_point_to_date = "metering_point_to_date"

    charge_link_periods = charge_link_periods.withColumnRenamed(
        DataProductColumnNames.from_date, link_from_date
    ).withColumnRenamed(DataProductColumnNames.to_date, link_to_date)
    metering_point_periods = metering_point_periods.withColumnRenamed(
        DataProductColumnNames.from_date, metering_point_from_date
    ).withColumnRenamed(DataProductColumnNames.to_date, metering_point_to_date)

    joined = (
        metering_point_periods.join(
            charge_link_periods,
            on=[
                DataProductColumnNames.calculation_id,
                DataProductColumnNames.metering_point_id,
            ],
            how="inner",
        )
        .where(
            (F.col(link_from_date) < F.col(metering_point_to_date))
            & (F.col(link_to_date) > F.col(metering_point_from_date))
        )
        .withColumn(
            DataProductColumnNames.from_date,
            F.greatest(F.col(link_from_date), F.col(metering_point_from_date)),
        )
        .withColumn(
            DataProductColumnNames.to_date,
            F.least(F.col(link_to_date), F.col(metering_point_to_date)),
        )
    )

    return joined
