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
data "template_file" "workbook_charge_information_template" {
  template = file("${path.module}/workbook-charge-information-template.json")
  vars = {
    workbook_display_name     = "Charge - Information (D18) SLI"
    workbook_id               = "67d32b14-3f26-4840-a1cc-c47ca4d052de"
    resouce_group_name        = azurerm_resource_group.this.name
    subscription_id           = data.azurerm_subscription.this.subscription_id
    application_insight_name  = data.azurerm_key_vault_secret.appi_shared_name.value
    shared_resouce_group_name = var.shared_resources_resource_group_name
  }
}

resource "azurerm_resource_group_template_deployment" "workbook_charge_information" {
  name                  = "workbook-information-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  resource_group_name   = azurerm_resource_group.this.name
  template_content      = data.template_file.workbook_charge_information_template.rendered
  deployment_mode       = "Incremental"
}
