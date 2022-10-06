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

namespace Energinet.DataHub.Wholesale.IntegrationTests.TestCommon
{
    public class EventualServiceBusMessage : IDisposable
    {
        private bool _isDisposed;

        public EventualServiceBusMessage()
        {
            _isDisposed = false;
        }

        public ManualResetEventSlim? MessageAwaiter { get; set; }

        public BinaryData? Body { get; set; }

        public string? CorrelationId { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Pattern used to prevent CA1063:
        // https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1063
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
                MessageAwaiter?.Dispose();

            _isDisposed = true;
        }

        ~EventualServiceBusMessage()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
    }
}
