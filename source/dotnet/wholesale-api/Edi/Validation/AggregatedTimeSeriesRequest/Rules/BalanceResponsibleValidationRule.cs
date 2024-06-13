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

public class BalanceResponsibleValidationRule : IValidationRule<DataHub.Edi.Requests.AggregatedTimeSeriesRequest>
{
    private static readonly string _propertyName = "BalanceResponsibleParty";
    private static readonly ValidationError _invalidBalanceResponsible = new($"Feltet {_propertyName} skal være udfyldt med et valid GLN/EIC når en balanceansvarlig anmoder om data / {_propertyName} must be submitted with a valid GLN/EIC when a balance responsible requests data", "E18");
    private static readonly ValidationError _notEqualToRequestedBy = new($"Den balanceansvarlige i beskeden stemmer ikke overenes med den balanceansvarlige i headeren / {_propertyName} in the message does not correspond with balance responsible in header", "E18");
    private static readonly ValidationError _invalidBusinessReason = new($"En balanceansvarlig kan kun benytte forretningsårsag D03 eller D04 i forbindelse med en anmodning / A {_propertyName} can only use business reason D03 or D04 in connection with a request", "D11");

    public Task<IList<ValidationError>> ValidateAsync(DataHub.Edi.Requests.AggregatedTimeSeriesRequest subject)
    {
        IList<ValidationError> errors = new List<ValidationError>();

        if (subject.RequestedForActorRole != DataHubNames.ActorRole.BalanceResponsibleParty)
            return Task.FromResult(errors);

        if (subject.BusinessReason is not DataHubNames.BusinessReason.BalanceFixing and not DataHubNames.BusinessReason.PreliminaryAggregation)
            errors.Add(_invalidBusinessReason);

        if (string.IsNullOrWhiteSpace(subject.BalanceResponsibleId))
        {
            errors.Add(_invalidBalanceResponsible);
            return Task.FromResult(errors);
        }

        if (!IsValidBalanceResponsibleIdFormat(subject.BalanceResponsibleId))
        {
            errors.Add(_invalidBalanceResponsible);
            return Task.FromResult(errors);
        }

        if (!subject.RequestedForActorNumber.Equals(subject.BalanceResponsibleId, StringComparison.OrdinalIgnoreCase))
            errors.Add(_notEqualToRequestedBy);

        return Task.FromResult(errors);
    }

    private static bool IsValidBalanceResponsibleIdFormat(string balanceResponsibleId)
    {
        return ActorNumberValidationHelper.IsValidGlnNumber(balanceResponsibleId) || ActorNumberValidationHelper.IsValidEicNumber(balanceResponsibleId);
    }
}
