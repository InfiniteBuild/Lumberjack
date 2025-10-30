Write-Host "Starting release creation via GitLab CLI (glab)..."
$glabExe = "$env:CI_PROJECT_DIR\buildtools\glab\glab.exe"

& $glabExe config set host $env:CI_SERVER_URL --global
$env:GITLAB_TOKEN = $env:CI_JOB_TOKEN

Write-Host "Logging in..."
& $glabExe auth login --hostname $env:CI_SERVER_URL --token $env:CI_JOB_TOKEN

Write-Host "Creating GitLab Release..."

# Example: Define release description content (use markdown)
$releaseDescription = "## Release $env:CI_COMMIT_TAG`n`nAutomated final release build."

$releaseCommand = 'release create $env:CI_COMMIT_TAG --name "Official Release $env:CI_COMMIT_TAG" --notes "$releaseDescription"'

Write-Host "Release Command Args: "
Write-Host "$releaseCommand"

& $glabExe $releaseCommand

Write-Host "Release creation initiated for tag $env:CI_COMMIT_TAG."

Write-Host "Upload Asset - zip file"
& $glabExe relase upload $env:CI_COMMIT_TAG "$env:publishDir\zip\Lumberjack_0.0.29.zip#Lumberjack"
