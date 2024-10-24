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

from collections.abc import Callable
from pyspark.sql import DataFrame
import pyspark.sql.functions as F


import settlement_report_job.domain.report_naming_convention as market_naming
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs

from settlement_report_job.domain.csv_column_names import (
    CsvColumnNames,
)
from settlement_report_job.utils import map_from_dict
from settlement_report_job.wholesale.column_names import DataProductColumnNames

from settlement_report_job.domain.energy_results.read_and_filter import (
    read_and_filter_from_view,
)
from settlement_report_job.domain.energy_results.prepare_for_csv import (
    prepare_for_csv,
)


def create_energy_results(
    args: SettlementReportArgs,
    repository: WholesaleRepository,
) -> DataFrame:
    energy = read_and_filter_from_view(args, repository)

    return prepare_for_csv(energy, args.split_report_by_grid_area)
