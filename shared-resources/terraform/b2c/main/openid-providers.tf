data "external" "mitid_provider" {
  program = ["pwsh", "${path.cwd}/scripts/AddMitIdProvider.ps1",
    var.b2c_tenant_id,
    var.b2c_client_id,
    var.b2c_client_secret,
    var.mitid_client_id,
    var.mitid_client_secret]
}
