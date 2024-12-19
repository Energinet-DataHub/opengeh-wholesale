Install-Module -Name PSGraphQL -Repository PSGallery -Scope CurrentUser -Force

###############################################################################
# SCRIPT CONSTANTS
# Functions within the script can use these variables as constants.
###############################################################################

# Zenhub workspace id for "Mandalorian" workspace
$WorkspaceId = "5fd22a1a59e4740018f5ab5a"

# Zenhub repository id for "team-mandalorian" repository; found it using the graph QL API, see https://developers.zenhub.com/guides/getting-entity-ids#repository-ids for details
$IssuesRepositoryId = "Z2lkOi8vcmFwdG9yL1JlcG9zaXRvcnkvMTMzODk1Mzc4"

###############################################################################
# FUNCTIONS
###############################################################################

<#
    .SYNOPSIS
    Return a Powershell hashtable with the authorization header set to env:ZENHUB_APIKEY.
    Throws an error if env:ZENHUB_APIKEY is not set
#>
function Get-AuthorizationHeader {
    if ($null -eq $env:ZENHUB_APIKEY) {
        throw "Environment variable ZENHUB_APIKEY is not set"
    }

    return @{Authorization = "Bearer $env:ZENHUB_APIKEY" }
}

<#
    .SYNOPSIS
    Get top 100 issues for a specific pipeline in Zenhub
#>
function Get-IssuesForPipeline {
    param(
        [Parameter(Mandatory)]
        [string]
        $PipelineId
    )

    $uri = "https://api.zenhub.com/public/graphql"

    $operationName = "getIssuesByPipeline"
    $variables = @"
        {
            "pipelineId": "$PipelineId",
            "filters": {
                "repositoryIds": "$IssuesRepositoryId"
            },
            "workspaceId": "$WorkspaceId"
        }
"@

    # WARNING:
    # The query below uses 'parentEpics' which is actually deprecated, but then again not.
    # Please read this issue to understand: https://app.zenhub.com/workspaces/the-outlaws-6193fe815d79fc0011e741b1/issues/gh/energinet-datahub/team-the-outlaws/1105
    $myQuery = '
    query getIssuesByPipeline($pipelineId: ID!, $filters: IssueSearchFiltersInput!, $workspaceId:ID!) {
        searchIssuesByPipeline(pipelineId: $pipelineId, filters: $filters, first: 100) {
            nodes {
                id
                number
                title
                body
                htmlUrl
                assignees {
                    totalCount
                }
                estimate {
                    value
                }
                labels {
                    nodes {
                        name
                    }
                }
                parentEpics {
                    totalCount
                }
                sprints {
                    totalCount
                    nodes {
                        state
                    }
                }
                pipelineIssue (workspaceId:$workspaceId) {
                    pipeline {
                      name
                    }
                }
                repository {
                    id
                    ghId
                    name
                }
            }
        }
    }'

    $result = Invoke-GraphQLQuery -Query $myQuery -Headers (Get-AuthorizationHeader) -OperationName $operationName -Variables $variables -Uri $uri

    if ($result.errors) {
        Write-Host ($result | ConvertTo-Json -Depth 20)
        throw "Error: $($result.errors.message) when invoking query '$operationName' with variables '$variables'"
    }

    return $result
}

<#
    .SYNOPSIS
    Get pipeline names from Zenhub given the workspace id
#>
function Get-Pipelines {
    param(
        [Parameter(Mandatory)]
        [string]
        $WorkspaceId
    )

    $uri = "https://api.zenhub.com/public/graphql"

    $operationName = "getPipelinesForWorkspace"
    $variables = @"
    {
        "workspaceId": '$WorkspaceId'
    }
"@

    $myQuery = '
    query getPipelinesForWorkspace($workspaceId: ID!) {
        workspace(id: $workspaceId) {
          id
          pipelinesConnection(first: 20) {
            nodes {
              id
              name
            }
          }
        }
      }
    '

    $result = Invoke-GraphQLQuery -Query $myQuery -Headers (Get-AuthorizationHeader) -OperationName $operationName -Variables $variables -Uri $uri
    if ($result.errors) {
        Write-Host ($result | ConvertTo-Json -Depth 20)
        throw "Error: $($result.errors.message) when invoking query '$operationName' with variables '$variables'"
    }

    $returnArr = @()
    foreach ($pipeline in $result.data.workspace.pipelinesConnection.nodes) {
        $returnArr += $pipeline
    }

    return $returnArr
}

