# Read description in the 'views.dsl' file.

wholesaleDomain = group "Wholesale" {
    wholesaleDataLake = container "Wholesale DataLake" {
        description "Contains calculation inputs and results"
        technology "Azure Data Lake Gen 2"
        tags "Data Storage" "Microsoft Azure - Data Lake Store Gen1" "Mandalorian"
    }
    wholesaleCalculator = container "Calculation Engine" {
        description "Executes calculations"
        technology "Azure Databricks"
        tags "Microsoft Azure - Azure Databricks" "Mandalorian"

        # Subsystem relationships
        this -> wholesaleDataLake "Read inputs / write results"
    }
    wholesaleDb = container "Wholesale Database" {
        description "Stores calculations and orchestration data"
        technology "SQL Database Schema"
        tags "Data Storage" "Microsoft Azure - SQL Database" "Mandalorian"
    }
    wholesaleApi = container "Wholesale API" {
        description "Backend server providing external web API for wholesale operations"
        technology "Asp.Net Core Web API"
        tags "Microsoft Azure - App Services" "Mandalorian"

        # Base model relationships
        this -> dh3.sharedServiceBus "Sends calculations" "integration event/amqp"

        # Subsystem relationships
        this -> wholesaleDb "Uses" "EF Core"
        this -> wholesaleCalculator "Sends requests to"
        this -> wholesaleDataLake "Retrieves results from"

        # Subsystem-to-Subsystem relationships
        this -> edi "Sends calculations" "integration event/amqp" {
            tags "Simple View"
        }
    }
}

