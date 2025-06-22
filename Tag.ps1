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

# Build
Write-Host "Building solution..."
dotnet build $solution --configuration Release

# Test
Write-Host "Running unit tests..."
dotnet test $solution --configuration Release --no-build

# Get version from Nerdbank.GitVersioning
$version = dotnet nbgv get-version --variable NuGetPackageVersion
if (-not $version) {
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
