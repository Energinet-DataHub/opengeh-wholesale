# Model platform tools like Health Checks UI.

toolDomain = group "Platform Tools" {
    hcDb = container "Health Checks Database" {
        description "Stores health status history."
        technology "SQL Database Schema"
        tags "Data Storage" "Microsoft Azure - SQL Database" "Outlaws"
    }
    hcApp = container "Health Checks UI" {
        description "Web App (UI) for monitoring health checks."
        technology "Asp.Net Core Web API"
        tags "Microsoft Azure - App Services" "Outlaws"

        # Domain relationships
        this -> hcDb "Reads and writes health status." "EF Core"
    }
}
