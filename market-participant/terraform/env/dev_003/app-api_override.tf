module "app_api" {
  app_settings = merge(local.default_api_app_settings, {
    ENFORCE_2FA = "false"
  })
}
