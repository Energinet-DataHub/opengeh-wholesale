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

from package.common import assert_schema


any_schema = t.StructType(
    [
        t.StructField("foo", t.StringType(), False),
        t.StructField("bar", t.IntegerType(), False),
    ]
)
schema_with_other_column_order = t.StructType(
    [
        t.StructField("bar", t.IntegerType(), False),
        t.StructField("foo", t.StringType(), False),
    ]
)
schema_with_other_nullability = t.StructType(
    [
        t.StructField("foo", t.StringType(), False),
        t.StructField("bar", t.IntegerType(), True),
    ]
)
schema_with_other_column_order_and_nullability = t.StructType(
    [
        t.StructField("bar", t.IntegerType(), False),
        t.StructField("foo", t.StringType(), True),
    ]
)


@pytest.mark.parametrize(
    "actual, expected, ignore_column_order, ignore_nullability",
    [
        (any_schema, any_schema, False, False),
        (any_schema, any_schema, False, True),
        (any_schema, any_schema, True, False),
        (any_schema, any_schema, True, True),
        (any_schema, schema_with_other_column_order, True, False),
        (any_schema, schema_with_other_column_order, True, True),
        (any_schema, schema_with_other_nullability, False, True),
        (any_schema, schema_with_other_nullability, True, True),
        (any_schema, schema_with_other_column_order_and_nullability, True, True),
    ],
)
def test__assert_schema__accepts_matching_schema(
    actual: t.StructType,
    expected: t.StructType,
    ignore_column_order: bool,
    ignore_nullability: bool,
) -> None:
    # Assert no error is raised
    assert_schema(
        actual,
        expected,
        ignore_column_order=ignore_column_order,
        ignore_nullability=ignore_nullability,
    )


@pytest.mark.parametrize(
    "actual, expected, ignore_column_order, ignore_nullability",
    [
        (any_schema, schema_with_other_column_order, False, False),
        (any_schema, schema_with_other_column_order, False, True),
        (any_schema, schema_with_other_nullability, False, False),
        (any_schema, schema_with_other_nullability, True, False),
        (any_schema, schema_with_other_column_order_and_nullability, False, False),
        (any_schema, schema_with_other_column_order_and_nullability, False, True),
        (any_schema, schema_with_other_column_order_and_nullability, True, False),
    ],
)
def test__assert_schema__when_schema_does_not_match__raises(
    actual: t.StringType,
    expected: t.StructType,
    ignore_column_order: bool,
    ignore_nullability: bool,
) -> None:
    with pytest.raises(ValueError):
        assert_schema(actual, expected)
