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

using Energinet.DataHub.Wholesale.EDI.Models;
using AggregatedTimeSeriesRequest = Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest;

namespace Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSeries.Rules;

public class SettlementMethodValidationRule : IValidationRule<AggregatedTimeSeriesRequest>
{
    private static readonly IReadOnlyList<string> _validSettlementMethods = new List<string> { SettlementMethod.Flex, SettlementMethod.NonProfiled };
    private static readonly string _validMeteringPointType = MeteringPointType.Consumption;

    private static readonly ValidationError _invalidSettlementMethod = new("SettlementMethod kan kun benyttes i kombination med E17 og skal være enten D01 og E02 / SettlementMethod can only be used in combination with E17 and must be either D01 or E02", "D15");

    public IList<ValidationError> Validate(AggregatedTimeSeriesRequest subject)
    {
        if (!subject.HasSettlementMethod)
             return NoError;

        if (!IsValidSettlementMethod(subject.SettlementMethod))
            return InvalidSettlementMethod;

        if (!IsMeteringPointTypeConsumption(subject.MeteringPointType))
            return InvalidSettlementMethod;

        return NoError;
    }

    private bool IsValidSettlementMethod(string settlementMethod)
    {
        return _validSettlementMethods.Contains(settlementMethod);
    }

    private bool IsMeteringPointTypeConsumption(string meteringPointType)
    {
        return meteringPointType.Equals(_validMeteringPointType, StringComparison.OrdinalIgnoreCase);
    }

    private static IList<ValidationError> NoError => new List<ValidationError>();

    private static IList<ValidationError> InvalidSettlementMethod => new List<ValidationError> { _invalidSettlementMethod };
}
