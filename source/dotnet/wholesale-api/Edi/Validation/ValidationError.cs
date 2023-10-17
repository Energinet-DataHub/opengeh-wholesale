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
    public static readonly ValidationError InvalidDateFormat = new("Forkert dato format for {PropertyName}, skal være YYYY-MM-DDT22:00:00Z eller YYYY-MM-DDT23:00:00Z / Wrong date format for {PropertyName}, must be YYYY-MM-DDT22:00:00Z or YYYY-MM-DDT23:00:00Z", "D66");
    public static readonly ValidationError InvalidWinterMidnightFormat = new("Forkert dato format for {PropertyName}, skal være YYYY-MM-DDT23:00:00Z / Wrong date format for {PropertyName}, must be YYYY-MM-DDT23:00:00Z", "D66");
    public static readonly ValidationError InvalidSummerMidnightFormat = new("Forkert dato format for {PropertyName}, skal være YYYY-MM-DDT22:00:00Z / Wrong date format for {PropertyName}, must be YYYY-MM-DDT22:00:00Z", "D66");
    public static readonly ValidationError StartDateMustBeLessThen3Years = new("Dato må max være 3 år tilbage i tid / Can maximum be 3 years back in time", "E17");
    public static readonly ValidationError PeriodIsGreaterThenAllowedPeriodSize = new("Dato må kun være for 1 måned af gangen / Can maximum be for a 1 month period", "E17");
    public static readonly ValidationError MissingStartOrAndEndDate = new("Start og slut dato skal udfyldes / Start and end date must be present in request", "E50");
    public static readonly ValidationError InvalidMeteringPointType = new("Metering point type skal være en af følgende: {PropertyName} / Metering point type has to be one of the following: {PropertyName}", "D18");
    public static readonly ValidationError InvalidEnergySupplierField = new("Feltet EnergySupplier skal være udfyldt med et valid GLN/EIC nummer når en elleverandør anmoder om data / EnergySupplier must be submitted with a valid GLN/EIC number when an energy supplier requests data", "E16");
    public static readonly ValidationError InvalidSettlementMethod = new("SettlementMethod kan kun benyttes i kombination med E17 og skal være enten D01 og E02 / SettlementMethod can only be used in combination with E17 and must be either D01 or E02", "D15");
    public static readonly ValidationError InvalidTimeSeriesTypeForActor = new("Den forespurgte tidsserie type kan ikke forespørges som en {PropertyName} / The requested times series type can not be requested as a {PropertyName}", "D11");

    private ValidationError(string message, string errorCode)
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
