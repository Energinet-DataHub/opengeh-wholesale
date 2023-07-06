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

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;

public class DatabricksSqlResponseParser : IDatabricksSqlResponseParser
{
    private readonly IDatabricksSqlStatusResponseParser _databricksSqlStatusResponseParser;
    private readonly IDatabricksSqlChunkResponseParser _databricksSqlChunkResponseParser;
    private readonly IDatabricksSqlChunkDataResponseParser _databricksSqlChunkDataResponseParser;

    public DatabricksSqlResponseParser(
        IDatabricksSqlStatusResponseParser databricksSqlStatusResponseParser,
        IDatabricksSqlChunkResponseParser databricksSqlChunkResponseParser,
        IDatabricksSqlChunkDataResponseParser databricksSqlChunkDataResponseParser)
    {
        _databricksSqlStatusResponseParser = databricksSqlStatusResponseParser;
        _databricksSqlChunkResponseParser = databricksSqlChunkResponseParser;
        _databricksSqlChunkDataResponseParser = databricksSqlChunkDataResponseParser;
    }

    public DatabricksSqlResponse ParseStatusResponse(string jsonResponse)
    {
        return _databricksSqlStatusResponseParser.Parse(jsonResponse);
    }

    public DatabricksSqlChunkResponse ParseChunkResponse(string jsonResponse)
    {
        return _databricksSqlChunkResponseParser.Parse(jsonResponse);
    }

    public TableChunk ParseChunkDataResponse(string jsonResponse, string[] columnNames)
    {
        return _databricksSqlChunkDataResponseParser.Parse(jsonResponse, columnNames);
    }
}
