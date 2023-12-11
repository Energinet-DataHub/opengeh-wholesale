resource "azuread_application" "frontend_app" {
  single_page_application {
    redirect_uris = ["https://${local.frontend_url}/", "https://localhost/"]
  }

  # Implicit flow enabled to allow for acceptance tests in certain environments.
  fallback_public_client_enabled = true

  web {
    implicit_grant {
      access_token_issuance_enabled = true
      id_token_issuance_enabled     = true
    }
  }
}
