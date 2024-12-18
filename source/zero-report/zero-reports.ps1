using namespace System.Collections.Generic

<#
    .SYNOPSIS
    Run on nightly schedule to clean up legacy releases and notify about changes to newly released Github hosted runner
#>
. "$PSScriptRoot/Zero-Report-Functions.ps1"
. "$PSScriptRoot/Zenhub-IssueChecks.ps1"

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

$errorMessages = @()

Write-Host 'Checking Zenhub issue quality...'
$errorMessages += Test-IssueQuality

# Notify team if errors were found
if ($errorMessages.Count -gt 0) {

    Write-Host ''
    Write-Host '*** ERRORS FOUND ***'
    Write-Host ($errorMessages -join [System.Environment]::NewLine)

    if ($errorMessages.Count -gt 20) {
        $errorMessages = @("Too many errors, see output in <a href='$env:GH_RUNURL' target='_blank'>run</a>")
    }

    #One or more errors were found - team should be notified
    $errorMessages += "<br /><br />Use this to call-to action such as link to Confluence page or similar"

    if ($null -eq $env:SENDGRID_APIKEY) {
        throw 'Environent variable SENDGRID_APIKEY is empty, mails cannot be relayed through Sendgrid'
    }

    if ($null -eq $env:FROM_EMAIL) {
        throw 'Environent variable FROM_EMAIL is empty'
    }

    if ($null -eq $env:TO_EMAIL) {
        throw 'Environent variable TO_EMAIL is empty'
    }

    Push-NotificationToTeam -FromEmail $env:FROM_EMAIL -ToEmail $env:TO_EMAIL -MessageSubject 'Message from scheduled zero-reports run' -Messages $errorMessages -SendgridApiKey $env:SENDGRID_APIKEY
}
else {
    Write-Host ''
    Write-Host '0 errors found, all is good'
}
