# SEC-A-Datahub-PreProd-001-Contributor-Controlplane

data "azuread_group" "preprod001_contributor_controlplane" {
  display_name     = "SEC-A-Datahub-PreProd-001-Contributor-Controlplane"
  security_enabled = true
}

import {
  to = azuread_privileged_access_group_eligibility_schedule.preprod001_contributor_controlplane_owner_platform_developers
  id = "69eec093-718d-4432-b5fd-010d7c77dfff_owner_7149b750-31c8-412d-96e1-69f6c1f60f04"
  # Import statement ids for the eligibleschedules are fetched with the following command
  # az rest --method GET --url "https://graph.microsoft.com/v1.0/identityGovernance/privilegedAccess/group/eligibilitySchedules?`$filter=groupId eq '{GROUP_ID}'"
}
resource "azuread_privileged_access_group_eligibility_schedule" "preprod001_contributor_controlplane_owner_platform_developers" {
  group_id             = data.azuread_group.preprod001_contributor_controlplane.object_id
  principal_id         = data.azuread_group.platform_developers.object_id
  assignment_type      = "owner"
  permanent_assignment = true
}

import {
  to = azuread_privileged_access_group_eligibility_schedule.preprod001_contributor_controlplane_member_platform_developers
  id = "69eec093-718d-4432-b5fd-010d7c77dfff_member_8e88a4c8-a66e-4d4f-85b0-f75afc163bc1"
}
resource "azuread_privileged_access_group_eligibility_schedule" "preprod001_contributor_controlplane_member_platform_developers" {
  group_id             = data.azuread_group.preprod001_contributor_controlplane.object_id
  principal_id         = data.azuread_group.platform_developers.object_id
  assignment_type      = "member"
  permanent_assignment = true
}

# SEC-A-Datahub-PreProd-001-Contributor-Dataplane

data "azuread_group" "preprod001_contributor_dataplane" {
  display_name     = "SEC-A-Datahub-PreProd-001-Contributor-Dataplane"
  security_enabled = true
}

import {
  to = azuread_privileged_access_group_eligibility_schedule.preprod001_contributor_dataplane_owner_platform_developers
  id = "1ca32382-c8b3-4e1d-8e6c-cbfc13c3e137_owner_9094dc1d-f3d1-4512-b6da-d1b77307cbb6"
}
resource "azuread_privileged_access_group_eligibility_schedule" "preprod001_contributor_dataplane_owner_platform_developers" {
  group_id             = data.azuread_group.preprod001_contributor_dataplane.object_id
  principal_id         = data.azuread_group.platform_developers.object_id
  assignment_type      = "owner"
  permanent_assignment = true
}

import {
  to = azuread_privileged_access_group_eligibility_schedule.preprod001_contributor_dataplane_member_pim_requesters
  id = "1ca32382-c8b3-4e1d-8e6c-cbfc13c3e137_member_63605a7e-1109-4a12-bc2e-c2f4b8929bdb"
}
resource "azuread_privileged_access_group_eligibility_schedule" "preprod001_contributor_dataplane_member_pim_requesters" {
  group_id             = data.azuread_group.preprod001_contributor_dataplane.object_id
  principal_id         = data.azuread_group.pim_requesters.object_id
  assignment_type      = "member"
  permanent_assignment = true
}

# SEC-A-Datahub-PreProd-001-Reader

data "azuread_group" "preprod001_reader" {
  display_name     = "SEC-A-Datahub-PreProd-001-Reader"
  security_enabled = true
}

import {
  to = azuread_privileged_access_group_eligibility_schedule.preprod001_reader_owner_platform_developers
  id = "928d70d2-5b98-4497-9a12-188246cf363a_owner_8fe1ae38-798d-4136-bd35-90f188610be1"
}
resource "azuread_privileged_access_group_eligibility_schedule" "preprod001_reader_owner_platform_developers" {
  group_id             = data.azuread_group.preprod001_reader.object_id
  principal_id         = data.azuread_group.platform_developers.object_id
  assignment_type      = "owner"
  permanent_assignment = true
}

import {
  to = azuread_privileged_access_group_eligibility_schedule.preprod001_reader_member_pim_requesters
  id = "928d70d2-5b98-4497-9a12-188246cf363a_member_c58f9d61-cf2b-4cce-a6d8-8238514c7a11"
}
resource "azuread_privileged_access_group_eligibility_schedule" "preprod001_reader_member_pim_requesters" {
  group_id             = data.azuread_group.preprod001_reader.object_id
  principal_id         = data.azuread_group.pim_requesters.object_id
  assignment_type      = "member"
  permanent_assignment = true
}
