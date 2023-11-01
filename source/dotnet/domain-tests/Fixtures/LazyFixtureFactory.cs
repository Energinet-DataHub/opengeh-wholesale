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

using Nito.AsyncEx;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Energinet.DataHub.Wholesale.DomainTests.Fixtures
{
    /// <summary>
    /// Factory that creates and initialize a xUnit fixture using lazy async initialization.
    ///
    /// The factory should be used as a xUnit collection or class fixture. This ensure the factory instance
    /// are shared between tests.
    ///
    /// During the test setup phase the <see cref="LazyFixture"/> property should be accessed. Doing so will
    /// create and initialize the "TFixture", but only once.
    ///
    /// During the test cleanup phase the factory will only call Dispose of the "TFixture" if it was actually created.
    /// </summary>
    /// <typeparam name="TFixture">A xUnit fixture that implements <see cref="IAsyncLifetime"/>.</typeparam>
    public sealed class LazyFixtureFactory<TFixture> : IAsyncLifetime
        where TFixture : LazyFixtureBase
    {
        public LazyFixtureFactory(IMessageSink diagnosticMessageSink)
        {
            LazyFixture = new AsyncLazy<TFixture>(() => LazyFixtureFactory<TFixture>.PrepareFixtureAsync(diagnosticMessageSink));
        }

        /// <summary>
        /// Accessing this property will create and initialize the "TFixture" using lazy async initialization.
        /// </summary>
        public AsyncLazy<TFixture> LazyFixture { get; }

        /// <summary>
        /// This method is only implemented to conform to <see cref="IAsyncLifetime"/>.
        /// </summary>
        Task IAsyncLifetime.InitializeAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Responsible for disposing the "TFixture" if it was ever created.
        /// </summary>
        async Task IAsyncLifetime.DisposeAsync()
        {
            if (LazyFixture.IsStarted)
            {
                var fixture = await LazyFixture;
                await fixture.DisposeAsync();
            }
        }

        private static async Task<TFixture> PrepareFixtureAsync(IMessageSink diagnosticMessageSink)
        {
            var fixture = new TFixture(diagnosticMessageSink);
            await fixture.InitializeAsync();

            return fixture;
        }
    }
}
