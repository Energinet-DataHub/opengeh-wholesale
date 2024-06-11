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

from pyspark.sql.types import DataType


class ColumnName:
    def __init__(self, name: str, data_type: DataType, deprecated: bool = False):
        self.name = name
        self.data_type = data_type
        self.deprecated = deprecated

    def get(self) -> Any:
        if self.deprecated:
            raise Warning(
                f"Column name '{self.name}' is deprecated and should not be used."
            )
        return self

    def __get__(self, instance: str, owner: str) -> Any:
        return self.name
