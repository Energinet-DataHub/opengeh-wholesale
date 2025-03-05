module "app_ui" {
  app_settings = {
    "NEXT_PUBLIC_BFF_API_URL"          = "https://test002.b2b.datahub3.dk/sauron/api"
    "NEXT_PUBLIC_ENTRA_AUTH_CLIENT_ID" = azuread_application.sauron.client_id
    "NEXT_PUBLIC_ENTRA_AUTH_TENANT_ID" = data.azuread_client_config.current.tenant_id
  }
}

resource "azuread_application" "sauron" {
  single_page_application {
    redirect_uris = [
      "https://app-ui-${local.name_suffix}.azurewebsites.net/",
      "https://sauron.test002.datahub3.dk/",
      "https://localhost:3000/"
    ]
  }
}
