// Local Variables
locals {
  blob_files_raw_container = {
    name        = "files-raw"
    access_type = "private"
  }
  blob_files_enrichments_container = {
    name        = "files-enrichments"
    access_type = "private"
  }
  blob_files_converted_container = {
    name        = "files-converted"
    access_type = "private"
  }
  blob_files_sent_container = {
    name        = "files-sent"
    access_type = "blob"
  }
  blob_files_error_container = {
    name        = "files-error"
    access_type = "blob"
  }
  blob_files_confirmed_container = {
    name        = "files-confirmed"
    access_type = "blob"
  }
  blob_files_other_container = {
    name        = "files-other"
    access_type = "blob"
  }
  blob_files_mga_imbalance_container = {
    name        = "files-mga-imbalance"
    access_type = "blob"
  }
  blob_files_brp_change_container = {
    name        = "files-brp-change"
    access_type = "blob"
  }
  blob_files_ack_container = {
    name        = "files-acknowledgement"
    access_type = "blob"
  }
  name_suffix="${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
  name_suffix_no_dash="${lower(var.domain_name_short)}${lower(var.environment_short)}we${lower(var.environment_instance)}"
}
