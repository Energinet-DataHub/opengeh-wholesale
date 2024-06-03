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
from features.utils.dataframes.edi_results.view_dataframes import (
    create_energy_result_points_per_ga_v1_view,
)
import features.utils.dataframes.edi_results as edi_results
import features.utils.dataframes.settlement_report as settlement_reports
import features.utils.dataframes.settlement_report.settlement_report_view_dataframes as settlement_report_dataframes
from features.utils.dataframes.edi_results.energy_result_points_per_ga_v1_view_schema import (
    energy_result_points_per_ga_v1_view_schema,
)


def get_output_specifications() -> dict[str, tuple]:
    """
    Contains the specifications for scenario outputs.
    """
    return {
        "wholesale_edi_results.energy_result_points_per_ga_v1.csv": (
            edi_results.energy_result_points_per_ga_v1_view_schema,
            create_energy_result_points_per_ga_v1_view,
        ),
        "settlement_reports.metering_point_periods_v1.csv": (
            settlement_reports.metering_point_period_v1_view_schema,
            settlement_report_dataframes.create_metering_point_periods_v1_view,
        ),
        "settlement_reports.metering_point_time_series_v1.csv": (
            settlement_reports.metering_point_time_series_v1_view_schema,
            settlement_report_dataframes.create_metering_point_time_series_v1_view,
        ),
        "settlement_reports.charge_link_periods_v1.csv": (
            settlement_reports.charge_link_periods_v1_view_schema,
            settlement_report_dataframes.create_charge_link_periods_v1_view,
        ),
        "settlement_reports.charge_prices_v1.csv": (
            settlement_reports.charge_prices_v1_view_schema,
            settlement_report_dataframes.create_charge_prices_v1_view,
        ),
        "settlement_report.energy_result_points_per_ga_v1.csv": (
            settlement_reports.energy_result_points_per_ga_v1_view_schema,
            settlement_report_dataframes.create_energy_result_points_per_ga_v1_view,
        ),
        "settlement_reports.wholesale_results_v1.csv": (
            settlement_reports.wholesale_results_v1_view_schema,
            settlement_report_dataframes.create_wholesale_results_v1_view,
        ),
        "settlement_reports.monthly_amounts_v1.csv": (
            settlement_reports.monthly_amounts_v1_view_schema,
            settlement_report_dataframes.create_monthly_amounts_v1_view,
        ),
        "settlement_reports.current_calculation_type_versions_v1.csv": (
            settlement_reports.current_calculation_type_versions_v1_view_schema,
            settlement_report_dataframes.create_current_calculation_type_versions_v1_view,
        ),
    }
