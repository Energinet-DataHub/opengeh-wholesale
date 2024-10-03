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


class WholesaleSettlementReportDatabase:
    DATABASE_NAME = "wholesale_settlement_reports"
    METERING_POINT_TIME_SERIES_VIEW_NAME = "metering_point_time_series_v1"


class WholesaleBasisDataDatabase:
    DATABASE_NAME = "wholesale_basis_data"
    CHARGE_LINKS_VIEW_NAME = "charge_link_periods_v1"


class WholesaleBasisDataDatabase:
    DATABASE_NAME = "wholesale_basis_data"
    CHARGE_LINKS_VIEW_NAME = "charge_price_information_periods_v1"


class WholesaleWholesaleResultsDatabase:
    DATABASE_NAME = "wholesale_results"
    ENERGY_V1_VIEW_NAME = "energy_v1"


def get_settlement_reports_output_path(catalog_name: str) -> str:
    return f"/Volumes/{catalog_name}/wholesale_settlement_report_output/settlement_reports"  # noqa: E501
