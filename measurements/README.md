# Measurements - a monorepository

The Measurements subsystem contains applications developed in different product goals. To enable developers to work efficiently on different parts of the system in parallel, the different parts must be build, tested and deployed separatly. Therefore the following structure is applied in the infrastructure configuration:

- **Core** contains shared infrastructure, such as a Databricks workspace, a keyvault (for the Databricks workspace URL to be used in other provider files), and a storage account with external locations
- **Calculated Measurements** Infrastructure, such as job, relevant permissions and compute (e.g. SQL warehouses and job clusters), Unity Catalog schema(s)

In the Azure portal that means:

- Databricks related resources (jobs, workspaces, clusters etc.) is deployed in the Databricks Workspace (named "dbw-mmcore-<d/t/b/p>-we-<001/002>") found in the "core" resource group named "rg-mmcore-<d/t/b/p>-we-<001/002>".
- Infrastructure components that is not bound to a Databricks Workspace (e.g. product specific storage accounts, application hosts such as Azure WebApps), are deployed in the resource groups belonging to the products building upon the Core named e.g. "rg-mmcpset-<d/t/b/p>-we-<001/002>" (Calculated Measurements).
