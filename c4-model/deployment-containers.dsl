# PoC on building a deployment diagram
# showing how domain services (containers)
# are deployed onto infrastructure (deployment nodes).

workspace extends https://raw.githubusercontent.com/Energinet-DataHub/opengeh-arch-diagrams/main/source/datahub3-model/model.dsl {

    model {
        #
        # DataHub 3.0 (extends)
        #
        !ref dh3 {
            #
            # Shared
            #
            sharedServiceBus = container "Message broker" {
                description "Message broker with message queues and publish-subscribe topics"
                technology "Azure Service Bus"
                tags "Intermediate Technology" "PaaS" "Microsoft Azure - Azure Service Bus"
            }

            #
            # Frontend
            #
            dh3WebApp = group "Frontend" {
                frontendStaticWebApp = container "Static Web App" {
                    description "Delivers the static content and the UI single page application"
                    technology "Static Web App"
                    tags "Intermediate Technology" "Microsoft Azure - Static Apps"

                    extUser -> this "Visits DH3 url" "https"
                    dhSystemAdmin -> this "Visits DH3 url" "https"
                }
                frontendSinglePageApplication = container "UI" {
                    description "Provides DH3 functionality to users via their web browser."
                    technology "Angular"
                    tags "Web Browser"

                    extUser -> this "Uses"
                    dhSystemAdmin -> this "Views and manages data across all actors"
                    frontendStaticWebApp -> this "Delivers to users web browser"
                }
                bffApi = container "BFF API" {
                    description "API Gateway to BFF Web API"
                    technology "Azure API Management Service"
                    tags "Intermediate Technology" "Microsoft Azure - API Management Services"

                    frontendSinglePageApplication -> this "Uses" "json/https"
                }
                bffApp = container "BFF Web API" {
                    description "Backend for frontend (BFF) combines data for presentation on DataHub 3 UI"
                    technology "Asp.Net Core Web API"
                    tags "Microsoft Azure - App Services"

                    bffApi -> this "Uses" "json/https"
                    frontendSinglePageApplication -> this "Uses" "json/https" {
                        tags "Simple View"
                    }
                }
            }

            #
            # EDI
            #
            edi = group "EDI" {
                ediApi = container "EDI API" {
                    description "API Gateway to EDI Web API"
                    technology "Azure API Management Service"
                    tags "Intermediate Technology" "Microsoft Azure - API Management Services"

                    actorB2BSystem -> this "Peek/Dequeue messages"
                }
                ediApiApp = container "EDI Web API" {
                    description "<add description>"
                    technology "Azure function, C#"
                    tags "Microsoft Azure - Function Apps"

                    actorB2BSystem -> this "Peek/Dequeue messages" {
                        tags "Simple View"
                    }

                    ediPeekComponent = component "Peek component" {
                        description "Handles peek requests from actors"
                        technology "Http Trigger"
                        tags "Microsoft Azure - Function Apps"

                        ediApi -> this "Peek messages"
                    }
                    ediDequeueComponent = component "Dequeue component" {
                        description "Handles dequeue requests from actors"
                        technology "Http Trigger"
                        tags "Microsoft Azure - Function Apps"

                        ediApi -> this "Dequeue messages"
                    }
                    ediTimeSeriesListener = component "TimeSeries listener" {
                        description "Listens for integration events indicating time series data is ready"
                        technology "Service Bus Trigger"
                        tags "Microsoft Azure - Function Apps"
                    }
                    ediTimeSeriesRequester = component "TimeSeries request component" {
                        description "Fetches time series data from relevant domain"
                        technology "<?> Trigger"
                        tags "Microsoft Azure - Function Apps"

                        ediTimeSeriesListener -> this "Triggers requester to fetch time series data"
                    }
                }
                ediDb = container "EDI Database" {
                    description "Stores information related to business transactions and outgoing messages"
                    technology "SQL Database Schema"
                    tags "Data Storage, Microsoft Azure - SQL Database"

                    ediPeekComponent -> this "Reads / generates messages" "EF Core"
                    ediDequeueComponent -> this "Deletes messages that have been peeked" "EF Core"
                    ediTimeSeriesRequester -> this "Writes time series data to database" "EF Core"
                }

                ediTimeSeriesListener -> sharedServiceBus "Handles events indicating time series data is available"
            }

            #
            # Migration
            #
            migration = group "Migration" {
                dropZoneStorage = container "Drop Zone" {
                    description "Store raw migration data"
                    technology "json/storage container"
                    tags "Data Storage" "Microsoft Azure - Storage Container"

                    dh2 -> this "Ingests"
                }
                migrationDatabricks = container "Data Migration" {
                    description "Extract migrated JSON files. Load and transform data using Notebooks"
                    technology "Azure Databricks"
                    tags "Microsoft Azure - Azure Databricks"

                    dropZoneStorage -> this "Ingest"
                }
                migrationTimeSeriesApi = container "Time Series API" {
                    description "<add description>"
                    technology "Asp.Net Core Web API"
                    tags "Not Compliant" "Microsoft Azure - App Services"

                    elOverblik -> this "Fetch"
                }
                migrationDataLake = container "Data Lake (Migration)" {
                    description "Store data using medalion architecture (bronze/silver/gold)"
                    technology "Azure Data Lake Gen 2"
                    tags "Data Storage" "Microsoft Azure - Data Lake Store Gen1"

                    migrationDatabricks -> this "Store"
                    migrationTimeSeriesApi -> this "Read"
                }
            }

            #
            # Wholesale
            #
            wholesale = group "Wholesale" {
                wholesaleProcessManager = container "Process Manager" {
                    description "<add description>"
                    technology "Azure function, C#"
                    tags "Microsoft Azure - Function Apps"
                }
                wholesaleApi = container "Wholesale API" {
                    description "<add description>"
                    technology "Asp.Net Core Web API"
                    tags "Microsoft Azure - App Services"

                    bffApp -> this "uses" "json/https"
                    wholesaleProcessManager -> this "uses (directly)"
                }
                wholesaleDb = container "Wholesale Database" {
                    description "<add description>"
                    technology "SQL Database Schema"
                    tags "Data Storage" "Microsoft Azure - SQL Database"

                    wholesaleApi -> this "uses" "EF Core"
                }
                wholesaleCalculator = container "Calculation Engine" {
                    description "<add description>"
                    technology "Azure Databricks"
                    tags "Microsoft Azure - Azure Databricks"

                    wholesaleApi -> this "sends requests to"
                }
                wholesaleDataLake = container "Data Lake (Wholesale)" {
                    description "Stores batch results"
                    technology "Azure Data Lake Gen 2"
                    tags "Data Storage" "Microsoft Azure - Data Lake Store Gen1"

                    migrationDatabricks -> this "Deliver"
                    wholesaleApi -> this "retrieves results from"
                    wholesaleCalculator -> this "write to"
                }

                wholesaleApi -> sharedServiceBus "publishes events"
                wholesaleApi -> wholesaleProcessManager "notify of <event>" "message/amqp" {
                    tags "Simple View"
                }
                wholesaleApi -> ediTimeSeriesListener "notify of <event>" "message/amqp" {
                    tags "Simple View"
                }
                wholesaleProcessManager -> sharedServiceBus "listens to events"
            }

            #
            # Market Participant
            #
            markpart = group "Market Participant" {
                markpartApi = container "Market Participant API" {
                    description "Multi-tenant API for managing actors, users and permissions."
                    technology "Asp.Net Core Web API"
                    tags "Microsoft Azure - App Services"

                    bffApp -> this "Uses" "json/https"
                }
                markpartOrganizationManager = container "Market Participant <Organization Manager>" {
                    description "<add description>"
                    technology "Azure function, C#"
                    tags "Microsoft Azure - Function Apps"

                    markpartEventActorSynchronizer = component "Actor Synchronizer" {
                        description "Creates B2C application registration for newly created actors."
                        technology "Timer Trigger"
                        tags "Microsoft Azure - Function Apps"
                    }
                    markpartMailDispatcher = component "Mail Dispatcher" {
                        description "Responsible for sending user invitations."
                        technology "Timer Trigger"
                        tags "Microsoft Azure - Function Apps"
                    }
                }
                markpartDb = container "Actors Database" {
                    description "Stores data regarding actors, users and permissions."
                    technology "SQL Database Schema"
                    tags "Data Storage" "Microsoft Azure - SQL Database"

                    markpartApi -> this "Reads and writes actor/user data." "EF Core"
                    markpartEventActorSynchronizer -> this "Updates actors with external B2C id." "EF Core"
                    markpartMailDispatcher -> this "Reads data regarding newly invited users." "EF Core"
                }

                #
                # Common (managed by Market Participant)
                #
                commonSendGrid = container "SendGrid" {
                    description "EMail dispatcher"
                    technology "Twilio SendGrid"
                    tags "Intermediate Technology" "SaaS" "Microsoft Azure - SendGrid Accounts"

                    markpartMailDispatcher -> this "Sends invitation mail" "SendGrid/https"
                }
                commonB2C = container "App Registrations" {
                    description "Cloud identity directory"
                    technology "Azure AD B2C"
                    tags "Microsoft Azure - Azure AD B2C"

                    markpartEventActorSynchronizer -> this "Creates B2C App Registration" "Microsoft.Graph/https"

                    actorB2BSystem -> this "Request OAuth token" "https" {
                        tags "OAuth"
                    }
                    frontendSinglePageApplication -> this "Request OAuth token" "https" {
                        tags "OAuth"
                    }
                    bffApi -> this "Validate OAuth token" "https" {
                        tags "OAuth"
                    }
                    ediApi -> this "Validate OAuth token" "https" {
                        tags "OAuth"
                    }
                }

                commonSendGrid -> extUser "Sends invitation mail"
                markpartMailDispatcher -> extUser "Sends invitation mail" {
                    tags "Simple View"
                }
            }
        }

        # Deployment model
        deploymentEnvironment "Production" {
            # Computer
            deploymentNode "User's computer" {
                description ""
                technology "Microsoft Windows or Apple macOS"

                deploymentNode "Web Browser" {
                    description ""
                    technology "Chrome, Firefox, Safari, or Edge"
                    tags "Microsoft Azure - Browser"

                    frontendSinglePageApplicationInstance = containerInstance frontendSinglePageApplication
                }
            }

            # Azure Cloud
            deploymentNode "Azure Cloud" {
                description ""
                technology "Azure Subscription"
                tags "Microsoft Azure - Subscriptions"

                deploymentNode "Static Web App" {
                    description ""
                    technology "Static Web App"
                    tags "Microsoft Azure - Static Apps"

                    frontendStaticWebAppInstance = containerInstance frontendStaticWebApp
                }

                deploymentNode "Active Directory B2C" {
                    description ""
                    technology "Azure AD B2C"
                    tags "Microsoft Azure - Azure AD B2C"

                    commonB2CInstance = containerInstance commonB2C
                }

                deploymentNode "API Gateway" {
                    description ""
                    technology "API Management Service"
                    tags "Microsoft Azure - API Management Services"

                    bffApiInstance = containerInstance bffApi
                    ediApiInstance = containerInstance ediApi
                }

                deploymentNode "Service Bus" {
                    description ""
                    technology "Azure Service Bus"
                    tags "Microsoft Azure - Azure Service Bus"

                    sharedServiceBusInstance = containerInstance sharedServiceBus
                }

                deploymentNode "Shared App Service Plan" {
                    description ""
                    technology "App Service Plan"
                    tags "Microsoft Azure - App Service Plans"

                    deploymentNode "BFF Web API" {
                        description ""
                        technology "App Service"
                        tags "Microsoft Azure - App Services"

                        bffAppInstance = containerInstance bffApp
                    }
                    deploymentNode "Migration Time Series API" {
                        description ""
                        technology "App Service"
                        tags "Microsoft Azure - App Services"

                        migrationTimeSeriesApiInstance = containerInstance migrationTimeSeriesApi
                    }
                    deploymentNode "Wholesale API" {
                        description ""
                        technology "App Service"
                        tags "Microsoft Azure - App Services"

                        wholesaleApiInstance = containerInstance wholesaleApi
                    }
                    deploymentNode "Wholesale Process Manager" {
                        description ""
                        technology "App Service"
                        tags "Microsoft Azure - Function Apps"

                        wholesaleProcessManagerInstance = containerInstance wholesaleProcessManager
                    }
                    deploymentNode "Market Participant API" {
                        description ""
                        technology "App Service"
                        tags "Microsoft Azure - App Services"

                        markpartApiInstance = containerInstance markpartApi
                    }
                    deploymentNode "Market Participant Organization Manager" {
                        description ""
                        technology "App Service"
                        tags "Microsoft Azure - Function Apps"

                        markpartOrganizationManagerInstance = containerInstance markpartOrganizationManager
                    }
                    deploymentNode "EDI Web API" {
                        description ""
                        technology "App Service"
                        tags "Microsoft Azure - Function Apps"

                        ediApiAppInstance = containerInstance ediApiApp
                    }
                }

                deploymentNode "Shared SQL Server" {
                    description ""
                    technology "Azure SQL Server"
                    tags "Microsoft Azure - SQL Server"

                    deploymentNode "Elastic Pool" {
                        description ""
                        technology "SQL Elastic Pool"
                        tags "Microsoft Azure - SQL Elastic Pools"

                        deploymentNode "Wholesale DB" {
                            description ""
                            technology "SQL Database"
                            tags "Microsoft Azure - SQL Database"

                            wholesaleDbInstance = containerInstance wholesaleDb
                        }
                        deploymentNode "Actors Database" {
                            description ""
                            technology "SQL Database"
                            tags "Microsoft Azure - SQL Database"

                            markpartDbInstance = containerInstance markpartDb
                        }
                        deploymentNode "EDI Database" {
                            description ""
                            technology "SQL Database"
                            tags "Microsoft Azure - SQL Database"

                            ediDbInstance = containerInstance ediDb
                        }
                    }
                }

                deploymentNode "Drop Zone" {
                    description ""
                    technology "Azure Storage Account"
                    tags "Microsoft Azure - Storage Accounts"

                    dropZoneStorageInstance = containerInstance dropZoneStorage
                }
                deploymentNode "Data Lake (Migration)" {
                    description ""
                    technology "Azure Storage Account"
                    tags "Microsoft Azure - Storage Accounts"

                    migrationDataLakeInstance = containerInstance migrationDataLake
                }
                deploymentNode "Data Lake (Wholesale)" {
                    description ""
                    technology "Azure Storage Account"
                    tags "Microsoft Azure - Storage Accounts"

                    wholesaleDataLakeInstance = containerInstance wholesaleDataLake
                }

                deploymentNode "Databricks" {
                    description ""
                    technology "Azure Databricks Service"
                    tags "Microsoft Azure - Azure Databricks"

                    migrationDatabricksInstance = containerInstance migrationDatabricks
                    wholesaleCalculatorInstance = containerInstance wholesaleCalculator
                }
            }
        }
    }

    views {
        container dh3 "All_no_OAuth" {
            title "[Container] DataHub 3.0 (Detailed, no OAuth)"
            description "Detailed 'as-is' view of all domains, which includes 'Intermediate Technology' elements, but excludes 'OAuth' relationships"
            include *
            exclude "relationship.tag==Deployment Diagram"
            exclude "relationship.tag==OAuth"
            exclude "relationship.tag==Simple View"
            autolayout
        }

        container dh3 "All_simple" {
            title "[Container] DataHub 3.0 (Simplified, no OAuth)"
            description "Simplified 'as-is' view of all domains, which excludes both 'Intermediate Technology' elements and 'OAuth' relationships"
            include *
            exclude "relationship.tag==Deployment Diagram"
            exclude "relationship.tag==OAuth"
            exclude "element.tag==Intermediate Technology"
            autolayout
        }

        container dh3 "Frontend" {
            title "[Container] DataHub 3.0 - Frontend (Detailed with OAuth)"
            include ->dh3WebApp->
            exclude "relationship.tag==Deployment Diagram"
            exclude "relationship.tag==Simple View"
            autolayout
        }

        container dh3 "Migration" {
            title "[Container] DataHub 3.0 - Migration (Detailed with OAuth)"
            include ->migration->
            exclude "relationship.tag==Deployment Diagram"
            exclude "relationship.tag==Simple View"
            autolayout
        }

        container dh3 "Wholesale" {
            title "[Container] DataHub 3.0 - Wholesale (Detailed with OAuth)"
            include ->wholesale->
            exclude "relationship.tag==Deployment Diagram"
            exclude "relationship.tag==Simple View"
            autolayout
        }

        container dh3 "Market_Participant" {
            title "[Container] DataHub 3.0 - Market Participant (Detailed with OAuth)"
            include ->markpart->
            exclude "relationship.tag==Deployment Diagram"
            exclude "relationship.tag==Simple View"
            autolayout
        }

        container dh3 "EDI" {
            title "[Container] DataHub 3.0 - EDI (Detailed with OAuth)"
            include ->edi->
            exclude "relationship.tag==Deployment Diagram"
            exclude "relationship.tag==Simple View"
            autolayout
        }

        deployment dh3 "Production" "Production_no_OAuth" {
            title "[Deployment] DataHub 3.0 (Detailed, no OAuth)"
            description "Detailed 'as-is' view of all domains and infrastructure dependencies. Excludes 'OAuth' relationships"
            include *
            exclude "relationship.tag==Container Diagram"
            exclude "relationship.tag==OAuth"
            exclude "relationship.tag==Simple View"
            autoLayout
        }

        deployment dh3 "Production" "Production_focus_OAuth" {
            title "[Deployment] DataHub 3.0 Azure AD B2C"
            description "'As-is' view of all relationships to and from the Azure AD B2C"
            include ->commonB2CInstance->
            exclude "relationship.tag==Container Diagram"
            exclude "relationship.tag==Simple View"
            autoLayout
        }

        styles {
            # Use to mark an element that is somehow not compliant to the projects standards.
            element "Not Compliant" {
                background #ffbb55
                color #ffffff
            }
            # Use to mark an element that is acting as a mediator between other elements in focus.
            # E.g. an element that we would like to show for the overall picture, to be able to clearly communicate dependencies.
            # Typically this is an element that we configure, instead of develop.
            element "Intermediate Technology" {
                background #dddddd
                color #999999
            }
        }
    }
}