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

public class BalanceResponsibleValidationRule : IValidationRule<AggregatedTimeSeriesRequest>
{
    private const string BalanceResponsibleRole = "DDK";

    public IList<ValidationError> Validate(AggregatedTimeSeriesRequest subject)
    {
        if (subject.RequestedByActorRole == BalanceResponsibleRole)
        {
            if (string.IsNullOrWhiteSpace(subject.BalanceResponsibleId))
                return new List<ValidationError>() { ValidationError.InvalidBalanceResponsible };

            if (!IsValidActorRoleFormat(subject.BalanceResponsibleId))
                return new List<ValidationError>() { ValidationError.InvalidBalanceResponsible };

            if (!subject.RequestedByActorId.Equals(subject.BalanceResponsibleId, StringComparison.OrdinalIgnoreCase))
                return new List<ValidationError>() { ValidationError.MismatchedBalanceResponsibleInHeaderAndMessage };
        }

        return new List<ValidationError>();
    }

    private static bool IsValidActorRoleFormat(string energySupplierId)
    {
        var isValidGlnNumber = energySupplierId.Length == 13;
        var isValidEicNumber = energySupplierId.Length == 16;

        return isValidGlnNumber || isValidEicNumber;
    }
}
