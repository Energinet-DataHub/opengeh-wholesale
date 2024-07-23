# Read description in the 'views.dsl' file.

wholesaleSubsystem = group "Wholesale" {
    wholesaleDataLake = container "Wholesale DataLake" {
        description "Calculation inputs and results"
        technology "Azure Data Lake Gen 2"
        tags "Data Storage" "Microsoft Azure - Data Lake Store Gen1" "Mandalorian"
    }
    wholesaleBlobStorage = container "Settlement Report Blob Storage" {
        description "Contains (drafts of) settlement reports"
        technology "Azure Blob Storage"
        tags "Data Storage" "Raccoons"
    }
    wholesaleCalculator = container "Calculation Engine" {
        description "Executes calculation job"
        technology "Azure Databricks"
        tags "Microsoft Azure - Azure Databricks" "Mandalorian"

        # Subsystem relationships
        this -> wholesaleDataLake "Read inputs / write results"
    }
    wholesaleDeploymentWarehouse = container "Deployment Warehouse" {
        description "Executes delta SQL migrations"
        technology "Azure Databricks SQL Warehouse"
        tags "Microsoft Azure - Azure Databricks" "Mandalorian"

        # Subsystem relationships
        this -> wholesaleDataLake "Read executed migrations / execute new migrations"
    }
    wholesaleRuntimeWarehouse = container "Runtime Warehouse" {
        description "Executes delta SQL queries"
        technology "Azure Databricks SQL Warehouse"
        tags "Microsoft Azure - Azure Databricks" "Mandalorian"

        # Subsystem relationships
        this -> wholesaleDataLake "Read basis data and results"
        edi -> this "Read calculation results and active data"
    }
    wholesaleDb = container "Wholesale Database" {
        description "Meta data of calculations"
        technology "SQL Database Schema"
        tags "Data Storage" "Microsoft Azure - SQL Database" "Mandalorian"
    }
    wholesaleApi = container "Wholesale API" {
        description "Backend server providing external web API for Wholesale subsystem"
        technology "Asp.Net Core Web API"
        tags "Microsoft Azure - App Services" "Mandalorian" "MarketParticipant Subscriber"

        # Base model relationships
        dh3.sharedServiceBus -> this "Listens on Wholesale Inbox + Integration Events" "message/amqp"
        this -> dh3.sharedServiceBus "Sends to EDI Inbox" "message/amqp"

        # Subsystem relationships
        this -> wholesaleDb "Uses" "EF Core"
        this -> wholesaleRuntimeWarehouse "Retrieves results from"

        # Subsystem-to-Subsystem relationships
        edi -> this "Sends to Wholesale Inbox" "message/amqp" {
            tags "Simple View"
        }
        markpartOrganizationManager -> this "Publish Grid Area Ownership Assigned" "integration event/amqp" {
            tags "Simple View"
        }
        this -> edi "Sends to EDI Inbox" "message/amqp" {
            tags "Simple View"
        }
    }
    wholesaleOrchestrations = container "Wholesale Orchestrations" {
        description "Orchestrate calculation workflow, generate settlement reports"
        technology "Azure function, C#"
        tags "Microsoft Azure - Function Apps" "Mandalorian"

        # Base model relationships
        this -> dh3.sharedServiceBus "Publish calculation completed" "integration event/amqp"

        # Subsystem relationships
        this -> wholesaleDb "Uses" "EF Core"
        this -> wholesaleCalculator "Sends requests to"
        this -> wholesaleDataLake "Retrieves results from"
        this -> wholesaleBlobStorage "Reads from and writes settlement reports to"

        # Subsystem-to-Subsystem relationships
        this -> edi "Publish calculation completed" "integration event/amqp" {
            tags "Simple View"
        }
    }
}
