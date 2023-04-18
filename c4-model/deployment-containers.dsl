# PoC on building a deployment diagram
# showing how domain services (containers)
# are deployed onto infrastructure (deployment nodes).

workspace extends https://raw.githubusercontent.com/Energinet-DataHub/opengeh-arch-diagrams/main/source/datahub3-model/model.dsl {

    model {
        #
        # SaaS
        #
        sendGrid = softwareSystem "SendGrid" "EMail dispatcher (SaaS)" "Microsoft Azure - SendGrid Accounts"

        #
        # DataHub 3.0 (extends)
        #
        !ref dh3 {
            group "Shared resources" {
                sharedB2C = container "Applications" "" "Azure B2C" "Data Storage, Microsoft Azure - Azure AD B2C"
                sharedServiceBus = container "Messages" "" "Azure Service Bus" "Data Storage, Microsoft Azure - Azure Service Bus"
            }

            #
            # Frontend
            #
            dh3WebApp = group "Frontend" {
                frontend = container "UI" "Provides DH3 functionality to users via their web browser." "Angular" "Web Browser"
                bff = container "Backend for frontend" "Combines data for presentation on DataHub 3 UI" "Asp.Net Core Web API" "Microsoft Azure - App Services"

                extUser -> frontend "Uses"
                frontendBffRelationship = frontend -> bff "Uses" "JSON/HTTPS"
            }

            #
            # Migration
            #
            dropZone = group "Drop Zone" {
                dropZoneStorage = container "Drop Zone" "Store raw migration data (JSON)" "Storage Container" "Data Storage, Microsoft Azure - Storage Container"

                dh2 -> dropZoneStorage "Ingests"
            }

            migration = group "Migration" {
                migrationDatabricks = container "Data Migration" "Extract migrated JSON files. Load and transform data using Notebooks" "Azure Databricks" "Microsoft Azure - Azure Databricks"
                migrationDataLake = container "Data Lake (Migration)" "Store data using medalion architecture (bronze/silver/gold)" "Azure Data Lake Gen 2" "Data Storage, Microsoft Azure - Data Lake Store Gen1"
                migrationTimeSeriesApi = container "Time Series API" "" "Azure Web App" "Microsoft Azure - App Services"

                elOverblik -> migrationTimeSeriesApi "Fetch"
                dropZoneStorage -> migrationDatabricks "Ingest"
                migrationDatabricks -> migrationDataLake "Store"
                migrationTimeSeriesApi -> migrationDataLake "Read"
            }

            #
            # Wholesale
            #
            wholesale = group "Wholesale" {
                wholesaleApi = container "Wholesale API" "" "Asp.Net Core Web API" "Microsoft Azure - App Services"
                wholesaleDb = container "Wholesale Database" "" "MS SQL Server" "Data Storage, Microsoft Azure - SQL Database"

                wholesaleProcessManager = container "Process Manager" "" "Azure Function App" "Microsoft Azure - Function Apps"

                wholesaleDataLake = container "Data Lake (Wholesale)" "Stores batch results" "Azure Data Lake Gen 2" "Data Storage, Microsoft Azure - Data Lake Store Gen1"
                wholesaleCalculator = container "Calculation Engine" "" "Azure Databricks" "Microsoft Azure - Azure Databricks"

                bff -> wholesaleApi "uses" "JSON/HTTPS"
                migrationDatabricks -> wholesaleDataLake "Deliver"
                wholesaleApi -> wholesaleCalculator "sends requests to"
                wholesaleApi -> wholesaleDb "uses"
                wholesaleApi -> wholesaleDataLake "retrieves results from"
                wholesaleApi -> sharedServiceBus "publishes events"
                wholesaleCalculator -> wholesaleDataLake "write to"
                wholesaleProcessManager -> wholesaleApi "uses (directly)"
                wholesaleProcessManager -> sharedServiceBus "listens to events"
            }

            #
            # Market Participant
            #
            markpart = group "Market Participant" {
                markpartApi = container "Market Participant API" "Multi-tenant API for managing actors, users and permissions." "Asp.Net Core Web API" "Microsoft Azure - App Services"
                markpartEventActorSynchronizer = container "Actor Synchronizer" "Creates B2C application registration for newly created actors." "Azure Function (Timer Trigger)" "Microsoft Azure - Function Apps"
                markpartMailDispatcher = container "Mail Dispatcher" "Responsible for sending user invitations." "Azure Function (Timer Trigger)" "Microsoft Azure - Function Apps"
                markpartDb = container "Actors Database" "Stores data regarding actors, users and permissions." "SQL Database Schema" "Data Storage, Microsoft Azure - SQL Database"

                markpartApi -> markpartDb "Reads and writes actor/user data." "Entity Framework Core"
                markpartEventActorSynchronizer -> markpartDb "Updates actors with external B2C id." "Entity Framework Core"
                markpartMailDispatcher -> markpartDb "Reads data regarding newly invited users." "Entity Framework Core"

                sendGrid -> extUser "Sends invitation mail"
                ### extUser -> frontend "Views and manages data within the users assigned actor"
                dhSysAdmin -> frontend "Views and manages data across all actors"
                bff -> markpartApi "Uses" "JSON/HTTPS"
                markpartEventActorSynchronizer -> sharedB2C "Creates B2C application registration" "Microsoft.Graph/HTTPS"
                markpartMailDispatcher -> sendGrid "Sends invitation mail" "SendGrid/HTTPS"
            }

            #
            # EDI
            #
            edi = group "EDI" {
                ediPeekComponent = container "Peek component" "Handles peek requests from actors" "C#, Azure function" "Microsoft Azure - Function Apps"
                ediDequeueComponent = container "Dequeue component" "Handles dequeue requests from actors" "C#, Azure function" "Microsoft Azure - Function Apps"
                ediTimeSeriesListener = container "TimeSeries listener" "Listens for integration events indicating time series data is ready" "C#, Azure function" "Microsoft Azure - Function Apps"
                ediTimeSeriesRequester = container "TimeSeries request component" "Fetches time series data from relevant domain" "C#, Azure function" "Microsoft Azure - Function Apps"
                ediDb = container "EDI Database" "Stores information related to business transactions and outgoing messages" "SQL server database" "Data Storage, Microsoft Azure - SQL Database"

                extSoftSystem -> ediPeekComponent "Peek messages"
                extSoftSystem -> ediDequeueComponent "Dequeue messages"
                ediTimeSeriesListener -> ediTimeSeriesRequester "Triggers requester to fetch time series data"
                ediTimeSeriesListener -> sharedServiceBus "Handles events indicating time series data is available"
                ediTimeSeriesRequester -> wholesaleApi "Fetch time series data"
                ediPeekComponent -> ediDb "Reads / generates messages"
                ediDequeueComponent -> ediDb "Deletes messages that have been peeked"
                ediTimeSeriesRequester -> ediDb "Writes time series data to database"
            }
        }

        # Deployment model
        deploymentEnvironment "Production" {
            # Computer
            deploymentNode "User's computer" "" "Microsoft Windows or Apple macOS" {
                deploymentNode "Web Browser" "" "Chrome, Firefox, Safari, or Edge" {
                    tags "Microsoft Azure - Browser"
                    frontendInstance = containerInstance frontend
                }
            }

            # Azure Cloud
            deploymentNode "Azure Cloud" "" "Azure Subscription" {
                tags "Microsoft Azure - Subscriptions"

                apim = infrastructureNode "APIM" "" "API Management Service" {
                    tags "Microsoft Azure - API Management Services"
                }

                deploymentNode "Shared App Service Plan" "" "App Service Plan" {
                    tags "Microsoft Azure - App Service Plans"
                    deploymentNode "BFF API" "" "App Service" {
                        tags "Microsoft Azure - App Services"
                        bffInstance = containerInstance bff
                    }
                    deploymentNode "Migration Time Series API" "" "App Service" {
                        tags "Microsoft Azure - App Services"
                        migrationTimeSeriesApiInstance = containerInstance migrationTimeSeriesApi
                    }
                    deploymentNode "Wholesale API" "" "App Service" {
                        tags "Microsoft Azure - App Services"
                        wholesaleApiInstance = containerInstance wholesaleApi
                    }
                    deploymentNode "Wholesale Process Manager" "" "App Service" {
                        tags "Microsoft Azure - Function Apps"
                        wholesaleProcessManagerInstance = containerInstance wholesaleProcessManager
                    }
                    deploymentNode "Market Participant API" "" "App Service" {
                        tags "Microsoft Azure - App Services"
                        markpartApiInstance = containerInstance markpartApi
                    }
                    deploymentNode "Market Participant Organization Manager" "" "App Service" {
                        tags "Microsoft Azure - Function Apps"
                        markpartEventActorSynchronizerInstance = containerInstance markpartEventActorSynchronizer
                        markpartMailDispatcherInstance = containerInstance markpartMailDispatcher
                    }
                    deploymentNode "EDI API" "" "App Service" {
                        tags "Microsoft Azure - Function Apps"
                        peekComponentInstance = containerInstance ediPeekComponent
                        dequeueComponentInstance = containerInstance ediDequeueComponent
                        timeSeriesListenerInstance = containerInstance ediTimeSeriesListener
                        timeSeriesRequesterInstance = containerInstance ediTimeSeriesRequester
                    }
                }

                deploymentNode "Shared SQL Server" "" "Azure SQL Server" {
                    tags "Microsoft Azure - SQL Server"
                    deploymentNode "Elastic Pool" "" "SQL Elastic Pool"{
                        tags "Microsoft Azure - SQL Elastic Pools"
                        deploymentNode "Wholesale DB" "" "SQL Database"{
                            tags "Microsoft Azure - SQL Database"
                            wholesaleDbInstance = containerInstance wholesaleDb
                        }
                        deploymentNode "Actors Database" "" "SQL Database"{
                            tags "Microsoft Azure - SQL Database"
                            markpartDbInstance = containerInstance markpartDb
                        }
                        deploymentNode "EDI Database" "" "SQL Database"{
                            tags "Microsoft Azure - SQL Database"
                            ediDbInstance = containerInstance ediDb
                        }
                    }
                }

                deploymentNode "Data Lake (Migration)" "" "Azure Storage Account" {
                    tags "Microsoft Azure - Storage Accounts"
                    migrationDataLakeInstance = containerInstance migrationDataLake
                }

                deploymentNode "Data Lake (Wholesale)" "" "Azure Storage Account" {
                    tags "Microsoft Azure - Storage Accounts"
                    wholesaleDataLakeInstance = containerInstance wholesaleDataLake
                }

                deploymentNode "Databricks" "" "Azure Databricks Service" {
                    tags "Microsoft Azure - Azure Databricks"
                    migrationDatabricksInstance = containerInstance migrationDatabricks
                    wholesaleCalculatorInstance = containerInstance wholesaleCalculator
                }

                # Infrastructure relations
                frontendInstance -> apim "Uses" "JSON/HTTPS"
                apim -> bffInstance "Forwards requests to" "JSON/HTTPS"
            }
        }
    }

    views {
        container dh3 {
            include *
            autolayout
        }

        deployment dh3 "Production" "LiveDeployment" {
            include *
            exclude frontendBffRelationship
            autoLayout
        }
    }
}