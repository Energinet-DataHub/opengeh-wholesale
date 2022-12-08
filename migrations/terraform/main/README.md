# Migrations

This folder contains Terraform files which may include hardcoded names of resources or similar.
This is due to the fact that HalfSpace is currently manually setting up a POC migration in U-001.

To support them in this endeavor they needed different resources (such as Role access to storage accounts).

This can not be done by hand due to missing access rights. Therefor it is done throught Terraform and the SPN attached to the subscription.
