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
from __future__ import annotations

from typing import Any

import intercepts
import logging

from azure.monitor.opentelemetry import configure_azure_monitor

DEFAULT_LOG_LEVEL = logging.INFO


def initialize_logging() -> None:
    """
    Configure logging to use OpenTelemetry and Azure Monitor.

    The logging is configured to add "Domain"="wholesale" as structured logging.
    This enables us to filter on the domain in Azure Monitor.
    """
    # Configure OpenTelemetry to use Azure Monitor.
    # The connection string is read from the environment variable APPLICATIONINSIGHTS_CONNECTION_STRING
    # as it is not provided explicitly.
    configure_azure_monitor()

    # Intercept all logging functions in order to add structured logging.
    intercepts.register(logging.Logger.debug, _add_extra)
    intercepts.register(logging.Logger.info, _add_extra)
    intercepts.register(logging.Logger.warning, _add_extra)
    # noinspection PyDeprecation
    intercepts.register(logging.Logger.warn, _add_extra)
    intercepts.register(logging.Logger.error, _add_extra)
    intercepts.register(logging.Logger.exception, _add_extra)
    intercepts.register(logging.Logger.critical, _add_extra)
    intercepts.register(logging.Logger.fatal, _add_extra)
    intercepts.register(logging.Logger.log, _add_extra)

    # Suppress Py4J logging to only show errors and above.
    # py4j logs a lot of information.
    logging.getLogger("py4j").setLevel(logging.ERROR)

    # Set default log level for all module loggers being created.
    intercepts.register(logging.getLogger, _set_default_log_level)


def _add_extra(self: logging.Logger, msg: str, *args: object, **kwargs: Any) -> None:
    """Add extra structured logging to enable filtering on e.g. "Domain" in Azure Monitor."""
    kwargs["extra"] = kwargs.get("extra", {})
    kwargs["extra"]["Domain"] = "wholesale"

    _(self, msg, *args, **kwargs)  # type: ignore # noqa: F821 (mypy and flake8)


def _set_default_log_level(name: str | None) -> logging.Logger:
    """Set default log level for all loggers on create."""
    _logger: logging.Logger = _(name)  # type: ignore # noqa: F821 (mypy and flake8)
    _logger.setLevel(DEFAULT_LOG_LEVEL)
    return _logger
