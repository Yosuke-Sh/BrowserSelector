# BrowserSelector Coverage Test Script

param(
    [string]$OutputDir = "coverage-results",
    [switch]$Clean = $false
)

Write-Host "=== BrowserSelector Coverage Test Script ===" -ForegroundColor Green

# Output directory settings
$TestResultsDir = "TestResults"
$CoverageReportDir = $OutputDir

# Cleanup
if ($Clean -or (Test-Path $TestResultsDir)) {
    Write-Host "Cleaning up existing test results..." -ForegroundColor Yellow
    Remove-Item -Path $TestResultsDir -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path $CoverageReportDir -Recurse -Force -ErrorAction SilentlyContinue
}

# Create directories
New-Item -ItemType Directory -Path $TestResultsDir -Force | Out-Null
New-Item -ItemType Directory -Path $CoverageReportDir -Force | Out-Null

Write-Host "`n=== 1. UnitTests (Core/Infrastructure/Presentation) ===" -ForegroundColor Cyan
dotnet test tests/BrowserSelector.UnitTests --collect:"XPlat Code Coverage" --results-directory "./$TestResultsDir/UnitTests" --verbosity normal

Write-Host "`n=== 2. AppTests (App Only) ===" -ForegroundColor Cyan
dotnet test tests/BrowserSelector.AppTests --collect:"XPlat Code Coverage" --results-directory "./$TestResultsDir/AppTests" --verbosity normal

Write-Host "`n=== 3. UITests (UI Only) ===" -ForegroundColor Cyan
dotnet test tests/BrowserSelector.UITests --collect:"XPlat Code Coverage" --results-directory "./$TestResultsDir/UITests" --verbosity normal

Write-Host "`n=== 4. Generate Integrated Coverage Report ===" -ForegroundColor Cyan
reportgenerator -reports:"$TestResultsDir\**\coverage.cobertura.xml" -targetdir:"$CoverageReportDir" -reporttypes:"Html"

Write-Host "`n=== AltCover (optional) ===" -ForegroundColor Cyan
try {
    dotnet test tests/BrowserSelector.UnitTests /p:AltCover=true --results-directory "./$TestResultsDir/UnitTests.AltCover" --verbosity normal
    dotnet test tests/BrowserSelector.AppTests   /p:AltCover=true --results-directory "./$TestResultsDir/AppTests.AltCover"   --verbosity normal
    dotnet test tests/BrowserSelector.UITests    /p:AltCover=true --results-directory "./$TestResultsDir/UITests.AltCover"    --verbosity normal
} catch {
    Write-Warning "AltCover execution failed: $($_.Exception.Message)"
}

Write-Host "`n=== 4b. Merge AltCover reports (if any) ===" -ForegroundColor Cyan
reportgenerator -reports:"$TestResultsDir\**\coverage.cobertura.xml;$TestResultsDir\**\coverage.xml" -targetdir:"$CoverageReportDir" -reporttypes:"Html"

Write-Host "`n=== 5. Check Results ===" -ForegroundColor Cyan

# Check coverage files
$coverageFiles = Get-ChildItem -Path $TestResultsDir -Recurse -Name "coverage.cobertura.xml"
Write-Host "Generated coverage files:" -ForegroundColor Green
foreach ($file in $coverageFiles) {
    Write-Host "  - $file" -ForegroundColor White
}

# Check integrated report
if (Test-Path "$CoverageReportDir/index.html") {
    Write-Host "`nIntegrated report generated:" -ForegroundColor Green
    Write-Host "  - $CoverageReportDir/index.html" -ForegroundColor White
} else {
    Write-Warning "Failed to generate integrated report"
}

Write-Host "`n=== Coverage Test Complete ===" -ForegroundColor Green
Write-Host "Please open $CoverageReportDir/index.html for detailed report." -ForegroundColor Yellow