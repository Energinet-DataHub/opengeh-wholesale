#XKBER
resource "azuread_directory_role_assignment" "xkber" {
  count = 0
}

resource "azuread_invitation" "xkber" {
  count = 0
}

#NHQ
resource "azuread_directory_role_assignment" "nhq" {
  count = 1
  role_id             = azuread_directory_role.global_admin.template_id
  principal_object_id = azuread_invitation.nhq[0].user_id
}

resource "azuread_invitation" "nhq" {
  count = 1
  user_email_address = "nhq@energinet.dk"
  user_display_name = "SEC-A-Greenforce-PlatformTeamAzure member"
  redirect_url       = "https://portal.azure.com"
}
#DBJ
resource "azuread_directory_role_assignment" "dbj" {
  count = 0
}

resource "azuread_invitation" "dbj" {
  count = 0
}

#XRTNI
resource "azuread_directory_role_assignment" "xrtni" {
  count = 0
}

resource "azuread_invitation" "xrtni" {
  count = 0
}

#XRLFR
resource "azuread_directory_role_assignment" "xrlfr" {
  count = 0
}

resource "azuread_invitation" "xrlfr" {
  count = 0
}
