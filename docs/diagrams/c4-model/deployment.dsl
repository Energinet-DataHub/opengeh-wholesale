# Extends the core DataHub 3 model to build a deployment diagram showing shared components and services used by product teams

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


            ##################################################################################
            # Includes below require a token because its located in a private repository     #
            # Run this file to open a browser on each model.dsl file easing token copy/paste #
            #                                                                                #
            #  ------->  docs\diagrams\c4-model\Open-ModelDslFiles.ps1 <------               #
            #                                                                                #
            ##################################################################################

            # Include Esett Exchange model - requires a token because its located in a private repository
            # Token is automatically appended in "Raw" view of the file
            !include https://raw.githubusercontent.com/Energinet-DataHub/opengeh-esett-exchange/main/docs/diagrams/c4-model/model.dsl?token=GHSAT0AAAAAACIBG65TDZKOKLZCPINAULFCZSNZBOQ

            # Include Grid Loss Imbalance Prices model - requires a token because its located in a private repository
            # Token is automatically appended in "Raw" view of the file
            !include https://raw.githubusercontent.com/Energinet-DataHub/opengeh-grid-loss-imbalance-prices/main/docs/diagrams/c4-model/model.dsl?token=GHSAT0AAAAAACIBG65TPDUN7P4OBZ4TYW4UZSNZCJQ

            # Include Migration model - requires a token because its located in a private repository
            # Token is automatically appended in "Raw" view of the file
            !include https://raw.githubusercontent.com/Energinet-DataHub/opengeh-migration/main/docs/diagrams/c4-model/model.dsl?token=GHSAT0AAAAAACIBG65T5XI3WUPPIV6UCG6CZSNZC7Q

            # Include Sauron - requires a token because its located in a private repository
            # Token is automatically appended in "Raw" view of the file
            !include https://raw.githubusercontent.com/Energinet-DataHub/dh3-operations/main/docs/diagrams/c4-model/model.dsl?token=GHSAT0AAAAAACIBG65T5RW3XWPOU7FNQMPYZSNZDPA

            # Include DH2 Bridge model - requires a token because its located in a private repository
            # Token is automatically appended in "Raw" view of the file
            !include https://raw.githubusercontent.com/Energinet-DataHub/dh2-bridge/main/docs/diagrams/c4-model/model.dsl?token=GHSAT0AAAAAACIBG65TS5HLMN4MUYXOZZLGZSNZEAQ
        }


        deploymentEnvironment "Production (prod_001)" {

            # Azure Cloud
            deploymentNode "Azure Cloud" {
                description ""
                technology "Azure Subscription"
                tags "Microsoft Azure - Subscriptions"

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

                deploymentNode "Static Web App" {
                    description ""
                    technology "Static Web App"
                    tags "Microsoft Azure - Static Apps"

                    frontendStaticWebAppInstance = containerInstance frontendStaticWebApp
                }

                deploymentNode "API Gateway" {
                    description ""
                    technology "API Management Service"
                    tags "Microsoft Azure - API Management Services"

                    bffApiInstance = containerInstance bffApi
                    ediApiInstance = containerInstance ediApi
                }

                waf = infrastructureNode "Azure Web Application Firewall" {
                    description ""
                    technology "Azure Web Application Firewall"
                    tags "Microsoft Azure - Web Application Firewall Policies(WAF)"
                }

                frontDoor = infrastructureNode "Azure Front Door" {
                    description ""
                    technology "Azure Front Door"
                    tags "Microsoft Azure - Front Door and CDN Profiles"
                    -> frontendStaticWebAppInstance "routes and protects traffic"
                    -> bffApiInstance "routes and protects traffic"
                    -> ediApiInstance "routes and protects traffic"
                    -> waf "filters, monitors, and blocks malicious traffic"
                }

                # Azure DNS
                azureDns = infrastructureNode "Azure DNS" {
                    description ""
                    technology "Azure DNS"
                    tags "Microsoft Azure - DNS Zones"
                    -> frontDoor "Routes traffic to front door"
                }

                b2c = infrastructureNode "Active Directory B2C" {
                    description "Authenticate and authorize B2C users"
                    technology "Azure AD B2C"
                    tags "Microsoft Azure - Azure AD B2C"
                }

                deploymentNode "Shared resources" {
                    infrastructureNode "Appliction Insights" {
                        description "Shared App. Insights for subproducts"
                        technology "Azure Application Insights"
                        tags "Microsoft Azure - Application Insights"
                    }

                    infrastructureNode "Log Analytics" {
                        description "Shared Log Analytics workspace"
                        technology "Azure Log Analytics"
                        tags "Microsoft Azure - Log Analytics"
                    }


                    infrastructureNode "Key Vault" {
                        description "Keyvault for exchanging sensitive information between subproducts"
                        technology "Azure Key Vault"
                        tags "Microsoft Azure - Key Vaults"
                    }

                    deploymentNode "Service Bus" {
                        description "Servicebus namespace for eventbased communication between subproducts"
                        technology "Azure Service Bus"
                        tags "Microsoft Azure - Azure Service Bus"
                    }

                    infrastructureNode "SQL Server Instance" {
                        description "Contains all databases for DataHub 3.0"
                        technology "Azure SQL Server"
                        tags "Microsoft Azure - SQL Server"
                    }

                    deploymentNode "Azure B2C tenant" {
                        description ""
                        technology "Azure AD B2C"
                        tags "Microsoft Azure - Azure AD B2C"

                        sharedB2CInstance = containerInstance dh3.sharedB2C
                    }
                }
            }
            # Computer
            deploymentNode "User's computer" {
                description ""
                technology "Microsoft Windows or Apple macOS"

                deploymentNode "Web Browser for DataHub UI" {
                    description ""
                    technology "Chrome, Firefox, Safari, or Edge"
                    tags "Microsoft Azure - Browser"

                    frontendSinglePageApplicationInstance = containerInstance frontendSinglePageApplication
                    this -> b2c "Authenticate users"
                    this -> frontDoor "All authenticated calls are routes through WAF and Front Door to DataHub 3.0"
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

        container dh3 "Sauron" {
            title "[Container] DataHub 3.0 - Sauron (Detailed with OAuth)"
            include ->sauronDomain->
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
            include ->migrationSubsystem->
            exclude "relationship.tag==Deployment Diagram"
            exclude "relationship.tag==Simple View"
        }

        container dh3 "Wholesale" {
            title "[Container] DataHub 3.0 - Wholesale (Detailed with OAuth)"
            include ->wholesaleSubsystem->
            exclude "relationship.tag==Deployment Diagram"
            exclude "relationship.tag==Simple View"
        }

        container dh3 "Market_Participant" {
            title "[Container] DataHub 3.0 - Market Participant (Detailed with OAuth)"
            include ->markpartDomain->
            exclude "relationship.tag==Deployment Diagram"
            exclude "relationship.tag==Simple View"
            exclude "element.tag==MarketParticipant Subscriber"
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

        container dh3 "DH2_Bridge" {
            title "[Container] DataHub 3.0 - DH2 Bridge (Detailed with OAuth)"
            include ->dh2BridgeDomain->
            exclude "relationship.tag==Deployment Diagram"
            exclude "relationship.tag==Simple View"
        }

        container dh3 "Grid_Loss_Imbalance_Prices" {
            title "[Container] DataHub 3.0 - Grid Loss Imbalance Prices (Detailed with OAuth) "
            include ->gridLossImbPrices->
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
