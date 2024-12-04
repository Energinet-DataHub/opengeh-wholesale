# SEC-A-Datahub-Prod-001-Contributor-Controlplane

data "azuread_group" "prod001_contributor_controlplane" {
  display_name     = "SEC-A-Datahub-Prod-001-Contributor-Controlplane"
  security_enabled = true
}

import {
  to = azuread_privileged_access_group_eligibility_schedule.prod001_contributor_controlplane_owner_platform_developers
  id = "2186231d-645b-45df-a4ef-048529508f62_owner_2ecff52f-5cce-4a2c-8434-1da270c13755"
  # Import statement ids for the eligibleschedules are fetched with the following command
  # az rest --method GET --url "https://graph.microsoft.com/v1.0/identityGovernance/privilegedAccess/group/eligibilitySchedules?`$filter=groupId eq '{GROUP_ID}'"
}
resource "azuread_privileged_access_group_eligibility_schedule" "prod001_contributor_controlplane_owner_platform_developers" {
  group_id             = data.azuread_group.prod001_contributor_controlplane.object_id
  principal_id         = data.azuread_group.platform_developers.object_id
  assignment_type      = "owner"
  permanent_assignment = true
}

import {
  to = azuread_privileged_access_group_eligibility_schedule.prod001_contributor_controlplane_member_platform_developers
  id = "2186231d-645b-45df-a4ef-048529508f62_member_31304cba-03cb-48a7-8207-74f2cfc5c094"
}
resource "azuread_privileged_access_group_eligibility_schedule" "prod001_contributor_controlplane_member_platform_developers" {
  group_id             = data.azuread_group.prod001_contributor_controlplane.object_id
  principal_id         = data.azuread_group.platform_developers.object_id
  assignment_type      = "member"
  permanent_assignment = true
}

# SEC-A-Datahub-Prod-001-Contributor-Dataplane

data "azuread_group" "prod001_contributor_dataplane" {
  display_name     = "SEC-A-Datahub-Prod-001-Contributor-Dataplane"
  security_enabled = true
}

import {
  to = azuread_privileged_access_group_eligibility_schedule.prod001_contributor_dataplane_owner_platform_developers
  id = "d587a895-9aa6-47e0-925c-e96550a88f9e_owner_174dcdad-7db0-470e-941a-ade70db12efd"
}
resource "azuread_privileged_access_group_eligibility_schedule" "prod001_contributor_dataplane_owner_platform_developers" {
  group_id             = data.azuread_group.prod001_contributor_dataplane.object_id
  principal_id         = data.azuread_group.platform_developers.object_id
  assignment_type      = "owner"
  permanent_assignment = true
}

import {
  to = azuread_privileged_access_group_eligibility_schedule.prod001_contributor_dataplane_member_pim_requesters
  id = "d587a895-9aa6-47e0-925c-e96550a88f9e_member_3bd22ebc-951a-4a4e-b99d-a72d938524d0"
}
resource "azuread_privileged_access_group_eligibility_schedule" "prod001_contributor_dataplane_member_pim_requesters" {
  group_id             = data.azuread_group.prod001_contributor_dataplane.object_id
  principal_id         = data.azuread_group.pim_requesters.object_id
  assignment_type      = "member"
  permanent_assignment = true
}

# SEC-A-Datahub-Prod-001-Reader

data "azuread_group" "prod001_reader" {
  display_name     = "SEC-A-Datahub-Prod-001-Reader"
  security_enabled = true
}

import {
  to = azuread_privileged_access_group_eligibility_schedule.prod001_reader_owner_platform_developers
  id = "492dce8f-f5e4-4580-ac3e-df0241670f8c_owner_f4dd81cf-312c-4b06-945d-20a6eba132f2"
}
resource "azuread_privileged_access_group_eligibility_schedule" "prod001_reader_owner_platform_developers" {
  group_id             = data.azuread_group.prod001_reader.object_id
  principal_id         = data.azuread_group.platform_developers.object_id
  assignment_type      = "owner"
  permanent_assignment = true
}

import {
  to = azuread_privileged_access_group_eligibility_schedule.prod001_reader_member_pim_requesters
  id = "492dce8f-f5e4-4580-ac3e-df0241670f8c_member_e285dada-c691-4ae6-aaa4-d9e6dc419c68"
}
resource "azuread_privileged_access_group_eligibility_schedule" "prod001_reader_member_pim_requesters" {
  group_id             = data.azuread_group.prod001_reader.object_id
  principal_id         = data.azuread_group.pim_requesters.object_id
  assignment_type      = "member"
  permanent_assignment = true
}
