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

from package.infrastructure import initialize_spark
from pyspark.sql import SparkSession
from package.common.logger import Logger
import package.infrastructure.environment_variables as env_vars


def run_settlement_report(catalog_name: str | None = None) -> None:
    logger = Logger(__name__)

    # spark = initialize_spark()
    catalog_name = catalog_name or env_vars.get_catalog_name()

    print("reached entrypoint")
    logger.info("reached entrypoint")
