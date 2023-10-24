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

import json
from pyspark.sql.types import StructType
from typing import Dict, List


def read_contract(path: str) -> Dict:
    jsonFile = open(path)
    return json.load(jsonFile)


def assert_contract_matches_schema(contract_path: str, schema: StructType) -> None:
    expected_schema = read_contract(contract_path)["fields"]
    actual_schema = json.loads(schema.json())["fields"]

    # Assert: Schema and contract has the same number of fields
    assert len(actual_schema) == len(expected_schema)

    # Assert: Schema matches contract
    for expected_field in expected_schema:
        actual_field = next(
            (x for x in actual_schema if expected_field["name"] == x["name"]), None
        )

        assert (
            actual_field is not None
        ), f"""Actual schema is missing field '{expected_field["name"]}' from contract."""

        actual_type = actual_field["type"]
        # Primitive anticipation of the actual type supporting e.g. "array<string>"
        if type(actual_type) is not str:
            actual_type = f"{actual_type['type']}<{actual_type['elementType']}>"
        assert (
            actual_type == expected_field["type"]
        ), f"""Actual type ({actual_field["type"]}) of field {expected_field["name"]}
        does not match the expected type ({expected_field["type"]})."""


def assert_codelist_matches_contract(codelist: List, contract_path: str) -> None:
    supported_literals = read_contract(contract_path)["literals"]
    literals = [member for member in codelist]

    # Assert: The number of enum values matches the number of values in the contract
    assert len(literals) == len(
        supported_literals
    ), f"The number of literals ({len(literals)}) does not meet the expectation ({len(supported_literals)}) of the contract"

    # Assert: The enum values must match contract
    for literal in literals:
        supported_arg = next(
            (x for x in supported_literals if literal.name == x["name"]), None
        )
        if supported_arg is None:
            raise Exception(f"Literal '{literal.name}' does not exist in contract.")
        assert (
            literal.value == supported_arg["value"]
        ), f"""The value {literal.value} of {literal.name} does not match expected value {supported_arg["value"]}"""


def get_message_type(contract_path: str) -> None:
    grid_area_updated_schema = read_contract(contract_path)
    return next(
        x for x in grid_area_updated_schema["fields"] if x["name"] == "MessageType"
    )["value"]


def get_column_names_from_contract(contract_path: str) -> list[str]:
    schema = read_contract(contract_path)["fields"]
    return [colname["name"] for colname in schema]
