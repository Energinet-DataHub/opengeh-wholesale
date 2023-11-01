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

from enum import Enum
from package.codelists import ChargeResolution


class TariffResolution(Enum):
    """
    The part of `ChargeResolution` which is relevant for tariffs.
    These are from charges that are read from the input delta table.
    """

    DAY = ChargeResolution.DAY
    HOUR = ChargeResolution.HOUR
