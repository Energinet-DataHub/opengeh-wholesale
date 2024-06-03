resource "azuread_directory_role" "global_reader" {
  display_name = "Global Reader"
}

resource "azuread_directory_role" "global_admin" {
  display_name = "Global Administrator"
}

# locals {
#   platform_team_members = "comma_delimited_list_of_members_in_SEC-G-Datahub-PlatformDevelopersAzure"
# }

# resource "azuread_directory_role_assignment" "member_role_assignment" {
#   for_each = [for v in values(azuread_invitation.team_members) : v.user_id]

#   role_id             = azuread_directory_role.global_reader.template_id
#   principal_object_id = each.value
# }

# resource "azuread_invitation" "team_members" {
#   for_each = toset(split(",", local.platform_team_members))
#   user_email_address  = each.value
#   user_display_name   = "SEC-G-Datahub-PlatformDevelopersAzure member"
#   redirect_url        = "https://portal.azure.com"
# }


# Note: We tried extracting members of SEC-G-Datahub-PlatformDevelopersAzure and iterate over the members in the group - no luck with the code above

# The hardcoded resources below is a consequence of Terraform not being able to
# correctly replace role assignments if the list of platform team members change (i.e. when adding or removing
# a member from SEC-G-Datahub-PlatformDevelopersAzure. The reason is a known quirk when using count where values on a
# given index changes, Terraform will not replace the resource but instead try to update the existing resource, resulting in a conflicted state
# Note: count = 1 from resources below can be removed once overrides on legacy environments has been removed

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


#XRTNI
resource "azuread_directory_role_assignment" "xrtni" {
  count               = 1
  role_id             = azuread_directory_role.global_reader.template_id
  principal_object_id = azuread_invitation.xrtni[0].user_id
}

resource "azuread_invitation" "xrtni" {
  count              = 1
  user_email_address = "xrtni@energinet.dk"
  user_display_name  = "SEC-G-Datahub-PlatformDevelopersAzure member"
  redirect_url       = "https://portal.azure.com"
}


#XRLFR
resource "azuread_directory_role_assignment" "xrlfr" {
  count               = 1
  role_id             = azuread_directory_role.global_reader.template_id
  principal_object_id = azuread_invitation.xrlfr[0].user_id
}

resource "azuread_invitation" "xrlfr" {
  count              = 1
  user_email_address = "xrlfr@energinet.dk"
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
