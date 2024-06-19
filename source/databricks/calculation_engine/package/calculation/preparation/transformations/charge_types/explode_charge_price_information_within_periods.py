# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

from pyspark.sql import DataFrame
import pyspark.sql.functions as f

from package.calculation.preparation.data_structures import ChargePriceInformation
from package.codelists import ChargeResolution
from package.constants import Colname


def explode_charge_price_information_within_periods(
    charge_price_information: ChargePriceInformation,
    resolution: ChargeResolution,
    time_zone: str,
) -> DataFrame:
    """
    This method takes explodes each row into a set of rows given by from_date, to_date and charge_resolution.
    """

    charge_price_information_df = charge_price_information.df.filter(
        f.col(Colname.resolution) == resolution.value
    )
    explode_with_charge_time = {
        ChargeResolution.DAY: lambda df: _explode_with_daily_charge_time(df, time_zone),
        ChargeResolution.HOUR: lambda df: _explode_with_hourly_charge_time(df),
    }
    charge_price_information_with_charge_time = explode_with_charge_time[resolution](
        charge_price_information_df
    )

    # If the charge stops and starts on the same day, then the charge will be included twice, so we need to remove duplicates
    charge_price_information_with_charge_time = (
        charge_price_information_with_charge_time.dropDuplicates(
            [Colname.charge_key, Colname.charge_time]
        )
    )

    return charge_price_information_with_charge_time.select(
        Colname.charge_key,
        Colname.charge_code,
        Colname.charge_type,
        Colname.charge_owner,
        Colname.charge_tax,
        Colname.resolution,
        Colname.charge_time,
    )


def _explode_with_daily_charge_time(
    charge_price_information_df: DataFrame,
    time_zone: str,
) -> DataFrame:
    # When resolution is DAY we need to deal with local time to get the correct start time of each day
    return charge_price_information_df.withColumn(
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
    )
