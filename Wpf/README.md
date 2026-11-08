# Stayly Admin Desktop — Phân Hệ Quản Trị Viên Sàn

> **Nền tảng:** Windows Presentation Foundation (WPF) trên nền **.NET 8.0** (`net8.0-windows`)  
> **Kiến trúc:** MVVM (Model - View - ViewModel), XAML Styling, MaterialDesignThemes  
> **Kết nối:** `HttpAdminService` gọi trực tiếp RESTful API Backend qua JWT Bearer Authentication

---

## 📌 Giới Thiệu Tổng Quan

**Stayly Admin Desktop** là ứng dụng chuyên biệt dành riêng cho đội ngũ **Quản trị viên sàn (Platform Admin)**, cung cấp bộ công cụ mạnh mẽ, trực quan và bảo mật cao để vận hành, kiểm duyệt, đối soát tài chính và bảo trì toàn bộ hệ sinh thái Stayly.

---

## 🌟 Các Phân Hệ & Chức Năng Chính

### 1. Thẩm Định & Kiểm Duyệt Cơ Sở Lưu Trú (KiemDuyetHomestay)
- **Danh sách hồ sơ chờ duyệt:** Tải danh sách các cơ sở Homestay / Khách sạn mới đăng ký hoặc vừa cập nhật gửi lại.
- **Thẩm định hồ sơ pháp lý:** Xem trực tiếp các giấy tờ chứng thực pháp lý đã nộp:
  - Giấy phép đăng ký kinh doanh (`GiayPhepKD_URL`)
  - Giấy chứng nhận đủ điều kiện Phòng cháy chữa cháy (`GiayToPCCC_URL`)
  - Giấy chứng nhận an ninh trật tự (`GiayToANTT_URL`)
- **Ra quyết định kiểm duyệt:**
  - **Phê duyệt (Approve):** Kích hoạt cơ sở lên trạng thái `DaDuyet`, cho phép chủ nhà mở bán phòng trên sàn.
  - **Từ chối (Reject):** Yêu cầu nhập lý do từ chối cụ thể để gửi phản hồi cho chủ nhà sửa đổi và nộp lại hồ sơ.
- **Lịch sử thẩm định:** Ghi nhận toàn bộ vết lịch sử xét duyệt (`LichSuDuyet`), người duyệt và thời gian thẩm định.

### 2. Quản Lý Tài Khoản Người Dùng (QuanLyTaiKhoan)
- Phân loại danh sách người dùng theo vai trò: **Guest (Khách thuê)**, **Owner (Chủ cơ sở)**, **Admin (Quản trị viên)**.
- Tra cứu chi tiết: Họ tên, Email, Số điện thoại, CCCD/CMND, Thông tin tài khoản ngân hàng thụ hưởng.
- **Khóa / Mở khóa tài khoản:** Tạm ngừng quyền truy cập đối với các tài khoản vi phạm chính sách sàn.
- Thống kê số lượng cơ sở lưu trú và số lượt đặt phòng gắn liền với từng tài khoản.

### 3. Đối Soát & Quyết Toán Doanh Thu (QuyetToanDoanhThu)
- **Mô hình tài chính 85 / 15 minh bạch:**
  - 15% Doanh thu hoa hồng dịch vụ sàn Stayly.
  - 85% Tiền thực nhận chi trả cho Chủ cơ sở lưu trú (`TienThucNhanChu`).
- Tra cứu danh sách đơn đã hoàn tất lưu trú cần thanh toán tiền về cho chủ nhà.
- Hiển thị trực tiếp thông tin ngân hàng thụ hưởng của chủ nhà (`BankName`, `AccountNumber`, `AccountHolder`).
- **Thao tác quyết toán:** Nhập mã giao dịch chuyển khoản ngân hàng đối soát (`MaGiaoDichQuyetToan`), ngày chuyển và ghi chú, đánh dấu hoàn thành quyết toán.

### 4. Quản Lý Mã Khuyến Mãi / Voucher (QuanLyGiamGia)
- Tạo và phát hành mã ưu đãi (Promo Code): % giảm giá, trần giảm tối đa (`ToiDa`), số lượt sử dụng tối đa, ngày bắt đầu và ngày hết hạn.
- Theo dõi số lượt đã sử dụng thực tế của từng mã ưu đãi trên toàn hệ thống.

### 5. Giám Sát Nhật Ký Hệ Thống (NhatKyHoatDong)
- Theo dõi toàn bộ luồng hoạt động trọng yếu: Đăng ký, đăng nhập, duyệt cơ sở, tạo đơn, thanh toán, thay đổi trạng thái, sao lưu CSDL.
- Lưu trữ địa chỉ IP, thời gian (`DateTime`), đối tượng thao tác và mô tả chi tiết.

### 6. Bảo Trì & Sao Lưu Phục Hồi CSDL (BaoTriHeThong)
- Xem thông số kỹ thuật máy chủ CSDL SQL Server: Tên database, phiên bản máy chủ, kích thước tệp Data/Log, số lượng bảng.
- **Sao lưu tức thì (Instant Backup):** Kích hoạt lệnh BACKUP DATABASE có nén (`COMPRESSION`).
- **Tải tệp sao lưu (.bak) từ xa:** Download tệp sao lưu từ server về máy tính cục bộ qua HTTP Stream an toàn.
- **Phục hồi CSDL (Database Restore):** Khôi phục dữ liệu từ tệp sao lưu trên server hoặc tải tệp `.bak` từ máy cá nhân lên để phục hồi, tự động ngắt các kết nối đang mở (`SINGLE_USER WITH ROLLBACK IMMEDIATE`).

---

## 🚀 Hướng Dẫn Cài Đặt & Khởi Chạy

### 1. Yêu Cầu Môi Trường
- Windows 10 / Windows 11
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (hỗ trợ `net8.0-windows`)
- Visual Studio 2022 (khuyến nghị chọn Workload *.NET Desktop Development*) hoặc Visual Studio Code.
- Backend API Stayly đang chạy (Local `http://localhost:5176` hoặc Azure Cloud).

### 2. Cấu Hình Địa Chỉ API
Trong tệp [`Services/HttpAdminService.cs`](file:///d:/Homestay/Wpf/HomestaySystem/HomestaySystem/Services/HttpAdminService.cs), cấu hình địa chỉ máy chủ API:
```csharp
// Mặc định kết nối tới API Cloud hoặc Localhost:
public HttpAdminService(
    string baseUrl = "http://localhost:5176/", // hoặc URL Azure API
    string adminEmail = "admin@gmail.com",
    string adminPassword = "123123123"
)
```

### 3. Biên Dịch & Chạy Ứng Dụng
```powershell
# Chuyển vào thư mục dự án WPF
cd d:\Homestay\Wpf\HomestaySystem

# Build solution
dotnet build HomestaySystem.sln

# Chạy ứng dụng WPF
dotnet run --project HomestaySystem\HomestaySystem.csproj
```
Hoặc mở tệp `HomestaySystem.sln` bằng Visual Studio 2022 và nhấn **F5 (Start Debugging)**.

---

## 👥 Tài Khoản Quản Trị Viên Mặc Định

| Email | Mật khẩu | Họ tên | Vai trò |
| :--- | :--- | :--- | :--- |
| `admin@gmail.com` | `123123123` | Trần Nhật Trường | Quản trị viên hệ thống (Admin) |
| `admin@stayly.com` | `Admin@123456` | Admin Stayly | Quản trị viên hệ thống (Admin) |

---
*© 2026 Stayly Management System. Đồ án môn học / Khóa luận cử nhân.*

