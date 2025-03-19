# Define the path of the workbook relative to the script's location
$workbookPath = Join-Path -Path $PSScriptRoot -ChildPath "..\The Covernator.xlsx"

# Create a new Excel application object
$Excel = New-Object -ComObject Excel.Application

# Set Excel to run in the background
$Excel.Visible = $false

# Open the workbook using the full path
$Workbook = $Excel.Workbooks.Open($workbookPath)

# Refresh all data connections
$Workbook.RefreshAll()

# Wait for refresh to complete (if asynchronous)
Start-Sleep -Seconds 5

# Save the changes
$Workbook.Save()

# Close the workbook and quit Excel
$Workbook.Close($true)
$Excel.Quit()

# Release COM objects
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($Workbook) | Out-Null
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($Excel) | Out-Null
[System.GC]::Collect()
[System.GC]::WaitForPendingFinalizers()

Write-Host "The Covernator has been updated ... "
