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

using Energinet.DataHub.Edi.Requests;
using Energinet.DataHub.Wholesale.EDI.Client;
using Energinet.DataHub.Wholesale.EDI.Factories;
using Energinet.DataHub.Wholesale.EDI.Validation;
using Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSerie;
using Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSerie.Rules;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.EDI;

/// <summary>
/// Registration of services required for the Batches module.
/// </summary>
public static class EdiRegistration
{
    public static void AddEdiModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IAggregatedTimeSeriesRequestHandler, AggregatedTimeSeriesRequestHandler>();
        serviceCollection.AddSingleton<IEdiClient, EdiClient>();
        serviceCollection.AddScoped<IAggregatedTimeSeriesRequestFactory, AggregatedTimeSeriesRequestFactory>();
        AddAggregatedTimeSeriesRequestValidation(serviceCollection);
    }

    private static void AddAggregatedTimeSeriesRequestValidation(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IValidator<AggregatedTimeSeriesRequest>, AggregatedTimeSeriesRequestValidator>();
        serviceCollection.AddSingleton<IValidationRule<AggregatedTimeSeriesRequest>, PeriodValidationRule>();
        serviceCollection.AddSingleton<IValidationRule<AggregatedTimeSeriesRequest>, MeteringPointTypeValidationRule>();
        serviceCollection.AddSingleton<IValidationRule<AggregatedTimeSeriesRequest>, EnergySupplierFieldValidationRule>();
        serviceCollection.AddSingleton<IValidationRule<AggregatedTimeSeriesRequest>, SettlementMethodValidationRule>();
        serviceCollection.AddSingleton<IValidationRule<AggregatedTimeSeriesRequest>, TimeSeriesTypeValidationRule>();
    }
}
