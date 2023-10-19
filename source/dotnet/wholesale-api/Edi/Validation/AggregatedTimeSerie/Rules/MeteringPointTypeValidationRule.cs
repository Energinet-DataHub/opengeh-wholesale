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
using Energinet.DataHub.Wholesale.EDI.Models;
using AggregatedTimeSeriesRequest = Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest;

namespace Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSerie.Rules;

public class MeteringPointTypeValidationRule : IValidationRule<AggregatedTimeSeriesRequest>
{
    private static readonly IReadOnlyList<string> _validMeteringPointTypes = new List<string>
    {
        MeteringPointType.Consumption,
        MeteringPointType.Production,
        MeteringPointType.Exchange,
    };

    private static readonly ValidationError _invalidMeteringPointType = new("Metering point type skal være en af følgende: {PropertyName} / Metering point type has to be one of the following: {PropertyName}", "D18");

    public IList<ValidationError> Validate(AggregatedTimeSeriesRequest subject)
    {
        if (IsValidMeteringPointType(subject.MeteringPointType)) return NoError;

        return InvalidMeteringPointType;
    }

    private static bool IsValidMeteringPointType(string meteringPointType)
    {
        return _validMeteringPointTypes.Contains(meteringPointType, StringComparer.OrdinalIgnoreCase);
    }

    private static IList<ValidationError> NoError => new List<ValidationError>();

    private static IList<ValidationError> InvalidMeteringPointType => new List<ValidationError> { _invalidMeteringPointType.WithPropertyName(string.Join(", ", _validMeteringPointTypes)) };
}
