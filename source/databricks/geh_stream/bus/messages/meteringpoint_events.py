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

from datetime import datetime
from dataclasses import dataclass
from geh_stream.bus.broker import Message
from geh_stream.codelists.colname import Colname
from geh_stream.schemas import metering_point_schema
from pyspark.sql.session import SparkSession
from pyspark.sql.types import StructType, StringType, StructField, TimestampType
from dataclasses_json import dataclass_json  # https://pypi.org/project/dataclasses-json/
import dateutil.parser


@dataclass_json
@dataclass
class MeteringPointCreated(Message):
    # Event properties:

    metering_point_id: StringType()
    metering_point_type: StringType()
    grid_area: StringType()
    settlement_method: StringType()
    metering_method: StringType()
    resolution: StringType()
    product: StringType()
    connection_state: StringType()
    unit: StringType()
    effective_date: StringType()

    # What to do when we want the dataframe for this event
    def get_dataframe(self):
        effective_date = dateutil.parser.parse(self.effective_date)

        create_consumption_mp_event = [(
            self.metering_point_id,
            self.metering_point_type,
            self.settlement_method,
            self.grid_area,
            self.connection_state,
            self.resolution,
            None,  # in_grid_area
            None,  # out_grid_area
            self.metering_method,
            None,  # parent_id
            self.unit,
            self.product,
            effective_date,
            datetime(9999, 1, 1, 0, 0))]

        return SparkSession.builder.getOrCreate().createDataFrame(create_consumption_mp_event, schema=metering_point_schema)


@dataclass_json
@dataclass
class SettlementMethodUpdated(Message):
    settlement_method_updated_schema = StructType([
        StructField(Colname.metering_point_id, StringType(), False),
        StructField(Colname.settlement_method, StringType(), False),
        StructField(Colname.effective_date, TimestampType(), False),
    ])

    metering_point_id: StringType()
    settlement_method: StringType()
    effective_date: TimestampType()

    def get_dataframe(self):
        effective_date = dateutil.parser.parse(self.effective_date)

        settlement_method_updated_event = [(
            self.metering_point_id,
            self.settlement_method,
            effective_date)]
        return SparkSession.builder.getOrCreate().createDataFrame(settlement_method_updated_event, schema=self.settlement_method_updated_schema)


@dataclass_json
@dataclass
class MeteringPointConnected(Message):
    metering_point_connected_schema = StructType([
        StructField(Colname.metering_point_id, StringType(), False),
        StructField(Colname.connection_state, StringType(), False),
        StructField(Colname.effective_date, TimestampType(), False)
    ])

    metering_point_id: StringType()
    connection_state: StringType()
    effective_date: TimestampType()

    def get_dataframe(self):
        effective_date = dateutil.parser.parse(self.effective_date)

        metering_point_connected_event = [(
            self.metering_point_id,
            self.connection_state,
            effective_date)]
        return SparkSession.builder.getOrCreate().createDataFrame(metering_point_connected_event, schema=self.metering_point_connected_schema)
