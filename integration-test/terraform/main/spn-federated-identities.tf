#
# Adds federated credentials for the service principle that
# we use in CI workflows to execute tests. This allows tests
# to manage (create/delete) resources as needed.
#

locals {
  federated_identity_credential = {
    audience_azuread = "api://AzureADTokenExchange"
    issuer_github    = "https://token.actions.githubusercontent.com"
  }
}

resource "azuread_application_federated_identity_credential" "geh_core" {
  application_object_id = azuread_application.app_ci.object_id

  display_name = "geh-core-azureauth"
  subject      = "repo:Energinet-DataHub/geh-core:environment:AzureAuth"

  audiences = [local.federated_identity_credential.audience_azuread]
  issuer    = local.federated_identity_credential.issuer_github
}

resource "azuread_application_federated_identity_credential" "geh_market_participant" {
  application_object_id = azuread_application.app_ci.object_id

  display_name = "geh-market-participant-azureauth"
  subject      = "repo:Energinet-DataHub/geh-market-participant:environment:AzureAuth"

  audiences = [local.federated_identity_credential.audience_azuread]
  issuer    = local.federated_identity_credential.issuer_github
}

resource "azuread_application_federated_identity_credential" "opengeh_wholesale" {
  application_object_id = azuread_application.app_ci.object_id

  display_name = "opengeh-wholesale-azureauth"
  subject      = "repo:Energinet-DataHub/opengeh-wholesale:environment:AzureAuth"

  audiences = [local.federated_identity_credential.audience_azuread]
  issuer    = local.federated_identity_credential.issuer_github
}

resource "azuread_application_federated_identity_credential" "opengeh_edi" {
  application_object_id = azuread_application.app_ci.object_id

  display_name = "opengeh-edi-azureauth"
  subject      = "repo:Energinet-DataHub/opengeh-edi:environment:AzureAuth"

  audiences = [local.federated_identity_credential.audience_azuread]
  issuer    = local.federated_identity_credential.issuer_github
}

resource "azuread_application_federated_identity_credential" "opengeh_esett_exchange" {
  application_object_id = azuread_application.app_ci.object_id

  display_name = "opengeh-esett-exchange-azureauth"
  subject      = "repo:Energinet-DataHub/opengeh-esett-exchange:environment:AzureAuth"

  audiences = [local.federated_identity_credential.audience_azuread]
  issuer    = local.federated_identity_credential.issuer_github
}

resource "azuread_application_federated_identity_credential" "opengeh_migration" {
  application_object_id = azuread_application.app_ci.object_id

  display_name = "opengeh-migration-azureauth"
  subject      = "repo:Energinet-DataHub/opengeh-migration:environment:AzureAuth"

  audiences = [local.federated_identity_credential.audience_azuread]
  issuer    = local.federated_identity_credential.issuer_github
}

resource "azuread_application_federated_identity_credential" "greenforce_frontend" {
  application_object_id = azuread_application.app_ci.object_id

  display_name = "greenforce-frontend-azureauth"
  subject      = "repo:Energinet-DataHub/greenforce-frontend:environment:AzureAuth"

  audiences = [local.federated_identity_credential.audience_azuread]
  issuer    = local.federated_identity_credential.issuer_github
}

resource "azuread_application_federated_identity_credential" "esett_deprecated" {
  application_object_id = azuread_application.app_ci.object_id

  display_name = "esett-deprecated-azureauth"
  subject      = "repo:Energinet-DataHub/esett-deprecated:environment:AzureAuth"

  audiences = [local.federated_identity_credential.audience_azuread]
  issuer    = local.federated_identity_credential.issuer_github
}

