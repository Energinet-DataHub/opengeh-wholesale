// Used for dh3-github repository login and for accessing the current Terraform state on the `plan` job
resource "azuread_application_federated_identity_credential" "dh3_github_plan" {
  application_id = data.azuread_application.current.id
  display_name   = "dh3-github-plan"
  subject        = "repo:Energinet-DataHub/dh3-github:environment:plan"
  audiences      = [local.federated_identity_credential.audience_azuread]
  issuer         = local.federated_identity_credential.issuer_github
}

// Used for dh3-github repository login and for accessing the current Terraform state on the `apply` job
resource "azuread_application_federated_identity_credential" "dh3_github_apply" {
  application_id = data.azuread_application.current.id
  display_name   = "dh3-github-apply"
  subject        = "repo:Energinet-DataHub/dh3-github:environment:apply"
  audiences      = [local.federated_identity_credential.audience_azuread]
  issuer         = local.federated_identity_credential.issuer_github
}
