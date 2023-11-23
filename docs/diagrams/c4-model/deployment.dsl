# Extends the core DataHub 3 model to build a deployment diagram
# showing how domain services (containers) are deployed onto infrastructure (deployment nodes).

workspace extends https://raw.githubusercontent.com/Energinet-DataHub/opengeh-arch-diagrams/main/docs/diagrams/c4-model/dh-base-model.dsl {
    model {
        #
        # DataHub 3.0 (extends)
        #
        !ref dh3 {

            # IMPORTANT:
            # The order by which models are included is important for how the domain-to-domain relationships are specified.
            # A domain-to-domain relationship should be specified in the "client" of a "client->server" dependency, and
            # hence domains that doesn't depend on others, should be listed first.

            # Include Market Participant model
            !include https://raw.githubusercontent.com/Energinet-DataHub/geh-market-participant/main/docs/diagrams/c4-model/model.dsl

            # Include EDI model
            !include https://raw.githubusercontent.com/Energinet-DataHub/opengeh-edi/main/docs/diagrams/c4-model/model.dsl

            # Include Wholesale model
            !include https://raw.githubusercontent.com/Energinet-DataHub/opengeh-wholesale/main/docs/diagrams/c4-model/model.dsl

            # Include Frontend model
            !include https://raw.githubusercontent.com/Energinet-DataHub/greenforce-frontend/main/docs/diagrams/c4-model/model.dsl

            # Include Esett Exchange model - requires a token because its located in a private repository
            # Token is automatically appended in "Raw" view of the file
            !include https://raw.githubusercontent.com/Energinet-DataHub/opengeh-esett-exchange/main/docs/diagrams/c4-model/model.dsl?token=GHSAT0AAAAAACJAEQ6GPS6MCKCH24B6MZWOZK3NMRA

            # Include Migration model - requires a token because its located in a private repository
            # Token is automatically appended in "Raw" view of the file
            !include https://raw.githubusercontent.com/Energinet-DataHub/opengeh-migration/main/docs/diagrams/c4-model/model.dsl?token=GHSAT0AAAAAACJAEQ6HPQL4WT5VV3ENWLCIZK3NKFA
            # Include platform tools - requires a token because its located in a private repository
            # Token is automatically appended in "Raw" view of the file
            !include https://raw.githubusercontent.com/Energinet-DataHub/dh3-operations/main/docs/diagrams/c4-model/model.dsl?token=GHSAT0AAAAAACJAEQ6HK6ZGQTKJAGP6X4HUZK3NNMQ
        }

        # Deployment model
        deploymentEnvironment "Production (prod_001)" {
            # Computer
            deploymentNode "User's computer" {
                description ""
                technology "Microsoft Windows or Apple macOS"

                deploymentNode "Web Browser for DataHub UI" {
                    description ""
                    technology "Chrome, Firefox, Safari, or Edge"
                    tags "Microsoft Azure - Browser"

                    frontendSinglePageApplicationInstance = containerInstance frontendSinglePageApplication
                }

                deploymentNode "Web Browser for Platform Tools UI" {
                    description ""
                    technology "Chrome, Firefox, Safari, or Edge"
                    tags "Microsoft Azure - Browser"

                    platformFrontendSinglePageApplicationInstance = containerInstance platformFrontendSinglePageApplication
                }
            }

            # SendGrid
            deploymentNode "SendGrid" {
                description ""
                technology "Twilio SendGrid"

                deploymentNode "Internal Shared SendGrid" {
                    description "Sending emails to Teams when pipelines fail"
                    technology "Twilio SendGrid"
                    tags "Intermediate Technology" "SaaS" "Microsoft Azure - SendGrid Accounts"

                    sharedInternalSendGridInstance = containerInstance dh3.sharedInternalSendGrid
                }

                deploymentNode "External Shared SendGrid" {
                    description "Sending emails when inviting users"
                    technology "Twilio SendGrid"
                    tags "Intermediate Technology" "SaaS" "Microsoft Azure - SendGrid Accounts"

                    sharedExternalSendGridInstance = containerInstance dh3.sharedExternalSendGrid
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

                    sharedB2CInstance = containerInstance dh3.sharedB2C
                }

                deploymentNode "API Gateway" {
                    description ""
                    technology "API Management Service"
                    tags "Microsoft Azure - API Management Services"

                    bffApiInstance = containerInstance bffApi
                    ediApiInstance = containerInstance ediApi
                }

                # Experiment:
                # We would like all (most) resources to appear on our deployment diagram,
                # but we don't want to add elements to the base model if they don't
                # appear in significant "flows". Hence we have decided to add this
                # as an infrastructure node for the moment.
                infrastructureNode "Appliction Insights" {
                    description ""
                    technology "Azure Application Insights"
                    tags "Microsoft Azure - Application Insights"
                }

                deploymentNode "Key Vault" {
                    description ""
                    technology "Azure Key Vault"
                    tags "Microsoft Azure - Key Vaults"

                    sharedKeyVaultInstance = containerInstance dh3.sharedKeyVault
                }

                deploymentNode "Service Bus" {
                    description ""
                    technology "Azure Service Bus"
                    tags "Microsoft Azure - Azure Service Bus"

                    sharedServiceBusInstance = containerInstance dh3.sharedServiceBus
                }

                deploymentNode "Shared App Service Plan" {
                    description ""
                    technology "App Service Plan"
                    tags "Microsoft Azure - App Service Plans"

                    deploymentNode "Health Checks UI" {
                        description ""
                        technology "App Service"
                        tags "Microsoft Azure - App Services"

                        hcAppInstance = containerInstance hcApp
                    }
                    deploymentNode "GitHub API" {
                        description ""
                        technology "App Service"
                        tags "Microsoft Azure - Function Apps"

                        gitHubStatusApiInstance = containerInstance gitHubStatusApi
                    }
                    deploymentNode "Platform Tools BFF" {
                        description ""
                        technology "App Service"
                        tags "Microsoft Azure - Function Apps"

                        platformToolsBffAppInstance = containerInstance platformToolsBffApp
                    }
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

                        ediInstance = containerInstance edi
                    }
                }

                deploymentNode "Shared SQL Server" {
                    description ""
                    technology "Azure SQL Server"
                    tags "Microsoft Azure - SQL Server"

                    deploymentNode "EDI Database" {
                        description ""
                        technology "SQL Database"
                        tags "Microsoft Azure - SQL Database"

                        ediDbInstance = containerInstance ediDb
                    }

                    deploymentNode "Elastic Pool" {
                        description ""
                        technology "SQL Elastic Pool"
                        tags "Microsoft Azure - SQL Elastic Pools"

                        deploymentNode "Platform Tools DB" {
                            description ""
                            technology "SQL Database"
                            tags "Microsoft Azure - SQL Database"

                            platformDbInstance = containerInstance platformDb
                        }
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
                    }
                }

                deploymentNode "Drop Zone" {
                    description ""
                    technology "Azure Storage Account"
                    tags "Microsoft Azure - Storage Accounts"

                    dh2DropZoneStorageInstance = containerInstance dh2DropZoneStorage
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
        }

        container dh3 "All_simple" {
            title "[Container] DataHub 3.0 (Simplified, no OAuth)"
            description "Simplified 'as-is' view of all domains, which excludes both 'Intermediate Technology' elements and 'OAuth' relationships"
            include *
            exclude "relationship.tag==Deployment Diagram"
            exclude "relationship.tag==OAuth"
            exclude "element.tag==Intermediate Technology"
        }

        container dh3 "PlatformTools" {
            title "[Container] DataHub 3.0 - Platform Tools (Detailed with OAuth)"
            include ->platformToolsDomain->
            exclude "relationship.tag==Deployment Diagram"
            exclude "relationship.tag==Simple View"
        }

        container dh3 "Frontend" {
            title "[Container] DataHub 3.0 - Frontend (Detailed with OAuth)"
            include ->frontendDomain->
            exclude "relationship.tag==Deployment Diagram"
            exclude "relationship.tag==Simple View"
        }

        container dh3 "Migration" {
            title "[Container] DataHub 3.0 - Migration (Detailed with OAuth)"
            include ->migrationDomain->
            exclude "relationship.tag==Deployment Diagram"
            exclude "relationship.tag==Simple View"
        }

        container dh3 "Wholesale" {
            title "[Container] DataHub 3.0 - Wholesale (Detailed with OAuth)"
            include ->wholesaleDomain->
            exclude "relationship.tag==Deployment Diagram"
            exclude "relationship.tag==Simple View"
        }

        container dh3 "Market_Participant" {
            title "[Container] DataHub 3.0 - Market Participant (Detailed with OAuth)"
            include ->markpartDomain->
            exclude "relationship.tag==Deployment Diagram"
            exclude "relationship.tag==Simple View"
        }

        container dh3 "EDI" {
            title "[Container] DataHub 3.0 - EDI (Detailed with OAuth)"
            include ->ediDomain->
            exclude "relationship.tag==Deployment Diagram"
            exclude "relationship.tag==Simple View"
        }

        container dh3 "Esett_Exchange" {
            title "[Container] DataHub 3.0 - Esett Exchange (Detailed with OAuth)"
            include ->eSettDomain->
            exclude "relationship.tag==Deployment Diagram"
            exclude "relationship.tag==Simple View"
        }

        deployment dh3 "Production (prod_001)" "Production_no_OAuth" {
            title "[Deployment] DataHub 3.0 (Detailed, no OAuth)"
            description "Detailed 'as-is' view of all domains and infrastructure dependencies. Excludes 'OAuth' relationships"
            include *
            exclude "relationship.tag==Container Diagram"
            exclude "relationship.tag==OAuth"
            exclude "relationship.tag==Simple View"
        }

        deployment dh3 "Production (prod_001)" "Production_focus_OAuth" {
            title "[Deployment] DataHub 3.0 Azure AD B2C"
            description "'As-is' view of all relationships to and from the Azure AD B2C"
            include ->sharedB2CInstance->
            exclude "relationship.tag==Container Diagram"
            exclude "relationship.tag==Simple View"
        }
    }
}