<#
    .SYNOPSIS
    Return a deep-link HTML URL to the issue on the Outlaws Zenhub board
#>
function Get-IssueUrl {
    param(
        [Parameter(Mandatory)]
        [int]
        $IssueNumber
    )

    return "<a href='https://app.zenhub.com/workspaces/the-outlaws-6193fe815d79fc0011e741b1/issues/gh/energinet-datahub/team-the-outlaws/$IssueNumber'>$IssueNumber</a>"
}


<#
    .SYNOPSIS
    Return list of pipeline names that are not expected
#>
function Test-PipelineNames {
    param(
        [Parameter(Mandatory)]
        [Array]
        $Pipelines,
        [Array]
        $ExpectedPipelines = @("Refinement", "Backlog", "Development", "Testing")
    )

    $errors = @()
    foreach ($pipeline in $pipelines) {
        if ($ExpectedPipelines -notcontains $pipeline.name) {
            $errors += "Pipeline name '$($pipeline.name)' in Zenhub was not expected"
        }
    }

    return $errors
}


<#
    .SYNOPSIS
    Return list of issues where title length is less than 10 characters
#>
function Test-Title {
    param(
        [Parameter(Mandatory)]
        [array]
        [AllowEmptyCollection()]
        $Issues
    )

    $errors = @()
    foreach ($issue in $issues) {
        if ($issue.title.length -lt 10) {
            $errors += "Issue $(Get-IssueUrl -IssueNumber $issue.number) in $($issue.pipelineIssue.pipeline.name) has less than 10 characters in the title"
        }
    }

    return $errors
}

<#
    .SYNOPSIS
    Return list of issues where description length is less than 50 characters
#>
function Test-Description {
    param(
        [Parameter(Mandatory)]
        [array]
        [AllowEmptyCollection()]
        $Issues
    )

    $errors = @()
    foreach ($issue in $issues) {
        if ((Get-IsIssue -Issue $issue) -and $issue.body.Length -lt 50) {
            $errors += "Issue $(Get-IssueUrl -IssueNumber $issue.number) in $($issue.pipelineIssue.pipeline.name) has less than 50 characters in the description"
        }
    }

    return $errors
}

<#
    .SYNOPSIS
    Return list of issues that do not have an estimate
#>
function Test-Estimate {
    param(
        [Parameter(Mandatory)]
        [array]
        [AllowEmptyCollection()]
        $Issues
    )

    $errors = @()
    foreach ($issue in $issues) {
        if ((Get-IsIssue -Issue $issue) -and (($null -eq $issue.estimate) -or ($null -eq $issue.estimate.value) -or ($issue.estimate.value -eq 0))) {
            $errors += "Issue $(Get-IssueUrl -IssueNumber $issue.number) in $($issue.pipelineIssue.pipeline.name) has no estimate"
        }
    }

    return $errors
}

<#
    .SYNOPSIS
    Return whether an issue is not labelled as a 'Productgoal', 'Metric', 'RetroAction' or 'Sprint Goal'
#>
function Get-IsIssue {
    param(
        [Parameter(Mandatory)]
        [Object]
        $Issue
    )

    foreach ($label in $Issue.labels.nodes.name) {
        if ('Productgoal,Metric,Sprintgoal,RetroAction'.Split(',') -contains $label) {
            return $false
        }
    }

    return $true
}

<#
    .SYNOPSIS
    Return list of issues that do not have expected labels attached
