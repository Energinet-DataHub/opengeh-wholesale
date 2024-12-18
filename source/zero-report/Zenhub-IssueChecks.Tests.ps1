BeforeAll {
    . "$PSScriptRoot/Zenhub-IssueChecks.ps1"
}

Describe "Test-Title" {
    BeforeEach {
        $script:issues = @()
        $script:pipelines = @()
    }

    Context 'Given no issues' {
        It 'should not return an error' {
            $errors = Test-Title -Issues $issues
            $errors | Should -Be $null
        }
    }
    Context "Given a title less than 15 characters" {
        It 'should return an error' {
            $issues += [PSCustomObject]@{
                number = 1
                title  = "<10 chars"
            }

            $errors = Test-Title -Issues $issues
            $errors | Should -BeLike '*has less than 10 characters in the title'
        }
    }
    Context "Given a title more than 10 characters" {
        It 'should not return an error' {
            $issues += [PSCustomObject]@{
                number = 1
                title  = "More than 10 chars"
            }

            $errors = Test-Title -Issues $issues
            $errors | Should -Be $null
        }
    }
}

Describe 'Test-PipelineNames' {
    BeforeEach {
        $script:pipelines = @()
    }

    Context "Given a pipeline in Zenhub that does not exist" {
        It 'should return an error' {
            $pipelines += [PSCustomObject]@{
                name = "FooBar"
            }

            $errors = Test-PipelineNames -Pipelines $pipelines
            $errors | Should -Be "Pipeline name 'FooBar' in Zenhub was not expected"
        }
    }
    Context "Given a pipeline that exists" {
        It 'should not return an error' {
            $pipelines += [PSCustomObject]@{
                name = "Development"
            }

            $errors = Test-PipelineNames -Pipelines $pipelines
            $errors | Should -Be $null
        }
    }
}

Describe 'Test-Description' {
    BeforeEach {
        $script:issues = @()
    }

    Context 'Given no issues' {
        It 'should not return an error' {
            $errors = Test-Description -Issues $issues
            $errors | Should -Be $null
        }
    }
    Context "Given a description less than 50 characters" {
        It 'should return an error when issue is an issue' {
            $issues += [PSCustomObject]@{
                number = 1
                body   = "Less than 50 chars"
            }

            $errors = Test-Description -Issues $issues
            $errors | Should -BeLike '*has less than 50 characters in the description'
        }

        It 'should not return an error when issue is a <Labelname>' -ForEach @(
            @{Labelname = 'Productgoal' }
            @{Labelname = 'Metric' }
        ) {
            $issues += [PSCustomObject]@{
                number = 1
                body   = "Less than 50 chars"
                labels = @(
                    [PSCustomObject]@{
                        nodes = @(
                            [PSCustomObject]@{
                                name = $Labelname
                            }
                        )
                    }
                )
            }

            $errors = Test-Description -Issues $issues
            $errors | Should -Be $null
        }
    }


    Context "Given a description more than 50 characters" {
        It 'should not return an error' {
            $issues += [PSCustomObject]@{
                number = 2
                body   = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit, sed..."
            }

            $errors = Test-Description -Issues $issues
            $errors | Should -Be $null
        }
    }
}


Describe 'Test-Estimate' {
    BeforeEach {
        $script:issues = @()
    }

    Context 'Given no issues' {
        It 'should not return an error' {
            $errors = Test-Estimate -Issues $issues
            $errors | Should -Be $null
        }
    }
    Context 'Given an issue without estimate' {
        It 'should return an error' {
            $issues += [PSCustomObject]@{
                number   = 1
                estimate = $null
            }

            $errors = Test-Estimate -Issues $issues
            $errors | Should -BeLike '*has no estimate'
        }
    }


    Context 'Given issues without estimate' {
        It 'should not return an error when label is <Labelname>' -ForEach @(
            @{Labelname = 'Productgoal' }
            @{Labelname = 'Metric' }
        ) {
            $issues += [PSCustomObject]@{
                number   = 1
                estimate = $null
                labels   = @(
                    [PSCustomObject]@{
                        nodes = @(
                            [PSCustomObject]@{
                                name = $Labelname
                            },
                            [PSCustomObject]@{
                                name = 'The Outlaws'
                            }
                        )
                    }
                )
            }

            $errors = Test-Estimate -Issues $issues
            $errors | Should -Be $null
        }
    }

    Context 'Given an issue with an estimate value of 2' {
        It 'should not return an error' {
            $issues += [PSCustomObject]@{
                number   = 1
                estimate = [PSCustomObject]@{
                    value = 2.0
                }
            }

            $errors = Test-Estimate -Issues $issues
            $errors | Should -Be $null
        }
    }

    Context 'Given an issue with an estimate value of 0' {
        It 'should return an error' {
            $issues += [PSCustomObject]@{
                number   = 1
                estimate = [PSCustomObject]@{
                    value = 0.0
                }
            }

            $errors = Test-Estimate -Issues $issues
            $errors | Should -BeLike '*has no estimate'
        }
    }
}


