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

using Energinet.DataHub.Wholesale.Edi.Validation.Helpers;

namespace Energinet.DataHub.Wholesale.Edi.Validation.AggregatedTimeSeriesRequest.Rules;

public class SettlementVersionValidationRule : IValidationRule<DataHub.Edi.Requests.AggregatedTimeSeriesRequest>
{
    private static readonly ValidationError _invalidSettlementVersionError = new("SettlementSeriesVersion kan kun benyttes i kombination med D32 og skal være enten D01, D02 eller D03 / SettlementSeriesVersion can only be used in combination with D32 and must be either D01, D02 or D03", "E86");

    public Task<IList<ValidationError>> ValidateAsync(DataHub.Edi.Requests.AggregatedTimeSeriesRequest subject)
    {
        var validSettlementVersion = SettlementVersionValidationHelper.IsSettlementVersionValid(
            subject.BusinessReason,
            subject.HasSettlementVersion ? subject.SettlementVersion : null);
        if (validSettlementVersion)
            return Task.FromResult(NoError);
        return Task.FromResult(InvalidSettlementVersionError);
    }

    private static IList<ValidationError> NoError => new List<ValidationError>();

    private static IList<ValidationError> InvalidSettlementVersionError => new List<ValidationError> { _invalidSettlementVersionError };
}
