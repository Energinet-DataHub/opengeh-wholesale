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

public class EnergySupplierFieldValidationRule : IValidationRule<AggregatedTimeSeriesRequest>
{
    private static readonly IList<ValidationError> _noError = new List<ValidationError>();
    private static readonly IList<ValidationError> _validationError = new List<ValidationError>
    {
        ValidationError.InvalidEnergySupplierField,
    };

    public IList<ValidationError> Validate(AggregatedTimeSeriesRequest subject)
    {
        if (subject.RequestedByActorRole != ActorRoleCode.EnergySupplier)
             return _noError;

        if (string.IsNullOrEmpty(subject.EnergySupplierId))
            return _validationError;

        if (!IsValidEnergySupplierIdFormat(subject.EnergySupplierId))
            return _validationError;

        if (!RequestedByIdEqualsEnergySupplier(subject.RequestedByActorId, subject.EnergySupplierId))
            return _validationError;

        return _noError;
    }

    private static bool IsValidEnergySupplierIdFormat(string energySupplierId)
    {
        var isValidGlnNumber = energySupplierId.Length == 13;
        var isValidEicNumber = energySupplierId.Length == 16;

        return isValidGlnNumber || isValidEicNumber;
    }

    private static bool RequestedByIdEqualsEnergySupplier(string requestedByActorId, string energySupplierId)
    {
        return requestedByActorId.Equals(energySupplierId, StringComparison.OrdinalIgnoreCase);
    }
}
