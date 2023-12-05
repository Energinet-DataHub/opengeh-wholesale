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


import logging
import sys

from package import calculation
from package import calculation_input
from package.calculator_job_args import get_calculator_args
from package.infrastructure import initialize_spark
from package.infrastructure.logging_configuration import initialize_logging
from package.infrastructure.storage_account_access import islocked


# The start() method should only have its name updated in correspondence with the
# wheels entry point for it. Further the method must remain parameterless because
# it will be called from the entry point when deployed.
def start() -> None:
    initialize_logging()
    logger = logging.getLogger(__name__)

    args = get_calculator_args()

    if islocked(args.data_storage_account_name, args.data_storage_account_credentials):
        logger.error(
            "Exiting because storage is locked due to data migrations running."
        )
        sys.exit(3)

    # Create calculation execution dependencies
    spark = initialize_spark()
    delta_table_reader = calculation_input.TableReader(
        spark, args.calculation_input_path, args.time_series_points_table_location
    )
    prepared_data_reader = calculation.PreparedDataReader(delta_table_reader)

    logger.info("Starting calculation")

    # Execute calculation
    calculation.execute(args, prepared_data_reader)

    logger.info("Calculation completed successfully")
