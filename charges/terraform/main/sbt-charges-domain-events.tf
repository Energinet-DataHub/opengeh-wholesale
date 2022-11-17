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

module "sbt_charges_domain_events" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic?ref=v9"
  name                = "domain-events"
  namespace_id        = data.azurerm_key_vault_secret.sb_domain_relay_namespace_id.value
  project_name        = var.domain_name_short
}

module "sbtsub-charges-info-command-received" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "info-command-received"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeInformationCommandReceivedEvent"
  }
}

module "sbtsub-charges-info-operations-accepted-publish" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "info-operations-accepted-publish"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeInformationOperationsAcceptedEvent"
  }
}

module "sbtsub-charges-info-operations-accepted-da" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "info-operations-accepted-da"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeInformationOperationsAcceptedEvent"
  }
}

module "sbtsub-charges-info-operations-accepted" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "info-operations-accepted"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeInformationOperationsAcceptedEvent"
  }
}

module "sbtsub-charges-info-operations-accepted-persist" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "info-operations-accepted-persist"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeInformationOperationsAcceptedEvent"
  }
}

module "sbtsub-charges-info-operations-accepted-persist-hi" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "info-operations-accepted-persist-hi"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeInformationOperationsAcceptedEvent"
  }
}

module "sbtsub-charges-info-operations-rejected" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "info-operations-rejected"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeInformationOperationsRejectedEvent"
  }
}

module "sbtsub-charges-default-charge-links-da" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "default-charge-links-da"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeLinksDataAvailableNotifiedEvent"
  }
}

module "sbtsub-charges-links-accepted-publish" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "links-accepted-publish"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeLinksAcceptedEvent"
  }
}

module "sbtsub-charges-links-accepted-da" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "links-accepted-da"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeLinksAcceptedEvent"
  }
}

module "sbtsub-charges-links-accepted" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "links-accepted"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label ="ChargeLinksAcceptedEvent"
  }
}

module "sbtsub-charges-links-command-received" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "links-command-received"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeLinksReceivedEvent"
  }
}

module "sbtsub-charges-links-command-rejected" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "links-command-rejected"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeLinksRejectedEvent"
  }
}

module "sbtsub-charges-price-command-received" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "price-command-received"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargePriceCommandReceivedEvent"
  }
}

module "sbtsub-charges-price-operations-rejected" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "price-operations-rejected"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargePriceOperationsRejectedEvent"
  }
}

module "sbtsub-charges-price-operations-accepted" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "price-operations-accepted"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargePriceOperationsAcceptedEvent"
  }
}

module "sbtsub-charges-price-operations-accepted-da" {  
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "price-operations-accepted-da"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargePriceOperationsAcceptedEvent"
  }
}

module "sbtsub-charges-price-operations-accepted-publish" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "price-operations-accepted-publish"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargePriceOperationsAcceptedEvent"
  }
}

module "sbtsub-charges-price-operations-accepted-persist" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "price-operations-accepted-persist"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargePriceOperationsAcceptedEvent"
  }
}
