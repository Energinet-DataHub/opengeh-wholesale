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

namespace Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSerie.Rules;

public class BalanceResponsibleValidationRule : IValidationRule<AggregatedTimeSeriesRequest>
{
    private static readonly ValidationError _invalidBalanceResponsible = new("Feltet BalanceResponsibleParty skal være udfyldt med et valid GLN/EIC når en balanceansvarlig anmoder om data / BalanceResponsibleParty must be submitted with a valid GLN/EIC when a balance responsible requests data", "E18");
    private static readonly ValidationError _notEqualToRequestedBy = new("BalanceResponsibleParty i besked stemmer ikke overenes med balanceansvarlig anmoder i header / BalanceResponsibleParty in message does not correspond with balance responsible in header", "E18");

    public IList<ValidationError> Validate(AggregatedTimeSeriesRequest subject)
    {
        if (subject.RequestedByActorRole != ActorRoleCode.BalanceResponsibleParty) return NoError;

        if (string.IsNullOrWhiteSpace(subject.BalanceResponsibleId))
            return InvalidBalanceResponsibleError;

        if (!IsValidBalanceResponsibleIdFormat(subject.BalanceResponsibleId))
            return InvalidBalanceResponsibleError;

        if (!subject.RequestedByActorId.Equals(subject.BalanceResponsibleId, StringComparison.OrdinalIgnoreCase))
            return NotEqualToRequestedByError;

        return NoError;
    }

    private static bool IsValidBalanceResponsibleIdFormat(string balanceResponsibleId)
    {
        return ActorNumberValidationHelper.IsValidGlnNumber(balanceResponsibleId) || ActorNumberValidationHelper.IsValidEicNumber(balanceResponsibleId);
    }

    private static IList<ValidationError> NoError => new List<ValidationError>();

    private static IList<ValidationError> InvalidBalanceResponsibleError => new List<ValidationError> { _invalidBalanceResponsible };

    private static IList<ValidationError> NotEqualToRequestedByError => new List<ValidationError> { _notEqualToRequestedBy };
}
