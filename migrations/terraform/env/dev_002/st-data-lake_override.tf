module "st_migrations" {
  source           = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=v13"
  prevent_deletion = false
}
