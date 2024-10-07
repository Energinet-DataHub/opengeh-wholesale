# Run script before committing changes to the dashboard JSON files

# Define the input file and output file paths
$file = "../dashboards/datahub-overview.json"

# Read the JSON file
$jsonContent = Get-Content $file -Raw

# Replace all occurrences of "*-we-00*" with "$envShort-we-$instance"
$updatedContent = $jsonContent -replace "[dtbp]-we-00[0-9]", '${env_short}-we-${instance}'


$updatedContent = $updatedContent -replace '\${__series.name}', '$$${__series.name}'

# Escape tenant id but replace all other occurrences of GUIDs with "$subscription"
$updatedContent = $updatedContent -replace "(?!f7619355-6c67-4100-9a78-1847f30742e2)[0-9A-Fa-f]{8}-([0-9A-Fa-f]{4}-){3}[0-9A-Fa-f]{12}", '${subscription}'

$updatedContent = $updatedContent -replace "\b(?:test|dev|preprod|prod)-00[0-9]", '${environment}-${instance}'

# Write the updated content to the output file
$updatedContent | Set-Content $file

Write-Host "Replacements complete. Updated JSON saved to $file"
