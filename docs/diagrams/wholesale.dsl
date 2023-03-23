workspace extends https://raw.githubusercontent.com/Energinet-DataHub/opengeh-arch-diagrams/main/source/datahub3-model/model.dsl {
    model {
        !ref dh3 {
            dh3WebApp = group "Web App" {
                frontend = container "UI" "Provides DH3 functionality to users via their web browser." "Angular"
                bff = container "Backend for frontend" "Combines data for presentation on DataHub 3 UI" "Asp.Net Core Web API"

                frontend -> bff "Uses" "JSON/HTTPS"
            }

            messageBus = container "Message Bus" "" "Azure Message Bus" "Microsoft Azure - Azure Service Bus"
            migration = container "Migration" "" ""

            wholesale = group "Wholesale" {
                wholesaleApi = container "Wholesale API" "" "Asp.Net Core Web API" "" 
                wholesaleDb = container "Database" "" "MS SQL Server" "Data Storage, Microsoft Azure - SQL Database" 

                wholesaleProcessManager = container "Process Manager" "" "Azure Function App" "Microsoft Azure - Function Apps"

                wholesaleStorage = container "Data Lake" "Stores batch results" "Azure Data Lake Gen 2" "Microsoft Azure - Data Lake Store Gen1"
                wholesaleCalculator = container "Calculation Engine" "" "Databricks" "Microsoft Azure - Azure Databricks"

                wholesaleApi -> wholesaleDb "uses"
                wholesaleApi -> wholesaleStorage "retrieves results from"
                wholesaleApi -> messageBus "publishes events"
                wholesaleProcessManager -> wholesaleApi "uses (directly)"
                wholesaleApi -> wholesaleCalculator "sends requests to"
                wholesaleCalculator -> wholesaleStorage "write to"
                migration -> wholesaleStorage "writes to" 
                wholesaleProcessManager -> messageBus "listens to events"
            }
        }
        # Relationships to/from containers
        dh3User -> frontend "View and start jobs using"
        extUser -> frontend "View and start jobs using"
        bff -> wholesaleApi "uses" "JSON/HTTPS"
    }

    views {
        container dh3 {
            include *
            autolayout lr
        }
   }
}
