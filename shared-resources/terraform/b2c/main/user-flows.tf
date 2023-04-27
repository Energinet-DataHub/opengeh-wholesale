data "external" "user_flows" {
  program = ["pwsh", "${path.cwd}/scripts/AddUserFlows.ps1",
    var.b2c_tenant_id,
    var.b2c_client_id,
    var.b2c_client_secret,
    data.external.mitid_provider.result.mitId]
}
