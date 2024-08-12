# Read description in the 'views.dsl' file.

wholesaleSubsystem = group "Wholesale" {
    wholesaleCalculator = container "Calculation Engine" {
        description "Executes calculation job"
        technology "Azure Databricks"
        tags "Microsoft Azure - Azure Databricks" "Mandalorian"

        # Subsystem relationships
        this -> sharedUnityCatalog "Read inputs / write results"
    }
    wholesaleDeploymentWarehouse = container "Deployment Warehouse" {
        description "Executes delta SQL migrations"
        technology "Azure Databricks SQL Warehouse"
        tags "Microsoft Azure - Azure Databricks" "Mandalorian" "Intermediate Technology"

        # Subsystem relationships
        this -> sharedUnityCatalog "Read/write executed migrations"
    }
    ediWarehouse = container "EDI Warehouse" {
        description "Executes delta SQL queries"
        technology "Azure Databricks SQL Warehouse"
        tags "Microsoft Azure - Azure Databricks" "Mandalorian" "Mosaic" "Intermediate Technology"

        # Subsystem relationships
        this -> sharedUnityCatalog "Read results"
        edi -> this "Read calculation results and active data"
    }
    settlementReportsWarehouse = container "Settlement Reports Warehouse" {
        description "Executes delta SQL queries"
        technology "Azure Databricks SQL Warehouse"
        tags "Microsoft Azure - Azure Databricks" "Mandalorian" "Raccoons" "Intermediate Technology"

        # Subsystem relationships
        this -> sharedUnityCatalog "Read basis data and results"
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
        dh3.sharedServiceBus -> this "Subscribes to Integration Events" "integration event/amqp"
        this -> dh3.sharedServiceBus "Sends to EDI Inbox" "message/amqp"

        # Subsystem relationships
        this -> wholesaleDb "Uses" "EF Core"
        this -> ediWarehouse "Retrieves results from"

        # Subsystem-to-Subsystem relationships
        markpartOrganizationManager -> this "Publish Grid Area Ownership Assigned" "integration event/amqp" {
            tags "Simple View"
        }
    }
    wholesaleOrchestrations = container "Wholesale Orchestrations" {
        description "Orchestrate calculation workflow, generate settlement reports"
        technology "Azure function, C#"
        tags "Microsoft Azure - Function Apps" "Mandalorian"

        # Base model relationships
        this -> dh3.sharedServiceBus "Publish calculation completed and grid loss" "integration event/amqp"

        # Subsystem relationships
        this -> wholesaleDb "Uses" "EF Core"
        this -> wholesaleCalculator "Sends requests to"
        this -> ediWarehouse "Retrieves results from"
        this -> wholesaleBlobStorage "Reads from and writes settlement reports to"

        # Subsystem-to-Subsystem relationships
        edi -> this "Sends to Wholesale Inbox" "message/amqp" {
            tags "Simple View"
        }
        this -> edi "Publish calculation completed and sends to EDI Inbox" "message/amqp" {
            tags "Simple View"
        }
    }
}
