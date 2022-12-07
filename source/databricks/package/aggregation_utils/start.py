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

sys.path.append(r"/workspaces/geh-aggregations/source/databricks")
sys.path.append(r"/opt/conda/lib/python3.8/site-packages")

import configargparse
import json
from pyspark.sql.functions import lit

# * from geh_stream.shared.data_classes import Metadata
# from geh_stream.shared.data_exporter import export_to_csv
# * from geh_stream.aggregation_utils.trigger_base_arguments import trigger_base_arguments
# * from geh_stream.shared.data_loader import initialize_spark
from package.aggregation_utils.aggregators import (
    get_time_series_dataframe,
    aggregate_net_exchange_per_ga,
    aggregate_net_exchange_per_neighbour_ga,
    aggregate_hourly_consumption,
    aggregate_flex_consumption,
    aggregate_hourly_production,
    aggregate_hourly_production_ga_es,
    aggregate_hourly_settled_consumption_ga_es,
    aggregate_flex_settled_consumption_ga_es,
    aggregate_hourly_production_ga_brp,
    aggregate_hourly_settled_consumption_ga_brp,
    aggregate_flex_settled_consumption_ga_brp,
    aggregate_hourly_production_ga,
    aggregate_hourly_settled_consumption_ga,
    aggregate_flex_settled_consumption_ga,
    calculate_grid_loss,
    calculate_residual_ga,
    calculate_added_system_correction,
    calculate_added_grid_loss,
    calculate_total_consumption,
    adjust_flex_consumption,
    adjust_production,
    combine_added_system_correction_with_master_data,
    combine_added_grid_loss_with_master_data,
    aggregate_quality,
)

from package.aggregation_utils.inputoutputprocessor import InputOutputProcessor

# from geh_stream.codelists import BasisDataKeyName, ResultKeyName

# from geh_stream.aggregation_utils.trigger_base_arguments import trigger_base_arguments

# p = trigger_base_arguments()

# p.add(
#     "--resolution",
#     type=str,
#     required=True,
#     help="Time window resolution eg. 60 minutes, 15 minutes etc.",
# )
# p.add(
#     "--process-type",
#     type=str,
#     required=True,
#     help="D03 (Aggregation) or D04 (Balance fixing) ",
# )
# p.add(
#     "--meta-data-dictionary",
#     type=json.loads,
#     required=True,
#     help="Meta data dictionary",
# )
# args, unknown_args = p.parse_known_args()

# spark = initialize_spark(args)

# io_processor = InputOutputProcessor(args)

# Add raw dataframes to basis data dictionary and return joined dataframe
# filtered = get_time_series_dataframe(
#     io_processor.load_basis_data(spark, BasisDataKeyName.time_series),
#     io_processor.load_basis_data(spark, BasisDataKeyName.metering_points),
#     io_processor.load_basis_data(spark, BasisDataKeyName.market_roles),
#     io_processor.load_basis_data(spark, BasisDataKeyName.es_brp_relations),
# )
def (enrichs_timeseries):
    
results = {}
# Aggregate quality for aggregated timeseries grouped by grid area, market evaluation point type and time window
results[ResultKeyName.aggregation_base_dataframe] = aggregate_quality(filtered)

# Get additional data for grid loss and system correction
results[ResultKeyName.grid_loss_sys_cor_master_data] = io_processor.load_basis_data(
    spark, BasisDataKeyName.grid_loss_sys_corr
)

# Create a keyvalue dictionary for use in postprocessing. Each result are stored as a keyval with value being dataframe
metadata =  lit(metadata.JobId).alias(Colname.job_id),
            lit(metadata.SnapshotId).alias(Colname.snapshot_id),
            lit(metadata.ResultId).alias(Colname.result_id),
            lit(metadata.ResultName).alias(Colname.result_name),
            lit(metadata.ResultPath).alias(Colname.result_path),


aggregate_net_exchange_per_neighbour_ga(results, enrichs_timeseries)
aggregate_net_exchange_per_ga,
aggregate_hourly_consumption,
aggregate_flex_consumption,
aggregate_hourly_production,
calculate_grid_loss,
calculate_added_system_correction,
calculate_added_grid_loss,
combine_added_system_correction_with_master_data,  # TODO to be added to results later
combine_added_grid_loss_with_master_data,  # TODO to be added to results later
adjust_flex_consumption,
adjust_production,
aggregate_hourly_production_ga_es,
aggregate_hourly_settled_consumption_ga_es,
aggregate_flex_settled_consumption_ga_es,
aggregate_hourly_production_ga_brp,
aggregate_hourly_settled_consumption_ga_brp,
aggregate_flex_settled_consumption_ga_brp,
aggregate_hourly_production_ga,
aggregate_hourly_settled_consumption_ga,
aggregate_flex_settled_consumption_ga,
calculate_total_consumption,
calculate_residual_ga,



for key, value in args.meta_data_dictionary.items():
    key = int(key)
    results[key] = functions[key](results, Metadata(**value))


# Enable to dump results to local csv files
# export_to_csv(results)

del results[ResultKeyName.aggregation_base_dataframe]
del results[ResultKeyName.grid_loss_sys_cor_master_data]
del results[90]
del results[100]

# Store aggregation results
io_processor.do_post_processing(
    args.process_type, args.job_id, args.result_url, results
)
