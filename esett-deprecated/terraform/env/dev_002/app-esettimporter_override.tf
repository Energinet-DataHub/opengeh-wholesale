module "app_importer" {
  app_settings = merge(local.default_importer_app_settings, {
    Endpoint                                    = "https://b2b.te7.datahub.dk"
    "FeatureManagement__EnableImporterApp"      = false
  })
}
