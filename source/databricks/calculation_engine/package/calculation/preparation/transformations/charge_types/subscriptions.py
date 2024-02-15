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

from pyspark.sql.dataframe import DataFrame
import pyspark.sql.functions as f

from package.codelists import ChargeType
from package.constants import Colname


def get_subscription_charges(
    charges: DataFrame,
    charge_link_metering_points: DataFrame,
) -> DataFrame:
    subscription_charges = charges.filter(
        f.col(Colname.charge_type) == ChargeType.SUBSCRIPTION.value
    )

    subscriptions = _explode_subscription(subscription_charges)

    subscriptions = subscription_charges.join(
        charge_link_metering_points,
        (
            subscription_charges[Colname.charge_key]
            == charge_link_metering_points[Colname.charge_key]
        )
        & (
            subscription_charges[Colname.charge_time]
            >= charge_link_metering_points[Colname.from_date]
        )
        & (
            subscription_charges[Colname.charge_time]
            < charge_link_metering_points[Colname.to_date]
        ),
        how="inner",
    ).select(
        subscription_charges[Colname.charge_key],
        Colname.charge_code,
        Colname.charge_type,
        Colname.charge_owner,
        Colname.charge_time,
        Colname.charge_price,
        charge_link_metering_points[Colname.metering_point_type],
        charge_link_metering_points[Colname.settlement_method],
        charge_link_metering_points[Colname.grid_area],
        charge_link_metering_points[Colname.energy_supplier_id],
    )

    return subscriptions


def _explode_subscription(charges_df: DataFrame) -> DataFrame:
    charges_df = (
        charges_df.withColumn(
            Colname.date,
            f.explode(
                f.expr(
                    f"sequence({Colname.from_date}, {Colname.to_date}, interval 1 day)"
                )
            ),
        )
        .filter((f.year(Colname.date) == f.year(Colname.charge_time)))
        .filter((f.month(Colname.date) == f.month(Colname.charge_time)))
        .drop(Colname.charge_time)
        .withColumnRenamed(Colname.date, Colname.charge_time)
        .select(
            Colname.charge_key,
            Colname.charge_code,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.charge_tax,
            Colname.resolution,
            Colname.charge_time,
            Colname.charge_price,
        )
    )
    return charges_df
