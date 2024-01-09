module "kv_shared" {
  access_policies = [
    {
      object_id = var.developers_security_group_object_id
      tenant_id = var.arm_tenant_id
      secret_permissions = [
        "Get",
        "List",
      ]
      key_permissions = [
        "Get",
        "List",
      ]
      certificate_permissions = [
        "Get",
        "List",
      ]
      storage_permissions = []
    }
  ]
}
