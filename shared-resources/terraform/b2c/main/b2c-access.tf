resource "azuread_directory_role" "global_reader" {
  display_name = "Global Reader"
}

resource "azuread_directory_role" "global_admin" {
  display_name = "Global Administrator"
}

############################################################################################################
# Product Owner for Spectrum and Product Owner for Outlaws have access to the 'Tenant Creator' role via PIM
# This allows them to create a new B2C tenant. The person creating the tenant become Global Administrator on the tenant
#
# However, to ensure that both the Productowner for Outlaws and Productowner for Spectrum are Global Admin on
# all B2C instances always, this is explicitly configured in the Terraform resources below. This mitigates the risk
# of locking ourselves out of the B2C tenant if our deployment serviceprincipal is invalidated without anybody
# being able to log in and recreate it
#
# The ISO control IK 8.3 ensures that privileged admin-access is kept up to date on a regular interval
############################################################################################################

#MRK
resource "azuread_directory_role_assignment" "mrk" {
  count               = 1
  role_id             = azuread_directory_role.global_admin.template_id
  principal_object_id = azuread_invitation.mrk[0].user_id
}

resource "azuread_invitation" "mrk" {
  count              = 1
  user_email_address = "mrk@energinet.dk"
  user_display_name  = "Morten Kjær Nicolaysen"
  redirect_url       = "https://portal.azure.com"
}

resource "azuread_directory_role_assignment" "kos" {
  count               = 1
  role_id             = azuread_directory_role.global_admin.template_id
  principal_object_id = azuread_invitation.kos[0].user_id
}

resource "azuread_invitation" "kos" {
  count              = 1
  user_email_address = "kos@energinet.dk"
  user_display_name  = "Kristoffer Højrup Moos"
  redirect_url       = "https://portal.azure.com"
}


############## Other users are configured below this line ##################

#XKBER
resource "azuread_directory_role_assignment" "xkber" {
  count               = 1
  role_id             = azuread_directory_role.global_reader.template_id
  principal_object_id = azuread_invitation.xkber[0].user_id
}

resource "azuread_invitation" "xkber" {
  count              = 1
  user_email_address = "xkber@energinet.dk"
  user_display_name  = "SEC-G-Datahub-PlatformDevelopersAzure member"
  redirect_url       = "https://portal.azure.com"
}

#NHQ
resource "azuread_directory_role_assignment" "nhq" {
  count               = 1
  role_id             = azuread_directory_role.global_reader.template_id
  principal_object_id = azuread_invitation.nhq[0].user_id
}

resource "azuread_invitation" "nhq" {
  count              = 1
  user_email_address = "nhq@energinet.dk"
  user_display_name  = "SEC-G-Datahub-PlatformDevelopersAzure member"
  redirect_url       = "https://portal.azure.com"
}


#DBJ
resource "azuread_directory_role_assignment" "dbj" {
  count               = 1
  role_id             = azuread_directory_role.global_reader.template_id
  principal_object_id = azuread_invitation.dbj[0].user_id
}

resource "azuread_invitation" "dbj" {
  count              = 1
  user_email_address = "dbj@energinet.dk"
  user_display_name  = "SEC-G-Datahub-PlatformDevelopersAzure member"
  redirect_url       = "https://portal.azure.com"
}

#LANNI
resource "azuread_directory_role_assignment" "lanni" {
  count               = 1
  role_id             = azuread_directory_role.global_reader.template_id
  principal_object_id = azuread_invitation.lanni[0].user_id
}

resource "azuread_invitation" "lanni" {
  count              = 1
  user_email_address = "LANNI@energinet.dk"
  user_display_name  = "SEC-G-Datahub-PlatformDevelopersAzure member"
  redirect_url       = "https://portal.azure.com"
}
