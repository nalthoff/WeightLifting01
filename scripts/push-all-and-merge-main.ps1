#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [string]$MainBranch = "main",
    [switch]$PushMain,
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Invoke-Git {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Args
    )

    if ($DryRun) {
        Write-Host "[dry-run] git $Args" -ForegroundColor Yellow
        return ""
    }

    Write-Host "git $Args" -ForegroundColor DarkGray
    $output = & git $Args.Split(" ")
    if ($LASTEXITCODE -ne 0) {
        throw "Git command failed: git $Args"
    }

    return ($output -join "`n")
}

function Get-CurrentBranch {
    $branch = (& git branch --show-current).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($branch)) {
        throw "Unable to determine current branch."
    }

    return $branch
}

Write-Host "Preparing branch push + merge workflow..." -ForegroundColor Cyan

& git rev-parse --is-inside-work-tree | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "Current directory is not a git repository."
}

& git remote get-url origin | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "Remote 'origin' is not configured."
}

$startingBranch = Get-CurrentBranch
Write-Host "Starting branch: $startingBranch" -ForegroundColor Green

$localBranchesRaw = & git for-each-ref --format='%(refname:short)' refs/heads
if ($LASTEXITCODE -ne 0) {
    throw "Failed to list local branches."
}

$localBranches = @($localBranchesRaw | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
if ($localBranches.Count -eq 0) {
    throw "No local branches found."
}

Write-Host "Pushing all local branches to origin..." -ForegroundColor Cyan
foreach ($branch in $localBranches) {
    # Use -u so branches without upstream are linked to origin/<branch>.
    Invoke-Git "push -u origin $branch" | Out-Null
}

if ($startingBranch -eq $MainBranch) {
    Write-Host "Current branch is already '$MainBranch'. Skipping merge step." -ForegroundColor Yellow
}
else {
    Write-Host "Merging '$startingBranch' into '$MainBranch'..." -ForegroundColor Cyan
    Invoke-Git "checkout $MainBranch" | Out-Null
    Invoke-Git "merge --no-ff $startingBranch"
}

if ($PushMain) {
    Write-Host "Pushing '$MainBranch' to origin..." -ForegroundColor Cyan
    Invoke-Git "push origin $MainBranch" | Out-Null
}
else {
    Write-Host "Skipped pushing '$MainBranch' (use -PushMain to enable)." -ForegroundColor Yellow
}

Write-Host "Done." -ForegroundColor Green
