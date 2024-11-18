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
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)


def should_have_result_file_per_grid_area(
    args: SettlementReportArgs,
) -> bool:
    exactly_one_grid_area_from_calc_ids = (
        args.calculation_id_by_grid_area is not None
        and len(args.calculation_id_by_grid_area) == 1
    )

    exactly_one_grid_area_from_grid_area_codes = (
        args.grid_area_codes is not None and len(args.grid_area_codes) == 1
    )

    return (
        exactly_one_grid_area_from_calc_ids
        or exactly_one_grid_area_from_grid_area_codes
        or args.split_report_by_grid_area
    )
