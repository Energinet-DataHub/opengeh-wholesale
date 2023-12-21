
using Energinet.DataHub.Core.App.FunctionApp.Diagnostics.HealthChecks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

public class HealthCheckEndpoint
{
    public HealthCheckEndpoint(IHealthCheckEndpointHandler healthCheckEndpointHandler)
    {
        EndpointHandler = healthCheckEndpointHandler;
    }

    private IHealthCheckEndpointHandler EndpointHandler { get; }

    [Function("HealthCheck")]
    public Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "monitor/{endpoint}")]
         HttpRequestData httpRequest,
        string endpoint)
    {
        return EndpointHandler.HandleAsync(httpRequest, endpoint);
    }
}
