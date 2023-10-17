module "app_importer" {
  app_settings = merge({
    Endpoint                        = "https://b2b.datahub.dk"
  }, local.default_importer_app_settings)
}
