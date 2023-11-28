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

from azure.monitor.opentelemetry import configure_azure_monitor


# Configure OpenTelemetry to use Azure Monitor with the specified connection
# string.


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
    logging.Logger.warn = _create_interceptor(logging.WARN)
    logging.Logger.warning = _create_interceptor(logging.WARNING)
    logging.Logger.error = _create_interceptor(logging.ERROR)
    logging.Logger.exception = _create_exception_interceptor()
    logging.Logger.critical = _create_interceptor(logging.CRITICAL)
    logging.Logger.fatal = _create_interceptor(logging.FATAL)
    logging.Logger.log = _create_log_interceptor()

    logging.basicConfig(level=logging.INFO)


def _create_interceptor(level: int, msg, *args, **kwargs):
    _add_extra(**kwargs)
    return lambda self: self._log(level, msg, args, **kwargs)


def _create_exception_interceptor(msg, *args, exc_info=True, **kwargs):
    _add_extra(**kwargs)
    return lambda self: self._log(logging.ERROR, msg, args, exc_info=exc_info, **kwargs)


def _create_log_interceptor(level: int, msg, *args, **kwargs):
    _add_extra(**kwargs)
    return lambda self: self._log(level, msg, args, **kwargs)


def _add_extra(**kwargs):
    kwargs["extra"] = {"Domain": "wholesale"}
    return kwargs
