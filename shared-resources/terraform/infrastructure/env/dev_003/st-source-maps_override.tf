# Temporary fix as the backup vault policy is broken and prevents deployment
module "st_source_maps" {
  blob_storage_backup_policy = null
}