Describe 'Search-Label' {
    Context 'When sourcearray is empty' {
        It 'should return false' {
            Search-Label -Source $null -Target 'foo,bar' | Should -Be $false
        }
    }

    Context 'When targetarray is empty' {
        It 'should return false' {
            Search-Label -Source 'foo,bar' -Target $null | Should -Be $false
        }
    }

    Context 'When targetarray contains element in source' {
        It 'should return true' {
            Search-Label -Source 'foo,bar' -Target 'bar' | Should -Be $true
        }
    }

    Context 'When targetarray does not contain element in source' {
        It 'should return false' {
            Search-Label -Source 'ToRefine,Refined' -Target 'Blocked,Spike' | Should -Be $false
        }
    }
}


Describe 'Get-IsIssue' {
    BeforeEach {
        $script:issues = @()
    }

    Context 'Given an issue has labels' {
        It 'should return <Expected> when issue is <Labelname>' -ForEach @(
            @{Labelname = 'Metric'; Expected = $false }
            @{Labelname = 'Sprintgoal'; Expected = $false }
            @{Labelname = 'Productgoal'; Expected = $false }
            @{Labelname = 'RetroAction'; Expected = $false }
            @{Labelname = 'EpicX'; Expected = $true }
            @{Labelname = 'Blocked'; Expected = $true }
        ) {
            $issue = [PSCustomObject]@{
                number = 1
                labels = @(
                    [PSCustomObject]@{
                        nodes = @(
                            [PSCustomObject]@{
                                name = $Labelname
                            }
                        )
                    }
                )
            }
            $errors = Get-IsIssue -Issue $issue
            $errors | Should -Be $Expected
        }
    }
}


Describe 'Test-Assignee' {
    BeforeEach {
        $script:issues = @()
    }

    Context 'Given no issues' {
        It 'should not return an error' {
            $errors = Test-Assignee -Issues $issues
            $errors | Should -Be $null
        }
    }
    Context 'Given an issue that has no assignee' {
        It 'should return an error' {
            $issues = @(
                [PSCustomObject]@{
                    number    = 1
                    assignees = @(
                        [PSCustomObject]@{
                            totalCount = 0
                        }
                    )
                }
            )
            $errors = Test-Assignee -Issues $issues
            $errors | Should -BeLike '*is not assigned to anyone'
        }
    }

    Context 'Given an issue that has been assigned to a user' {
        It 'should not return an error' {
            $issues = @(
                [PSCustomObject]@{
                    number    = 1
                    assignees = @(
                        [PSCustomObject]@{
                            totalCount = 1
                        }
                    )
                }
            )
            $errors = Test-Assignee -Issues $issues
            $errors | Should -Be $null
        }
    }
}



