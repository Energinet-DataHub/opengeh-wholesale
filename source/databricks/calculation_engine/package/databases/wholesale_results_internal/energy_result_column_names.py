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


from package.databases.wholesale_results_internal.result_column_names import (
    ResultColumnNames,
)


class EnergyResultColumnNames(ResultColumnNames):
    """Column names for the energy result storage model."""

    time_series_type = "time_series_type"
    neighbor_grid_area_code = "neighbor_grid_area_code"
    """The delta table column name ought to be renamed to 'from_grid_area'.
    It is, however, rather complicated compared to the value unless using Delta Lake column mapping,
    which comes with its own set of problems/limitations."""
    balance_responsible_id = "balance_responsible_id"
    """Obsolete"""
    aggregation_level = "aggregation_level"
    """Obsolete"""
    metering_point_type = "metering_point_type"
    metering_point_id = "metering_point_id"
    quantity = "quantity"
    quantity_qualities = "quantity_qualities"
    resolution = "resolution"