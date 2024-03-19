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

using System.Text.Json.Serialization;
using Asp.Versioning;
using Energinet.DataHub.Core.App.WebApp.Authentication;
using Energinet.DataHub.Core.App.WebApp.Authorization;
using Energinet.DataHub.Core.App.WebApp.Diagnostics.HealthChecks;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Security;
using Energinet.DataHub.Wholesale.Edi.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.WebApi;
using Energinet.DataHub.Wholesale.WebApi.Extensions.Builder;
using Energinet.DataHub.Wholesale.WebApi.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

/*
// Add services to the container.
*/

// Common
builder.Services.AddApplicationInsightsForWebApp();
builder.Services.AddHealthChecksForWebApp();

// Shared by modules
builder.Services.AddNodaTimeForApplication(builder.Configuration);
builder.Services.AddDatabricksJobsForApplication(builder.Configuration);
builder.Services.AddServiceBusClientForApplication(builder.Configuration);

// Modules
builder.Services.AddCalculationsModule(builder.Configuration);
builder.Services.AddCalculationResultsModule(builder.Configuration);

// ServieBus channels
builder.Services.AddIntegrationEventsSubscription();
builder.Services.AddInboxHandling();
builder.Services.AddEdiModule();

// Http channels
builder.Services
    .AddControllers(options => options.Filters.Add<BusinessValidationExceptionFilter>())
    .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

// => Open API generation
builder.Services.AddSwaggerForWebApp();

// => API versioning
builder.Services.AddApiVersioningForWebApp(new ApiVersion(3, 0));

// => Authentication/authorization
builder.Services
    .AddTokenAuthenticationForWebApp(builder.Configuration)
    .AddUserAuthenticationForWebApp<FrontendUser, FrontendUserProvider>()
    .AddPermissionAuthorization();

var app = builder.Build();

/*
// Configure the HTTP request pipeline.
*/

app.UseRouting();
app.UseSwaggerForWebApplication();
app.UseHttpsRedirection();

// Authentication/authorization
app.UseAuthentication();
app.UseAuthorization();
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseUserMiddleware<FrontendUser>();
}

app.MapControllers().RequireAuthorization();

// Health check
app.MapLiveHealthChecks();
app.MapReadyHealthChecks();

app.Run();

// Enable testing
public partial class Program { }
