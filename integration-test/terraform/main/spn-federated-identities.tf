#
# Adds federated credentials for the service principle that
# we use in CI workflows to execute tests. This allows tests
# to manage (create/delete) resources as needed.
#

locals {
  fic_audience_azuread = "api://AzureADTokenExchange"
  fic_issuer_github    = "https://token.actions.githubusercontent.com"
}

resource "azuread_application_federated_identity_credential" "fic_core" {
  application_object_id = azuread_application.app_ci.object_id

  display_name = "geh-core-azureauth"
  subject      = "repo:Energinet-DataHub/geh-core:environment:AzureAuth"

  audiences = [local.fic_audience_azuread]
  issuer    = local.fic_issuer_github
}

resource "azuread_application_federated_identity_credential" "fic_market_participant" {
  application_object_id = azuread_application.app_ci.object_id

  display_name = "geh-market-participant-azureauth"
  subject      = "repo:Energinet-DataHub/geh-market-participant:environment:AzureAuth"

  audiences = [local.fic_audience_azuread]
  issuer    = local.fic_issuer_github
}
