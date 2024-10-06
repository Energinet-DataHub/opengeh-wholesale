from pyspark.sql import DataFrame, functions as F

from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.infrastructure.column_names import DataProductColumnNames


def filter_time_series_on_charge_owner(
    df: DataFrame,
    system_operator_id: str,
    charge_link_periods: DataFrame,
    charge_price_information_periods: DataFrame,
) -> DataFrame:

    charge_price_information_periods = charge_price_information_periods.where(
        F.col(DataProductColumnNames.is_tax) == False
    ).where(
        F.col(DataProductColumnNames.charge_owner_id) == system_operator_id,
    )

    filtered_charge_link_periods = charge_link_periods.join(
        charge_price_information_periods,
        on=[DataProductColumnNames.calculation_id, DataProductColumnNames.charge_key],
        how="inner",
    )
    filtered_charge_link_periods.show()

    filtered_df = (
        df.join(
            filtered_charge_link_periods,
            on=[
                DataProductColumnNames.calculation_id,
                DataProductColumnNames.metering_point_id,
            ],
            # how="leftsemi",
            how="inner",
        )
        # .where(
        #     (
        #         F.col(DataProductColumnNames.observation_time)
        #         >= DataProductColumnNames.from_date
        #     )
        #     | (
        #         F.col(DataProductColumnNames.observation_time)
        #         < DataProductColumnNames.from_date
        #     )
        #     & (
        #         F.col(DataProductColumnNames.observation_time)
        #         < DataProductColumnNames.to_date
        #     )
        # )
        # )
    )
    filtered_df.show()

    return filtered_df
