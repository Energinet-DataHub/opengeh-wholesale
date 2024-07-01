Describe "Add-Assets" {
    BeforeAll {
        . $PSScriptRoot/Add-Assets.ps1
    }

    BeforeEach {
        New-Item -Path '.\test-files\package' -ItemType 'directory'
        New-Item -Path '.\test-files\package\datamigration' -ItemType 'directory'
        New-Item -Path '.\test-files\package\datamigration\migration_scripts' -ItemType 'directory'
        New-Item -Path '.\test-files\package\datamigration\migration_scripts\test-script-1.sql' -ItemType 'file'
    }

    Context "Given a working directory containing wheel distribution files" {
        It "resulting artifacts folder should contain schema migration scripts" {
            # Act
            Add-Assets -WorkingDirectory '.\test-files'

            # Assert
            Test-Path '.\test-files\artifacts\migration_scripts\test-script-1.sql' |
                Should -Be $true
        }
    }

    AfterEach {
        Remove-Item -LiteralPath ".\test-files" -Force -Recurse
    }
}

Describe "Add-Assets-Hive" {
    BeforeAll {
        . $PSScriptRoot/Add-Assets.ps1
    }

    BeforeEach {
        New-Item -Path '.\test-files\package' -ItemType 'directory'
        New-Item -Path '.\test-files\package\datamigration_hive' -ItemType 'directory'
        New-Item -Path '.\test-files\package\datamigration_hive\migration_scripts' -ItemType 'directory'
        New-Item -Path '.\test-files\package\datamigration_hive\migration_scripts\test-script-1.sql' -ItemType 'file'
    }

    Context "Given a working directory containing wheel distribution files" {
        It "resulting artifacts folder should contain schema migration scripts" {
            # Act
            Add-Assets -WorkingDirectory '.\test-files'

            # Assert
            Test-Path '.\test-files\artifacts\hive\migration_scripts\test-script-1.sql' |
                Should -Be $true
        }
    }

    AfterEach {
        Remove-Item -LiteralPath ".\test-files" -Force -Recurse
    }
}