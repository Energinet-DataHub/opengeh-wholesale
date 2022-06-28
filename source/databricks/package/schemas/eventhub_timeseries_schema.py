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
from pyspark.sql.types import StringType, StructType, StructField, ArrayType, IntegerType, DecimalType

eventhub_timeseries_schema = StructType([
    StructField('Document', StructType([
        StructField('Id', StringType(), True),
        StructField('CreatedDateTime', StringType(), False),
        StructField('Sender', StructType([
            StructField('Id', StringType(), True),
            StructField('BusinessProcessRole', StringType(), True)
        ]), True)]), True),
    StructField('Series', ArrayType(StructType([
                StructField('Id', StringType(), True),
                StructField('TransactionId', StringType(), False),
                StructField('MeteringPointId', StringType(), False),
                StructField('MeteringPointType', StringType(), True),
                StructField('RegistrationDateTime', StringType(), True),
                StructField('Product', StringType(), True),
                StructField('Period', StructType([
                    StructField('Resolution', IntegerType(), False),
                    StructField('StartDateTime', StringType(), False),
                    StructField('EndDateTime', StringType(), True),
                    StructField('Points', ArrayType(StructType([
                        StructField('Quantity', DecimalType(38, 3), False),
                        StructField('Quality', IntegerType(), False),
                        StructField('Position', IntegerType(), False),
                    ])), False),
                ]), False)
                ]), False))
])
