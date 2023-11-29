resource "azuread_application" "app_datalake_contributor" {
  display_name = "sp-datalakecontributor-${local.resources_suffix}"
}
