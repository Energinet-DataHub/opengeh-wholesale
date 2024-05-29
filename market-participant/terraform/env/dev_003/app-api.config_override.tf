module "app_api" {
  app_settings = merge(local.app_api.app_settings, {
    ENFORCE_2FA = "false"
  })
}
