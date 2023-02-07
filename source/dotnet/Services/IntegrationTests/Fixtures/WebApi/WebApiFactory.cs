// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Energinet.DataHub.Wholesale.Application;
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Application.Batches.Model;
using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Application.ProcessStep;
using Energinet.DataHub.Wholesale.Application.ProcessStep.Model;
using Energinet.DataHub.Wholesale.Application.SettlementReport;
using Energinet.DataHub.Wholesale.Domain.ActorAggregate;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.BatchExecutionStateDomainService;
using Energinet.DataHub.Wholesale.Domain.CalculationDomainService;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using Energinet.DataHub.Wholesale.Domain.SettlementReportAggregate;
using Energinet.DataHub.Wholesale.WebApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Energinet.DataHub.Wholesale.IntegrationTests.Fixtures.WebApi;

/// <summary>
/// When we execute the tests on build agents we use the builded output (assemblies).
/// To avoid an 'System.IO.DirectoryNotFoundException' exception from WebApplicationFactory
/// during creation, we must set the path to the 'content root' using an environment variable
/// named 'ASPNETCORE_TEST_CONTENTROOT_ENERGINET_DATAHUB_WHOLESALE_WEBAPI'.
/// </summary>
public class WebApiFactory : WebApplicationFactory<Startup>
{
    private bool _authenticationEnabled;

    /// <summary>
    /// Integration tests run without authentication, unless explicitly enabled.
    /// </summary>
    public void ReenableAuthentication()
    {
        _authenticationEnabled = true;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        // This can be used for changing registrations in the container (e.g. for mocks).
        builder.ConfigureServices(services =>
        {
            if (!_authenticationEnabled)
            {
                services.AddSingleton<IAuthorizationHandler>(new AllowAnonymous());
            }

            services.AddScoped(
                provider =>
                {
                    if (SettlementReportApplicationServiceMock != null)
                        return SettlementReportApplicationServiceMock.Object;

                    var batchRepository = provider.GetRequiredService<IBatchRepository>();
                    var settlementReportRepository = provider.GetRequiredService<ISettlementReportRepository>();
                    var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
                    return new SettlementReportApplicationService(batchRepository, settlementReportRepository, unitOfWork);
                });

            services.AddScoped(
                provider =>
                {
                    if (ProcessStepApplicationServiceMock != null)
                        return ProcessStepApplicationServiceMock.Object;

                    return new ProcessStepApplicationService(
                        provider.GetRequiredService<IProcessStepResultRepository>(),
                        provider.GetRequiredService<IProcessStepResultMapper>(),
                        provider.GetRequiredService<IActorRepository>());
                });

            services.AddScoped(
                provider =>
                {
                    if (BatchApplicationServiceMock != null)
                        return BatchApplicationServiceMock.Object;

                    return new BatchApplicationService(
                        provider.GetRequiredService<IBatchFactory>(),
                        provider.GetRequiredService<IBatchRepository>(),
                        provider.GetRequiredService<IUnitOfWork>(),
                        provider.GetRequiredService<ICalculationDomainService>(),
                        provider.GetRequiredService<IBatchExecutionStateDomainService>(),
                        provider.GetRequiredService<IBatchDtoMapper>(),
                        provider.GetRequiredService<IProcessTypeMapper>(),
                        provider.GetRequiredService<IDomainEventPublisher>());
                });
        });

        ConfigureEnvironmentVars();
    }

    private static void ConfigureEnvironmentVars()
    {
        const string anyValue = "fake_value";
        var anyServiceBusConnectionString = "Endpoint=sb://foo.servicebus.windows.net/;SharedAccessKeyName=someKeyName;SharedAccessKey=someKeyValue";

        Environment.SetEnvironmentVariable(EnvironmentSettingNames.AppInsightsInstrumentationKey, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.BackendAppId, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.ExternalOpenIdUrl, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.InternalOpenIdUrl, anyValue);
        Environment.SetEnvironmentVariable($"CONNECTIONSTRINGS:{EnvironmentSettingNames.DbConnectionString}", "UseDevelopmentStorage=true");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.CalculationStorageConnectionString, "UseDevelopmentStorage=true");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.CalculationStorageContainerName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.ServiceBusSendConnectionString, anyServiceBusConnectionString);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.ServiceBusManageConnectionString, anyServiceBusConnectionString);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.BatchCreatedEventName, "batch-created");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DomainEventsTopicName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DateTimeZoneId, "Europe/Copenhagen");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DatabricksWorkspaceUrl, "http://localhost/");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DatabricksWorkspaceToken, "no_token");
    }

    /// <summary>
    /// Allow configuring the behaviour of the <see cref="ISettlementReportApplicationService"/> by providing a custom <see cref="Moq.Mock{ISettlementReportApplicationService}" /> mock.
    /// NOTE: This will only work as expected as long as no tests are executed in parallel.
    /// </summary>
    public Mock<ISettlementReportApplicationService>? SettlementReportApplicationServiceMock { get; set; }

    public Mock<IProcessStepApplicationService>? ProcessStepApplicationServiceMock { get; set; }

    public Mock<IBatchApplicationService>? BatchApplicationServiceMock { get; set; }

    private sealed class AllowAnonymous : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (var requirement in context.PendingRequirements.ToList())
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
