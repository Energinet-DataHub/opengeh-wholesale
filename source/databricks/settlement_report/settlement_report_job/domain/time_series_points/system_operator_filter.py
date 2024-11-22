from pyspark.sql import DataFrame, functions as F

from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)


def filter_time_series_points_on_charge_owner(
    time_series_points: DataFrame,
    system_operator_id: str,
    charge_link_periods: DataFrame,
    charge_price_information_periods: DataFrame,
) -> DataFrame:
    """
    Filters away all time series data that is not related to the system operator, and which is not a tax.
    """

    charge_price_information_periods = charge_price_information_periods.where(
        (~F.col(DataProductColumnNames.is_tax))
        & (F.col(DataProductColumnNames.charge_owner_id) == system_operator_id)
    )

    filtered_charge_link_periods = charge_link_periods.join(
        charge_price_information_periods,
        on=[DataProductColumnNames.calculation_id, DataProductColumnNames.charge_key],
        how="inner",
    ).select(
        charge_link_periods[DataProductColumnNames.calculation_id],
        charge_link_periods[DataProductColumnNames.metering_point_id],
        charge_link_periods[DataProductColumnNames.from_date],
        charge_link_periods[DataProductColumnNames.to_date],
    )

    filtered_df = time_series_points.join(
        filtered_charge_link_periods,
        on=[
            time_series_points[DataProductColumnNames.calculation_id]
            == filtered_charge_link_periods[DataProductColumnNames.calculation_id],
            time_series_points[DataProductColumnNames.metering_point_id]
            == filtered_charge_link_periods[DataProductColumnNames.metering_point_id],
            F.col(DataProductColumnNames.observation_time)
            >= F.col(DataProductColumnNames.from_date),
            F.col(DataProductColumnNames.observation_time)
            < F.col(DataProductColumnNames.to_date),
        ],
        how="leftsemi",
    )

    return filtered_df
