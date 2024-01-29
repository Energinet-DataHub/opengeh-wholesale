module "func_dh2importer" {
  app_settings = merge(local.default_dh2importer_settings, {
    Endpoint                                    = "https://b2b.te7.datahub.dk"
    "FeatureManagement__EnableImporter"         = false
  })
}
