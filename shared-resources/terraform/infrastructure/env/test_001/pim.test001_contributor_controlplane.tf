data "azuread_group" "test001_contributor_controlplane" {
  display_name     = "SEC-A-Datahub-Test-001-Contributor-Controlplane"
  security_enabled = true
}

resource "azuread_group_role_management_policy" "owner_test001_contributor_controlplane" {
  group_id = data.azuread_group.test001_contributor_controlplane.object_id
  role_id  = "owner"

  activation_rules {
    maximum_duration      = "PT8H"
    require_justification = true

    require_approval = true
    approval_stage {
      primary_approver {
        object_id = data.azuread_group.pim_approvers.object_id
        type      = "groupMembers"
      }
    }
  }

  eligible_assignment_rules {
    expiration_required = false
  }
}

resource "azuread_group_role_management_policy" "member_test001_contributor_controlplane" {
  group_id = data.azuread_group.test001_contributor_controlplane.object_id
  role_id  = "member"

  activation_rules {
    maximum_duration      = "PT8H"
    require_justification = true

    require_approval = false
    // approval_stage should be omitted when require_approval is false but due to a bug in the provider, it is required to be present with a dummy value to avoid constant drift
    // https://github.com/hashicorp/terraform-provider-azuread/issues/1398
    approval_stage {
      primary_approver {
        object_id = data.azuread_group.pim_approvers.object_id
        type      = "groupMembers"
      }
    }
  }

  eligible_assignment_rules {
    expiration_required = false
  }
}

resource "azuread_privileged_access_group_eligibility_schedule" "test001_contributor_controlplane_member_platform_developers" {
  group_id             = data.azuread_group.test001_contributor_controlplane.object_id
  principal_id         = data.azuread_group.platform_developers.object_id
  assignment_type      = "member"
  permanent_assignment = true

  depends_on = [azuread_group_role_management_policy.member_test001_contributor_controlplane]
}
