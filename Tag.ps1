# Tag.ps1
# PowerShell script to build, test, tag, push, and publish NuGet package

$ErrorActionPreference = 'Stop'

# Parse arguments
$force = $false
if ($args -contains '--force') {
    $force = $true
    Write-Output "[WARNING] --force specified: Will publish even if unit tests fail."
}

# Variables
$solution = "Ollama.Api.slnx"
$project = "Ollama.Api/Ollama.Api.csproj"
$tokenFile = "nuget_token.txt"

# Ensure token file exists
if (!(Test-Path $tokenFile)) {
    Write-Error "NuGet token file '$tokenFile' not found. Aborting."
    exit 1
}
$nugetToken = Get-Content $tokenFile | Select-Object -First 1

# Ensure nbgv is installed
if (-not (Get-Command "nbgv" -ErrorAction SilentlyContinue)) {
    Write-Output "nbgv not found. Installing..."
    dotnet tool install -g nbgv
    $env:PATH += ";$env:USERPROFILE\.dotnet\tools"
}

# Ensure git working directory is clean and up to date
Write-Output "Checking git status..."
$gitStatus = git status --porcelain
if ($gitStatus) {
    Write-Error "You have uncommitted changes. Please commit or stash them before running this script."
    exit 1
}

Write-Output "Fetching latest from origin..."
git fetch origin

Write-Output "Checking for unpushed commits..."
$localHash = git rev-parse '@'
$remoteHash = git rev-parse '@{u}'
if ($localHash -ne $remoteHash) {
    Write-Error "Your branch is not in sync with origin. Please push or pull changes before running this script."
    exit 1
}

# Build
Write-Output "Building solution..."
dotnet build $solution --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed. Aborting release."
    exit 1
}

# Test (skip if --force specified)
if (-not $force) {
    Write-Output "Running unit tests..."
    $testResult = dotnet test $solution --configuration Release --no-build
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Unit tests failed. Aborting release. (Use --force to override)"
        exit 1
    }
} else {
    Write-Output "[INFO] Skipping unit tests due to --force."
}

# Get version from Nerdbank.GitVersioning
$nbgvOutput = nbgv get-version
$versionLine = $nbgvOutput | Select-String 'NuGetPackageVersion:'
$version = $versionLine -replace 'NuGetPackageVersion:\s*', '' -replace '\s', ''
if (-not $version) {
    Write-Error "Could not determine version from Nerdbank.GitVersioning."
    exit 1
}

# Tag and push
Write-Output "Tagging version v$version..."
git tag v$version
Write-Output "Pushing tags..."
git push origin v$version

# Pack
Write-Output "Packing NuGet package..."
dotnet pack $project --configuration Release --no-build -p:PackageVersion=$version

# Publish
$packagePath = "Ollama.Api/bin/Release/Ollama.Api.$version.nupkg"
if (!(Test-Path $packagePath)) {
    Write-Error "NuGet package not found at $packagePath."
    exit 1
}
Write-Output "Publishing NuGet package..."
dotnet nuget push $packagePath --api-key $nugetToken --source https://api.nuget.org/v3/index.json

Write-Output "Done."
