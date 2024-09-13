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
from typing import TypeVar

from package.calculation.calculation_output import CalculationOutput
from package.calculation.wholesale.handlers.repository_interfaces import (
    CalculationMetaData,
)

# Define generic type variables
RequestType = TypeVar("RequestType")
ResponseType = TypeVar("ResponseType")


class CalculationStep(ABC):

    @abstractmethod
    def set_next(self, handler: CalculationStep) -> CalculationStep:
        pass

    @abstractmethod
    def execute(self, output: CalculationOutput) -> CalculationOutput:
        pass


class BaseCalculationStep(CalculationStep):
    """
    The default chaining behavior can be implemented inside a base handler class.
    """

    def __init__(
        self,
    ):
        self._next_handler = None

    def set_next(self, handler: CalculationStep) -> CalculationStep:
        self._next_handler = handler
        return handler

    def execute(self, output: CalculationOutput) -> CalculationOutput:
        if self._next_handler:
            return self._next_handler.execute(output)
        return output


class CacheBucket:
    calculator_args: CalculationMetaData
