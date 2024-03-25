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
/// - EDI: BuildingBlocks.Domain/DataHub/DataHubNames.cs
/// - Wholesale: Edi/Edi/Contracts/DataHubNames.cs
/// In the future this should be shared through a NuGet package instead
/// </summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Keep names in a single file to easily share between EDI and Wholesale")]
public static class DataHubNames
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

    public static class SettlementMethod
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

    public static class ActorRole
    {
        public const string MeteringPointAdministrator = "MeteringPointAdministrator";
        public const string MeteredDataResponsible = "MeteredDataResponsible";
        public const string EnergySupplier = "EnergySupplier";
        public const string BalanceResponsibleParty = "BalanceResponsibleParty";
        public const string GridOperator = "GridOperator";
        public const string MeteredDataAdministrator = "MeteredDataAdministrator";
        public const string ImbalanceSettlementResponsible = "ImbalanceSettlementResponsible";
        public const string SystemOperator = "SystemOperator";
        public const string DanishEnergyAgency = "DanishEnergyAgency";
        public const string Delegated = "Delegated";
    }
}
