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

from features.utils.dataframes.settlement_report.settlement_report_view_column_names import (
    EnergyResultsV1ColumnNames,
)

energy_results_v1_schema = StructType(
    [
        StructField(EnergyResultsV1ColumnNames.calculation_id, StringType(), False),
        StructField(EnergyResultsV1ColumnNames.calculation_type, StringType(), False),
        StructField(EnergyResultsV1ColumnNames.grid_area, StringType(), False),
        StructField(
            EnergyResultsV1ColumnNames.metering_point_type, StringType(), False
        ),
        StructField(EnergyResultsV1ColumnNames.settlement_method, StringType(), True),
        StructField(EnergyResultsV1ColumnNames.resolution, StringType(), False),
        StructField(EnergyResultsV1ColumnNames.time, TimestampType(), False),
        StructField(EnergyResultsV1ColumnNames.quantity, DecimalType(18, 3), False),
        StructField(EnergyResultsV1ColumnNames.energy_supplier_id, StringType(), True),
        StructField(EnergyResultsV1ColumnNames.aggregation_level, StringType(), False),
    ]
)
