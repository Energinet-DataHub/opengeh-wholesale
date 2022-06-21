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

using FluentAssertions;
using Xunit;
using Xunit.Sdk;

namespace TestHelpers.FluentAssertionsExtensions
{
    public class ObjectAssertionsExtensionsTest
    {
        [Fact]
        public void PropertyIsNotNull()
        {
            // Arrange
            var sut = new RecursiveTestHelperWithOneProp
            {
                Number = 1,
            };

            // Assert
            sut.Should().NotContainNullsOrEmptyEnumerables();
        }

        [Fact]
        public void PropertyIsNull()
        {
            // Arrange
            var sut = new RecursiveTestHelperWithOneProp
            {
                Number = null,
            };

            // Assert
            Assert.Throws<XunitException>(() => sut.Should().NotContainNullsOrEmptyEnumerables());
        }

        [Fact]
        public void NestedPropertyIsNull()
        {
            // Arrange
            var sut = new RecursiveTestHelperWithOnePropAndOneList { Number = 1, RecursiveTestHelperWithOneProps = null! };

            // Assert
            Assert.Throws<XunitException>(() => sut.Should().NotContainNullsOrEmptyEnumerables());
        }

        [Fact]
        public void NestedPropertyIsEmpty()
        {
            // Arrange
            var sut = new RecursiveTestHelperWithOnePropAndOneList { Number = 1, RecursiveTestHelperWithOneProps = new List<RecursiveTestHelperWithOneProp>() };

            // Assert
            Assert.Throws<XunitException>(() => sut.Should().NotContainNullsOrEmptyEnumerables());
        }

        [Fact]
        public void NestedPropertyIsNotNullContainedObjectAndItsProbIsNotNull()
        {
            // Arrange
            var sut = new RecursiveTestHelperWithOnePropAndOneList
            {
                Number = 1,
                RecursiveTestHelperWithOneProps = new List<RecursiveTestHelperWithOneProp> { new RecursiveTestHelperWithOneProp { Number = 1 } },
            };

            // Assert
            sut.Should().NotContainNullsOrEmptyEnumerables();
        }

        [Fact]
        public void StackOverflowGuard()
        {
            // Arrange
            var testHelper1 = new RecursiveTestHelper();
            var testHelper2 = new RecursiveTestHelper();
            testHelper1.TestHelperHelper = testHelper2;
            testHelper2.TestHelperHelper = testHelper1;

            // Assert
            testHelper1.Should().NotContainNullsOrEmptyEnumerables();
        }

        [Fact]
        public void StackOverflowListGuard()
        {
            // Arrange
            var testHelper1 = new RecursiveTestListHelper { List = new List<RecursiveTestListHelper>() };
            var testHelper2 = new RecursiveTestListHelper { List = new List<RecursiveTestListHelper>() };
            testHelper1.List.Add(testHelper2);
            testHelper2.List.Add(testHelper1);

            // Assert
            testHelper1.Should().NotContainNullsOrEmptyEnumerables();
        }
    }
}
