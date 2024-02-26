# Read description in the 'views.dsl' file.

wholesaleDomain = group "Wholesale" {
    wholesaleDataLake = container "Wholesale DataLake" {
        description "Calculations inputs and results"
        technology "Azure Data Lake Gen 2"
        tags "Data Storage" "Microsoft Azure - Data Lake Store Gen1" "Mandalorian"
    }
    wholesaleCalculator = container "Calculation Engine" {
        description "Executes calculation job"
        technology "Azure Databricks"
        tags "Microsoft Azure - Azure Databricks" "Mandalorian"

        # Subsystem relationships
        this -> wholesaleDataLake "Read inputs / write results"
    }
    wholesaleDb = container "Wholesale Database" {
        description "Meta data of calculations and orchestrations"
        technology "SQL Database Schema"
        tags "Data Storage" "Microsoft Azure - SQL Database" "Mandalorian"
    }
    wholesaleApi = container "Wholesale API" {
        description "Backend server providing external web API for Wholesale subsystem"
        technology "Asp.Net Core Web API"
        tags "Microsoft Azure - App Services" "Mandalorian"

        # Base model relationships
        dh3.sharedServiceBus -> this "Listens on Wholesale Inbox + Integration Events" "message/amqp"
        this -> dh3.sharedServiceBus "Sends to EDI Inbox" "message/amqp"

        # Subsystem relationships
        this -> wholesaleDb "Uses" "EF Core"
        this -> wholesaleDataLake "Retrieves results from"

        # Subsystem-to-Subsystem relationships
        # CONSIDER: Should live in EDI model(?)
        edi -> this "Sends to Wholesale Inbox" "message/amqp" {
            tags "Simple View"
        }
        this -> edi "Sends to EDI Inbox" "message/amqp" {
            tags "Simple View"
        }
    }
    wholesaleOrchestration = container "Wholesale Orchestration" {
        description "Orchestrate calculation workflow"
        technology "Azure function, C#"
        tags "Microsoft Azure - Function Apps" "Mandalorian"

        # Base model relationships
        this -> dh3.sharedServiceBus "Publish calculation results" "integration event/amqp"

        # Subsystem relationships
        this -> wholesaleDb "Uses" "EF Core"
        this -> wholesaleCalculator "Sends requests to"
        this -> wholesaleDataLake "Retrieves results from"

        # Subsystem-to-Subsystem relationships
        # CONSIDER: Should live in EDI model(?)
        this -> edi "Publish calculation results" "integration event/amqp" {
            tags "Simple View", "Wholesale Subscription"
        }
        # CONSIDER: Each subsystem model could exclude subscriptions based on their name; e.g. if Market Participant doesn't want to see subscribers they can exclude "MarketParticipant Subscription"
        markpartOrganizationManager -> this "Publish Grid Area Ownership Assigned" "integration event/amqp" {
            tags "Simple View", "MarketParticipant Subscription"
        }
    }
}

