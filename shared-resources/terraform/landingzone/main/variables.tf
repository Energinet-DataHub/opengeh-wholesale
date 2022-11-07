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
variable subscription_id {
  type        = string
  description = "Subscription that the infrastructure code is deployed into."
}

variable resource_group_name {
  type        = string
  description = "Resource Group that the infrastructure code is deployed into."
}

variable environment_short {
  type          = string
  description   = "Enviroment that the infrastructure code is deployed into."
}

variable environment_instance {
  type          = string
  description   = "Enviroment instance that the infrastructure code is deployed into."
}

variable domain_name_short {
  type          = string
  description   = "Shortest possible edition of the domain name."
}

variable project_name {
  type          = string
}

variable vm_user_name {
  type          = string
  description   = "Deployment Agent VM user name for SSH connection."
}

variable github_runner_token {
  type          = string
  description   = "Registration token for GitHub self-hosted runner."
}

variable virtual_network_resource_group_name {
  type          = string
  description   = "Resource group name of the virtual network"
}

variable virtual_network_name {
  type          = string
  description   = "Name of the virtual network"
}

variable deployment_agent_address_space {
  type          = string
  description   = "Address space of the deployment agent subnet"
}