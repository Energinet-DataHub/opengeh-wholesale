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

from pyspark.sql import SparkSession

# TODO BJM: Basic idea:
# - Run all migrations up to but not including the actual migration
# - Setup data
# - Run the actual migration
# - Assert the resulting data

actual_migration = "202311171100_Restore_overwritten_quantity_of_energy_results"


def test_restores_overwritten_quantity_in_old_data(spark: SparkSession):
    # Arrange
    migrations.execute_until_but_not_including(spark, actual_migration)
    df = spark.createDataFrame(data=[{Colname.quantity: None}])
    df.write.format("delta").save("table name")

    # Get timestamp
    # Write data again

    # Act: Use timestamp to restore between old and new data
    migrations.execute(actual_migration)

    # Assert
    actual = spark.read.table("")
    actual_rows = actual.collect()
    assert


def test_preserves_new_data(spark: SparkSession):
    pass


def test_preserves_old_data_except_quantity(spark: SparkSession):
    pass
