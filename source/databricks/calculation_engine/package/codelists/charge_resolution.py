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


class ChargeResolution(Enum):
    """
    Time resolution of the charges, which is read from input delta table
    """

    MONTH = "P1M"
    """Applies to subscriptions and fees"""
    DAY = "P1D"
    """Applies to tariffs"""
    HOUR = "PT1H"
    """Applies to tariffs"""
