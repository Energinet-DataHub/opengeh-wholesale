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

using Energinet.DataHub.Wholesale.Edi.Contracts;

namespace Energinet.DataHub.Wholesale.Edi.Validation.WholesaleServicesRequest.Rules;

/// <summary>
/// Validation rule for the resolution property when there is requested for wholesale services.
/// </summary>
/// <remarks>
/// This is registered as a singleton service in the service collection.
/// </remarks>
public class ResolutionValidationRule
    : IValidationRule<DataHub.Edi.Requests.WholesaleServicesRequest>
{
    private const string PropertyName = "aggregationSeries_Period.resolution";
    private static readonly ValidationError _notMonthlyResolution =
        new(
            $"{PropertyName} skal være 'P1M'/{PropertyName} must be 'P1M'",
            "D23");

    public Task<IList<ValidationError>> ValidateAsync(DataHub.Edi.Requests.WholesaleServicesRequest subject)
    {
        var errors = new List<ValidationError>();
        if (subject.HasResolution && subject.Resolution != DataHubNames.Resolution.Monthly)
        {
            errors.Add(_notMonthlyResolution);
        }

        return Task.FromResult<IList<ValidationError>>(errors);
    }
}
