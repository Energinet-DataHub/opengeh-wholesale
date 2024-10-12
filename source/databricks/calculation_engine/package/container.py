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
from pyspark.sql import SparkSession

import package
from package.infrastructure.infrastructure_settings import InfrastructureSettings


class Container(containers.DeclarativeContainer):
    infrastructure_settings = providers.Configuration()
    spark = providers.Factory(lambda: None)
    metering_point_period_repository = providers.Factory(lambda: None)
    cache_bucket = providers.Factory(lambda: None)
    calculator_args = providers.Factory(lambda: None)


def create_and_configure_container(
    spark: SparkSession,
    infrastructure_settings: InfrastructureSettings,
) -> Container:

    container = Container()

    container.spark.override(spark)

    container.infrastructure_settings.from_value(infrastructure_settings)

    container.wire(packages=[package])

    return container
