locals {
  resources_suffix = "${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
  datahub2_certificate_thumbprint = resource.azurerm_app_service_certificate.dh2_certificate_app.thumbprint
}
