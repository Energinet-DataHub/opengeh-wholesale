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

using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Persistence.SettlementReportRequest;

public class SettlementReportRequestRepository : ISettlementReportRequestRepository
{
    private readonly IDatabaseContext _context;

    public SettlementReportRequestRepository(IDatabaseContext context)
    {
        _context = context;
    }

    public async Task AddOrUpdateAsync(Application.SettlementReports_v2.SettlementReportRequest request)
    {
        if (request.Id == 0)
        {
            await _context.SettlementReportRequests.AddAsync(request).ConfigureAwait(false);
        }

        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    public Task<Application.SettlementReports_v2.SettlementReportRequest> GetAsync(string requestId)
    {
        return _context.SettlementReportRequests
            .FirstAsync(x => x.RequestId == requestId);
    }

    public async Task<IEnumerable<Application.SettlementReports_v2.SettlementReportRequest>> GetAsync()
    {
        return await _context.SettlementReportRequests
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Application.SettlementReports_v2.SettlementReportRequest>> GetAsync(Guid userId, Guid actorId)
    {
        return await _context.SettlementReportRequests
            .Where(x => x.UserId == userId && x.ActorId == actorId)
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
