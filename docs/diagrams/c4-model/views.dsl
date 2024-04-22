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

            # Include Market Participant model
            !include https://raw.githubusercontent.com/Energinet-DataHub/geh-market-participant/main/docs/diagrams/c4-model/model.dsl

            # Include EDI model
            !include https://raw.githubusercontent.com/Energinet-DataHub/opengeh-edi/main/docs/diagrams/c4-model/model.dsl

            # Include Wholesale model
            !include model.dsl

            # Include Frontend model
            !include https://raw.githubusercontent.com/Energinet-DataHub/greenforce-frontend/main/docs/diagrams/c4-model/model.dsl

            # Include Migration model - placeholders
            migrationSubsystem = group "Migration" {
                migrationDatabricks = container "Data Migration" {
                    description "Extract migrated JSON files. Load and transform data using Notebooks"
                    technology "Azure Databricks"
                    tags "Microsoft Azure - Azure Databricks"

                    # Subsystem-to-subsystem relationships.
                    this -> wholesaleDataLake "Deliver calculation inputs"
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