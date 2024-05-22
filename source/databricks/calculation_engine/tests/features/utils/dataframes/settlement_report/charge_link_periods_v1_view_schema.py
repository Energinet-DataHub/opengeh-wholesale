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
    IntegerType,
)

from features.utils.dataframes.settlement_report.settlement_report_view_column_names import (
    ChargeLinkPeriodsV1ColumnNames,
)
from package.constants import MeteringPointPeriodColname

charge_link_periods_v1_view_schema = StructType(
    [
        StructField(ChargeLinkPeriodsV1ColumnNames.calculation_id, StringType(), False),
        StructField(
            ChargeLinkPeriodsV1ColumnNames.calculation_type, StringType(), False
        ),
        StructField(
            ChargeLinkPeriodsV1ColumnNames.metering_point_id, StringType(), False
        ),
        StructField(
            ChargeLinkPeriodsV1ColumnNames.metering_point_type, StringType(), False
        ),
        StructField(ChargeLinkPeriodsV1ColumnNames.charge_type, StringType(), False),
        StructField(ChargeLinkPeriodsV1ColumnNames.charge_code, StringType(), False),
        StructField(
            ChargeLinkPeriodsV1ColumnNames.charge_owner_id, StringType(), False
        ),
        StructField(ChargeLinkPeriodsV1ColumnNames.quantity, IntegerType(), False),
        StructField(ChargeLinkPeriodsV1ColumnNames.from_date, TimestampType(), False),
        StructField(ChargeLinkPeriodsV1ColumnNames.to_date, TimestampType(), True),
        StructField(MeteringPointPeriodColname.grid_area, StringType(), False),
        StructField(MeteringPointPeriodColname.energy_supplier_id, StringType(), True),
    ]
)
