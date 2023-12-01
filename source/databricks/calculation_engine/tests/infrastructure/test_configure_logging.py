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

import pytest
from opentelemetry import trace

from package.common.logger import Logger
from package.infrastructure import configure_logging
from package.infrastructure.logging_configuration import get_extras

# Logging format to enable testing of structured logging data (e.g. "Domain").
LOGGING_FORMAT = "%(levelname)s - %(message)s - %(Domain)s"

# Acquire a tracer
tracer = trace.get_tracer("test.tracer")


def simulate_calculation_execute():
    logger = Logger("calculation", extras={"some-logger-value": "Hello, world!"})
    logger.info("calculation executing...", extras={"custom-log-value": "Hey, there!"})


class TestWhenInvoked:
    def test_tracer_poc_2(self) -> None:
        configure_logging(
            cloud_role_name="calculation-engine-6",
            applicationinsights_connection_string="",
            extras={"Domain": "wholesale"},
        )

        # Create a logger
        logger = Logger(__name__, extras={"calculation_id": 42})

        # Use tracer and span to set operation ID and thus decouple from all other logs without operation ID
        with tracer.start_as_current_span("root", attributes=get_extras()):
            logger.info("Calculator job started")
            simulate_calculation_execute()
            logger.info("Calculator job ended")

    # def test_succeeds(self) -> None:
    #     initialize_logging()

    def test_can_log_debug(self, capfd, caplog) -> None:
        # configure_logging()
        # caplog.set_level(logging.DEBUG, logger=__name__)
        logger = logging.getLogger(__name__)

        # Setup handler in order to be able to verify that "Domain" has been added as structured logging data.
        self.create_test_handler(logger)

        logger.info("test info message")
        out, err = capfd.readouterr()
        print("####### " + repr(out))
        print("####### " + repr(err))
        assert out == ""
        assert err == "DEBUG - test info message - wholesale\n"

    # def test_can_log_debug_with_all_args(self) -> None:
    #     initialize_logging()
    #     logger = logging.getLogger(__name__)
    #     logger.debug(
    #         "test info message", foo="bar", baz="qux", quux={"corge": "grault"}
    #     )

    @pytest.mark.parametrize(
        "func, level",
        [
            (logging.Logger.debug, "DEBUG"),
            (logging.Logger.info, "INFO"),
            (logging.Logger.warn, logging.WARN),
            (logging.Logger.warning, logging.WARNING),
            (logging.Logger.error, logging.ERROR),
            # (logging.Logger.exception, logging.EXCEPTION),
            (logging.Logger.critical, logging.CRITICAL),
            (logging.Logger.fatal, logging.FATAL),
        ],
    )
    def test_can_log_info_with_domain(self, capfd, func, level) -> None:
        # Act
        # configure_logging()

        # Assert
        logger = logging.getLogger(__name__)
        self.create_test_handler(logger)

        # logger.info("test info message")
        func(logger, "test info message")
        out, err = capfd.readouterr()
        assert err == f"{level} - test info message - wholesale\n"

    def create_test_handler(self, logger):
        """
        Setup handler in order to be able to verify:
        - The log level
        - The log message
        - That "Domain" has been added as structured logging data with expected value.
        """
        stderr_handler = logging.StreamHandler()
        stderr_handler.setLevel(
            logging.DEBUG
        )  # Same level as the Azure Monitor handler
        formatter = logging.Formatter(LOGGING_FORMAT)
        stderr_handler.setFormatter(formatter)
        logger.addHandler(stderr_handler)

    # def test_can_log_warning(self) -> None:
    #     initialize_logging()
    #     logger = logging.getLogger(__name__)
    #     logger.warning("test info message")
    #
    # def test_can_log_warn(self) -> None:
    #     initialize_logging()
    #     logger = logging.getLogger(__name__)
    #     logger.warn("test info message")
    #
    # def test_can_log_error(self) -> None:
    #     initialize_logging()
    #     logger = logging.getLogger(__name__)
    #     logger.error("test info message")
    #
    # def test_can_log_exception(self) -> None:
    #     initialize_logging()
    #     logger = logging.getLogger(__name__)
    #     logger.exception("test info message")
    #
    # def test_can_log_critical(self) -> None:
    #     initialize_logging()
    #     logger = logging.getLogger(__name__)
    #     logger.critical("test info message")
    #
    # def test_can_log_fatal(self) -> None:
    #     initialize_logging()
    #     logger = logging.getLogger(__name__)
    #     logger.fatal("test info message")
    #
    # def test_can_log_log(self) -> None:
    #     initialize_logging()
    #     logger = logging.getLogger(__name__)
    #     logger.log(logging.INFO, "test info message")
