locals {
  resources_suffix          = "${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
  resources_suffix_no_dash  = "${lower(var.domain_name_short)}_${lower(var.environment_short)}_we_${lower(var.environment_instance)}"
  ip_restrictions_as_string = join(",", [for rule in var.ip_restrictions : "${rule.ip_address}"])
}
