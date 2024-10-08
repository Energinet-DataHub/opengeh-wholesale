﻿// Copyright 2020 Energinet DataHub A/S
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

using System.Reflection;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Energinet.DataHub.Core.App.Common.Extensions.DependencyInjection;
using Energinet.DataHub.Core.App.WebApp.Extensions.Builder;
using Energinet.DataHub.Core.App.WebApp.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Security;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Telemetry;
using Energinet.DataHub.Wholesale.Common.Interfaces.Security;
using Energinet.DataHub.Wholesale.WebApi;
using Energinet.DataHub.Wholesale.WebApi.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

/*
// Add services to the container.
*/

// Common
builder.Services.AddApplicationInsightsForWebApp(TelemetryConstants.SubsystemName);
builder.Services.AddHealthChecksForWebApp();

// Shared by modules
builder.Services.AddNodaTimeForApplication();
builder.Services.AddServiceBusClientForApplication(builder.Configuration);

// Modules
builder.Services.AddCalculationsModule(builder.Configuration);
builder.Services.AddCalculationResultsModule(builder.Configuration);

// ServiceBus channels
builder.Services.AddIntegrationEventsSubscription(builder.Configuration);

// Http channels
builder.Services
    .AddControllers(options => options.Filters.Add<BusinessValidationExceptionFilter>());

// => Open API generation
builder.Services.AddSwaggerForWebApp(Assembly.GetExecutingAssembly(), swaggerUITitle: "Wholesale Web API");

// => API versioning
builder.Services.AddApiVersioningForWebApp(new ApiVersion(3, 0));

// => Authentication/authorization
builder.Services
    .AddJwtBearerAuthenticationForWebApp(builder.Configuration)
    .AddUserAuthenticationForWebApp<FrontendUser, FrontendUserProvider>()
    .AddPermissionAuthorizationForWebApp();

var app = builder.Build();

/*
// Configure the HTTP request pipeline.
*/

app.UseRouting();
app.UseSwaggerForWebApp();
app.UseHttpsRedirection();

// Authentication/authorization
app.UseAuthentication();
app.UseAuthorization();
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseUserMiddlewareForWebApp<FrontendUser>();
}

app.MapControllers().RequireAuthorization();

// Health check
app.MapLiveHealthChecks();
app.MapReadyHealthChecks();
app.MapStatusHealthChecks();

app.Run();

// Enable testing
public partial class Program { }
