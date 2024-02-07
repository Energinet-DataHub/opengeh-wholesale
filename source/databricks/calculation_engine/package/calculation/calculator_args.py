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

from azure.identity import ClientSecretCredential
from dataclasses import dataclass, field
from datetime import datetime
from source.databricks.calculation_engine.package.codelists.calculation_type import (
    CalculationType,
)


@dataclass
class CalculatorArgs:
    data_storage_account_name: str
    # Prevent the credentials from being printed or logged (using e.g. print() or repr())
    data_storage_account_credentials: ClientSecretCredential = field(repr=False)
    wholesale_container_path: str
    calculation_input_path: str
    time_series_points_table_name: str | None
    metering_point_periods_table_name: str | None
    grid_loss_metering_points_table_name: str | None
    calculation_id: str
    calculation_grid_areas: list[str]
    calculation_period_start_datetime: datetime
    calculation_period_end_datetime: datetime
    calculation_process_type: CalculationType
    calculation_execution_time_start: datetime
    time_zone: str
