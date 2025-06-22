# Tag.ps1
# PowerShell script to build, test, tag, push, and publish NuGet package

$ErrorActionPreference = 'Stop'

# Variables
$solution = "Ollama.Api.sln"
$project = "Ollama.Api/Ollama.Api.csproj"
$tokenFile = "nuget_token.txt"

# Ensure token file exists
if (!(Test-Path $tokenFile)) {
    Write-Error "NuGet token file '$tokenFile' not found. Aborting."
    exit 1
}
$nugetToken = Get-Content $tokenFile | Select-Object -First 1

# Ensure dotnet-nbgv is installed
if (-not (Get-Command "dotnet-nbgv" -ErrorAction SilentlyContinue)) {
    Write-Host "dotnet-nbgv not found. Installing..."
    dotnet tool install -g nbgv
    $env:PATH += ";$env:USERPROFILE\.dotnet\tools"
}

# Ensure git working directory is clean and up to date
Write-Host "Checking git status..."
$gitStatus = git status --porcelain
if ($gitStatus) {
    Write-Error "You have uncommitted changes. Please commit or stash them before running this script."
    exit 1
}

Write-Host "Fetching latest from origin..."
git fetch origin

Write-Host "Checking for unpushed commits..."
$localHash = git rev-parse '@'
$remoteHash = git rev-parse '@{u}'
if ($localHash -ne $remoteHash) {
    Write-Error "Your branch is not in sync with origin. Please push or pull changes before running this script."
    exit 1
}

# Build
Write-Host "Building solution..."
dotnet build $solution --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed. Aborting release."
    exit 1
}

# Test
Write-Host "Running unit tests..."
$testResult = dotnet test $solution --configuration Release --no-build
if ($LASTEXITCODE -ne 0) {
    Write-Error "Unit tests failed. Aborting release."
    exit 1
}

# Get version from Nerdbank.GitVersioning
$version = dotnet nbgv get-version --variable NuGetPackageVersion | Out-String
$version = $version.Trim()
if (-not $version -or $version -match 'Possible reasons') {
    Write-Error "Could not determine version from Nerdbank.GitVersioning."
    exit 1
}

# Tag and push
Write-Host "Tagging version v$version..."
git tag v$version
Write-Host "Pushing tags..."
git push origin v$version

# Pack
Write-Host "Packing NuGet package..."
dotnet pack $project --configuration Release --no-build -p:PackageVersion=$version

# Publish
$packagePath = "Ollama.Api/bin/Release/Ollama.Api.$version.nupkg"
if (!(Test-Path $packagePath)) {
    Write-Error "NuGet package not found at $packagePath."
    exit 1
}
Write-Host "Publishing NuGet package..."
dotnet nuget push $packagePath --api-key $nugetToken --source https://api.nuget.org/v3/index.json

Write-Host "Done."
