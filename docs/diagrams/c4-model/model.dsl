# Read description in the 'views.dsl' file.

wholesaleDomain = group "Wholesale" {
    wholesaleDataLake = container "Data Lake (Wholesale)" {
        description "Stores calculation results"
        technology "Azure Data Lake Gen 2"
        tags "Data Storage" "Microsoft Azure - Data Lake Store Gen1" "Mandalorian"
    }
    wholesaleCalculator = container "Calculation Engine" {
        description "Executes calculations"
        technology "Azure Databricks"
        tags "Microsoft Azure - Azure Databricks" "Mandalorian"

        # Domain relationships
        this -> wholesaleDataLake "Read / write"
    }
    wholesaleDb = container "Wholesale Database" {
        description "Stores calculations and operations data"
        technology "SQL Database Schema"
        tags "Data Storage" "Microsoft Azure - SQL Database" "Mandalorian"
    }
    wholesaleApi = container "Wholesale API" {
        description "Backend server providing external web API for wholesale operations"
        technology "Asp.Net Core Web API"
        tags "Microsoft Azure - App Services" "Mandalorian"

        # Base model relationships
        this -> dh3.sharedServiceBus "Sends calculations" "integration event/amqp"

        # Domain relationships
        this -> wholesaleDb "Uses" "EF Core"
        this -> wholesaleCalculator "Sends requests to"
        this -> wholesaleDataLake "Retrieves results from"

        # Domain-to-domain relationships
        this -> edi "Sends calculations" "integration event/amqp" {
            tags "Simple View"
        }
    }
}

