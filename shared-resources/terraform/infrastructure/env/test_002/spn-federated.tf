
locals {
  federated_identity_credential = {
    audience_azuread = "api://AzureADTokenExchange"
    issuer_github    = "https://token.actions.githubusercontent.com"
  }
}

resource "azuread_application_federated_identity_credential" "dh3_infrastructure_test_002" {
  application_id  = "/applications/8ca9c5dc-ba2f-4cc7-8abf-ed4198b43b74"
  display_name    = "dh3-infrastructure-test_002"
  subject         = "repo:Energinet-DataHub/dh3-infrastructure:environment:test_002"
  audiences       = [local.federated_identity_credential.audience_azuread]
  issuer          = local.federated_identity_credential.issuer_github
}
