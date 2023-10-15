module "app_importer" {
  app_settings = {
    # EndRegion
    WEBSITE_LOAD_CERTIFICATES       = resource.azurerm_key_vault_certificate.dh2_certificate.thumbprint
    BLOB_FILES_ERROR_CONTAINER_NAME = local.blob_files_error_container.name
    BLOB_FILES_RAW_CONTAINER_NAME   = local.blob_files_raw_container.name
    RunIntervalSeconds              = "20"
    Endpoint                        = "https://b2b.datahub.dk"
    NamespacePrefix                 = "ns0"
    NamespaceUri                    = "un:unece:260:data:EEM-DK_AggregatedMeteredDataTimeSeriesForNBS:v3"
    ResponseNamespaceUri            = "urn:www:datahub:dk:b2b:v01"
    CONNECTION_STRING_DATABASE      = local.connection_string_database
  }
}
