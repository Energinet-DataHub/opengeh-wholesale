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

using System.Xml.Serialization;
using NodaTime;

// ReSharper disable NotAccessedPositionalProperty.Global - apparent unused properties are used when serializing
namespace Energinet.DataHub.Wholesale.Sender.Infrastructure.Services;

/// <summary>
/// DTO representing a document model of an RSM-014 message.
/// The type is used to serialize to the CIM XML representation.
/// </summary>
[XmlRoot("NotifyAggregatedMeasureData_MarketDocument")]
public sealed record Rsm014Dto(
    [property: XmlElement("mRID", Order = 0)]
    string MRid,
    [property: XmlIgnore] string ReceiverGln,
    [property: XmlIgnore] Instant CreatedDateTime,
    [property: XmlElement("Series", Order = 9)]
    Rsm014Dto.SeriesDto Series)
{
    /// <summary>
    /// Parameterless ctor is required to support serialization.
    /// </summary>
    public Rsm014Dto()
        : this(string.Empty, string.Empty, Instant.MinValue, new SeriesDto())
    {
    }

    [XmlElement("type", Order = 1)]
    public string Type { get; init; } = "E31";

    [XmlElement("process.processType", Order = 2)]
    public string ProcessType { get; init; } = "D04";

    [XmlElement("businessSector.type", Order = 3)]
    public int BusinessSectorType { get; init; } = 23;

    [XmlElement("sender_MarketParticipant.mRID", Order = 4)]
    public ActorMRid SenderMRid { get; init; } = new("5790001330552");

    [XmlElement("sender_MarketParticipant.marketRole.type", Order = 5)]
    public string SenderMarketRole { get; init; } = "DGL";

    [XmlElement("receiver_MarketParticipant.mRID", Order = 6)]
    public ActorMRid ReceiverMRid { get; init; } = new(ReceiverGln);

    [XmlElement("receiver_MarketParticipant.marketRole.type", Order = 7)]
    public string ReceiverMarketRole { get; init; } = "MDR";

    [XmlElement("createdDateTime", Order = 8)]
    public string CreateDateTimeString { get; init; } = CreatedDateTime.ToString();

    [XmlIgnore]
    public static string Namespace => "urn:ediel.org:measure:notifyaggregatedmeasuredata:0:1";

    /// <summary>
    /// See <see cref="Rsm014Dto"/>.
    /// </summary>
    public sealed record ActorMRid([property: XmlText] string MRid)
    {
        /// <summary>
        /// Parameterless ctor is required to support serialization.
        /// </summary>
        public ActorMRid()
            : this(string.Empty)
        {
        }

        [XmlAttribute("codingScheme")]
        public string CodingScheme { get; init; } = "A10";
    }

    /// <summary>
    /// See <see cref="Rsm014Dto"/>.
    /// </summary>
    public sealed record SeriesDto(
        [property: XmlElement("mRID", Order = 0)]
        string MRid,
        [property: XmlIgnore] string GridArea,
        [property: XmlElement("Period", Order = 6)]
        Period Period)
    {
        /// <summary>
        /// Parameterless ctor is required to support serialization.
        /// </summary>
        public SeriesDto()
            : this(string.Empty, string.Empty, new Period())
        {
        }

        [XmlElement("version", Order = 1)]
        public int Version { get; init; } = 1;

        [XmlElement("marketEvaluationPoint.type", Order = 2)]
        public string MeteringPointType { get; init; } = "E18";

        [XmlElement("meteringGridArea_Domain.mRID", Order = 3)]
        public MeteringGridAreaDomainMRid MeteringGridAreaDomainMRid { get; init; } = new(GridArea);

        /// <summary>
        /// Fixed to 8716867000030 (Active Energy) for current document type.
        /// </summary>
        [XmlElement("product", Order = 4)]
        public string Product { get; init; } = "8716867000030";

        /// <summary>
        /// Fixed to kWh for current document type.
        /// </summary>
        [XmlElement("quantity_Measure_Unit.name", Order = 5)]
        public string QuantityMeasureUnitName { get; init; } = "KWH";
    }

    /// <summary>
    /// See <see cref="Rsm014Dto"/>.
    /// </summary>
    public sealed record MeteringGridAreaDomainMRid([property: XmlText] string GridArea)
    {
        /// <summary>
        /// Parameterless ctor is required to support serialization.
        /// </summary>
        public MeteringGridAreaDomainMRid()
            : this(string.Empty)
        {
        }

        [XmlAttribute("codingScheme")]
        public string CodingScheme { get; init; } = "NDK";
    }

    /// <summary>
    /// See <see cref="Rsm014Dto"/>.
    /// </summary>
    public sealed record Period(
        [property: XmlElement("timeInterval", Order = 1)]
        TimeInterval TimeInterval,
        [property: XmlElement("Point", Order = 2)]
        List<Point> Points)
    {
        /// <summary>
        /// Parameterless ctor is required to support serialization.
        /// </summary>
        public Period()
            : this(new TimeInterval(), new List<Point>())
        {
        }

        [XmlElement("resolution", Order = 0)]
        public string Resolution { get; init; } = "PT15M";
    }

    /// <summary>
    /// See <see cref="Rsm014Dto"/>.
    /// </summary>
    public sealed record TimeInterval([property: XmlIgnore] Instant Start, [property: XmlIgnore] Instant End)
    {
        /// <summary>
        /// Parameterless ctor is required to support serialization.
        /// </summary>
        public TimeInterval()
            : this(Instant.MinValue, Instant.MinValue)
        {
        }

        [XmlElement("start", Order = 0)]
        public string StartString { get; init; } = Start.ToString();

        [XmlElement("end", Order = 1)]
        public string EndString { get; init; } = End.ToString();
    }

    /// <summary>
    /// See <see cref="Rsm014Dto"/>.
    /// </summary>
    public sealed record Point(
        [property: XmlElement("position", Order = 0)]
        int Position,
        [property: XmlElement("quantity", Order = 1)]
        string Quantity,
        [property: XmlElement("quality", Order = 2)]
        string? Quality)
    {
        /// <summary>
        /// Parameterless ctor is required to support serialization.
        /// </summary>
        public Point()
            : this(-1, string.Empty, string.Empty)
        {
        }
    }
}
