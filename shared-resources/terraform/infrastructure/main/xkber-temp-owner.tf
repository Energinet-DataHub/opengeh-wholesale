# Temporarily allow XKBER full access allow editing flawed TF-state regarding B2C
# Should be removed once the TF-state is fixed (deploy of PR-565 in dh3-infrastructure runs successfully)

resource "azurerm_role_assignment" "xkber_tf_state" {
  scope                = data.azurerm_subscription.current.id
  role_definition_name = "Owner"
  principal_id         = "787bf5d7-ea80-45dc-b6da-c8e1bc60efdd"  #xkber-aadadmin@energinet.dk
}
