Write-Host "Starting release creation via GitLab CLI (glab)..."
$glabExe = "$env:CI_PROJECT_DIR\buildtools\glab\glab.exe"

& $glabExe config set host $env:CI_SERVER_URL --global
$env:GITLAB_TOKEN = $env:CI_JOB_TOKEN

Write-Host "Creating GitLab Release..."

# Example: Define release description content (use markdown)
$releaseDescription = "## Release $env:CI_COMMIT_TAG`n`nAutomated final release build."

$assetLinkObject = @{
	name = "Lumberjack"
	url = "$CI_PROJECT_URL/-/jobs/$CI_JOB_ID/artifacts/file/publish/zip/Lumberjack_$CI_BUILD_VERSION.zip"
	link_type = "package"
}
$assetsJson = @($assetLinkObject) | ConvertTo-Json -Compress -Depth 100
Write-Host "Generated Assets JSON: $assetsJson"

$releaseCommand = "
	$glabExe release create $env:CI_COMMIT_TAG 
		--name 'Official Release $env:CI_COMMIT_TAG' 
		--notes '$releaseDescription' 
		--assets-links '$assetsJson'
	"

Write-Host "Final Command String:"
Write-Host $releaseCommand

& $glabExe $glabArgs

Write-Host "Release creation initiated for tag $env:CI_COMMIT_TAG."