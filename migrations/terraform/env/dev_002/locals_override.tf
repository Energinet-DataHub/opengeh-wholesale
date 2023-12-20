locals {
  datahub2_certificate_thumbprint = resource.azurerm_app_service_certificate.dh2_certificate_app.thumbprint
}
