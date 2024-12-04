# SEC-A-Datahub-Dev-001-Contributor-Controlplane

data "azuread_group" "dev001_contributor_controlplane" {
  display_name     = "SEC-A-Datahub-Dev-001-Contributor-Controlplane"
  security_enabled = true
}

import {
  to = azuread_privileged_access_group_eligibility_schedule.dev001_contributor_controlplane_owner_platform_developers
  id = "cb9db503-22e0-44e9-897e-d4c03c1beb85_owner_a526e71f-d06a-4189-9cae-151dd22805a5"
  # Import statement ids for the eligibleschedules are fetched with the following command
  # az rest --method GET --url "https://graph.microsoft.com/v1.0/identityGovernance/privilegedAccess/group/eligibilitySchedules?`$filter=groupId eq '{GROUP_ID}'"
}
resource "azuread_privileged_access_group_eligibility_schedule" "dev001_contributor_controlplane_owner_platform_developers" {
  group_id             = data.azuread_group.dev001_contributor_controlplane.object_id
  principal_id         = data.azuread_group.platform_developers.object_id
  assignment_type      = "owner"
  permanent_assignment = true
}

import {
  to = azuread_privileged_access_group_eligibility_schedule.dev001_contributor_controlplane_member_platform_developers
  id = "cb9db503-22e0-44e9-897e-d4c03c1beb85_member_f649440a-180a-4c5e-9c1a-e3914a8c5a59"
}
resource "azuread_privileged_access_group_eligibility_schedule" "dev001_contributor_controlplane_member_platform_developers" {
  group_id             = data.azuread_group.dev001_contributor_controlplane.object_id
  principal_id         = data.azuread_group.platform_developers.object_id
  assignment_type      = "member"
  permanent_assignment = true
}

# SEC-A-Datahub-Dev-001-Contributor-Dataplane

data "azuread_group" "dev001_contributor_dataplane" {
  display_name     = "SEC-A-Datahub-Dev-001-Contributor-Dataplane"
  security_enabled = true
}

import {
  to = azuread_privileged_access_group_eligibility_schedule.dev001_contributor_dataplane_owner_platform_developers
  id = "c3d66bf6-7c12-443a-a5b1-5896473ee58e_owner_180ed0ab-514b-4b7c-b731-4893ed10ff79"
}
resource "azuread_privileged_access_group_eligibility_schedule" "dev001_contributor_dataplane_owner_platform_developers" {
  group_id             = data.azuread_group.dev001_contributor_dataplane.object_id
  principal_id         = data.azuread_group.platform_developers.object_id
  assignment_type      = "owner"
  permanent_assignment = true
}

import {
  to = azuread_privileged_access_group_eligibility_schedule.dev001_contributor_dataplane_member_pim_requesters
  id = "c3d66bf6-7c12-443a-a5b1-5896473ee58e_member_da5f0113-9a0a-4b76-85cb-486fcb25b32f"
}
resource "azuread_privileged_access_group_eligibility_schedule" "dev001_contributor_dataplane_member_pim_requesters" {
  group_id             = data.azuread_group.dev001_contributor_dataplane.object_id
  principal_id         = data.azuread_group.pim_requesters.object_id
  assignment_type      = "member"
  permanent_assignment = true
}
