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
from pyspark.sql import DataFrame


class CacheBucket:

    def __init__(self) -> None:
        self._metering_points = None

    @property
    def metering_point_periods(self) -> DataFrame:
        return self._metering_points

    @metering_point_periods.setter
    def metering_point_periods(self, value: DataFrame) -> None:
        if self._metering_points is not None:
            raise AttributeError("metering_points can only be set once.")
        self._metering_points = value
