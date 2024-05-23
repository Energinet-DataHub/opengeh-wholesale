locals {
  federated_identity_credential = {
    audience_azuread = "api://AzureADTokenExchange"
    issuer_github    = "https://token.actions.githubusercontent.com"
  }

  get_environment_name = {
    "d" = "dev"
    "t" = "test"
    "b" = "preprod"
    "p" = "prod"
  }
  environment_name = local.get_environment_name[var.environment_short]
}

# Needed to get the application id to assign the federated credential to
data "azuread_application" "current" {
  client_id = data.azurerm_client_config.current.client_id
}

# Assign federated credentials here
# NOTE: dh3-environments must be created manually

resource "azuread_application_federated_identity_credential" "recovery" {
  application_id = data.azuread_application.current.id

  display_name = "dh3-recovery-${local.environment_name}-${var.environment_instance}"
  subject      = "repo:Energinet-DataHub/dh3-recovery:environment:${local.environment_name}_${var.environment_instance}"

  audiences = [local.federated_identity_credential.audience_azuread]
  issuer    = local.federated_identity_credential.issuer_github

  depends_on = [ null_resource.delete_fed ]
}


# TEMPORARY FIX: delete the current federated credential by using az cli
resource "null_resource" "delete_fed" {
    provisioner "local-exec" {
        command = "az ad app federated-credential delete --federated-credential-id ${local.environment_name}_${var.environment_instance}-dh3-recovery --id '${data.azuread_application.current.object_id}'"
    }
}

resource "null_resource" "delete_federated_automation" {
    provisioner "local-exec" {
        command = "az ad app federated-credential delete --federated-credential-id ${local.environment_name}_${var.environment_instance}-automation --id '${data.azuread_application.current.object_id}'"
    }
}

