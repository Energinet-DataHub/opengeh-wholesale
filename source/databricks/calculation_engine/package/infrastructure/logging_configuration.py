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
import os
from typing import Union, Any

from azure.monitor.opentelemetry import configure_azure_monitor

DEFAULT_LOG_LEVEL: int = logging.INFO
_EXTRAS: dict[str, Any] = {}


def configure_logging(
    *,
    cloud_role_name: str,
    applicationinsights_connection_string: Union[str, None] = None,
    extras: Union[dict[str, Any], None] = None,
) -> None:
    """
    Configure logging to use OpenTelemetry and Azure Monitor.

    :param cloud_role_name:
    :param applicationinsights_connection_string:
    :param extras: Custom structured logging data to be included in every log message.
    :return:

    If connection string is None, then logging will not be sent to Azure Monitor.
    This is useful for unit testing.
    """

    # Configure structured logging data to be included in every log message.
    if extras is not None:
        global _EXTRAS
        _EXTRAS = extras.copy()

    # Add cloud role name when logging
    os.environ["OTEL_SERVICE_NAME"] = cloud_role_name

    # Configure OpenTelemetry to log to Azure Monitor.
    if applicationinsights_connection_string is not None:
        configure_azure_monitor(connection_string=applicationinsights_connection_string)

    # Reduce Py4J logging. py4j logs a lot of information.
    logging.getLogger("py4j").setLevel(logging.WARNING)


def get_extras() -> dict[str, Any]:
    return _EXTRAS.copy()


def add_extras(extras: dict[str, Any]) -> None:
    global _EXTRAS
    _EXTRAS = _EXTRAS | extras
