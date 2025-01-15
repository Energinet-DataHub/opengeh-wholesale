# Read description in the 'views.dsl' file.

wholesaleSubsystem = group "Wholesale" {
    wholesaleDataLake = container "Wholesale DataLake" {
        description "Calculation inputs and results"
        technology "Azure Data Lake Gen 2"
        tags "Data Storage" "Microsoft Azure - Data Lake Store Gen1" "Mandalorian"

        # Relations to shared
        dh3.sharedUnityCatalog -> this "Read data / write data"
    }

    wholesaleDatabricksWorkspace = group "Databricks Workspace" {
        wholesaleCalculatorJob = container "Calculator Job" {
            description "Executes calculations"
            technology "Azure Databricks"
            tags "Microsoft Azure - Azure Databricks" "Mandalorian"

            # Subsystem relationships
            this -> dh3.sharedUnityCatalog "Read inputs / write results"
            this -> wholesaleDataLake "Read inputs / write results" {
                tags "Simple View"
            }
        }
        wholesaleMigrationJob = container "Migration Job" {
            description "Executes delta migrations (invoked during deployment)"
            technology "Azure Databricks"
            tags "Microsoft Azure - Azure Databricks" "Mandalorian"

            # Subsystem relationships
            this -> dh3.sharedUnityCatalog "Migrate database objects and data"
            this -> wholesaleDataLake "Read inputs / write results" {
                tags "Simple View"
            }
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
        this -> dh3.sharedServiceBus "Subscribes to integration events" "integration event/amqp"

        # Subsystem relationships
        this -> wholesaleDb "Uses" "EF Core"
        this -> wholesaleRuntimeWarehouse "Retrieves results from"
        this -> wholesaleDataLake "Retrieves results from" {
            tags "Simple View"
        }
    }
    wholesaleOrchestrations = container "Wholesale Orchestrations" {
        description "Orchestrate calculation workflow, generate settlement reports"
        technology "Azure function, C#"
        tags "Microsoft Azure - Function Apps" "Mandalorian"

        # Base model relationships
        this -> dh3.sharedServiceBus "Listens on Wholesale Inbox queue" "message/amqp"

        # Subsystem relationships
        this -> wholesaleDb "Uses" "EF Core"
        this -> wholesaleCalculatorJob "Invokes"
        this -> wholesaleRuntimeWarehouse "Retrieves results from"
        this -> wholesaleDataLake "Retrieves results from" {
            tags "Simple View"
        }
    }
}
