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
using Energinet.DataHub.Wholesale.Edi.Models;

namespace Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSerie.Rules;

public class SettlementMethodValidationRule : IValidationRule<AggregatedTimeSeriesRequest>
{
    private static readonly IReadOnlyList<string> _validSettlementMethods = new List<string> { SettlementMethodType.Flex, SettlementMethodType.NonProfiled };

    private static readonly IList<ValidationError> _noError = new List<ValidationError>();
    private static readonly IList<ValidationError> _validationError = new List<ValidationError>
    {
        ValidationError.InvalidSettlementMethod,
    };

    public IList<ValidationError> Validate(AggregatedTimeSeriesRequest subject)
    {
        if (!subject.HasSettlementMethod)
             return _noError;

        var settlementMethod = subject.SettlementMethod;

        if (!IsValidSettlementMethod(settlementMethod))
            return _validationError;

        if (!IsMeteringPointTypeConsumption(subject.MeteringPointType))
            return _validationError;

        return _noError;
    }

    private bool IsValidSettlementMethod(string settlementMethod)
    {
        return _validSettlementMethods.Contains(settlementMethod);
    }

    private bool IsMeteringPointTypeConsumption(string meteringPointType)
    {
        return meteringPointType.Equals(MeteringPointType.Consumption);
    }
}
