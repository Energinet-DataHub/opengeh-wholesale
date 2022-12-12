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

# Uncomment the lines below to include modules distributed by wheel
import sys
sys.path.append(r'/workspaces/geh-aggregations/source/databricks')
sys.path.append(r'/opt/conda/lib/python3.8/site-packages')

import json
from pyspark.sql import SparkSession

from geh_stream.shared.services import InputOutputProcessor
from geh_stream.shared.data_loader import \
    load_metering_points, \
    load_time_series_points, \
    load_market_roles, \
    load_es_brp_relations, \
    load_charges, \
    load_charge_links, \
    load_charge_prices, \
    load_grid_loss_sys_corr, \
    initialize_spark

from geh_stream.codelists import BasisDataKeyName
from geh_stream.aggregation_utils.trigger_base_arguments import trigger_base_arguments


def create_snapshot(spark: SparkSession, areas, args: dict):
    # Create a keyvalue dictionary for use in store basis data. Each snapshot data are stored as a keyval with value being dataframe
    snapshot_data = {}

    # Fetch metering point df
    metering_points = load_metering_points(args.beginning_date_time, args.end_date_time, args, spark, areas)
    snapshot_data[BasisDataKeyName.metering_points] = metering_points

    # Fetch time series dataframe
    snapshot_data[BasisDataKeyName.time_series] = load_time_series_points(args, spark, metering_points)

    # Fetch market roles df
    snapshot_data[BasisDataKeyName.market_roles] = load_market_roles(args, spark)

    # Fetch energy supplier, balance responsible relations df
    snapshot_data[BasisDataKeyName.es_brp_relations] = load_es_brp_relations(args, spark, areas)

    # Fetch charges for wholesale
    snapshot_data[BasisDataKeyName.charges] = load_charges(args, spark)

    # Fetch charge links for wholesale
    snapshot_data[BasisDataKeyName.charge_links] = load_charge_links(args, spark)

    # Fetch charge prices for wholesale
    snapshot_data[BasisDataKeyName.charge_prices] = load_charge_prices(args, spark)

    # Fetch system correction metering points
    snapshot_data[BasisDataKeyName.grid_loss_sys_corr] = load_grid_loss_sys_corr(args, spark, areas)

    # Store basis data
    io_processor = InputOutputProcessor(args)
    io_processor.store_basis_data(args.snapshot_notify_url, snapshot_data)
