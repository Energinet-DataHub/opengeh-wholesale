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

            # Include Wholesale model
            !include model.dsl

            # Include related subsystems - placeholders
            relatedSubsystems = group "Related Subsystems" {
                markpartSubsystem = container "Market Participant" {
                    tags "Subsystem"

                    wholesaleApi -> this "Subscribes to integration event Grid Area Ownership Assigned" "integration event/amqp" {
                        tags "Simple View"
                    }
                    this -> dh3.sharedServiceBus "Publish Grid Area Ownership Assigned" "integration event/amqp" {
                        tags "Detailed View"
                    }
                }

                bffSubsystem = container "BFF" {
                    description "Backend for Frontend. Partnership relationship"
                    tags "Subsystem"

                    this -> wholesaleApi "Uses HTTP API"
                }

                migrationSubsystem = container "Data Migration" {
                    description "Delivers DataHub 2.0 data to DataHub 3.0"
                    tags "Subsystem"

                    this -> dh3.sharedUnityCatalog "Deliver calculation inputs" {
                        tags "Detailed View"
                    }
                    wholesaleCalculatorJob -> this "Read DataHub 2.0 data" "integration event/amqp" {
                        tags "Simple View"
                    }
                }

                downstreamReaderSubsystems = container "Downstream Readers" {
                    description "Downstream subsystems reading data from wholesale"
                    tags "Subsystem"

                    this -> wholesaleDataLake "Read calculation output data" {
                        tags "Simple View"
                    }
                    this -> dh3.sharedUnityCatalog "Read calculation output data" {
                        tags "Detailed View"
                    }
                }

                downstreamSubscriberSubsystems = container "Downstream Subscribers" {
                    description "Downstream subsystems subscribing to integration events from wholesale"
                    tags "Subsystem"

                    this -> wholesaleApi "Subscribes to calculation_completed_v1 integration events" "integration event/amqp" {
                        tags "Simple View"
                    }
                    this -> dh3.sharedServiceBus "Subscribes to integration events" "integration event/amqp" {
                        tags "Detailed View"
                    }
                }

            }
        }
    }

    views {
        # TODO BJM: Move these styles to the base model?
        styles {
            # Use to mark a related subsystem.
            element "Subsystem" {
                background #6ec1f8
                color #ffffff
            }
        }
        
        container dh3 "Wholesale" {
            title "[Container] DataHub 3.0 - Wholesale (Simplified)"
            include ->wholesaleSubsystem->
            exclude "relationship.tag==OAuth"
            exclude "element.tag==Intermediate Technology"
            exclude "relationship.tag==Detailed View"
        }

        container dh3 "WholesaleDetailed" {
            title "[Container] DataHub 3.0 - Wholesale (Detailed with OAuth)"
            include ->wholesaleSubsystem->
            exclude "relationship.tag==Simple View"
        }
    }
}
