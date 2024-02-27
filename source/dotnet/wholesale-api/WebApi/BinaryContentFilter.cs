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

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Energinet.DataHub.Wholesale.WebApi;

public class BinaryContentFilter : IOperationFilter
{
    /// <summary>
    /// Configures operations decorated with the <see cref="BinaryContentAttribute" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="context">The context.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var attribute = context.MethodInfo.GetCustomAttributes(typeof(BinaryContentAttribute), false).FirstOrDefault();
        if (attribute == null)
        {
            return;
        }

        operation.Responses = new OpenApiResponses
        {
            {
                "200",
                new OpenApiResponse()
                {
                    Content = new Dictionary<string, OpenApiMediaType>()
            {
                {
                    "application/octet-stream",
                    new OpenApiMediaType()
                    {
                        Schema = new OpenApiSchema()
                        {
                            Type = "string",
                            Format = "binary",
                        },
                    }
                },
            },
                }
            }
        };
    }
}
