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

namespace Energinet.DataHub.Wholesale.EDI.Validation;

public sealed class ValidationError
{
    public static readonly ValidationError NoDataAvailable = new("Ingen data tilgængelig / No data available", "E0H");
    public static readonly ValidationError InvalidEnergySupplierField = new("Feltet EnergySupplier skal være udfyldt med et valid GLN/EIC nummer når en elleverandør anmoder om data / EnergySupplier must be submitted with a valid GLN/EIC number when an energy supplier requests data", "E16");

    public ValidationError(string message, string errorCode)
    {
        Message = message;
        ErrorCode = errorCode;
    }

    public string Message { get; }

    public string ErrorCode { get; }

    public ValidationError WithPropertyName(string propertyName)
    {
        return new ValidationError(Message.Replace("{PropertyName}", propertyName), ErrorCode);
    }
}
