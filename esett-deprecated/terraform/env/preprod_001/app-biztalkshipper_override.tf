module "app_biztalkshipper" {
  app_settings = merge({
    "biztalk:RootUrl"       = "https://datahub.preproduction.biztalk.energinet.local"
  }, local.default_biztalkshipper_app_settings)
}
