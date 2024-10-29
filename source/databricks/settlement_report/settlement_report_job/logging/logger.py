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
from typing import Any

import settlement_report_job.logging.logging_configuration as config


class Logger:
    def __init__(self, name: str, extras: dict[str, Any] | None = None) -> None:
        # Configuring logging with basicConfig ensures that log statements are
        # displayed in task logs in Databricks.
        logging.basicConfig(
            level=config.DEFAULT_LOG_LEVEL, format=config.DEFAULT_LOG_FORMAT
        )
        self._logger = logging.getLogger(name)
        self._logger.setLevel(config.DEFAULT_LOG_LEVEL)
        self._extras = (extras or {}) | config.get_extras()
        # According to DataHub 3.0 guide book
        self._extras["CategoryName"] = "Energinet.DataHub." + name

    def debug(self, message: str, extras: dict[str, Any] | None = None) -> None:
        extras = (extras or {}) | self._extras
        self._logger.debug(message, extra=extras)

    def info(self, message: str, extras: dict[str, Any] | None = None) -> None:
        extras = (extras or {}) | self._extras
        self._logger.info(message, extra=extras)

    def warning(self, message: str, extras: dict[str, Any] | None = None) -> None:
        extras = (extras or {}) | self._extras
        self._logger.warning(message, extra=extras)

    def error(self, message: str, extras: dict[str, Any] | None = None) -> None:
        extras = (extras or {}) | self._extras
        self._logger.error(message, extra=extras)
