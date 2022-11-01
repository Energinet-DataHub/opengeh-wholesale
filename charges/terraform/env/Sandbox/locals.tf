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

locals {
    # The string value is the shared keyvault key name
    INTEGRATION_EVENTS_LISTENER_CONNECTION_STRING       = "SHARED-RESOURCES--SB-INTEGRATIONEVENTS-LISTEN-CONNECTION-STRING"
    INTEGRATION_EVENTS_SENDER_CONNECTION_STRING         = "SHARED-RESOURCES--SB-INTEGRATIONEVENTS-SEND-CONNECTION-STRING"
    INTEGRATION_EVENTS_MANAGER_CONNECTION_STRING        = "SHARED-RESOURCES--SB-INTEGRATIONEVENTS-MANAGE-CONNECTION-STRING"

    # Message Hub
    MESSAGEHUB_STORAGE_CONNECTION_STRING_KEY            = "SHARED-RESOURCES-MARKETOPERATOR-RESPONSE-CONNECTION-STRING"
    MESSAGEHUB_STORAGE_CONTAINER_KEY                    = "SHARED-RESOURCES-MARKETOPERATOR-CONTAINER-REPLY-NAME"
    CHARGE_LINK_CREATED_TOPIC_NAME                      = "charge-link-created"
    CHARGE_CREATED_TOPIC_NAME                           = "charge-created"
    CHARGE_PRICES_UPDATED_TOPIC_NAME                    = "charge-prices-updated"
    CREATE_LINK_REQUEST_QUEUE_NAME                      = "create-link-request"
    CREATE_LINK_REPLY_QUEUE_NAME                        = "create-link-reply"

    # Message Hub
    MESSAGEHUB_DATAAVAILABLE_QUEUE                      = "dataavailable"
    MESSAGEHUB_BUNDLEREQUEST_QUEUE                      = "charges"
    MESSAGEHUB_BUNDLEREPLY_QUEUE                        = "charges-reply"
}