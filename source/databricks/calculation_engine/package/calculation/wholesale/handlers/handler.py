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

# Define generic type variables
RequestType = TypeVar("RequestType")
ResponseType = TypeVar("ResponseType")


class Handler(ABC, Generic[RequestType, ResponseType]):
    """
    The Handler interface declares a method for building the chain of handlers.
    It also declares a method for executing a request.
    """

    @abstractmethod
    def set_next(
        self, handler: "Handler[RequestType, ResponseType]"
    ) -> "Handler[RequestType, ResponseType]":
        pass

    @abstractmethod
    def handle(self, request: RequestType) -> Optional[ResponseType]:
        pass


class BaseHandler(Handler[RequestType, ResponseType]):
    """
    The default chaining behavior can be implemented inside a base handler class.
    """

    _next_handler: Optional[Handler[RequestType, ResponseType]] = None

    def set_next(
        self, handler: "Handler[RequestType, ResponseType]"
    ) -> "Handler[RequestType, ResponseType]":
        self._next_handler = handler
        # Returning a handler from here will let us link handlers in a convenient way like handler1.set_next(handler2).set_next(handler3)
        return handler

    @abstractmethod
    def handle(self, request: RequestType) -> Optional[ResponseType]:
        if self._next_handler:
            return self._next_handler.handle(request)
        return None
