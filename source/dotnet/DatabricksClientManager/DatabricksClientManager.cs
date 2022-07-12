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

namespace DatabricksClientManager
{
    public sealed class DatabricksClientManager : IAsyncDisposable
    {
        private readonly DatabricksHttpListener _client;

        public DatabricksClientManager()
        {
            _client = new DatabricksHttpListener(DatabricksUrl);
        }

        public string DatabricksUrl { get; set; } = "http://localhost:8000/";

        public string DatabricksToken { get; } = "no_token";

        public void InitializeClient()
        {
#pragma warning disable VSTHRD110, CS4014
            _client.BeginListenAsync();
#pragma warning restore VSTHRD110, CS4014
        }

        public ValueTask DisposeAsync()
        {
            _client.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
