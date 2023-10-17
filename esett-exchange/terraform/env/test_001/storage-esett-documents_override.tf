module "storage_esett_documents" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account?ref=v13"

  role_assignments = [
    {
      principal_id         = var.developer_ad_group_object_id
      role_definition_name = "Storage Blob Data Reader"
    }
  ]
}
