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


from .result_column_names import ResultColumnNames


class EnergyResultColumnNames(ResultColumnNames):
    time_series_type = "time_series_type"
    from_grid_area = "out_grid_area"
    """The delta table column name ought to be renamed to 'from_grid_area'.
    It is, however, rather complicated compared to the value unless using Delta Lake column mapping,
    which comes with its own set of problems/limitations."""
    balance_responsible_id = "balance_responsible_id"
    aggregation_level = "aggregation_level"
