# Builds the release zip and copies it to nexus/build/<Name><Version>.zip (ready to upload to Nexus).
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$manifest = Get-Content (Join-Path $repo 'manifest.json') -Raw | ConvertFrom-Json
$name = $manifest.Name -replace '\s', ''
$out = Join-Path $PSScriptRoot "build\$name$($manifest.Version).zip"
New-Item -ItemType Directory -Force -Path (Split-Path $out) | Out-Null
Remove-Item $out -ErrorAction SilentlyContinue

$csproj = Get-ChildItem $repo -Filter *.csproj | Select-Object -First 1
if ($csproj) {
    # ModBuildConfig creates the zip (<Mod> <version>.zip) for Release builds
    Remove-Item (Join-Path $repo 'bin\Release') -Recurse -Force -ErrorAction SilentlyContinue
    dotnet build $csproj.FullName -c Release -nologo
    if ($LASTEXITCODE -ne 0) { throw 'dotnet build failed' }
    $zip = Get-ChildItem (Join-Path $repo 'bin\Release') -Recurse -Filter *.zip | Sort-Object LastWriteTime | Select-Object -Last 1
    if (-not $zip) { throw 'ModBuildConfig did not produce a zip' }
    Copy-Item $zip.FullName $out
}
else {
    # Content pack: zip manifest + content + i18n + assets inside a <Name>/ folder
    $stage = Join-Path ([IO.Path]::GetTempPath()) "svpack_$name"
    Remove-Item $stage -Recurse -Force -ErrorAction SilentlyContinue
    $dest = Join-Path $stage $name
    New-Item -ItemType Directory -Force -Path $dest | Out-Null
    foreach ($item in 'manifest.json', 'content.json', 'i18n', 'assets', 'patches') {
        $p = Join-Path $repo $item
        if (Test-Path $p) { Copy-Item $p $dest -Recurse }
    }
    Get-ChildItem $dest -Recurse -Force -Filter .keep | Remove-Item -Force
    # Build the zip by hand so entry names use forward slashes (Compress-Archive on Windows PowerShell 5 uses backslashes)
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zipFile = [IO.Compression.ZipFile]::Open($out, 'Create')
    foreach ($f in Get-ChildItem $dest -Recurse -File -Force) {
        $entry = $f.FullName.Substring($stage.Length + 1).Replace('\', '/')
        [void][IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zipFile, $f.FullName, $entry)
    }
    $zipFile.Dispose()
    Remove-Item $stage -Recurse -Force
}
Write-Host "Created $out"