Describe 'Test-SegmentationMetadata' {
    BeforeEach {
        $script:issues = @()
    }

    Context 'Given no issues' {
        It 'should not return an error' {
            $errors = Test-SegmentationMetadata -Issues $issues
            $errors | Should -Be $null
        }
    }

    Context 'Given an issue that is not assigned to an Epic and has labels assigned' {
        It 'should return <Expected> when label is <Labelname>' -ForEach @(
            @{Labelname = 'RandomLabel'; Expected = "*has no Epic and no label named 'Maintenance', 'Bug' or 'Spike'" }
            @{Labelname = 'Maintenance'; Expected = $null }
            @{Labelname = 'Bug'; Expected = $null }
            @{Labelname = 'Spike'; Expected = $null }
        ) {
            $issues = @(
                [PSCustomObject]@{
                    number      = 1
                    labels      = @(
                        [PSCustomObject]@{
                            nodes = @(
                                [PSCustomObject]@{
                                    name = $Labelname
                                }
                            )
                        }
                    )
                    parentEpics = @(
                        [PSCustomObject]@{
                            totalCount = 0
                        }
                    )
                }
            )
            $errors = Test-SegmentationMetadata -Issues $issues
            $errors | Should -BeLike $Expected
        }
    }

    Context 'Given an issue that has been assigned to an Epic' {
        It 'should not return an error' {
            $issues = @(
                [PSCustomObject]@{
                    number      = 1
                    parentEpics = @(
                        [PSCustomObject]@{
                            totalCount = 1
                        }
                    )
                }
            )
            $errors = Test-SegmentationMetadata -Issues $issues
            $errors | Should -Be $null
        }
    }

    Context 'If issues exist in Refinement without ToRefine or Refined label' {
        It 'should return an error' {
            $issues = @(
                [PSCustomObject]@{
                    number        = 1
                    pipelineIssue = @(
                        [PSCustomObject]@{
                            pipeline = @(
                                [PSCustomObject]@{
                                    name = 'Refinement'
                                }
                            )
                        }
                    )
                    parentEpics   = @(
                        [PSCustomObject]@{
                            totalCount = 0
                        }
                    )
                }
            )
            $errors = Test-SegmentationMetadata -Issues $issues
            ($errors | Where-Object { $_ -like '*is in Refinement without either a ToRefine or Refined label' } | Measure-Object).Count | Should -Be 1
        }
    }


    Context 'If issues exist in Refinement with Refined or ToRefine label' {
        It 'should not return an error when label is <Labelname>' -ForEach @(
            @{Labelname = 'ToRefine' }
            @{Labelname = 'Refined' }
        ) {
            $issues = @(
                [PSCustomObject]@{
                    number        = 1
                    pipelineIssue = @(
                        [PSCustomObject]@{
                            pipeline = @(
                                [PSCustomObject]@{
                                    name = 'Refinement'
                                }
                            )
                        }
                    )
                    labels        = @(
                        [PSCustomObject]@{
                            nodes = @(
                                [PSCustomObject]@{
                                    name = $Labelname
                                }
                            )
                        }
                    )
                    parentEpics   = @(
                        [PSCustomObject]@{
                            totalCount = 0
                        }
                    )
                }
            )
            $errors = Test-SegmentationMetadata -Issues $issues
            $errors | Should -Be $null
        }
    }

    Context 'If issues exist in other pipelines than Refinement with a ToRefine label' {
        It 'should return an error when label is <Labelname> and pipelinename is <Pipelinename>' -ForEach @(
            @{Pipelinename = 'Product backlog'; Labelname = 'ToRefine' }
            @{Pipelinename = 'Product backlog'; Labelname = 'Refined' }
            @{Pipelinename = 'Issues ready for sprint'; Labelname = 'ToRefine' }
            @{Pipelinename = 'Issues ready for sprint'; Labelname = 'Refined' }
            @{Pipelinename = 'Sprint backlog'; Labelname = 'ToRefine' }
            @{Pipelinename = 'Sprint backlog'; Labelname = 'Refined' }
            @{Pipelinename = 'In progress'; Labelname = 'ToRefine' }
            @{Pipelinename = 'In progress'; Labelname = 'Refined' }
            @{Pipelinename = 'Review/QA'; Labelname = 'ToRefine' }
            @{Pipelinename = 'Review/QA'; Labelname = 'Refined' }
            @{Pipelinename = 'Awaiting'; Labelname = 'ToRefine' }
            @{Pipelinename = 'Awaiting'; Labelname = 'Refined' }
        ) {
            $issues = @(
                [PSCustomObject]@{
                    number        = 1
                    pipelineIssue = @(
                        [PSCustomObject]@{
                            pipeline = @(
                                [PSCustomObject]@{
                                    name = $Pipelinename
                                }
                            )
                        }
                    )
                    labels        = @(
                        [PSCustomObject]@{
                            nodes = @(
                                [PSCustomObject]@{
                                    name = $Labelname
                                },
                                [PSCustomObject]@{
                                    name = 'Bug'  # Use a secondary 'allowed' label
                                }
                            )
                        }
                    )
                    parentEpics   = @(
                        [PSCustomObject]@{
                            totalCount = 0
                        }
                    )
                }
            )
            $errors = Test-SegmentationMetadata -Issues $issues
            $count = ($errors | Where-Object { $_ -like "*in $Pipelinename has label ToRefine or Refined" } | Measure-Object).Count
            $count | Should -Be 1
        }
    }
}


Describe 'Test-AssignedSprint' {
    BeforeEach {
        $script:issues = @()
    }

    Context 'Given no issues' {
        It 'should not return an error' {
            $errors = Test-AssignedSprint -Issues $issues
            $errors | Should -Be $null
        }
    }
    Context 'Given an issue that has no sprint assigned' {
        It 'should return an error' {
            $issues = @(
                [PSCustomObject]@{
                    number  = 1
                    sprints = @(
                        [PSCustomObject]@{
                            totalCount = 0
                        }
                    )
                }
            )
            $errors = Test-AssignedSprint -Issues $issues
            $errors | Should -BeLike '*is not assigned to an open sprint'
        }
    }

    Context 'Given an issue that has been assigned to multiple sprints that are all closed' {
        It 'should not return an error' {
            $issues = @(
                [PSCustomObject]@{
                    number  = 1
                    sprints = @(
                        [PSCustomObject]@{
                            totalCount = 2
                            nodes      = @(
                                [PSCustomObject]@{
                                    state = 'CLOSED'
                                },
                                [PSCustomObject]@{
                                    state = 'CLOSED'
                                }
                            )
                        }
                    )
                }
            )
            $errors = Test-AssignedSprint -Issues $issues
            $errors | Should -BeLike '*is not assigned to an open sprint'
        }
    }

    Context 'Given an issue that has been assigned to multiple sprints where one sprint is open' {
        It 'should not return an error' {
            $issues = @(
                [PSCustomObject]@{
                    number  = 1
                    sprints = @(
                        [PSCustomObject]@{
                            totalCount = 2
                            nodes      = @(
                                [PSCustomObject]@{
                                    state = 'CLOSED'
                                },
                                [PSCustomObject]@{
                                    state = 'OPEN'
                                }
                            )
                        }
                    )
                }
            )
            $errors = Test-AssignedSprint -Issues $issues
            $errors | Should -Be $null
        }
    }
}
