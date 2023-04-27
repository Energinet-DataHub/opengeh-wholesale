output "backend_b2b_app_id" {
  description = "The Application ID of the backend B2B app registration."
  value       = azuread_application.backend_b2b_app.application_id
  sensitive   = false
}

output "backend_b2b_app_obj_id" {
  description = "The Object ID of the backend B2B app registration."
  value       = azuread_application.backend_b2b_app.object_id
  sensitive   = false
}

output "backend_b2b_app_sp_id" {
  description = "The Object ID of the service principal for backend B2B app registration."
  value       = azuread_service_principal.backend_b2b_app_sp.object_id
  sensitive   = false
}

output "backend_bff_app_id" {
  description = "The Application ID of the backend BFF app registration."
  value       = azuread_application.backend_bff_app.application_id
  sensitive   = false
}

output "backend_bff_app_sp_id" {
  description = "The Object ID of the service principal for backend BFF app registration."
  value       = azuread_service_principal.backend_bff_app_sp.object_id
  sensitive   = false
}

output "backend_bff_app_scope_id" {
  description = "The ID of the scope needed by the frontend app to access backend BFF app."
  value       = random_uuid.backend_bff_scope_uuid.result
  sensitive   = false
}

output "backend_bff_app_scope" {
  description = "The qualified value of the scope needed by the frontend app to access backend BFF app."
  value       = "${local.bff_id_uri}/api"
  sensitive   = false
}

output "authentication_sign_in_user_flow_id" {
  description = "The id of the user flow used for signing users in."
  value       = data.external.user_flows.result.signInUserFlowId
  sensitive   = false
}

output "authentication_invitation_user_flow_id" {
  description = "The id of the user flow used for inviting users."
  value       = data.external.user_flows.result.inviteUserFlowId
  sensitive   = false
}

output "authentication_mitid_invitation_user_flow_id" {
  description = "The id of the user flow used for inviting users using MitID."
  value       = data.external.user_flows.result.mitIdInviteUserFlowId
  sensitive   = false
}
