module "func_biztalkshipper" {
  app_settings = merge(local.default_biztalkshipper_settings, {
    "biztalk:senderCode"                        = "45V0000000000601"
    "biztalk:receiverCode"                      = "44V000000000028C"
    "biztalk:RootUrl"                           = "https://datahub.preproduction.biztalk.energinet.local"
    "FeatureManagement__EnableBizTalkShipper"   = false
  })
}
