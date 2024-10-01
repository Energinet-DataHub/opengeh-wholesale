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

from __future__ import annotations

from abc import ABC, abstractmethod

from package.calculation.calculation_output import CalculationOutput


class CalculationLink(ABC):

    @abstractmethod
    def set_next(self, calculation_link: CalculationLink) -> CalculationLink:
        pass

    @abstractmethod
    def execute(self, output: CalculationOutput) -> CalculationOutput:
        pass


class BaseCalculationLink(CalculationLink):

    def __init__(
        self,
    ) -> None:
        self._next_link: CalculationLink | None = None

    def set_next(self, calculation_link: CalculationLink) -> CalculationLink:
        self._next_link = calculation_link
        return calculation_link

    def execute(self, output: CalculationOutput) -> CalculationOutput:
        if self._next_link:
            return self._next_link.execute(output)
        return output
