module "func_biztalkshipper" {
  app_settings = merge(local.default_biztalkshipper_settings, {
    "biztalk:senderCode"                        = "45V000000000056T"
    "biztalk:receiverCode"                      = "44V000000000029A"
    "biztalk:RootUrl"                           = "https://datahub.biztalk.energinet.local"
    "FeatureManagement__EnableBizTalkShipper"   = true
  })
}
