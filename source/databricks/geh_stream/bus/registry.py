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

from inspect import isclass
from typing import Dict, Union, Type, Optional

from geh_stream.bus.broker import Message


TDictItem = Union[str, Message, Type[Message]]


class MessageRegistry(Dict[str, Type[Message]]):
    """
    A registry of all messages that the bus knows of.

    Works a a dict where:
        Key = Message type name (str)
        Value = Message class (Serializable)
    """

    @classmethod
    def from_message_types(cls, *types: Type[Message]) -> 'MessageRegistry':
        return cls({c.__name__: c for c in types})

    def add(self, *message_types: Type[Message]):
        self.update({c.__name__: c for c in message_types})

    def __contains__(self, item: TDictItem) -> bool:
        """
        Check whether an item is known by the registry.

        Item can be either of the following:
            - A string (name of the message type)
            - A class type (the message type itself)
            - An instance of a class (an instance of a message type)
        """
        if isinstance(item, str):
            return item in self.keys()
        elif isclass(item):
            return item in self.values()
        elif isinstance(item, Message):
            return item.__class__ in self.values()
        else:
            return False
