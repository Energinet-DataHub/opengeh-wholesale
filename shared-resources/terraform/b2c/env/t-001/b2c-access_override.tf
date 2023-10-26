#Don't do anything for te

resource "azuread_directory_role_assignment" "platformteam_member" {
  count = 0
}

resource "azuread_invitation" "platformteam_member" {
  count = 0
}
