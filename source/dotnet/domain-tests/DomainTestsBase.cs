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

using System.Diagnostics.CodeAnalysis;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.LazyFixture;
using Xunit;

namespace Energinet.DataHub.Wholesale.DomainTests
{
    /// <summary>
    /// Simplify the implementation of domain tests for which we only want to perform the
    /// test setup phase if they should actually be executed.
    /// </summary>
    /// <typeparam name="TLazyFixture">A xUnit fixture that inherits from <see cref="LazyFixtureBase"/>.</typeparam>
    public abstract class DomainTestsBase<TLazyFixture> : IClassFixture<LazyFixtureFactory<TLazyFixture>>, IAsyncLifetime
        where TLazyFixture : LazyFixtureBase
    {
        /// <summary>
        /// Any fixture given in the constructor is always created and initialized by xUnit, even if no test is executed
        /// in the test class. For this reason we use a factory to handle lazy initialization of the fixture, which means
        /// we only perform the initialization if we actually execute any test.
        /// </summary>
        public DomainTestsBase(LazyFixtureFactory<TLazyFixture> lazyFixtureFactory)
        {
            LazyFixtureFactory = lazyFixtureFactory;
        }

        /// <summary>
        /// This property only contains a value if any test is executed.
        /// </summary>
        [NotNull]
        protected TLazyFixture? Fixture { get; set; }

        private LazyFixtureFactory<TLazyFixture> LazyFixtureFactory { get; }

        /// <summary>
        /// This method only gets called by xUnit if any test is executed.
        /// It gets called before each test.
        ///
        /// It is responsible for initializing the <see cref="Fixture"/>.
        /// </summary>
        public async Task InitializeAsync()
        {
            Fixture = await LazyFixtureFactory.LazyFixture;
        }

        /// <summary>
        /// This method only gets called by xUnit if any test is executed.
        /// It gets called after each test.
        /// </summary>
        public Task DisposeAsync()
        {
            // We currently don't need to clean up anything between test executions.
            return Task.CompletedTask;
        }
    }
}
