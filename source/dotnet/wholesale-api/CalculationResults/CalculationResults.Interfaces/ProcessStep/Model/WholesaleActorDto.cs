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

namespace Energinet.DataHub.Wholesale.Contracts;

/// <summary>
/// The desired name of this type is "ActorDto". That now, however, causes problems
/// regarding the front-end as it is also used in the wholesale API, which in turn is used
/// in the published wholesale NuGet package, which is used to generate DTOs in
/// the front-end. In the front-end the name "ActorDto" has already been taken.
/// </summary>
/// <param name="Gln"></param>
public sealed record WholesaleActorDto(string Gln);
