# PoC on building a model where
#  - "deploymentNodes" are used for Azure resources
#  - "containers" are Azure resource agnostics
#
# Since "architectualRunway.dsl" doesn't use this separation we cannot use "extend"
# and instead have a copy of necessary model pieces.
workspace "DataHub 3.0" {

    model {
        dh3User = person "DH3 User" "Person that uses the DataHub 3 Web App"

        dh2 = softwareSystem "DataHub 2" "Developed and maintained by CGI"

        dhOrganization = enterprise "DataHub Organization" {
            dh3 = softwareSystem "DataHub 3.0" "Provides uniform communication and standardized processes for actors operating on the Danish electricity market." {
                dh3WebApp = group "Web App" {
                    frontend = container "UI" "Provides DH3 functionality to users via their web browser." "Angular"
                    bff = container "Backend for frontend" "Combines data for presentation on DataHub 3 UI" "Asp.Net Core Web API"

                    frontend -> bff "Uses" "JSON/HTTPS"
                }

                wholesale = group "Wholesale" {
                    wholesaleApi = container "Wholesale API" "" "Asp.Net Core Web API"
                    wholesaleDb = container "Database" "Stores batch processing state" "SQL Database Schema" "Data Storage"

                    wholesaleProcessManager = container "Process Manager" "Handles batch processing" "Azure Function App"

                    wholesaleStorage = container "Storage" "Stores batch results" "JSON and CSV files" "Data Storage"
                    wholesaleCalculator = container "Calculator" "" "Python wheel"

                    wholesaleApi -> wholesaleDb "Reads from and writes to"
                    wholesaleApi -> wholesaleStorage "Reads from"

                    wholesaleProcessManager -> wholesaleDb "Reads from"
                    wholesaleProcessManager -> wholesaleCalculator "Triggers"
                    wholesaleCalculator -> wholesaleStorage "Write to"
                }
            }
        }

        # Relationships to/from containers
        dh3User -> frontend "View and start jobs using"
        bff -> wholesaleApi "Uses" "JSON/HTTPS"

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
                    deploymentNode "Wholesale API" "" "App Service" {
                        tags "Microsoft Azure - App Services"
                        wholesaleApiInstance = containerInstance wholesaleApi
                    }
                    deploymentNode "Wholesale Process Manager" "" "App Service" {
                        tags "Microsoft Azure - Function Apps"
                        wholesaleProcessManagerInstance = containerInstance wholesaleProcessManager
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
                    }
                }

                deploymentNode "Data Lake" "" "Azure Storage Account" {
                    tags "Microsoft Azure - Storage Accounts"
                    wholesaleStorageInstance = containerInstance wholesaleStorage
                }

                deploymentNode "Databricks" "" "Azure Databricks Service" {
                    tags "Microsoft Azure - Azure Databricks"
                    wholesaleCalculatorInstance = containerInstance wholesaleCalculator
                }

                # Infrastructure relations
                frontendInstance -> apim "Uses" "JSON/HTTPS"
                apim -> bffInstance "Forwards requests to" "JSON/HTTPS"
            }
        }
    }

    views {
        systemlandscape "SystemLandscape" {
            include *
            autoLayout
        }

        systemcontext dh3 "SystemContext" {
            include *
            autoLayout
        }

        container dh3 {
            include *
            autolayout
        }

        container dh3 "WebApp" {
            include dh3WebApp wholesaleApi
            autolayout
        }

        container dh3 "Wholesale" {
            include wholesale
            autolayout
        }

        deployment dh3 "Production" "LiveDeployment" {
            include *
            autoLayout
        }

        themes default https://static.structurizr.com/themes/microsoft-azure-2021.01.26/theme.json

        styles {
            element "Data Storage" {
                shape Cylinder
            }

            element "Group" {
                color #444444
            }
        }
    }
}
