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
from typing import Optional, TypeVar, Generic

from package.calculation.calculation_results import CalculationResultsContainer

# Define generic type variables
RequestType = TypeVar("RequestType")
ResponseType = TypeVar("ResponseType")


class CalculationStep(ABC, Generic[RequestType, ResponseType]):

    @abstractmethod
    def set_next(
        self, handler: "CalculationStep[RequestType, ResponseType]"
    ) -> "CalculationStep[RequestType, ResponseType]":
        pass

    @abstractmethod
    def handle(self) -> Optional[ResponseType]:
        pass


class BaseCalculationStep(CalculationStep[RequestType, ResponseType]):
    """
    The default chaining behavior can be implemented inside a base handler class.
    """

    def __init__(
            self,
            bucket: Bucket,
            calculation_results_container: CalculationResultsContainer,
    ):
        #
        self.bucket = bucket
        self.calculation_results_container = calculation_results_container

    _next_handler: Optional[CalculationStep[RequestType, ResponseType]] = None

    def set_next(
        self, handler: "CalculationStep[RequestType, ResponseType]"
    ) -> "CalculationStep[RequestType, ResponseType]":
        self._next_handler = handler
        # Returning a handler from here will let us link handlers in a convenient way like handler1.set_next(handler2).set_next(handler3)
        return handler

    def handle(self) -> Optional[ResponseType]:
        if self._next_handler:
            return self._next_handler.handle()
        return None


class Bucket:

