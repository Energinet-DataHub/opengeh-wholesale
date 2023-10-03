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

from typing import Any


def qualname(symbol: Any) -> str:
    """
    Returns the fully qualified name of the symbol.

    Example: `qualname(some_method)`, where `some_method` is a method of a class `Bar`
             in module `foo` will return `foo.Bar.some_method`.
    """
    return symbol.__module__ + "." + symbol.__qualname__
