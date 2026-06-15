@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Export-ProjectFiles.ps1" ^
  -RootPath "%~dp0..\.." ^
  -PathsFile "%~dp0paths.txt" ^
  -OutputFile "%~dp0kosmozeki-export.md"
pause