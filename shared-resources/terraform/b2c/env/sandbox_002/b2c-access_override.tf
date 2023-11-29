# Add platform team as Global Admins to B2C on sand_002
resource "azuread_directory_role" "global_reader" {
   display_name = "Global Administrator"
}
