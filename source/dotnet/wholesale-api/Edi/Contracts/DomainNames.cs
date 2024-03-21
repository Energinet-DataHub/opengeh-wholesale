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

using System.Diagnostics.CodeAnalysis;

namespace Energinet.DataHub.Wholesale.Edi.Contracts;

/// <summary>
/// These domain names are shared between the EDI and Wholesale subsystems
/// When updating these, you need to manually update the classes in the other subsystem
/// Files to manually keep in sync:
/// - EDI: BuildingBlocks.Domain/DataHub/DomainNames.cs
/// - Wholesale: Edi/Edi/Contracts/DomainNames.cs
/// </summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Keep names in a single file to easily share between EDI and Wholesale")]
public static class DomainNames
{
    public static class BusinessReason
    {
        public const string MoveIn = "MoveIn";
        public const string BalanceFixing = "BalanceFixing";
        public const string PreliminaryAggregation = "PreliminaryAggregation";
        public const string WholesaleFixing = "WholesaleFixing";
        public const string Correction = "Correction";
    }

    public static class ChargeType
    {
        public const string Subscription = "Subscription";
        public const string Fee = "Fee";
        public const string Tariff = "Tariff";
    }

    public static class Currency
    {
        public const string DanishCrowns = "DanishCrowns";
    }

    public static class MeasurementUnit
    {
        public const string Kwh = "Kwh";
        public const string Pieces = "Pieces";
    }

    public static class MeteringPointType
    {
        public const string Consumption = "Consumption";
        public const string Production = "Production";
        public const string Exchange = "Exchange";
    }

    public static class Resolution
    {
        public const string QuarterHourly = "QuarterHourly";
        public const string Hourly = "Hourly";
        public const string Daily = "Daily";
        public const string Monthly = "Monthly";
    }

    public static class SettlementType
    {
        public const string NonProfiled = "NonProfiled";
        public const string Flex = "Flex";
    }

    public static class SettlementVersion
    {
        public const string FirstCorrection = "FirstCorrection";
        public const string SecondCorrection = "SecondCorrection";
        public const string ThirdCorrection = "ThirdCorrection";
    }
}
