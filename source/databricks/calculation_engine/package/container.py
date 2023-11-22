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

from dependency_injector import containers, providers

import package
import package.infrastructure.environment_variables as env_vars
from package.calculation import PreparedDataReader
from package.calculation_input import TableReader
from package.infrastructure import initialize_spark
from package.infrastructure import paths


class Container(containers.DeclarativeContainer):
    calculation_input_path = providers.Configuration()

    spark = providers.Singleton(initialize_spark)

    table_reader = providers.Factory(
        TableReader,
        spark=spark,
        calculation_input_path=calculation_input_path,
    )

    prepared_data_reader = providers.Factory(
        PreparedDataReader, delta_table_reader=table_reader
    )


def create_and_configure_container() -> None:
    """Configure and enable dependency injector container."""
    container = Container()
    container.calculation_input_path.from_value(_get_calculation_input_path())
    container.wire(packages=[package])


def _get_calculation_input_path() -> str:
    storage_account_name = env_vars.get_storage_account_name()
    return paths.get_calculation_input_path(storage_account_name)
