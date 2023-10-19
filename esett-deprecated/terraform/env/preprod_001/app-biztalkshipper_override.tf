module "app_biztalkshipper" {
  app_settings = merge({
    "biztalk:senderCode"    = "45V000000000056T"
    "biztalk:receiverCode"  = "44V000000000029A"
    "biztalk:RootUrl"       = "https://datahub.biztalk.energinet.local"
  }, local.default_biztalkshipper_app_settings)
}
