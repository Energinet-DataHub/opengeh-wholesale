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

namespace Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSerie.Rules;

public class MeteringPointTypeValidationRule : IValidationRule<AggregatedTimeSeriesRequest>
{
    private static readonly IReadOnlyList<string> _validMeteringPointTypes = new List<string>
    {
        "E17", // Consumption
        "E18", // Production
        "E20", // Exchange
    };

    public IList<ValidationError> Validate(AggregatedTimeSeriesRequest subject)
    {
        if (subject.MeteringPointType == null) throw new ArgumentNullException(nameof(subject.MeteringPointType));
        if (InvalidMeteringPointType(subject.MeteringPointType))
        {
            return new List<ValidationError>
            {
                ValidationError.InvalidMeteringPointType.WithPropertyName(
                    string.Join(", ", _validMeteringPointTypes)),
            };
        }

        return new List<ValidationError>();
    }

    private bool InvalidMeteringPointType(string meteringPointType)
    {
        return !_validMeteringPointTypes.Contains(meteringPointType);
    }
}
