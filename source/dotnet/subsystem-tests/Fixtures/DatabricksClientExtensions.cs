using Microsoft.Azure.Databricks.Client;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Fixtures;

/// <summary>
/// Convenient methods for using the <see cref="DatabricksClient"/>.
///
/// If we need to make several calls using the <see cref="DatabricksClient"/>,
/// we should not use these extensions but instead create and maintain an
/// instance of the client in the class where we use it.
///
/// Client documentation: https://github.com/Azure/azure-databricks-client
/// </summary>
public static class DatabricksClientExtensions
{
    /// <summary>
    /// Retrieve calculator job id from databricks.
    /// </summary>
    public static async Task<long> GetCalculatorJobIdAsync(this DatabricksClient databricksClient)
    {
        var job = await databricksClient.Jobs
            .ListPageable(name: "CalculatorJob")
            .SingleAsync();

        return job.JobId;
    }
}
