# Read description in the 'views.dsl' file.

wholesaleSubsystem = group "Wholesale" {
    wholesaleDataLake = container "Wholesale DataLake" {
        description "Calculation inputs and results"
        technology "Azure Data Lake Gen 2"
        tags "Data Storage" "Microsoft Azure - Data Lake Store Gen1" "Mandalorian"

        # Relations to shared
        dh3.sharedUnityCatalog -> this "Read data / write data"
    }
    wholesaleCalculatorJob = container "Calculator Job" {
        description "Executes calculations"
        technology "Azure Databricks"
        tags "Microsoft Azure - Azure Databricks" "Mandalorian"

        # Subsystem relationships
        this -> dh3.sharedUnityCatalog "Read inputs / write results"
    }
    wholesaleMigrationJob = container "Migration Job" {
        description "Executes delta migrations"
        technology "Azure Databricks"
        tags "Microsoft Azure - Azure Databricks" "Mandalorian"

        # Subsystem relationships
        this -> dh3.sharedUnityCatalog "Migrate database objects and data"
    }
    wholesaleDeploymentWarehouse = container "Deployment Warehouse" {
        description "Executes delta SQL migrations"
        technology "Azure Databricks SQL Warehouse"
        tags "Microsoft Azure - Azure Databricks" "Mandalorian" "Intermediate Technology"

        # Subsystem relationships
        this -> dh3.sharedUnityCatalog "Read/write executed migrations"
    }
    wholesaleRuntimeWarehouse = container "Runtime Warehouse" {
        description "Executes delta SQL queries (also used by EDI)"
        technology "Azure Databricks SQL Warehouse"
        tags "Microsoft Azure - Azure Databricks" "Mandalorian" "Mosaic" "Intermediate Technology"

        # Subsystem relationships
        this -> dh3.sharedUnityCatalog "Read results"
    }
    settlementReportsWarehouse = container "Settlement Reports Warehouse" {
        description "Executes delta SQL queries"
        technology "Azure Databricks SQL Warehouse"
        tags "Microsoft Azure - Azure Databricks" "Mandalorian" "Raccoons" "Intermediate Technology"

        # Subsystem relationships
        this -> dh3.sharedUnityCatalog "Read basis data and results"
    }

    # TODO: Split into subsystem areas?

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
        this -> wholesaleRuntimeWarehouse "Retrieves results from"

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
        this -> wholesaleCalculatorJob "Invokes"
        this -> wholesaleRuntimeWarehouse "Retrieves results from"
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
