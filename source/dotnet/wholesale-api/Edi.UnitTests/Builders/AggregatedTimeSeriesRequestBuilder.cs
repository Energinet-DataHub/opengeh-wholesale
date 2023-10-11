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

using Energinet.DataHub.Edi.Requests;
using NodaTime;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Builders;

public class AggregatedTimeSeriesRequestBuilder
{
    private AggregationPerGridArea _aggregationPerGridArea = new();
    private string _start;
    private string _end;
    private string _meteringPointType = "E18";

    private AggregatedTimeSeriesRequestBuilder()
    {
        var now = SystemClock.Instance.GetCurrentInstant();
        _start = Instant.FromUtc(now.InUtc().Year, 1, 1, 23, 0, 0).ToString();
        _end = Instant.FromUtc(now.InUtc().Year, 1, 2, 23, 0, 0).ToString();
    }

    public static AggregatedTimeSeriesRequestBuilder AggregatedTimeSeriesRequest()
    {
        return new AggregatedTimeSeriesRequestBuilder();
    }

    public AggregatedTimeSeriesRequest Build()
    {
        return new AggregatedTimeSeriesRequest
        {
            AggregationPerGridarea = _aggregationPerGridArea,
            Period = new Edi.Requests.Period()
            {
                Start = _start,
                End = _end,
            },
            MeteringPointType = _meteringPointType,
        };
    }

    public AggregatedTimeSeriesRequestBuilder WithStartDate(string start)
    {
        _start = start;
        return this;
    }

    public AggregatedTimeSeriesRequestBuilder WithEndDate(string end)
    {
        _end = end;
        return this;
    }
}
