module "func_mp_import_df" {
  app_settings = merge(local.func_mp_import_df.app_settings, {
    FeatureManagement__ActorTestModeEnabled = true
  })
}
