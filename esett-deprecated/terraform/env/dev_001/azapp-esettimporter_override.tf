module "app_importer" {
  app_settings = {
    # Region: Default Values
    WEBSITE_LOAD_USER_PROFILE           = 1,
    WEBSITE_ENABLE_SYNC_UPDATE_SITE     = true,
    WEBSITE_RUN_FROM_PACKAGE            = 1,
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = true,
    # EndRegion
    WEBSITE_LOAD_CERTIFICATES       = resource.azurerm_key_vault_certificate.dh2_certificate.thumbprint
    BLOB_FILES_ERROR_CONTAINER_NAME = local.blob_files_error_container.name
    BLOB_FILES_RAW_CONTAINER_NAME   = local.blob_files_raw_container.name
    RunIntervalSeconds              = "20"
    Endpoint                        = "https://b2b.te7.datahub.dk"
    NamespacePrefix                 = "ns0"
    NamespaceUri                    = "un:unece:260:data:EEM-DK_AggregatedMeteredDataTimeSeriesForNBS:v3"
    ResponseNamespaceUri            = "urn:www:datahub:dk:b2b:v01"
    CONNECTION_STRING_DATABASE      = "Server=${module.sqlsrv_esett.fully_qualified_domain_name};Database=${module.sqldb_esett.name};User Id=${local.sqlServerAdminName};Password=${random_password.sqlsrv_admin_password.result};"
  }
}
