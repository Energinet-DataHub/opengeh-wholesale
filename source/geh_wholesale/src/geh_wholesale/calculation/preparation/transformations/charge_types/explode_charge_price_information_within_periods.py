import pyspark.sql.functions as f
from pyspark.sql import DataFrame

from geh_wholesale.calculation.preparation.data_structures import ChargePriceInformation
from geh_wholesale.codelists import ChargeResolution
from geh_wholesale.constants import Colname


def explode_charge_price_information_within_periods(
    charge_price_information: ChargePriceInformation,
    resolution: ChargeResolution,
    time_zone: str,
) -> DataFrame:
    """Explode charge_price_information_periods between from_date and to_date with a resolution set according to 'charge_resolution'."""
    if resolution != ChargeResolution.HOUR and resolution != ChargeResolution.DAY:
        raise ValueError(f"Unsupported resolution {resolution}")

    explode_with_charge_time = {
        ChargeResolution.DAY: lambda df: _explode_with_daily_charge_time(df, time_zone),
        ChargeResolution.HOUR: lambda df: _explode_with_hourly_charge_time(df),
    }
    return explode_with_charge_time[resolution](charge_price_information.df)


def _explode_with_daily_charge_time(
    charge_price_information_df: DataFrame,
    time_zone: str,
) -> DataFrame:
    # When resolution is DAY we need to deal with local time to get the correct start time of each day
    return (
        charge_price_information_df.withColumn(
            Colname.charge_time,
            f.explode(
                # Create a sequence of the start of each day in the period. The times are local time
                f.sequence(
                    f.from_utc_timestamp(Colname.from_date, time_zone),
                    f.from_utc_timestamp(Colname.to_date, time_zone),
                    f.expr("interval 1 day"),
                )
            ),
            # Convert local day start times back to UTC
        ).withColumn(
            Colname.charge_time,
            f.to_utc_timestamp(Colname.charge_time, time_zone),
        )
    ).where(
        # drop rows where charge_time is greater than or equal to to_date. This can happen when a charge stops and starts on the same day
        f.col(Colname.charge_time) < f.col(Colname.to_date)
    )


def _explode_with_hourly_charge_time(
    charge_price_information_df: DataFrame,
) -> DataFrame:
    return charge_price_information_df.withColumn(
        Colname.charge_time,
        f.explode(
            f.sequence(
                Colname.from_date,
                Colname.to_date,
                f.expr("interval 1 hour"),
            )
        ),
    ).where(
        # drop rows where charge_time is greater than or equal to to_date. This can happen when a charge stops and starts on the same day
        f.col(Colname.charge_time) < f.col(Colname.to_date)
    )
