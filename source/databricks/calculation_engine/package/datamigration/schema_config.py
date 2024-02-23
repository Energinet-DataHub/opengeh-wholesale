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

from spark_sql_migrations import Schema, Table
from package.infrastructure.paths import (
    WHOLESALE_RESULT_TABLE_NAME,
    OUTPUT_DATABASE_NAME,
    ENERGY_RESULT_TABLE_NAME,
)

# calculation_output
from package.calculation_output.schemas.wholesale_results_schema import (
    wholesale_results_schema,
)
from package.calculation_output.schemas.energy_results_schema import (
    energy_results_schema,
)

schema_config = [
    Schema(
        name=OUTPUT_DATABASE_NAME,
        tables=[
            Table(
                name=WHOLESALE_RESULT_TABLE_NAME,
                schema=wholesale_results_schema,
            ),
            Table(
                name=ENERGY_RESULT_TABLE_NAME,
                schema=energy_results_schema,
            ),
        ],
    )
]
