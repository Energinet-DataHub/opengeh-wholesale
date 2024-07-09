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
from dataclasses import dataclass

from pyspark.sql import DataFrame


@dataclass
class ExpectedOutput:
    """
    A single expected output of a calculation.
    This can be a specific energy og wholesale result like total_production, and some basis data.
    """

    name: str
    """
    The name of the output.
    This should match the name of the corresponding property in the CalculationResults class.
    """

    df: DataFrame
    """The expected output."""
