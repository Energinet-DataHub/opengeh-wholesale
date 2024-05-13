# module "mssql_database_application_access" {
#   source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database-application-access?ref=v13"

#   sql_server_name = data.azurerm_mssql_server.mssqlsrv.name
#   database_name   = module.mssqldb_esett_exchange.name

#   application_hosts_names = [
#     module.func_entrypoint_exchange_event_receiver.name,
#     module.func_entrypoint_ecp_inbox.name,
#     module.func_entrypoint_ecp_outbox.name,
#     module.func_entrypoint_application_workers.name,
#     module.app_webapi.name,
#   ]

#   depends_on = [
#     module.func_entrypoint_exchange_event_receiver.name,
#     module.func_entrypoint_ecp_inbox.name,
#     module.func_entrypoint_ecp_outbox.name,
#     module.func_entrypoint_application_workers.name,
#     module.app_webapi.name,
#   ]
# }

resource "null_resource" "remove_db_logins" {
  # Sync account level user into the workspace
  provisioner "local-exec" {
    interpreter = ["pwsh", "-Command"]
    command     = <<EOF
$sqlServerName = "${data.azurerm_mssql_server.mssqlsrv.name}"
$databaseName = "${module.mssqldb_esett_exchange.name}"
$environmentShort = "${var.environment_short}"
$environmentInstance = "${var.environment_instance}"
if ([string]::IsNullOrWhiteSpace($sqlServerName)) {
    Write-Host "SQL server name null or whitespace"
    Return
}
if ([string]::IsNullOrWhiteSpace($databaseName)) {
    Write-Host "Database name null or whitespace"
    Return
}
if ([string]::IsNullOrWhiteSpace($environmentShort)) {
    Write-Host "EnvironmentShort was null or whitespace"
    Return
}
if ([string]::IsNullOrWhiteSpace($environmentInstance)) {
    Write-Host "EnvironmentInstance was null or whitespace"
    Return
}
try {
    # Does not require administration rights, unlike the Install-Module command
    Add-Type -AssemblyName "System.Data.SqlClient"
    $sqlServerName = "$sqlServerName.database.windows.net"
    $accessToken = $(az account get-access-token --resource=https://database.windows.net --query accessToken --output tsv)
    if ($LASTEXITCODE) {
        $errorMessage = "'az account get-access-token' returned a non-zero exit code"
        Write-Host $errorMessage
        throw $errorMessage
    }
    $connectionString = "Server=tcp:$sqlServerName,1433;Initial Catalog=$databaseName;"
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.AccessToken = $accessToken
    $connection.Open()
    $queryManagedIndentities = $connection.CreateCommand()
    $queryManagedIndentities.CommandText = "SELECT [name] FROM [sys].[database_principals] WHERE [name] like N'app-%-$environmentShort-we-$environmentInstance' OR [name] like N'func-%-$environmentShort-we-$environmentInstance'"
    $reader = $queryManagedIndentities.ExecuteReader()
    $identitites = @()
    if ($reader.HasRows) {
        while ($reader.Read()) {
            $identitites += $reader[0]
        }
    }
    $reader.Dispose()
    if (0 -ne $identitites.Length) {
        $queryDropUsers = $identitites | Join-String -Property { "DROP USER [$($_)]" } -Separator ";`n"
        $commandDropUsers = $connection.CreateCommand()
        $commandDropUsers.CommandText = $queryDropUsers
        $commandDropUsers.ExecuteNonQuery()
        Write-Host "Executed $queryDropUsers"
    }
    $connection.Close()
    $connection.Dispose()
}
catch {
    throw
}
    EOF
  }
}
