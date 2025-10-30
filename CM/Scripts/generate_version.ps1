Write-Host "----------------------------------------------"
Write-Host "  generate_version.ps1"
Write-Host "----------------------------------------------"

# Define GitLab API variables
$apiUrl = "$env:CI_API_V4_URL/projects/$env:CI_PROJECT_ID/variables"
$headers = @{ "PRIVATE-TOKEN" = "$env:GITLAB_API_TOKEN" }

Write-Host "API Url: $apiUrl"
Write-Host "Static Version: $env:STATIC_VERSION"
Write-Host "Current Branch: $env:CI_COMMIT_REF_NAME"

# Check if this pipeline was triggered by a Git tag
$isTagRelease = $env:CI_COMMIT_TAG -ne $null
if ($isTagRelease) {
    Write-Host "Detected Git tag: $env:CI_COMMIT_TAG. Adjusting build number logic for release."
}

$createLastVersion = $false
$createBuildCount = $false
$buildNumber = 0 # Initialize buildNumber

# -------------------------------------------------------------------
# STEP 1: Determine the next build number
# -------------------------------------------------------------------

if ($isTagRelease) {
    # --- LOGIC FOR TAG RELEASE ---
    # When a tag is pushed, we fetch the LATEST build number from the
    # MAIN branch (as defined by $env:MAIN_BUILD_COUNTER_VAR).
    # We DO NOT increment this number, nor do we update the main branch's counter.

    # Ensure the main build counter variable name is available
    if (-not $env:MAIN_BUILD_COUNTER_VAR) {
        Write-Error "Error: MAIN_BUILD_COUNTER_VAR is not set for release build. Exiting."
        exit 1
    }

    Write-Host "Fetching last build number from MAIN branch variable '$env:MAIN_BUILD_COUNTER_VAR'..."
    try {
        $buildCounterResponse = Invoke-RestMethod -Uri "$apiUrl/$env:MAIN_BUILD_COUNTER_VAR" -Headers $headers -Method Get
        # Set the release build number to the last completed build number
        $buildNumber = [int]$buildCounterResponse.value
        Write-Host "Using main branch build number: $buildNumber"
    } catch {
        Write-Host "Warning: Could not fetch main branch build number. Starting release build number at 1."
        $buildNumber = 1
    }

} else {
    # --- LOGIC FOR REGULAR BRANCH BUILD (Original Logic) ---

    # Fetch the last stored static version for the current branch
    Write-Host "Fetching last stored static version for branch variable '$env:LAST_VERSION_VAR'..."
    try {
        $lastVersionResponse = Invoke-RestMethod -Uri "$apiUrl/$env:LAST_VERSION_VAR" -Headers $headers -Method Get
        $lastVersion = $lastVersionResponse.value
    } catch {
        Write-Host "No previous static version found. Setting to empty."
        $lastVersion = ""
        $createLastVersion = $true
    }

    # Fetch and increment the build number for the current branch
    Write-Host "Fetching and incrementing build number for branch variable '$env:BUILD_COUNTER_VAR'..."
    try {
        $buildCounterResponse = Invoke-RestMethod -Uri "$apiUrl/$env:BUILD_COUNTER_VAR" -Headers $headers -Method Get
        # Increment the fetched value
        $buildNumber = [int]$buildCounterResponse.value + 1
    } catch {
        Write-Host "No previous build number found. Starting at 1."
        $buildNumber = 1
        $createBuildCount = $true
    }

    # Check for static version change
    if ($lastVersion -ne $env:STATIC_VERSION) {
        Write-Host "Static Version changed! Resetting build number to 1..."
        $buildNumber = 1
    }
}

# -------------------------------------------------------------------
# STEP 2: Generate version and set environment variable
# -------------------------------------------------------------------

# Generate the new version
$env:VERSION = "$env:STATIC_VERSION.$buildNumber"
Write-Host "Generated Version: $env:VERSION"

# Save version to file
$env:VERSION | Out-File -FilePath "version.txt"

# -------------------------------------------------------------------
# STEP 3: Update CI/CD Variables (Only for Regular Branch Builds)
# -------------------------------------------------------------------

if (-not $isTagRelease) {
    Write-Host "Updating CI/CD variables for regular build (incrementing counter)..."

    # --- Update BUILD_COUNTER_VAR ---
    if ($createBuildCount) {
        Write-Host "Creating variable $env:BUILD_COUNTER_VAR"
        $payloadBuildNum = @{
            key = $env:BUILD_COUNTER_VAR
            value = "$buildNumber"
        } | ConvertTo-Json -Depth 10
        Invoke-RestMethod -Uri "$apiUrl" -Headers $headers -Method Post -Body $payloadBuildNum -ContentType "application/json"
    } else {
        Write-Host "Updating variable $env:BUILD_COUNTER_VAR"
        $payloadBuildNum = @{
            value = "$buildNumber"
        } | ConvertTo-Json -Depth 10
        Invoke-RestMethod -Uri "$apiUrl/$env:BUILD_COUNTER_VAR" -Headers $headers -Method Put -Body $payloadBuildNum -ContentType "application/json"
    }

    # --- Update LAST_VERSION_VAR ---
    if ($createLastVersion) {
        Write-Host "Creating variable $env:LAST_VERSION_VAR"
        $payloadLastVer = @{
            key = $env:LAST_VERSION_VAR
            value = "$env:STATIC_VERSION"
        } | ConvertTo-Json -Depth 10
        Invoke-RestMethod -Uri "$apiUrl" -Headers $headers -Method Post -Body $payloadLastVer -ContentType "application/json"
    } else {
        Write-Host "Updating variable $env:LAST_VERSION_VAR"
        $payloadLastVer = @{
            value = "$env:STATIC_VERSION"
        } | ConvertTo-Json -Depth 10
        Invoke-RestMethod -Uri "$apiUrl/$env:LAST_VERSION_VAR" -Headers $headers -Method Put -Body $payloadLastVer -ContentType "application/json"
    }
} else {
    Write-Host "Skipping CI/CD variable updates because this is a release tag. Counter is not incremented."
}
