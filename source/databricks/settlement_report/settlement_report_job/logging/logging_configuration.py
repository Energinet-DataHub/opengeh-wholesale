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
import contextlib
import logging
import os
from typing import Any, Iterator

from azure.monitor.opentelemetry import configure_azure_monitor
from opentelemetry import trace
from opentelemetry.trace import Span, Tracer

DEFAULT_LOG_FORMAT: str = "%(asctime)s %(levelname)s %(name)s: %(message)s"
DEFAULT_LOG_LEVEL: int = logging.INFO
_EXTRAS: dict[str, Any] = {}
_IS_INSTRUMENTED: bool = False
_TRACER: Tracer | None = None
_TRACER_NAME: str


def configure_logging(
    *,
    cloud_role_name: str,
    tracer_name: str,
    applicationinsights_connection_string: str | None = None,
    extras: dict[str, Any] | None = None,
) -> None:
    """
    Configure logging to use OpenTelemetry and Azure Monitor.

    :param cloud_role_name:
    :param tracer_name:
    :param applicationinsights_connection_string:
    :param extras: Custom structured logging data to be included in every log message.
    :return:

    If connection string is None, then logging will not be sent to Azure Monitor.
    This is useful for unit testing.
    """

    global _TRACER_NAME
    _TRACER_NAME = tracer_name

    # Only configure logging once.
    global _IS_INSTRUMENTED
    if _IS_INSTRUMENTED:
        return

    # Configure structured logging data to be included in every log message.
    if extras is not None:
        global _EXTRAS
        _EXTRAS = extras.copy()

    # Add cloud role name when logging
    os.environ["OTEL_SERVICE_NAME"] = cloud_role_name

    # Configure OpenTelemetry to log to Azure Monitor.
    if applicationinsights_connection_string is not None:
        configure_azure_monitor(connection_string=applicationinsights_connection_string)
        _IS_INSTRUMENTED = True

    # Reduce Py4J logging. py4j logs a lot of information.
    logging.getLogger("py4j").setLevel(logging.WARNING)


def get_extras() -> dict[str, Any]:
    return _EXTRAS.copy()


def add_extras(extras: dict[str, Any]) -> None:
    global _EXTRAS
    _EXTRAS = _EXTRAS | extras


def get_tracer() -> Tracer:
    global _TRACER
    if _TRACER is None:
        global _TRACER_NAME
        _TRACER = trace.get_tracer(_TRACER_NAME)
    return _TRACER


@contextlib.contextmanager
def start_span(name: str) -> Iterator[Span]:
    with get_tracer().start_as_current_span(name, attributes=get_extras()) as span:
        yield span
