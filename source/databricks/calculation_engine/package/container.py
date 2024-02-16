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
from package.infrastructure.infrastructure_settings import InfrastructureSettings


class Container(containers.DeclarativeContainer):
    infrastructure_settings = providers.Configuration()


def create_and_configure_container(
    infrastructure_settings: InfrastructureSettings,
) -> None:
    container = Container()

    container.infrastructure_settings.from_value(infrastructure_settings)

    container.wire(packages=[package])
