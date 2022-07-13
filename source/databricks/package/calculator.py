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

from pyspark.sql.functions import lit
from pyspark.sql.types import StructType, StructField, IntegerType
from package import initialize_spark
import click


@click.command()
@click.option("--data-storage-account-name", type=str, required=True)
@click.option("--data-storage-account-key", type=str, required=True)
@click.option("--integration-events-path", type=str, required=True)
@click.option("--time-series-points-path", type=str, required=True)
@click.option("--process-results-path", type=str, required=True)
@click.option("--batch-id", type=str, required=True)
def start(
    data_storage_account_name,
    data_storage_account_key,
    integration_events_path,
    time_series_points_path,
    process_results_path,
    batch_id,
):
    spark = initialize_spark(data_storage_account_name, data_storage_account_key)
    rdd = spark.sparkContext.parallelize(list(range(1, 97)))
    df_seq = spark.createDataFrame(rdd, schema=IntegerType()).withColumnRenamed(
        "value", "position"
    )
    df_805 = df_seq.withColumn("grid_area", lit("805"))
    df_806 = df_seq.withColumn("grid_area", lit("806"))
    df = df_805.union(df_806)
    df = df.withColumn("quantity", lit(None)).withColumn("quality", lit(None))
    df.coalesce(1).write.partitionBy("grid_area").json(
        f"{process_results_path}/batch_id={batch_id}"
    )

def start_job():
    try:
        start()

    # TODO: Try to remove when using wheel directly. PR #157
    # Clicks uses sys.exit(0) when job completes, which is intepreted as an error.
    # https://github.com/ipython/ipython/issues/12831#issuecomment-1064866151
    except SystemExit as e:
        if e.code != 0:
            raise
