$ErrorActionPreference = "Stop"

# 1. Create new directories
Write-Host "Creating new directories..."
$dirs = @("Controllers", "Middleware", "Services", "DTOs", "Models", "Repositories", "Security")
foreach ($dir in $dirs) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir | Out-Null
    }
}

# 2. Move files
Write-Host "Moving files..."
Move-Item -Path "API\Controllers\*" -Destination "Controllers" -Force -ErrorAction SilentlyContinue
Move-Item -Path "API\Middleware\*" -Destination "Middleware" -Force -ErrorAction SilentlyContinue

Move-Item -Path "Application\DTOs\*" -Destination "DTOs" -Force -ErrorAction SilentlyContinue
Move-Item -Path "Application\Interfaces\*" -Destination "Services" -Force -ErrorAction SilentlyContinue
Move-Item -Path "Application\Services\*" -Destination "Services" -Force -ErrorAction SilentlyContinue

Move-Item -Path "Core\Entities\*" -Destination "Models" -Force -ErrorAction SilentlyContinue
Move-Item -Path "Core\Interfaces\*" -Destination "Repositories" -Force -ErrorAction SilentlyContinue

Move-Item -Path "Infrastructure\Data\*" -Destination "Repositories" -Force -ErrorAction SilentlyContinue
Move-Item -Path "Infrastructure\Repositories\*" -Destination "Repositories" -Force -ErrorAction SilentlyContinue
Move-Item -Path "Infrastructure\Security\*" -Destination "Security" -Force -ErrorAction SilentlyContinue

# 3. Delete old directories
Write-Host "Deleting old directories..."
Remove-Item -Path "API" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Application" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Core" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Infrastructure" -Recurse -Force -ErrorAction SilentlyContinue

# 4. Find and Replace strings in all .cs files
Write-Host "Refactoring namespaces..."
$files = Get-ChildItem -Path . -Filter *.cs -Recurse

foreach ($f in $files) {
    $content = Get-Content $f.FullName -Raw

    $content = $content -replace "EmployeeManagementSystem\.API\.Controllers", "EmployeeManagementSystem.Controllers"
    $content = $content -replace "EmployeeManagementSystem\.API\.Middleware", "EmployeeManagementSystem.Middleware"

    $content = $content -replace "EmployeeManagementSystem\.Application\.DTOs", "EmployeeManagementSystem.DTOs"
    $content = $content -replace "EmployeeManagementSystem\.Application\.Interfaces", "EmployeeManagementSystem.Services"
    $content = $content -replace "EmployeeManagementSystem\.Application\.Services", "EmployeeManagementSystem.Services"

    $content = $content -replace "EmployeeManagementSystem\.Core\.Entities", "EmployeeManagementSystem.Models"
    $content = $content -replace "EmployeeManagementSystem\.Core\.Interfaces", "EmployeeManagementSystem.Repositories"

    $content = $content -replace "EmployeeManagementSystem\.Infrastructure\.Data", "EmployeeManagementSystem.Repositories"
    $content = $content -replace "EmployeeManagementSystem\.Infrastructure\.Repositories", "EmployeeManagementSystem.Repositories"
    $content = $content -replace "EmployeeManagementSystem\.Infrastructure\.Security", "EmployeeManagementSystem.Security"

    Set-Content -Path $f.FullName -Value $content
}

Write-Host "Refactoring complete."
