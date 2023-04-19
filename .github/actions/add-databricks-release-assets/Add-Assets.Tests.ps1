Describe "Add-Assets" {
    BeforeAll {
        . $PSScriptRoot/Add-Assets.ps1
    }

    BeforeEach {
        New-Item -Path '.\test-files' -ItemType 'directory'
        New-Item -Path '.\test-files\dashboards' -ItemType 'directory'
        New-Item -Path '.\test-files\dashboards\test-dashboard-1.dbdash' -ItemType 'file'
        New-Item -Path '.\test-files\dashboards\test-dashboard-2.dbdash' -ItemType 'file'
    }

    Context "Given a working directory containing wheel distribution files" {
        It "resulting artifacts folder should contain both assets and wheel files" {
            # Act
            Add-Assets -WorkingDirectory '.\test-files'

            # Assert
            Test-Path '.\test-files\artifacts\dashboards' |
                Should -Be $true
            Test-Path '.\test-files\artifacts\dashboards\test-dashboard-1.dbdash' |
                Should -Be $true
            Test-Path '.\test-files\artifacts\dashboards\test-dashboard-2.dbdash' |
                Should -Be $true
        }
    }


    AfterEach {
        Remove-Item -LiteralPath ".\test-files" -Force -Recurse
    }
}
