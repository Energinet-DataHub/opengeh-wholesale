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
from typing import Dict, Type
from geh_stream.bus.broker import Message, TMessageHandler


class MessageDispatcher(Dict[Type[Message], TMessageHandler]):
    """
    A message handler that dispatches incoming messages to the appropriate
    handler. Each message type can have a single handler associated.
    """
    def __call__(self, msg: Message):
        message_type = type(msg)
        if message_type in self:
            print('DISPATCH: %s' % msg)
            handler = self[message_type]
            handler(msg)
        else:
            print('IGNORING: %s' % message_type)

    def set_master_data_root_path(self, path: str):
        self.master_data_root_path = path
