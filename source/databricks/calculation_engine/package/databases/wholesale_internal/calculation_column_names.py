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


class CalculationColumnNames(ResultColumnNames):
    """Column names for the calculation storage model."""

    execution_time_start = "execution_time_start"
    calculation_type = "calculation_type"
    period_start = "period_start"
    period_end = "period_end"
    created_by_user_id = "created_by_user_id"
    version = "version"
    is_internal_calculation = "is_internal_calculation"
    """True if the calculation is an internal calculation, False otherwise."""
    calculation_completed_time = "calculation_completed_time"
    """The time when the calculation was completed. """
