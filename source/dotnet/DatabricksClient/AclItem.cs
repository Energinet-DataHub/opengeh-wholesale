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

using Newtonsoft.Json;

namespace Microsoft.Azure.Databricks.Client
{
    /// <summary>
    /// An item representing an ACL rule applied to the given principal (user or group) on the associated scope point.
    /// </summary>
    public class AclItem
    {
        /// <summary>
        /// The principal to which the permission is applied. This field is required.
        /// </summary>
        [JsonProperty(PropertyName = "principal")]
        public string Principal { get; set; }

        /// <summary>
        /// The permission level applied to the principal. This field is required.
        /// </summary>
        [JsonProperty(PropertyName = "permission")]
        public AclPermission Permission { get; set; }
    }
}
