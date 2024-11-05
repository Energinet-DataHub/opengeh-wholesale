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


import logging
from unittest.mock import patch

import pytest

from package.common.logger import Logger


@pytest.mark.parametrize(
    "log_method, log_func",
    [
        ("debug", Logger.debug),
        ("info", Logger.info),
        ("warning", Logger.warning),
        ("warning", Logger.error),
    ],
)
def test__log_method__when_called_with_custom_extras__passes_correct_extras(
    log_method, log_func
):
    # Arrange
    logger = Logger("test_logger")
    test_message = f"Test {log_method} message"
    custom_extras = {"key": "value"}
    expected_extras = custom_extras | logger._extras

    with patch.object(Logger, log_method) as mock_log_method:
        # Act
        getattr(logger, log_method)(test_message, extras=custom_extras)

        # Assert
        mock_log_method.assert_called_once_with(test_message, extra=expected_extras)
