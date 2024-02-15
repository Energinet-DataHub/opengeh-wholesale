module "func_receiver" {
  app_settings = merge(local.default_func_api_app_settings, {
    # Feature flags
    FeatureManagement__UseMonthlyAmountPerChargeResultProduced = "true"
    # Endregion: Feature flags
  })
}
