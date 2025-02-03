data "azuread_application" "spn" {
  client_id = var.b2c_client_id
}

module "spn_b2c_rotating_secret" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/entra-application-rotating-secret?ref=entra-application-rotating-secret_1.0.1"

  application_id = data.azuread_application.spn.id
}
