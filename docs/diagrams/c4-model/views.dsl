# The 'views.dsl' file is intended as a mean for viewing and validating the model
# in the subsystem repository. It should
#   * Extend the base model and override the 'dh3' software system
#   * Include of the `model.dsl` files from each subsystem repository using an URL
#
# The `model.dsl` file must contain the actual model, and is the piece that must
# be reusable and included in other Structurizr files like `views.dsl` and
# deployment diagram files.

workspace extends https://raw.githubusercontent.com/Energinet-DataHub/opengeh-arch-diagrams/main/docs/diagrams/c4-model/dh-base-model.dsl {
    model {
        #
        # DataHub 3.0 (extends)
        #
        !ref dh3 {

            # IMPORTANT:
            # The order by which models are included is important for how the subsystem-to-subsystem relationships are specified.
            # A subsystem-to-subsystem relationship should be specified in the "client" of a "client->server" dependency, and
            # hence subsystems that doesn't depend on others, should be listed first.

            # IMPORTANT: The token expires within an hour (or so). Go to the repo and find the file and view the raw content to get a new token (copy from the url)
            !include https://raw.githubusercontent.com/Energinet-DataHub/opengeh-revision-log/main/docs/diagrams/c4-model/model.dsl?token=GHSAT0AAAAAACVEMB2FQHQIBK33ML7PVCU4ZV6DMMQ

            # Include Market Participant model
            !include https://raw.githubusercontent.com/Energinet-DataHub/geh-market-participant/main/docs/diagrams/c4-model/model.dsl

            # Include Wholesale model
            !include model.dsl

            # Include frontend model - placeholders
            frontendSubsystem = group "Frontend" {
                frontendBff = container "BFF" {
                    description "Backend for Frontend"

                    # Subsystem-to-Subsystem relationships
                    this -> wholesaleApi "Interact using HTTP API"
                }
            }

            # Include Migration model - placeholders
            migrationSubsystem = group "Migrations" {
                migrationDatabricks = container "Data Migration" {
                    description "Delivers DataHub 2.0 data to DataHub 3.0"

                    # Subsystem-to-Subsystem relationships
                    this -> dh3.sharedUnityCatalog "Deliver calculation inputs"
                    wholesaleCalculatorJob -> this "Read DataHub 2.0 data" "integration event/amqp" {
                        tags "Simple View"
                    }
                }
            }
        }
    }

    views {
        container dh3 "Wholesale" {
            title "[Container] DataHub 3.0 - Wholesale (Simplified)"
            include ->wholesaleSubsystem->
            exclude "relationship.tag==OAuth"
            exclude "element.tag==Intermediate Technology"
        }

        container dh3 "WholesaleDetailed" {
            title "[Container] DataHub 3.0 - Wholesale (Detailed with OAuth)"
            include ->wholesaleSubsystem->
            exclude "relationship.tag==Simple View"
        }
    }
}
