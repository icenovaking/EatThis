[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$frontendSource = Join-Path $repositoryRoot 'src/EatThis.Web/src'
$publicConfig = Join-Path $repositoryRoot 'src/EatThis.Api/appsettings.example.json'
$apiProgram = Join-Path $repositoryRoot 'src/EatThis.Api/Program.cs'
$providerSource = Join-Path $repositoryRoot 'src/EatThis.Api/Infrastructure/GooglePlacesProvider.cs'
$secretGuide = Join-Path $repositoryRoot 'docs/development-secrets.md'
$gitignore = Join-Path $repositoryRoot '.gitignore'

function Assert-Contains([string] $content, [string] $needle, [string] $message) {
    if ($content.IndexOf($needle, [StringComparison]::Ordinal) -lt 0) {
        throw $message
    }
}

function Assert-NotMatch([string] $content, [string] $pattern, [string] $message) {
    if ($content -match $pattern) {
        throw $message
    }
}

if (-not (Test-Path -LiteralPath $frontendSource)) {
    throw "Frontend source directory not found: $frontendSource"
}

$frontendFiles = Get-ChildItem -LiteralPath $frontendSource -Recurse -File |
    Where-Object { $_.Name -notlike '*.spec.ts' -and $_.Name -notlike '*.test.ts' }
$frontendContent = ($frontendFiles | ForEach-Object { Get-Content -Raw -LiteralPath $_.FullName }) -join "`n"

Assert-NotMatch $frontendContent 'AIza[0-9A-Za-z_-]{20,}' 'A Google API key-shaped value is present in frontend source.'
Assert-NotMatch $frontendContent 'X-Goog-Api-Key' 'The Google provider credential header is present in frontend source.'
Assert-NotMatch $frontendContent 'places\.googleapis\.com' 'The Google Places upstream endpoint is present in frontend source.'
Assert-NotMatch $frontendContent 'GooglePlaces(__|:)ApiKey' 'The server secret configuration name is present in frontend source.'

$exampleContent = Get-Content -Raw -LiteralPath $publicConfig
Assert-Contains $exampleContent '"ApiKey": ""' 'The public configuration example must contain an empty API key.'
Assert-NotMatch $exampleContent 'AIza[0-9A-Za-z_-]{20,}' 'A Google API key-shaped value is present in the public configuration example.'

$programContent = Get-Content -Raw -LiteralPath $apiProgram
Assert-Contains $programContent 'AddRateLimiter' 'API rate limiting registration is missing.'
Assert-Contains $programContent 'UseRateLimiter' 'API rate limiting middleware is missing.'
Assert-Contains $programContent 'PermitLimit' 'API rate limit permit bound is missing.'

$providerContent = Get-Content -Raw -LiteralPath $providerSource
Assert-NotMatch $providerContent 'Log\w*\([^\r\n]*(apiKey|response\.Content|raw|body)' 'Provider logging appears to include a credential or raw response.'

$guideContent = Get-Content -Raw -LiteralPath $secretGuide
Assert-Contains $guideContent 'GooglePlaces__ApiKey' 'Production secret wiring documentation is missing.'
Assert-Contains (Get-Content -Raw -LiteralPath $gitignore) 'appsettings.Development.json' 'Development secret file is not ignored.'

Write-Output 'EatThis security checks passed.'
