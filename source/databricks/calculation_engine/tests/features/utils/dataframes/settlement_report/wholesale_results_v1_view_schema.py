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
    LongType,
)

from features.utils.dataframes.settlement_report.settlement_report_view_column_names import (
    WholesaleResultsV1ColumnNames,
)
from package.constants import WholesaleResultColumnNames, Colname

wholesale_results_v1_view_schema = StructType(
    [
        StructField(WholesaleResultsV1ColumnNames.calculation_id, StringType(), False),
        StructField(
            WholesaleResultsV1ColumnNames.calculation_type, StringType(), False
        ),
        StructField(
            WholesaleResultsV1ColumnNames.calculation_version, LongType(), False
        ),
        StructField(WholesaleResultsV1ColumnNames.result_id, StringType(), False),
        StructField(WholesaleResultsV1ColumnNames.grid_area_code, StringType(), False),
        StructField(
            WholesaleResultsV1ColumnNames.energy_supplier_id, StringType(), False
        ),
        StructField(WholesaleResultsV1ColumnNames.time, TimestampType(), False),
        StructField(WholesaleResultsV1ColumnNames.resolution, StringType(), False),
        StructField(
            WholesaleResultsV1ColumnNames.metering_point_type, StringType(), False
        ),
        StructField(
            WholesaleResultsV1ColumnNames.settlement_method, StringType(), True
        ),
        StructField(WholesaleResultsV1ColumnNames.quantity_unit, StringType(), False),
        StructField(WholesaleResultsV1ColumnNames.currency, StringType(), False),
        StructField(WholesaleResultsV1ColumnNames.quantity, DecimalType(18, 3), False),
        StructField(WholesaleResultsV1ColumnNames.price, DecimalType(18, 6), False),
        StructField(WholesaleResultsV1ColumnNames.amount, DecimalType(18, 6), True),
        StructField(WholesaleResultsV1ColumnNames.charge_type, StringType(), True),
        StructField(WholesaleResultsV1ColumnNames.charge_code, StringType(), True),
        StructField(WholesaleResultsV1ColumnNames.charge_owner_id, StringType(), True),
    ]
)
