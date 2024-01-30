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

using System.Text.RegularExpressions;
using Energinet.DataHub.Wholesale.Batches.Interfaces;

namespace Energinet.DataHub.Wholesale.Batches.Application.Model;

public sealed record GridAreaCode
{
    public GridAreaCode(string code)
    {
        ArgumentNullException.ThrowIfNull(code);
        if (!Regex.IsMatch(code, @"^((00\d)|(0[1-9]\d)|([1-9]\d\d))$", RegexOptions.ECMAScript))
            throw new BusinessValidationException("Code must be 3 characters number with left padded zeros");

        Code = code;
    }

    /// <summary>
    /// A max 3 digit number with left padded zeros to ensure an exact total of 3 characters.
    /// Examples: 001, 010, 987
    /// </summary>
    public string Code { get; }
}
