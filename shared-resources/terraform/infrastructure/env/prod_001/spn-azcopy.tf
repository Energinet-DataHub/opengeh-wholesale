#
# Application registration with service principal
#
resource "azuread_application" "app_azcopy" {
  display_name = "sp-datahub-azcopy"
  owners = [
    data.azuread_client_config.current_client.object_id
  ]
}

resource "azuread_service_principal" "spn_azcopy" {
  application_id               = azuread_application.app_azcopy.application_id
  app_role_assignment_required = false
  owners = [
    data.azuread_client_config.current_client.object_id
  ]
}

#
# Federated identity credentials for the azcopy service principal
#

locals {
  federated_identity_credential = {
    audience_azuread = "api://AzureADTokenExchange"
    issuer_github    = "https://token.actions.githubusercontent.com"
  }
}

resource "azuread_application_federated_identity_credential" "dev_001" {
  application_object_id = azuread_application.app_azcopy.object_id

  display_name = "dev_001"
  subject      = "repo:Energinet-DataHub/dh3-environments:environment:dev_001"

  audiences = [local.federated_identity_credential.audience_azuread]
  issuer    = local.federated_identity_credential.issuer_github
}

resource "azuread_application_federated_identity_credential" "dev_002" {
  application_object_id = azuread_application.app_azcopy.object_id

  display_name = "dev_002"
  subject      = "repo:Energinet-DataHub/dh3-environments:environment:dev_002"

  audiences = [local.federated_identity_credential.audience_azuread]
  issuer    = local.federated_identity_credential.issuer_github
}

resource "azuread_application_federated_identity_credential" "test_001" {
  application_object_id = azuread_application.app_azcopy.object_id

  display_name = "test_001"
  subject      = "repo:Energinet-DataHub/dh3-environments:environment:test_001"

  audiences = [local.federated_identity_credential.audience_azuread]
  issuer    = local.federated_identity_credential.issuer_github
}

resource "azuread_application_federated_identity_credential" "test_002" {
  application_object_id = azuread_application.app_azcopy.object_id

  display_name = "test_002"
  subject      = "repo:Energinet-DataHub/dh3-environments:environment:test_002"

  audiences = [local.federated_identity_credential.audience_azuread]
  issuer    = local.federated_identity_credential.issuer_github
}

resource "azuread_application_federated_identity_credential" "preprod_001" {
  application_object_id = azuread_application.app_azcopy.object_id

  display_name = "preprod_001"
  subject      = "repo:Energinet-DataHub/dh3-environments:environment:preprod_001"

  audiences = [local.federated_identity_credential.audience_azuread]
  issuer    = local.federated_identity_credential.issuer_github
}

resource "azuread_application_federated_identity_credential" "prod_001" {
  application_object_id = azuread_application.app_azcopy.object_id

  display_name = "prod_001"
  subject      = "repo:Energinet-DataHub/dh3-environments:environment:prod_001"

  audiences = [local.federated_identity_credential.audience_azuread]
  issuer    = local.federated_identity_credential.issuer_github
}
