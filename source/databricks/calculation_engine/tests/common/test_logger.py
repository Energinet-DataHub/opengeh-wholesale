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
from package.common.logger import Logger


def test__given_custom_extras__when_debug_called__then_correct_extras_passed():
    # Arrange
    logger = Logger("test_logger")
    test_message = "Test debug message"
    custom_extras = {"key": "value"}
    expected_extras = custom_extras | logger.extras
    with patch.object(logging.Logger, "debug") as mock_debug:
        # Act
        logger.debug(test_message, extras=custom_extras)

        # Assert
        mock_debug.assert_called_once_with(test_message, extra=expected_extras)


def test__given_custom_extras__when_info_called__then_correct_extras_passed():
    # Arrange
    logger = Logger("test_logger")
    test_message = "Test info message"
    custom_extras = {"key": "value"}
    expected_extras = custom_extras | logger.extras
    with patch.object(logging.Logger, "info") as mock_info:
        # Act
        logger.info(test_message, extras=custom_extras)

        # Assert
        mock_info.assert_called_once_with(test_message, extra=expected_extras)


def test__given_custom_extras__when_warning_called__then_correct_extras_passed():
    # Arrange
    logger = Logger("test_logger")
    test_message = "Test warning message"
    custom_extras = {"key": "value"}
    expected_extras = custom_extras | logger.extras
    with patch.object(logging.Logger, "warning") as mock_warning:
        # Act
        logger.warning(test_message, extras=custom_extras)

        # Assert
        mock_warning.assert_called_once_with(test_message, extra=expected_extras)
