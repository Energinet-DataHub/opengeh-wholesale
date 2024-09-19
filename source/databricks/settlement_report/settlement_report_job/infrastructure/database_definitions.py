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
from settlement_report_job.infrastructure.environment_variables import get_catalog_name


def get_energy_view_name() -> str:
    return f"{get_catalog_name()}.wholesale_results.energy_v1"  # noqa: E501


def get_metering_point_time_series_view_name() -> str:
    return f"{get_catalog_name()}.wholesale_settlement_reports.metering_point_time_series_v1"  # noqa: E501


def get_output_volume_name() -> str:
    return f"/Volumes/{get_catalog_name()}/wholesale_settlement_report_output/settlement_reports"  # noqa: E501
