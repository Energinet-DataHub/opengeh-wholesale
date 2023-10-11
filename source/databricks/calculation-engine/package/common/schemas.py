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

from pyspark.sql.types import StructType


def verify_schema(
    actual: StructType,
    expected: StructType,
    ignore_nullability=False,
    ignore_column_order=False,
) -> None:
    if actual == expected:
        return

    if not ignore_nullability and not ignore_column_order:
        if actual != expected:
            raise ValueError(f"Schema mismatch. Expected {expected}, but got {actual}.")

    # TODO BJM: The following is a workaround while transitioning code base to support exact schema match

    actual_fields = actual.fields
    expected_fields = expected.fields

    if not ignore_column_order:
        actual_fields.sort(key=lambda f: f.name)
        expected_fields.sort(key=lambda f: f.name)

    for a, e in zip(actual_fields, expected_fields):
        if a.name != e.name:
            raise ValueError(f"Expected column name {e.name}, but found {a.name}")

        if not ignore_nullability and a.type != e.type:
            raise ValueError(
                f"Expected column name {e.name} to have type {e.type}, but got type {a.type}"
            )
