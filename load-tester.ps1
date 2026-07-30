# =========================================================
# spawn_infcube_instances.ps1
# Spawns N instances of the InfCube build, tiled in a grid.
# =========================================================

# ---- CONFIG ----
$exePath = "E:\Unity\InfCube\Builds\Mutliplayer testing\InfCube.exe"
$count     = 12                         # how many instances to spawn
$logDir    = ".\logs"
$staggerMs = 300                        # delay between launches

# Window size (per instance) - shrink so more fit on screen
$winWidth  = 480
$winHeight = 270

# Grid layout - how many columns before wrapping to next row
$cols = 4

# ---- SETUP ----
if (-not (Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir | Out-Null
}

if (-not (Test-Path $exePath)) {
    Write-Error "Game executable not found at: $exePath`nEdit the `$exePath variable at the top of this script."
    exit 1
}

# ---- Win32 helper: move a window by handle ----
Add-Type @"
using System;
using System.Runtime.InteropServices;
public class Win32 {
    [DllImport("user32.dll")]
    public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
}
"@

function Move-GameWindow($proc, $x, $y) {
    $proc.Refresh()
    if ($proc.MainWindowHandle -ne [IntPtr]::Zero) {
        [Win32]::MoveWindow($proc.MainWindowHandle, $x, $y, $winWidth, $winHeight, $true) | Out-Null
        return $true
    }
    return $false
}

# ---- LAUNCH + PLACE ----
$row = 0
$col = 0

for ($i = 1; $i -le $count; $i++) {
    $logFile = Join-Path $logDir "client_$i.log"
    $args = "-logFile `"$logFile`" -screen-width $winWidth -screen-height $winHeight"

    Write-Host "Launching instance $i..."
    $proc = Start-Process -FilePath $exePath -ArgumentList $args -PassThru

    # Wait until the window actually exists, then move it immediately
    $x = $col * $winWidth
    $y = $row * $winHeight
    for ($tries = 0; $tries -lt 20; $tries++) {
        Start-Sleep -Milliseconds 200
        if (Move-GameWindow $proc $x $y) { break }
    }

    $col++
    if ($col -ge $cols) {
        $col = 0
        $row++
    }

    Start-Sleep -Milliseconds $staggerMs
}

Write-Host "`nDone. $count instances launched and tiled in a grid ($cols columns)."
Write-Host "Logs are in: $logDir"
Write-Host "To close all instances, run:"
Write-Host "  Get-Process | Where-Object { `$_.Path -eq '$exePath' } | Stop-Process"