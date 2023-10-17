module "app_biztalkshipper" {
  app_settings = merge({
    "biztalk:senderCode"                = "45V0000000000601"
    "biztalk:receiverCode"              = "44V000000000028C"
  }, local.default_biztalkshipper_app_settings)
}
