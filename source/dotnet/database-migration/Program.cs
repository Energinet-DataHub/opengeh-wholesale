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

namespace Energinet.DataHub.Wholesale.DatabaseMigration;

internal static class Program
{
    public static int Main(string[] args)
    {
        // If you are migrating to SQL Server Express use connection string "Server=(LocalDb)\\MSSQLLocalDB;..."
        // If you are migrating to SQL Server use connection string "Server=localhost;..."
        var connectionString =
            args.FirstOrDefault()
            ?? "Server=localhost;Database=wholesale;Trusted_Connection=True;Encrypt=No;";

        Console.WriteLine($"Performing upgrade on {connectionString}");
        var result = Upgrader.DatabaseUpgrade(connectionString);

        if (!result.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(result.Error);
            Console.ResetColor();
#if DEBUG
            Console.ReadLine();
#endif
            return -1;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Success!");
        Console.ResetColor();
        return 0;
    }
}
