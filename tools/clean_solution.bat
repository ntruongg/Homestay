@echo off
echo =========================================================================
echo STAYLY - DANG DON DEP BIN, OBJ, .VS, PUBLISH DE NEN NOP PROJECT
echo =========================================================================
echo.
echo Vui long dam bao ban da DONG Visual Studio, VS Code hoac tien trinh .NET dang chay...
echo.
powershell -NoProfile -ExecutionPolicy Bypass -Command "Get-ChildItem -Path '%~dp0..' -Include bin, obj, .vs, publish -Recurse -Force -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue; Write-Host '-> Da xoa sach tat ca thu muc bin, obj, .vs, publish thanh cong!' -ForegroundColor Green"
echo.
echo =========================================================================
echo HOAN TAT! Thu muc du an da cuc ky nhe va sach se.
echo Ban co the chuot phai vao thu muc Homestay de nen thanh file .zip!
echo =========================================================================
echo.
pause
