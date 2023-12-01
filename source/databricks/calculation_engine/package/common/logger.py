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

from package.infrastructure.logging_configuration import DEFAULT_LOG_LEVEL, get_extras


class Logger:
    def __init__(self, name: str, extras: dict[str, Any] = None) -> None:
        self.logger = logging.getLogger(name)
        self.logger.setLevel(DEFAULT_LOG_LEVEL)
        x = get_extras()
        self.extras = (extras or {}) | get_extras()
        print(self.extras)

    def info(self, message: str, extras: dict[str, Any] = None) -> None:
        extras = (extras or {}) | self.extras
        self.logger.info(message, extra=extras)
