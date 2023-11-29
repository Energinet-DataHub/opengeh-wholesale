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

from azure.monitor.opentelemetry import configure_azure_monitor


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

    logging.Logger.debug = _create_interceptor(logging.DEBUG)
    logging.Logger.info = _create_interceptor(logging.INFO)
    logging.Logger.warning = _create_interceptor(logging.WARNING)
    logging.Logger.warn = logging.Logger.warning
    logging.Logger.error = _create_interceptor(logging.ERROR)
    logging.Logger.exception = _create_exception_interceptor()
    logging.Logger.critical = _create_interceptor(logging.CRITICAL)
    logging.Logger.fatal = _create_interceptor(logging.FATAL)
    logging.Logger.log = _create_log_interceptor()

    logging.getLogger("py4j").setLevel(logging.ERROR)


def _create_interceptor(level: int) -> callable:
    def _interceptor(
        self: logging.Logger,
        msg,
        *args,
        exc_info=None,
        extra=None,
        stack_info=False,
        stacklevel=1
    ):
        extra = _add_extra(extra)
        if self.isEnabledFor(level):
            return self._log(level, msg, args, exc_info, extra, stack_info, stacklevel)

    return _interceptor


def _create_exception_interceptor() -> callable:
    def _interceptor(self, msg, *args, exc_info=True, **kwargs):
        kwargs = _add_extra(**kwargs)
        if self.isEnabledFor(logging.ERROR):
            return self._log(logging.ERROR, msg, args, exc_info=exc_info, **kwargs)

    return _interceptor


def _create_log_interceptor():
    def _interceptor(self, level, msg, *args, **kwargs):
        kwargs = _add_extra(**kwargs)
        if self.isEnabledFor(level):
            return self._log(level, msg, args, kwargs)

    return _interceptor


def _add_extra(extra) -> dict[str, Any]:
    """Add extra structured logging to enable filtering on domain in Azure Monitor."""
    extra = extra or {}
    extra["Domain"] = "wholesale"
    return extra
