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
import os
import shutil
from datetime import timedelta, datetime
from zoneinfo import ZoneInfo

from settlement_report_job.domain.utils.market_role import MarketRole
from settlement_report_job.domain.utils.report_data_type import ReportDataType
from settlement_report_job.infrastructure.report_name_factory import (
    MarketRoleInFileName,
)
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from settlement_report_job.infrastructure import paths


class Dates:
    JAN_1ST = datetime(2023, 12, 31, 23)
    JAN_2ND = datetime(2024, 1, 1, 23)
    JAN_3RD = datetime(2024, 1, 2, 23)
    JAN_4TH = datetime(2024, 1, 3, 23)
    JAN_5TH = datetime(2024, 1, 4, 23)
    JAN_6TH = datetime(2024, 1, 5, 23)
    JAN_7TH = datetime(2024, 1, 6, 23)
    JAN_8TH = datetime(2024, 1, 7, 23)
    JAN_9TH = datetime(2024, 1, 8, 23)


DEFAULT_TIME_ZONE = "Europe/Copenhagen"


def cleanup_output_path(settlement_reports_output_path: str) -> None:
    if os.path.exists(settlement_reports_output_path):
        shutil.rmtree(settlement_reports_output_path)
        os.makedirs(settlement_reports_output_path)


def get_actual_files(
    report_data_type: ReportDataType, args: SettlementReportArgs
) -> list[str]:
    path = paths.get_report_output_path(args)
    if not os.path.isdir(path):
        return []

    return [
        f
        for f in os.listdir(path)
        if os.path.isfile(os.path.join(path, f))
        and f.startswith(_get_file_prefix(report_data_type))
        and f.endswith(".csv")
    ]


def _get_file_prefix(report_data_type) -> str:
    if report_data_type == ReportDataType.TimeSeriesHourly:
        return "TSSD60"
    elif report_data_type == ReportDataType.TimeSeriesQuarterly:
        return "TSSD15"
    elif report_data_type == ReportDataType.MeteringPointPeriods:
        return "MDMP"
    elif report_data_type == ReportDataType.ChargeLinks:
        return "CHARGELINK"
    elif report_data_type == ReportDataType.ChargePricePoints:
        return "CHARGEPRICE"
    elif report_data_type == ReportDataType.EnergyResults:
        return "RESULTENERGY"
    elif report_data_type == ReportDataType.WholesaleResults:
        return "RESULTWHOLESALE"
    elif report_data_type == ReportDataType.MonthlyAmounts:
        return "RESULTMONTHLY"
    raise NotImplementedError(f"Report data type {report_data_type} is not supported.")


def get_start_date(period_start: datetime) -> str:
    time_zone_info = ZoneInfo(DEFAULT_TIME_ZONE)
    return period_start.astimezone(time_zone_info).strftime("%d-%m-%Y")


def get_end_date(period_end: datetime) -> str:
    time_zone_info = ZoneInfo(DEFAULT_TIME_ZONE)
    return (period_end.astimezone(time_zone_info) - timedelta(days=1)).strftime(
        "%d-%m-%Y"
    )


def get_market_role_in_file_name(
    requesting_actor_market_role: MarketRole,
) -> str | None:
    if requesting_actor_market_role == MarketRole.ENERGY_SUPPLIER:
        return MarketRoleInFileName.ENERGY_SUPPLIER
    elif requesting_actor_market_role == MarketRole.GRID_ACCESS_PROVIDER:
        return MarketRoleInFileName.GRID_ACCESS_PROVIDER

    return None
