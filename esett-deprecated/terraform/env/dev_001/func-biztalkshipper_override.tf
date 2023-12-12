module "func_biztalkshipper" {
  app_settings = merge(local.default_biztalkshipper_settings, {
    "biztalk:senderCode"                        = "N/A"
    "biztalk:receiverCode"                      = "N/A"
    "biztalk:RootUrl"                           = "https://example.com"
    "FeatureManagement__EnableBizTalkShipper"   = false
  })
}
