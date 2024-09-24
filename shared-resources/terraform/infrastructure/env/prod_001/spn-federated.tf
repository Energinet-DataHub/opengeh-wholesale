// Used for dh-githubautomation repository login and for accessing the current Terraform state on the `apply` job
resource "azuread_application_federated_identity_credential" "dh_githubautomation_apply" {
  application_id = data.azuread_application.current.id
  display_name   = "dh-githubautomation-apply"
  subject        = "repo:Energinet-DataHub/dh-githubautomation:environment:apply"
  audiences      = [local.federated_identity_credential.audience_azuread]
  issuer         = local.federated_identity_credential.issuer_github
}
