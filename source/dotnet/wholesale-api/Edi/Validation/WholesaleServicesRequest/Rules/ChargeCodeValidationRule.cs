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

namespace Energinet.DataHub.Wholesale.Edi.Validation.WholesaleServicesRequest.Rules;

public class ChargeCodeValidationRule : IValidationRule<DataHub.Edi.Requests.WholesaleServicesRequest>
{
    private static readonly ValidationError _chargeCodeLengthInvalidError = new(
        "Følgende chargeType mRID er for lang: {PropertyName}. Den må højst indeholde 10 karaktere/"
        + "The following chargeType mRID is to long: {PropertyName} It must at most be 10 characters",
        "D14");

    public Task<IList<ValidationError>> ValidateAsync(DataHub.Edi.Requests.WholesaleServicesRequest subject)
    {
        var chargeTypesWithToLongType = subject.ChargeTypes.Where(chargeType => chargeType.ChargeCode.Length > 10).ToList();

        if (chargeTypesWithToLongType.Count != 0)
        {
            var errors = chargeTypesWithToLongType.Select(chargeType => _chargeCodeLengthInvalidError.WithPropertyName(chargeType.ChargeCode)).ToList();
            return Task.FromResult<IList<ValidationError>>(errors);
        }

        return Task.FromResult(NoError);
    }

    private static IList<ValidationError> NoError => new List<ValidationError>();
}
