module "storage_esett_documents" {
  role_assignments = [
    {
      principal_id         = var.developer_ad_group_object_id
      role_definition_name = "Storage Blob Data Reader"
    }
  ]
}
