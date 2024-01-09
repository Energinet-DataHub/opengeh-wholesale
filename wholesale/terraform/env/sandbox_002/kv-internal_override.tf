module "kv_internal" {
  access_policies = [
    {
      object_id = var.developers_security_group_object_id
      tenant_id = var.tenant_id
      secret_permissions = [
        "Get",
        "List",
      ]
      key_permissions         = []
      certificate_permissions = []
      storage_permissions     = []
    }
  ]
}
