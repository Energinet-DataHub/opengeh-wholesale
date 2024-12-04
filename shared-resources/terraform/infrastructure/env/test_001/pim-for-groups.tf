# SEC-A-Datahub-Test-001-Contributor-Controlplane

data "azuread_group" "test001_contributor_controlplane" {
  display_name     = "SEC-A-Datahub-Test-001-Contributor-Controlplane"
  security_enabled = true
}

import {
  to = azuread_privileged_access_group_eligibility_schedule.test001_contributor_controlplane_owner_platform_developers
  id = "5c21df4e-f2ed-457a-ae86-6b935564761f_owner_ad52e5ef-7dc7-4b18-b84a-65336b5e94e6"
  # Import statement ids for the eligibleschedules are fetched with the following command
  # az rest --method GET --url "https://graph.microsoft.com/v1.0/identityGovernance/privilegedAccess/group/eligibilitySchedules?`$filter=groupId eq '{GROUP_ID}'"
}
resource "azuread_privileged_access_group_eligibility_schedule" "test001_contributor_controlplane_owner_platform_developers" {
  group_id             = data.azuread_group.test001_contributor_controlplane.object_id
  principal_id         = data.azuread_group.platform_developers.object_id
  assignment_type      = "owner"
  permanent_assignment = true
}

import {
  to = azuread_privileged_access_group_eligibility_schedule.test001_contributor_controlplane_member_platform_developers
  id = "5c21df4e-f2ed-457a-ae86-6b935564761f_member_a81a6a06-afca-4f0f-bb3b-10ef9e11ecef"
}
resource "azuread_privileged_access_group_eligibility_schedule" "test001_contributor_controlplane_member_platform_developers" {
  group_id             = data.azuread_group.test001_contributor_controlplane.object_id
  principal_id         = data.azuread_group.platform_developers.object_id
  assignment_type      = "member"
  permanent_assignment = true
}

# SEC-A-Datahub-Test-001-Contributor-Dataplane

data "azuread_group" "test001_contributor_dataplane" {
  display_name     = "SEC-A-Datahub-Test-001-Contributor-Dataplane"
  security_enabled = true
}

import {
  to = azuread_privileged_access_group_eligibility_schedule.test001_contributor_dataplane_owner_platform_developers
  id = "0a507481-fae8-4498-b3ce-1b4c1018e6d6_owner_2bd7f40c-7093-43b4-8389-91adce2f0359"
}
resource "azuread_privileged_access_group_eligibility_schedule" "test001_contributor_dataplane_owner_platform_developers" {
  group_id             = data.azuread_group.test001_contributor_dataplane.object_id
  principal_id         = data.azuread_group.platform_developers.object_id
  assignment_type      = "owner"
  permanent_assignment = true
}

import {
  to = azuread_privileged_access_group_eligibility_schedule.test001_contributor_dataplane_member_pim_requesters
  id = "0a507481-fae8-4498-b3ce-1b4c1018e6d6_member_aaea3537-47cd-4a5c-8fd5-bda54286435f"
}
resource "azuread_privileged_access_group_eligibility_schedule" "test001_contributor_dataplane_member_pim_requesters" {
  group_id             = data.azuread_group.test001_contributor_dataplane.object_id
  principal_id         = data.azuread_group.pim_requesters.object_id
  assignment_type      = "member"
  permanent_assignment = true
}
