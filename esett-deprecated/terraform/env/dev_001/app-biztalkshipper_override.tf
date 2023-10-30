module "app_biztalkshipper" {
  app_settings = merge(local.default_biztalkshipper_app_settings, {
    "biztalk:RootUrl"       = "https://datahub.preproduction.biztalk.energinet.local"
  })
}
