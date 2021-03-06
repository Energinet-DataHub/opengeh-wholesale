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

using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Wholesale.Sender.Infrastructure.Persistence.Processes;

public class ProcessRepository : IProcessRepository
{
    private readonly IDatabaseContext _context;

    public ProcessRepository(IDatabaseContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Process process)
    {
        await _context.Processes.AddAsync(process).ConfigureAwait(false);
    }

    public Task<Process> GetAsync(MessageHubReference messageHubReference)
    {
        return _context
            .Processes
            .FromSqlInterpolated(
                $"SELECT * FROM messagehub.Process WHERE MessageHubReference = {messageHubReference.Value}")
            .SingleAsync();
    }
}
