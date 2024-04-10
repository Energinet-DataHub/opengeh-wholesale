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

using Energinet.DataHub.Wholesale.Edi.Contracts;
using Energinet.DataHub.Wholesale.Edi.Models;

namespace Energinet.DataHub.Wholesale.Edi.Validation.AggregatedTimeSeriesRequest.Rules;

public class SettlementMethodValidationRule : IValidationRule<DataHub.Edi.Requests.AggregatedTimeSeriesRequest>
{
    private static readonly IReadOnlyList<string> _validSettlementMethods = new List<string> { DataHubNames.SettlementMethod.Flex, DataHubNames.SettlementMethod.NonProfiled };
    private static readonly string _validMeteringPointType = DataHubNames.MeteringPointType.Consumption;

    private static readonly ValidationError _invalidSettlementMethod = new("SettlementMethod kan kun benyttes i kombination med E17 og skal være enten D01 og E02 / SettlementMethod can only be used in combination with E17 and must be either D01 or E02", "D15");

    public Task<IList<ValidationError>> ValidateAsync(DataHub.Edi.Requests.AggregatedTimeSeriesRequest subject)
    {
        if (!subject.HasSettlementMethod)
            return Task.FromResult(NoError);

        if (!IsValidSettlementMethod(subject.SettlementMethod))
            return Task.FromResult(InvalidSettlementMethod);

        if (!IsMeteringPointTypeConsumption(subject.MeteringPointType))
            return Task.FromResult(InvalidSettlementMethod);

        return Task.FromResult(NoError);
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
