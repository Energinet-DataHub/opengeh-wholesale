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
from geh_stream.codelists import Colname
from pyspark.sql.types import StructType, StructField, StringType

integration_event_schema = StructType([StructField(Colname.event_id, StringType()),
                                       StructField(Colname.processed_date, StringType()),
                                       StructField(Colname.event_name, StringType()),
                                       StructField(Colname.domain, StringType()),
                                       StructField("body", StringType())])
