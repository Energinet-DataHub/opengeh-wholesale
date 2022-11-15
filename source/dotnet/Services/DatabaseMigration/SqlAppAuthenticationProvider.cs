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

using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;

namespace Energinet.DataHub.Wholesale.DatabaseMigration
{
    /// <summary>
    /// An implementation of SqlAuthenticationProvider that implements Active Directory Interactive SQL authentication.
    /// Reference: https://www.pluralsight.com/guides/how-to-use-managed-identity-with-azure-sql-database
    /// </summary>
    public class SqlAppAuthenticationProvider : SqlAuthenticationProvider
    {
        private static readonly AzureServiceTokenProvider _tokenProvider = new AzureServiceTokenProvider();

        /// <summary>
        /// Acquires an access token for SQL using AzureServiceTokenProvider with the given SQL authentication parameters.
        /// </summary>
        /// <param name="parameters">The parameters needed in order to obtain a SQL access token</param>
        public override async Task<SqlAuthenticationToken> AcquireTokenAsync(SqlAuthenticationParameters parameters)
        {
            var authResult = await _tokenProvider.GetAuthenticationResultAsync("https://database.windows.net/").ConfigureAwait(false);

            return new SqlAuthenticationToken(authResult.AccessToken, authResult.ExpiresOn);
        }

        /// <summary>
        /// Implements virtual method in SqlAuthenticationProvider. Only Active Directory Interactive Authentication is supported.
        /// </summary>
        /// <param name="authenticationMethod">The SQL authentication method to check whether supported</param>
        public override bool IsSupported(SqlAuthenticationMethod authenticationMethod)
        {
            return authenticationMethod == SqlAuthenticationMethod.ActiveDirectoryInteractive;
        }
    }
}
