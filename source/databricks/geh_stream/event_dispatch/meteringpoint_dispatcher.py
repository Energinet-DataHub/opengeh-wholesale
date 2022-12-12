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
from geh_stream.bus import MessageDispatcher, messages as m
from pyspark.sql.session import SparkSession
from geh_stream.codelists.colname import Colname
from geh_stream.event_dispatch.dispatcher_base import period_mutations


def meteringpoint_master_data_path() -> str:
    return meteringpoint_dispatcher.master_data_root_path + "/metering-points"


def on_consumption_metering_point_created(msg: m.MeteringPointCreated):
    # Event --> Dataframe
    df = msg.get_dataframe()
    print(df.show())

    # Save Dataframe to that path
    df \
        .write \
        .format("delta") \
        .mode("append") \
        .partitionBy(Colname.metering_point_id) \
        .save(meteringpoint_master_data_path())


def on_settlement_method_updated(msg: m.SettlementMethodUpdated):

    spark = SparkSession.builder.getOrCreate()

    # Get all existing metering point periods
    mps_df = spark.read.format("delta").load(meteringpoint_master_data_path).where(f"{Colname.metering_point_id} = '{msg.metering_point_id}'")

    # Get the event data frame
    settlement_method_updated_df = msg.get_dataframe()

    result_df = period_mutations(mps_df, settlement_method_updated_df, [Colname.settlement_method])

    # persist updated mps
    result_df \
        .write \
        .format("delta") \
        .mode("overwrite") \
        .partitionBy(Colname.metering_point_id) \
        .option("replaceWhere", f"{Colname.metering_point_id} == '{msg.metering_point_id}'") \
        .save(meteringpoint_master_data_path())

    print("update smethod " + msg.settlement_method + " on id " + msg.metering_point_id)


def on_metering_point_connected(msg: m.MeteringPointConnected):

    spark = SparkSession.builder.getOrCreate()

    # Get all existing metering point periods
    mps_df = spark.read.format("delta").load(meteringpoint_master_data_path).where(f"{Colname.metering_point_id} = '{msg.metering_point_id}'")

    metering_point_connected_df = msg.get_dataframe()

    result_df = period_mutations(mps_df, metering_point_connected_df, [Colname.connection_state])

    # persist updated mps
    result_df \
        .write \
        .format("delta") \
        .mode("overwrite") \
        .partitionBy(Colname.metering_point_id) \
        .option("replaceWhere", f"{Colname.metering_point_id} == '{msg.metering_point_id}'") \
        .save(meteringpoint_master_data_path())


# -- Dispatcher --------------------------------------------------------------
meteringpoint_dispatcher = MessageDispatcher({
    m.MeteringPointCreated: on_consumption_metering_point_created,
    m.SettlementMethodUpdated: on_settlement_method_updated,
    m.MeteringPointConnected: on_metering_point_connected,
})
