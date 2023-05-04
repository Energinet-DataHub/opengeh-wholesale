# Read description in the 'views.dsl' file.

wholesaleDomain = group "Wholesale" {
    wholesaleDataLake = container "Data Lake (Wholesale)" {
        description "Stores batch results"
        technology "Azure Data Lake Gen 2"
        tags "Data Storage" "Microsoft Azure - Data Lake Store Gen1"
    }
    wholesaleCalculator = container "Calculation Engine" {
        description "<add description>"
        technology "Azure Databricks"
        tags "Microsoft Azure - Azure Databricks"

        # Domain relationships
        this -> wholesaleDataLake "write to"
    }
    wholesaleDb = container "Wholesale Database" {
        description "<add description>"
        technology "SQL Database Schema"
        tags "Data Storage" "Microsoft Azure - SQL Database"
    }
    wholesaleApi = container "Wholesale API" {
        description "<add description>"
        technology "Asp.Net Core Web API"
        tags "Microsoft Azure - App Services"

        # Base model relationships
        this -> dh3.sharedServiceBus "publishes events"

        # Domain relationships
        this -> wholesaleDb "uses" "EF Core"
        this -> wholesaleCalculator "sends requests to"
        this -> wholesaleDataLake "retrieves results from"

        # Domain-to-domain relationships
        this -> ediTimeSeriesListener "notify of <event>" "message/amqp" {
            tags "Simple View"
        }
    }
    wholesaleProcessManager = container "Process Manager" {
        description "<add description>"
        technology "Azure function, C#"
        tags "Microsoft Azure - Function Apps"

        # Base model relationships
        this -> dh3.sharedServiceBus "listens to events"

        # Domain relationships
        this -> wholesaleApi "uses (directly)"
    }

    # Domain relationships (circular references)
    wholesaleApi -> wholesaleProcessManager "notify of <event>" "message/amqp" {
        tags "Simple View"
    }
}

