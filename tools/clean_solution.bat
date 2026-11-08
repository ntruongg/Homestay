@echo off
chcp 65001 >nul
echo =========================================================================
echo STAYLY - DỌN DẸP CÁC THƯ MỤC TẠM (BIN, OBJ, .VS) ĐỂ NÉN NỘP ĐỒ ÁN
echo =========================================================================
echo.
echo Đang tiến hành dọn dẹp các thư mục bin, obj, .vs trong toàn bộ solution...

cd /d "%~dp0\.."

for /d /r . %%d in (bin,obj,.vs) do (
    if exist "%%d" (
        echo - Đang xóa: %%d
        rd /s /q "%%d" 2>nul
    )
)

echo.
echo =========================================================================
echo ĐÃ DỌN DẸP HOÀN TẤT!
echo Thư mục dự án hiện đã siêu nhẹ và sạch sẽ, sẵn sàng nén thành file .ZIP để nộp.
echo =========================================================================
echo.
pause
