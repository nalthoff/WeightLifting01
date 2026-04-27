#!/usr/bin/env pwsh

# MainBranch: target integration branch (default main)
# PushMain: optionally push main after merge
# DryRun: print git commands without executing them
[CmdletBinding()]
param(
    [string]$MainBranch = "main",
    [switch]$PushMain,
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Wrapper for git commands so dry-run and error handling are consistent.
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
    # Used to identify the source branch to merge into main.
    $branch = (& git branch --show-current).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($branch)) {
        throw "Unable to determine current branch."
    }

    return $branch
}

Write-Host "Preparing branch push + merge workflow..." -ForegroundColor Cyan

# Safety check: ensure we are inside a git repository.
& git rev-parse --is-inside-work-tree | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "Current directory is not a git repository."
}

# Safety check: ensure there is an origin remote to push to.
& git remote get-url origin | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "Remote 'origin' is not configured."
}

$startingBranch = Get-CurrentBranch
Write-Host "Starting branch: $startingBranch" -ForegroundColor Green

# Enumerate all local branches under refs/heads.
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
    # Push each branch and set upstream tracking (origin/<branch>) when missing.
    # This is why we push branches individually instead of `git push --all`.
    Invoke-Git "push -u origin $branch" | Out-Null
}

if ($startingBranch -eq $MainBranch) {
    # If already on main, there is nothing to merge.
    Write-Host "Current branch is already '$MainBranch'. Skipping merge step." -ForegroundColor Yellow
}
else {
    # Merge the original starting branch into main using a merge commit (--no-ff).
    Write-Host "Merging '$startingBranch' into '$MainBranch'..." -ForegroundColor Cyan
    Invoke-Git "checkout $MainBranch" | Out-Null
    Invoke-Git "merge --no-ff $startingBranch"
}

if ($PushMain) {
    # Optional final step: publish updated main to origin.
    Write-Host "Pushing '$MainBranch' to origin..." -ForegroundColor Cyan
    Invoke-Git "push origin $MainBranch" | Out-Null
}
else {
    # Keep main local unless explicitly requested with -PushMain.
    Write-Host "Skipped pushing '$MainBranch' (use -PushMain to enable)." -ForegroundColor Yellow
}

Write-Host "Done." -ForegroundColor Green
