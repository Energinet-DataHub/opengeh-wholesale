<#
.SYNOPSIS
Push a list of messages to i.e. a Teams channel
#>
function Push-NotificationToTeam {
    param(
        [Parameter(Mandatory)]
        [string]
        $MessageSubject,
        [Parameter(Mandatory)]
        [Array]$Messages,
        [Parameter(Mandatory)]
        [string]$FromEmail,
        [Parameter(Mandatory)]
        [string]$ToEmail,
        [Parameter(Mandatory)]
        [string]$SendgridApiKey
    )

    if ($Messages.Length -eq 0) {
        return
    }

    try {
        # Note: You can add multiple recipients by adding more objects to the 'sendTo' array
        $sendTo = @"
        [
            {
                "email": "$ToEmail",
                "name": "Team Mandalorian"
            }
        ]
"@

        $bodyData = @"
        {
            "personalizations": [
                {
                    "to": $sendTo
                }
            ],
            "from": {
                "email": "$FromEmail",
                "name": "DataHub Github"
            },
            "subject": "$MessageSubject",
            "content": [
                {
                    "type": "text/html",
                    "value": "$($Messages -join '<br />')"
                }
            ]
        }
"@

        Write-Host $bodyData
        $headers = @{Authorization = "Bearer $SendgridApiKey" }

        $result = Invoke-WebRequest -Uri 'https://api.sendgrid.com/v3/mail/send' -Method Post -Headers $headers -Body $bodyData -ContentType 'application/json'
        Write-Host $result
    }
    catch {
        Write-Host 'An exception occurred: ', $_.Exception.Message
        Write-Host 'Inner exception: ', $_.Exception.InnerException
        throw
    }
}
