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

import pyspark.sql.types as t
import pytest

from package.common.schemas import assert_contract

some_contract = t.StructType(
    [
        t.StructField("foo", t.StringType(), False),
        t.StructField("bar", t.IntegerType(), False),
        t.StructField("array", t.ArrayType(t.StringType(), True), True),
    ]
)
actual_schema_with_other_datatype = t.StructType(
    [
        t.StructField("foo", t.StringType(), False),
        t.StructField("bar", t.DecimalType(), False),
        t.StructField("array", t.ArrayType(t.StringType(), True), True),
    ]
)
actual_schema_with_more_columns_and_different_column_order = t.StructType(
    [
        t.StructField("bar", t.IntegerType(), False),
        t.StructField("another-field", t.StringType(), True),
        t.StructField("foo", t.StringType(), False),
        t.StructField("array", t.ArrayType(t.StringType(), True), True),
    ]
)


@pytest.mark.parametrize(
    "contract, actual_schema",
    [
        (some_contract, some_contract),
        (some_contract, actual_schema_with_more_columns_and_different_column_order),
    ],
)
def test__when_schema_complies_with_contract__does_not_raise(
    contract: t.StructType,
    actual_schema: t.StructType,
) -> None:
    # Assert no error is raised
    assert_contract(
        actual_schema,
        contract,
    )


@pytest.mark.parametrize(
    "contract, actual_schema",
    [
        (some_contract, actual_schema_with_other_datatype),
    ],
)
def test__when_schema_does_not_comply_with_contract__raises(
    contract: t.StructType,
    actual_schema: t.StructType,
) -> None:
    with pytest.raises(AssertionError):
        assert_contract(
            actual_schema,
            contract,
        )
