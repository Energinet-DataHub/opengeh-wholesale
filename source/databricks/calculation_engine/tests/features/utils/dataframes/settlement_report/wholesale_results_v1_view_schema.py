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

from pyspark.sql.types import (
    StructField,
    StringType,
    TimestampType,
    StructType,
    DecimalType,
)

from package.constants import WholesaleResultColumnNames, Colname

wholesale_results_v1_view_schema = StructType(
    [
        StructField(WholesaleResultColumnNames.calculation_id, StringType(), False),
        StructField(WholesaleResultColumnNames.calculation_type, StringType(), False),
        StructField(WholesaleResultColumnNames.grid_area_code, StringType(), False),
        StructField(WholesaleResultColumnNames.energy_supplier_id, StringType(), False),
        StructField(Colname.start_date_time, TimestampType(), False),
        StructField(WholesaleResultColumnNames.resolution, StringType(), False),
        StructField(
            WholesaleResultColumnNames.metering_point_type, StringType(), False
        ),
        StructField(WholesaleResultColumnNames.settlement_method, StringType(), True),
        StructField(WholesaleResultColumnNames.quantity_unit, StringType(), False),
        StructField(Colname.currency, StringType(), False),
        StructField(WholesaleResultColumnNames.quantity, DecimalType(18, 3), False),
        StructField(WholesaleResultColumnNames.price, DecimalType(18, 6), False),
        StructField(WholesaleResultColumnNames.amount, DecimalType(18, 6), True),
        StructField(WholesaleResultColumnNames.charge_type, StringType(), True),
        StructField(WholesaleResultColumnNames.charge_code, StringType(), True),
        StructField(WholesaleResultColumnNames.charge_owner_id, StringType(), True),
    ]
)
