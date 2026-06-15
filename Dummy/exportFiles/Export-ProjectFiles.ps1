param(
    [Parameter(Mandatory = $true)]
    [string]$RootPath,

    [Parameter(Mandatory = $true)]
    [string]$PathsFile,

    [string]$OutputFile = "exported-files.md"
)

$ErrorActionPreference = "Continue"

function Resolve-ProjectPath {
    param(
        [string]$Root,
        [string]$RelativeOrAbsolutePath
    )

    if ([System.IO.Path]::IsPathRooted($RelativeOrAbsolutePath)) {
        return $RelativeOrAbsolutePath
    }

    return (Join-Path $Root $RelativeOrAbsolutePath)
}

function Is-BinaryFile {
    param([string]$FilePath)

    try {
        $bytes = [System.IO.File]::ReadAllBytes($FilePath)
        $sampleSize = [Math]::Min($bytes.Length, 8000)

        for ($i = 0; $i -lt $sampleSize; $i++) {
            if ($bytes[$i] -eq 0) {
                return $true
            }
        }

        return $false
    }
    catch {
        return $true
    }
}

$root = (Resolve-Path $RootPath).Path

$list = Get-Content $PathsFile |
    ForEach-Object { $_.Trim() } |
    Where-Object { $_ -and -not $_.StartsWith("#") }

Set-Content -Path $OutputFile -Value "# Exported project files" -Encoding UTF8
Add-Content -Path $OutputFile -Value ""
Add-Content -Path $OutputFile -Value "Root: $root"
Add-Content -Path $OutputFile -Value "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Add-Content -Path $OutputFile -Value ""

foreach ($item in $list) {
    $fullPath = Resolve-ProjectPath -Root $root -RelativeOrAbsolutePath $item

    Add-Content -Path $OutputFile -Value "---"
    Add-Content -Path $OutputFile -Value ""
    Add-Content -Path $OutputFile -Value "## FILE: $item"
    Add-Content -Path $OutputFile -Value ""

    if (-not (Test-Path $fullPath -PathType Leaf)) {
        Add-Content -Path $OutputFile -Value "_ERROR: file not found_"
        Add-Content -Path $OutputFile -Value ""
        continue
    }

    if (Is-BinaryFile -FilePath $fullPath) {
        Add-Content -Path $OutputFile -Value "_SKIPPED: binary file_"
        Add-Content -Path $OutputFile -Value ""
        continue
    }

    $ext = [System.IO.Path]::GetExtension($fullPath).TrimStart(".")
    if ([string]::IsNullOrWhiteSpace($ext)) {
        $ext = "txt"
    }

    $content = Get-Content -Path $fullPath -Raw

    Add-Content -Path $OutputFile -Value ('```' + $ext)
    Add-Content -Path $OutputFile -Value $content
    Add-Content -Path $OutputFile -Value '```'
    Add-Content -Path $OutputFile -Value ""
}

Write-Host ("Done. Output file: {0}" -f $OutputFile)