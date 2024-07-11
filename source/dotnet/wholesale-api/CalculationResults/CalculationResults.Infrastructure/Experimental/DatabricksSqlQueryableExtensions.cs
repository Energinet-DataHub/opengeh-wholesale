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

using System.Text.Json.Serialization;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Experimental;

public static class DatabricksSqlQueryableExtensions
{
    public static Task<int> DatabricksSqlCountAsync<TElement>(this IQueryable<TElement> source, CancellationToken cancellationToken = default)
    {
        if (source is not DatabricksSqlQueryable<TElement> dbSource)
        {
            throw new NotSupportedException("Only IQueryable from DatabricksContextBase are supported.");
        }

        return ((DatabricksQueryProvider)dbSource.Provider)
            .DatabricksSqlQueryExecutor
            .CountAsync(dbSource, cancellationToken);
    }

    public static class Functions
    {
        public static Instant ToStartOfDayInTimeZone(Instant source, string timeZone)
            => throw new NotSupportedException("Do not call the user-defined EF Core function.");

        public static Instant ToUtcFromTimeZoned(Instant source, string timeZone)
            => throw new NotSupportedException("Do not call the user-defined EF Core function.");

        public static IEnumerable<TimeQuantityStruct> AggregateFields(Instant timeProjection, decimal quantityProjection)
            => throw new NotSupportedException("Do not call the user-defined EF Core function.");
    }

    public sealed class TimeQuantityStruct
    {
        [JsonPropertyName("observation_time")]
        public Instant Time { get; set; }

        [JsonPropertyName("quantity")]
        public decimal Quantity { get; set; }
    }
}
