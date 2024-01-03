locals {
  resources_suffix = "${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
  ip_restrictions_as_string = join(",", [for rule in var.ip_restrictions : "${rule.ip_address}"])
}