#>
function Test-SegmentationMetadata {
    param(
        [Parameter(Mandatory)]
        [array]
        [AllowEmptyCollection()]
        $Issues
    )

    $errors = @()
    foreach ($issue in $Issues) {
        $isRefinementSwimlane = $issue.pipelineIssue.pipeline.name -eq 'Refinement'

        $issueLabels = $issue.labels.nodes.name -join ','
        $issueHasToRefineOrRefinedLabel = Search-Label -Source 'ToRefine,Refined' -Target $issueLabels
        if (!$isRefinementSwimlane -and $issueHasToRefineOrRefinedLabel) {
            $errors += "Issue $(Get-IssueUrl -IssueNumber $issue.number) in $($issue.pipelineIssue.pipeline.name) has label ToRefine or Refined"
        }

        $expectedLabels = 'Bug,Maintenance,Spike'
        if ($isRefinementSwimlane) {
            $expectedLabels += ',ToRefine,Refined'
        }
        $expectedLabelsFound = Search-Label -Source $expectedLabels -Target $issueLabels
        if ($expectedLabelsFound -eq $false) {
            # Somebody forgot labelling issues according to team agreements, assist support with helpful errormessage
            if ($isRefinementSwimlane -and !$issueHasToRefineOrRefinedLabel) {
                $errors += "Issue $(Get-IssueUrl -IssueNumber $issue.number) in $($issue.pipelineIssue.pipeline.name) is in Refinement without either a ToRefine or Refined label"
            }

            if ((Get-IsIssue -Issue $issue) -and ($issue.parentEpics.totalCount -eq 0)) {
                $errors += "Issue $(Get-IssueUrl -IssueNumber $issue.number) in $($issue.pipelineIssue.pipeline.name) has no Epic and no label named 'Maintenance', 'Bug' or 'Spike'"
            }
        }
    }
    return $errors
}

<#
.SYNOPSIS
Return true if a label in Target exists in Source
Source and target are comma-delimited strings

.EXAMPLE
Search-Label -Source 'ToRefine,Refined' -Target 'Blocked,Spike' --> returns false
Search-Label -Source 'ToRefine,Refined' -Target 'Refined,Blocked' --> returns true

#>
function Search-Label {
    param(
        [string]
        $Source = '',
        [string]
        $Target = ''
    )

    foreach ($src in ($Source -split ',')) {
        foreach ($tgt in ($Target -split ',')) {
            if ($src -eq $tgt) {
                return $true
            }
        }
    }

    return $false
}

<#
    .SYNOPSIS
    Return list of issues that are not assigned to anyone
#>
function Test-Assignee {
    param(
        [Parameter(Mandatory)]
        [array]
        [AllowEmptyCollection()]
        $Issues
    )

    $errors = @()
    foreach ($issue in $issues) {
        if ((Get-IsIssue -Issue $issue) -and $issue.assignees.totalCount -eq 0) {
            $errors += "Issue $(Get-IssueUrl -IssueNumber $issue.number) in $($issue.pipelineIssue.pipeline.name) is not assigned to anyone"
        }
    }

    return $errors
}

<#
    .SYNOPSIS
    Return list of issues that are not assigned to an open sprint
#>
function Test-AssignedSprint {
    param(
        [Parameter(Mandatory)]
        [array]
        [AllowEmptyCollection()]
        $Issues
    )

    $errors = @()
    foreach ($issue in $Issues) {
        if (Get-IsIssue -Issue $issue) {
            if (($issue.sprints.totalCount -eq 0) -or ($null -eq ($issue.sprints.nodes | Where-Object state -Contains 'OPEN'))) {
                $errors += "Issue $(Get-IssueUrl -IssueNumber $issue.number) in $($issue.pipelineIssue.pipeline.name) is not assigned to an open sprint"
            }
        }
    }
    return $errors
}

<#
    .SYNOPSIS
    Test all issues in the Outlaws workspace for quality
    Returns a list of issues that need to be corrected containing deeplinks to individual issues
#>
function Test-IssueQuality {
    $pipelines = Get-Pipelines -WorkspaceId $WorkspaceId
    $testresults = @()
    $testresults += Test-PipelineNames -Pipelines $pipelines

    foreach ($pipeline in $pipelines) {
        Write-Host "Pipeline: $($pipeline.name)"
        $issues = (Get-IssuesForPipeline -PipelineId $pipeline.id).data.searchIssuesByPipeline.nodes

        Write-Host "  Issues count: $($issues.Count)"

        switch ($pipeline.name) {
            "Refinement" {
                $testresults += Test-Title -Issues $issues
            }
            "Backlog" {
                $testresults += Test-Title -Issues $issues
            }       
            "Development" {
                $testresults += Test-Title -Issues $issues
                $testresults += Test-Assignee -Issues $issues
                
            }           
            "Testing" {
                $testresults += Test-Title -Issues $issues
                $testresults += Test-Assignee -Issues $issues
            }                  
        }
    }

    return $testresults
}
