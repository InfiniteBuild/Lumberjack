Write-Host "Starting release creation via GitLab CLI (glab)..."
$glabExe = "$env:CI_PROJECT_DIR\buildtools\glab\glab.exe"
$zipTool = "$env:CI_PROJECT_DIR\buildtools\7-zip\7z.exe"
$version = "$env:CI_BUILD_VERSION"
$zipOutDir = "$env:publishDir\zip"

& $glabExe config set host $env:CI_SERVER_URL --global

Write-Host "Logging in..."
& $glabExe auth login --hostname $env:CI_SERVER_HOST --token $env:GITLAB_API_TOKEN

Write-Host "Creating GitLab Release..."

# Example: Define release description content (use markdown)
$releaseDescription = "## Release $env:CI_COMMIT_TAG`n`nAutomated final release build."

Write-Host "Release Command: "
Write-Host "$glabExe release create $env:CI_COMMIT_TAG"

& $glabExe release create $env:CI_COMMIT_TAG --ref $env:CI_COMMIT_SHA --name "Lumberjack V$env:CI_COMMIT_TAG"

Write-Host "Release created for tag $env:CI_COMMIT_TAG."

Write-Host "Creating Archives"
$zipTool a $zipOutDir\Lumberjack_$version.zip $env:pubRelDir\Lumberjack\**
$zipTool a $zipOutDir\Lumberjack48_$version.zip $env:pubRelDir\Lumberjack48\**

Write-Host "Upload zip files"
& $glabExe release upload $env:CI_COMMIT_TAG "$zipOutDir\Lumberjack_$version.zip#Lumberjack#package"
& $glabExe release upload $env:CI_COMMIT_TAG "$zipOutDir\Lumberjack48_$version.zip#Lumberjack dotnet 4.8#package"


