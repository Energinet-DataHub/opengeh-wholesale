resource "azuread_application_federated_identity_credential" "dh_systemtests_prod_001" {
  application_id = data.azuread_application.current.id
  display_name   = "dh-systemtests-${local.environment_name}_${var.environment_instance}"
  subject        = "repo:Energinet-DataHub/dh-systemtests:environment:${local.environment_name}_${var.environment_instance}"
  audiences      = [local.federated_identity_credential.audience_azuread]
  issuer         = local.federated_identity_credential.issuer_github
}
