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
from package.constants import Colname


def join_charge_master_data_and_charge_price(
    charge_master_data: DataFrame, charge_prices: DataFrame
) -> DataFrame:
    charge_master_data = (
        charge_master_data.join(
            charge_prices,
            [
                charge_prices[Colname.charge_key]
                == charge_master_data[Colname.charge_key],
                charge_prices[Colname.charge_time]
                >= charge_master_data[Colname.from_date],
                charge_prices[Colname.charge_time]
                < charge_master_data[Colname.to_date],
            ],
            "inner",
        )
        .distinct()
        .select(
            charge_master_data[Colname.charge_key],
            charge_master_data[Colname.charge_code],
            charge_master_data[Colname.charge_type],
            charge_master_data[Colname.charge_owner],
            charge_master_data[Colname.charge_tax],
            charge_master_data[Colname.resolution],
            charge_master_data[Colname.from_date],
            charge_master_data[Colname.to_date],
            charge_prices[Colname.charge_time],
            charge_prices[Colname.charge_price],
        )
    )
    return charge_master_data
