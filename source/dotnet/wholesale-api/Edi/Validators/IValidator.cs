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

namespace Energinet.DataHub.Wholesale.EDI.Validators;

public interface IValidator<T>
{
    public IReadOnlyList<IValidationRule<T>> Rules { get; set; }

    public bool Validate(T subject, out List<ValidationError> errors)
    {
        if (subject == null) throw new ArgumentNullException(nameof(subject));

        errors = new List<ValidationError>();
        foreach (var rule in Rules.Where(rule => rule.Support(subject.GetType())))
        {
            rule.Validate(subject, out var ruleErrors);
            errors.AddRange(ruleErrors);
        }

        return !errors.Any();
    }
}
