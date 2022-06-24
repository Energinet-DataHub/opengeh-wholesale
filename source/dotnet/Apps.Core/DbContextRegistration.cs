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

using Energinet.DataHub.Wholesale.Apps.Core.Configuration;
using Energinet.DataHub.Wholesale.Domain;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.Apps.Core
{
    internal static class DbContextRegistration
    {
        public static void AddDbContexts(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IDatabaseContext, DatabaseContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            var connectionString = configuration.GetSetting(Settings.DatabaseConnectionString);
            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseSqlServer(connectionString, o => o.UseNodaTime());
            });
        }
    }
}
