# Grant IP opening for halfspace office in order to copy data as part of #1722

module "st_data_lake" {
  ip_rules = "152.115.174.2,${local.ip_restrictions_as_string}"
}
