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

using System.Collections;
using FluentAssertions;
using FluentAssertions.Primitives;

namespace TestHelpers.FluentAssertionsExtensions
{
    public static class ObjectAssertionsExtensions
    {
        /// <summary>
        /// Recursively checks all properties if any are null. Any Enumerable must be instantiated and contain elements.
        /// </summary>
        /// <param name="obj"></param>
        public static void NotContainNullsOrEmptyEnumerables(this ObjectAssertions obj)
        {
            ArgumentNullException.ThrowIfNull(obj);

            NotContainNullsOrEmptyEnumerables(obj.Subject, new List<object>());
        }

        private static void NotContainNullsOrEmptyEnumerables(object obj, ICollection<object> visitedObjects)
        {
            if (visitedObjects.Any(v => ReferenceEquals(v, obj))) return;
            visitedObjects.Add(obj);

            var objType = obj.GetType();
            var properties = objType.GetProperties();
            foreach (var property in properties)
            {
                var inspectedObjectsValue = property.GetValue(obj);
                if (inspectedObjectsValue is IList inspectedListObject)
                {
                    if (inspectedListObject.Count < 1)
                    {
                        inspectedListObject.Cast<object>().Should().NotBeEmpty($"{property.Name} is instantiated");
                    }

                    foreach (var item in inspectedListObject)
                    {
                        NotContainNullsOrEmptyEnumerables(item!, visitedObjects);
                    }
                }
                else
                {
                    inspectedObjectsValue.Should().NotBe(null, $"You asserted the property with name: {property.Name} not to be");

                    if (property.PropertyType.Assembly == objType.Assembly)
                    {
                        NotContainNullsOrEmptyEnumerables(inspectedObjectsValue!, visitedObjects);
                    }
                }
            }
        }
    }
}
