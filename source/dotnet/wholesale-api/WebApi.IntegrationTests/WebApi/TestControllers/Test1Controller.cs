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

using Asp.Versioning;
using Energinet.DataHub.Wholesale.WebApi.V3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.WebApi.TestControllers;

public class Test1Controller : V3ControllerBase
{
    [HttpPost(Name = "CreateTest")]
    [MapToApiVersion(Version)]
    [Authorize(Roles = "TestRole")]
    public async Task CreateAsync()
    {
        await Task.CompletedTask;
    }

    [HttpPost(Name = "CreateTest2")]
    [MapToApiVersion(Version)]
    [Authorize(Roles = "TestRole1")]
    [Authorize(Roles = "TestRole2")]
    public async Task Create2Async()
    {
        await Task.CompletedTask;
    }

    [HttpPost("CreateTest3")]
    [MapToApiVersion(Version)]
    [Authorize(Roles = "TestRole1, TestRole2")]
    public async Task Create3Async()
    {
        await Task.CompletedTask;
    }

    [HttpPost("CreateTest4")]
    [MapToApiVersion(Version)]
    [Authorize]
    public async Task Create4Async()
    {
        await Task.CompletedTask;
    }
}
