output subnet_name_landing_zone {
  description = "Name of the subnet hosting the deployment agents."
  value       = module.snet_deployagent.name
  sensitive   = false
}