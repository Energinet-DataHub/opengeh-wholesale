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
from features.utils.dataframes.settlement_report import (
    metering_point_period_v1_view_schema,
    metering_point_time_series_v1_view_schema,
)
from features.utils.dataframes.settlement_report.charge_link_periods_v1_view_schema import (
    charge_link_periods_v1_view_schema,
)
from features.utils.dataframes.settlement_report.charge_prices_v1_view_schema import (
    charge_prices_v1_view_schema,
)
from features.utils.dataframes.settlement_report.energy_results_v1_view_schema import (
    energy_results_v1_view_schema,
)
from features.utils.dataframes.settlement_report.settlement_report_view_dataframes import (
    create_metering_point_periods_v1_view,
    create_metering_point_time_series_v1_view,
    create_energy_results_v1_view,
    create_charge_link_periods_v1_view,
    create_charge_prices_v1_view,
    create_wholesale_results_v1_view,
)
from features.utils.dataframes.settlement_report.wholesale_results_v1_view_schema import (
    wholesale_results_v1_view_schema,
)
from features.utils.readers import (
    SettlementReportViewReader,
)


def get_output_specifications() -> dict[str, tuple]:
    """
    Contains the specifications for scenario outputs.
    """
    return {
        "metering_point_periods.csv": (
            metering_point_period_v1_view_schema,
            SettlementReportViewReader.read_metering_point_periods_v1,
            create_metering_point_periods_v1_view,
        ),
        "metering_point_time_series.csv": (
            metering_point_time_series_v1_view_schema,
            SettlementReportViewReader.read_metering_point_time_series_v1,
            create_metering_point_time_series_v1_view,
        ),
        "charge_link_periods_v1.csv": (
            charge_link_periods_v1_view_schema,
            SettlementReportViewReader.read_charge_link_periods_v1,
            create_charge_link_periods_v1_view,
        ),
        "charge_prices_v1.csv": (
            charge_prices_v1_view_schema,
            SettlementReportViewReader.read_charge_prices_v1,
            create_charge_prices_v1_view,
        ),
        "energy_results_v1.csv": (
            energy_results_v1_view_schema,
            SettlementReportViewReader.read_energy_results_v1,
            create_energy_results_v1_view,
        ),
        "wholesale_results_v1.csv": (
            wholesale_results_v1_view_schema,
            SettlementReportViewReader.read_wholesale_results_v1,
            create_wholesale_results_v1_view,
        ),
    }
