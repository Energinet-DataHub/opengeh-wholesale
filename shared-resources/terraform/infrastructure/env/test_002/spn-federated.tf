resource "azuread_application_federated_identity_credential" "dh3_infrastructure_test_002" {
  application_id = data.azuread_application.current.id
  display_name   = "dh3-infrastructure-${local.environment_name}_${var.environment_instance}"
  subject        = "repo:Energinet-DataHub/dh3-infrastructure:environment:${local.environment_name}_${var.environment_instance}"
  audiences      = [local.federated_identity_credential.audience_azuread]
  issuer         = local.federated_identity_credential.issuer_github
}

resource "azuread_application_federated_identity_credential" "automation_test_002" {
  application_id = data.azuread_application.current.id
  display_name   = "dh3-automation-${local.environment_name}_${var.environment_instance}"
  subject        = "repo:Energinet-DataHub/dh3-automation:environment:${local.environment_name}_${var.environment_instance}"
  audiences      = [local.federated_identity_credential.audience_azuread]
  issuer         = local.federated_identity_credential.issuer_github
}

resource "azuread_application_federated_identity_credential" "modules_azureauth_test_002" {
  application_id = data.azuread_application.current.id
  display_name   = "geh-terraform-modules-azureauth-${local.environment_name}_${var.environment_instance}"
  subject        = "repo:Energinet-DataHub/geh-terraform-modules:environment:${local.environment_name}_${var.environment_instance}"
  audiences      = [local.federated_identity_credential.audience_azuread]
  issuer         = local.federated_identity_credential.issuer_github
}
