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

from package.infrastructure import initialize_logging


class TestWhenInvoked:
    # def test_succeeds(self) -> None:
    #     initialize_logging()

    def test_can_log_debug(self, capfd) -> None:
        initialize_logging()

        logger = logging.getLogger(__name__)
        logger.setLevel(logging.DEBUG)

        stderr_handler = logging.StreamHandler()
        stderr_handler.setLevel(logging.DEBUG)
        formatter = logging.Formatter("%(levelname)s - %(message)s - %(Domain)s")
        stderr_handler.setFormatter(formatter)

        logger.addHandler(stderr_handler)

        logger.debug("test info message")
        out, err = capfd.readouterr()
        print(out)
        print(err)
        assert out == ""
        assert err == "DEBUG - test info message - wholesale\n"

    def test_can_log_debug_with_all_args(self) -> None:
        initialize_logging()
        logger = logging.getLogger(__name__)
        logger.debug(
            "test info message", foo="bar", baz="qux", quux={"corge": "grault"}
        )

    def test_can_log_info(self) -> None:
        initialize_logging()
        logger = logging.getLogger(__name__)
        logger.info("test info message")

    def test_can_log_warning(self) -> None:
        initialize_logging()
        logger = logging.getLogger(__name__)
        logger.warning("test info message")

    def test_can_log_warn(self) -> None:
        initialize_logging()
        logger = logging.getLogger(__name__)
        logger.warn("test info message")

    def test_can_log_error(self) -> None:
        initialize_logging()
        logger = logging.getLogger(__name__)
        logger.error("test info message")

    def test_can_log_exception(self) -> None:
        initialize_logging()
        logger = logging.getLogger(__name__)
        logger.exception("test info message")

    def test_can_log_critical(self) -> None:
        initialize_logging()
        logger = logging.getLogger(__name__)
        logger.critical("test info message")

    def test_can_log_fatal(self) -> None:
        initialize_logging()
        logger = logging.getLogger(__name__)
        logger.fatal("test info message")

    def test_can_log_log(self) -> None:
        initialize_logging()
        logger = logging.getLogger(__name__)
        logger.log(logging.INFO, "test info message")
