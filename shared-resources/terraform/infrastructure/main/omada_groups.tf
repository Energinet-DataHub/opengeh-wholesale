# OMADA groups
data "azuread_group" "platform_developers" {
  display_name     = "SEC-G-Datahub-PlatformDevelopersAzure"
  security_enabled = true
}

data "azuread_group" "developers" {
  display_name     = "SEC-G-Datahub-DevelopersAzure"
  security_enabled = true
}

data "azuread_group" "pim_requesters" {
  display_name     = "SEC-G-Datahub-Pim-Requesters"
  security_enabled = true
}

data "azuread_group" "pim_approvers" {
  display_name     = "SEC-G-Datahub-Pim-Approvers"
  security_enabled = true
}
