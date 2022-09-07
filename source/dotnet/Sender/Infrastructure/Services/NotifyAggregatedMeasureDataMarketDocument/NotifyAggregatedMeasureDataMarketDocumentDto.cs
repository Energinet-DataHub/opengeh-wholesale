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

namespace Energinet.DataHub.Wholesale.Sender.Infrastructure.Services.NotifyAggregatedMeasureDataMarketDocument;

/* TODO: Namespaces
<cim:NotifyAggregatedMeasureData_MarketDocument xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:cim=""urn:ediel.org:measure:notifyaggregatedmeasuredata:0:1"" xsi:schemaLocation=""urn:ediel.org:measure:notifyaggregatedmeasuredata:0:1 urn-ediel-org-measure-notifyaggregatedmeasuredata-0-1.xsd"">

TODO: Property order (using classes?)
*/

/// <summary>
/// Also known as a RSM-014 document
/// </summary>
[XmlRoot("NotifyAggregatedMeasureData_MarketDocument")]
public record NotifyAggregatedMeasureDataMarketDocumentDto(
    [property: XmlElement("mRID")] string MRid,
    [property: XmlIgnore] string ReceiverGln,
    [property: XmlIgnore] Instant CreatedDateTime,
    SeriesDto Series)
{
    /// <summary>
    /// Parameterless ctor is required to support serialization.
    /// </summary>
    public NotifyAggregatedMeasureDataMarketDocumentDto()
        : this(string.Empty, string.Empty, Instant.MinValue, new SeriesDto())
    {
    }

    public string Type { get; init; } = "E31";

    [property: XmlElement("process.processType")]
    public string ProcessType { get; init; } = "D04";

    [property: XmlElement("businessSector.type")]
    public int BusinessSectorType { get; init; } = 23;

    [XmlElement("sender_MarketParticipant.mRID")]
    public ActorMRidDto SenderMRid { get; init; } = new ActorMRidDto("5790001330552");

    [XmlElement("sender_MarketParticipant.marketRole.type")]
    public string SenderMarketRole { get; init; } = "DGL";

    [XmlElement("receiver_MarketParticipant.mRID")]
    public ActorMRidDto ReceiverMRid { get; init; } = new ActorMRidDto(ReceiverGln);

    [XmlElement("receiver_MarketParticipant.marketRole.type")]
    public string ReceiverMarketRole { get; init; } = "MDR";

    [XmlElement("createdDateTime")]
    public string CreateDateTimeString { get; init; } = CreatedDateTime.ToString();
}

public record ActorMRidDto([property: XmlText] string MRid)
{
    /// <summary>
    /// Parameterless ctor is required to support serialization.
    /// </summary>
    public ActorMRidDto()
        : this(string.Empty)
    {
    }

    [XmlAttribute]
    public string CodingSchema { get; init; } = "A10";
}

public sealed record SeriesDto(
    [property: XmlElement("mRID")] string MRid,
    [property: XmlIgnore] string GridArea,
    PeriodDto Period)
{
    /// <summary>
    /// Parameterless ctor is required to support serialization.
    /// </summary>
    public SeriesDto()
        : this(string.Empty, string.Empty, new PeriodDto())
    {
    }

    public int Version { get; init; } = 1;

    [XmlElement("marketEvaluationPoint.type")]
    public string MeteringPointType { get; init; } = "E18";

    [XmlElement("meteringGridArea_Domain.mRID")]
    public MeteringGridAreaDomainMRid MeteringGridAreaDomainMRid { get; init; } = new(GridArea);

    [XmlElement("product")]
    public string Product { get; init; } = "8716867000030";

    [XmlElement("quantity_Measure_Unit.name")]
    public string QuantityMeasureUnitName { get; init; } = "KWH";
}

public record MeteringGridAreaDomainMRid([XmlText] string GridArea)
{
    /// <summary>
    /// Parameterless ctor is required to support serialization.
    /// </summary>
    public MeteringGridAreaDomainMRid()
        : this(string.Empty) { }

    [XmlAttribute]
    public string CodingSchema { get; init; } = "NDK";
}

public sealed record PeriodDto(TimeIntervalDto TimeInterval, List<PointDto> Points)
{
    /// <summary>
    /// Parameterless ctor is required to support serialization.
    /// </summary>
    public PeriodDto()
        : this(new TimeIntervalDto(), new List<PointDto>()) { }

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

    [XmlElement("start")]
    public string StartString { get; init; } = Start.ToString();

    [XmlElement("end")]
    public string EndString { get; init; } = End.ToString();
}

public sealed record PointDto(int Position, string Quantity, string Quality)
{
    /// <summary>
    /// Parameterless ctor is required to support serialization.
    /// </summary>
    public PointDto()
        : this(-1, string.Empty, string.Empty) { }
}
