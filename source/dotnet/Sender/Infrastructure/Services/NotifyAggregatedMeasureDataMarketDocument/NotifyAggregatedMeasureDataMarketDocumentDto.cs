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

// ReSharper disable NotAccessedPositionalProperty.Global -
using System.Xml.Serialization;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Sender.Infrastructure.Services.NotifyAggregatedMeasureDataMarketDocument;

/// <summary>
/// Also known as a RSM-014 document
/// </summary>
[XmlRoot("NotifyAggregatedMeasureData_MarketDocument")]
public sealed record NotifyAggregatedMeasureDataMarketDocumentDto(
    [property: XmlElement("mRID", Order = 0)] string MRid,
    [property: XmlIgnore] string ReceiverGln,
    [property: XmlIgnore] Instant CreatedDateTime,
    [property: XmlElement("Series", Order = 9)] SeriesDto Series)
{
    /// <summary>
    /// Parameterless ctor is required to support serialization.
    /// </summary>
    public NotifyAggregatedMeasureDataMarketDocumentDto()
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
    public ActorMRidDto SenderMRid { get; init; } = new("5790001330552");

    [XmlElement("sender_MarketParticipant.marketRole.type", Order = 5)]
    public string SenderMarketRole { get; init; } = "DGL";

    [XmlElement("receiver_MarketParticipant.mRID", Order = 6)]
    public ActorMRidDto ReceiverMRid { get; init; } = new(ReceiverGln);

    [XmlElement("receiver_MarketParticipant.marketRole.type", Order = 7)]
    public string ReceiverMarketRole { get; init; } = "MDR";

    [XmlElement("createdDateTime", Order = 8)]
    public string CreateDateTimeString { get; init; } = CreatedDateTime.ToString();

    [XmlIgnore]
    public static string Namespace => "urn:ediel.org:measure:notifyaggregatedmeasuredata:0:1";
}

public sealed record ActorMRidDto([property: XmlText] string MRid)
{
    /// <summary>
    /// Parameterless ctor is required to support serialization.
    /// </summary>
    public ActorMRidDto()
        : this(string.Empty)
    {
    }

    [XmlAttribute("codingScheme")]
    public string CodingScheme { get; init; } = "A10";
}

public sealed record SeriesDto(
    [property: XmlElement("mRID", Order = 0)] string MRid,
    [property: XmlIgnore] string GridArea,
    [property: XmlElement("Period", Order = 6)] PeriodDto Period)
{
    /// <summary>
    /// Parameterless ctor is required to support serialization.
    /// </summary>
    public SeriesDto()
        : this(string.Empty, string.Empty, new PeriodDto())
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

public sealed record MeteringGridAreaDomainMRid([property: XmlText] string GridArea)
{
    /// <summary>
    /// Parameterless ctor is required to support serialization.
    /// </summary>
    public MeteringGridAreaDomainMRid()
        : this(string.Empty) { }

    [XmlAttribute("codingScheme")]
    public string CodingScheme { get; init; } = "NDK";
}

public sealed record PeriodDto(
    [property: XmlElement("timeInterval", Order = 1)] TimeIntervalDto TimeInterval,
    [property: XmlElement("Point", Order = 2)] List<PointDto> Points)
{
    /// <summary>
    /// Parameterless ctor is required to support serialization.
    /// </summary>
    public PeriodDto()
        : this(new TimeIntervalDto(), new List<PointDto>()) { }

    [XmlElement("resolution", Order = 0)]
    public string Resolution { get; init; } = "PT15M";
}

public sealed record TimeIntervalDto([property: XmlIgnore] Instant Start, [property: XmlIgnore] Instant End)
{
    /// <summary>
    /// Parameterless ctor is required to support serialization.
    /// </summary>
    public TimeIntervalDto()
        : this(Instant.MinValue, Instant.MinValue)
    {
    }

    [XmlElement("start", Order = 0)]
    public string StartString { get; init; } = Start.ToString();

    [XmlElement("end", Order = 1)]
    public string EndString { get; init; } = End.ToString();
}

public sealed record PointDto(
    [property: XmlElement("position", Order = 0)] int Position,
    [property: XmlElement("quantity", Order = 1)] string Quantity,
    [property: XmlElement("quality", Order = 2)] string? Quality)
{
    /// <summary>
    /// Parameterless ctor is required to support serialization.
    /// </summary>
    public PointDto()
        : this(-1, string.Empty, string.Empty) { }
}
