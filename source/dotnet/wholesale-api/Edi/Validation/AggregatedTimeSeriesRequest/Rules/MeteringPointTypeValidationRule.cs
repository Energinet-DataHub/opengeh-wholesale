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

namespace Energinet.DataHub.Wholesale.Edi.Validation.AggregatedTimeSeriesRequest.Rules;

public class MeteringPointTypeValidationRule : IValidationRule<DataHub.Edi.Requests.AggregatedTimeSeriesRequest>
{
    private static readonly IReadOnlyList<string> _validMeteringPointTypes = new List<string>
    {
        DataHubNames.MeteringPointType.Consumption,
        DataHubNames.MeteringPointType.Production,
        DataHubNames.MeteringPointType.Exchange,
    };

    private static readonly ValidationError _invalidMeteringPointType =
        new(
            "Metering point type skal være en af følgende: {PropertyName} eller undladt / Metering point type has one of the following: {PropertyName} or omitted",
            "D18");

    public Task<IList<ValidationError>> ValidateAsync(DataHub.Edi.Requests.AggregatedTimeSeriesRequest subject)
    {
        if (!subject.HasMeteringPointType)
            return Task.FromResult(NoError);

        if (_validMeteringPointTypes.Contains(subject.MeteringPointType, StringComparer.OrdinalIgnoreCase))
            return Task.FromResult(NoError);

        return Task.FromResult(InvalidMeteringPointType);
    }

    private static IList<ValidationError> NoError => new List<ValidationError>();

    private static IList<ValidationError> InvalidMeteringPointType => new List<ValidationError> { _invalidMeteringPointType.WithPropertyName("E17, E18, E20") };
}
