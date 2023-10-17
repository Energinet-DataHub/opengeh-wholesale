module "app_biztalkshipper" {
  app_settings = merge({
    "biztalk:senderCode"                = "45V000-ENERGINET"
    "biztalk:receiverCode"              = "45V000-ENERGINET"
  }, local.default_biztalkshipper_app_settings)
}
