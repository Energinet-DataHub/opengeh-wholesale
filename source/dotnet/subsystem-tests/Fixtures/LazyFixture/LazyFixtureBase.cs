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

using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.LazyFixture
{
    /// <summary>
    /// Base class for lazy fixtures which should be constructed using <see cref="LazyFixtureFactory{TFixture}"/>.
    /// </summary>
    public abstract class LazyFixtureBase : IAsyncLifetime
    {
        /// <summary>
        /// Create lazy fixture.
        /// </summary>
        /// <param name="diagnosticMessageSink">Used for writing messages to the output from xUnit fixtures.</param>
        protected LazyFixtureBase(IMessageSink diagnosticMessageSink)
        {
            DiagnosticMessageSink = diagnosticMessageSink;
        }

        /// <summary>
        /// Can be used from xUnit fixtures for writing messages to output.
        /// Messages are only written if the test runner has been configured to do so.
        /// See details at https://xunit.net/docs/capturing-output#output-in-extensions
        /// </summary>
        protected IMessageSink DiagnosticMessageSink { get; }

        public Task InitializeAsync()
        {
            return OnInitializeAsync();
        }

        public Task DisposeAsync()
        {
            return OnDisposeAsync();
        }

        protected abstract Task OnInitializeAsync();

        protected abstract Task OnDisposeAsync();
    }
}
