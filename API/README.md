# Stayly Backend RESTful API

> **Nền tảng:** ASP.NET Core 8.0 Web API, C# 12  
> **ORM & Cơ sở dữ liệu:** Entity Framework Core 8.0, Microsoft SQL Server (`HOMESTAY_DB`)  
> **Xác thực & Bảo mật:** JWT Bearer Tokens, Role-based Authorization (`ADMIN`, `OWNER`, `GUEST`), ASP.NET Identity PasswordHasher (PBKDF2 HMAC-SHA512)  
> **Tích hợp bên thứ 3:** VNPay Sandbox Payment Gateway, Cloudinary Image CDN

---

## 📌 Giới Thiệu Tổng Quan

**Stayly Web API** là trung tâm xử lý nghiệp vụ của toàn bộ hệ sinh thái Stayly. API phục vụ đồng thời cho 2 lớp ứng dụng khách:
1. **Web Client MVC** (Du khách đặt phòng & Chủ nhà quản lý cơ sở).
2. **Desktop Admin WPF** (Quản trị viên sàn kiểm duyệt pháp lý, đối soát doanh thu, sao lưu CSDL).

Hệ thống cung cấp kiến trúc RESTful chuẩn mực, phân tách rõ ràng giữa Controller, DTO, Model và Service, đảm bảo tính mở rộng và an toàn dữ liệu cao.

---

## 🏛️ Các Dịch Vụ Nền Tự Động (Background & Hosted Services)

1. **`ExpiredBookingCleanupService`:**
   - Tiến trình chạy nền định kỳ mỗi 30 giây.
   - Quét và tự động chuyển các đơn đặt phòng ở trạng thái `ChoThanhToan` đã quá hạn 30 phút sang trạng thái `DaHuy`.
   - Tự động hoàn trả lịch phòng (`LichLuuTru`) về trạng thái `Trống` để khách khác có thể đặt.

2. **`AutomatedBackupHostedService`:**
   - Tự động sao lưu cơ sở dữ liệu định kỳ hàng ngày vào lúc **02:00 AM**.
   - Áp dụng thuật toán nén `COMPRESSION` giúp tối ưu dung lượng tệp `.bak`.
   - Tự động dọn dẹp các tệp sao lưu cũ hơn 14 ngày để giải phóng bộ nhớ.

---

## 🌟 Danh Sách Các Nhóm API Chính (Endpoints)

### 1. Xác thực & Tài khoản (`/api/auth`)
- `POST /api/auth/register`: Đăng ký tài khoản mới (Guest hoặc Owner).
- `POST /api/auth/login`: Đăng nhập hệ thống, cấp phát JWT Bearer Token.
- `GET /api/auth/profile`: Lấy thông tin tài khoản hiện tại.
- `PUT /api/auth/profile`: Cập nhật thông tin cá nhân, CCCD, thông tin ngân hàng.
- `POST /api/auth/change-password`: Đổi mật khẩu.

### 2. Khám phá Cơ sở lưu trú (`/api/properties`)
- `GET /api/properties`: Tìm kiếm và lọc cơ sở lưu trú (theo địa điểm, tiện ích, khoảng giá, loại hình). Chỉ trả về các cơ sở đã duyệt (`DaDuyet`) và đang bật hoạt động (`TrangThaiHoatDong = true`).
- `GET /api/properties/{id}`: Xem chi tiết cơ sở lưu trú, danh sách phòng, tiện nghi, dịch vụ và đánh giá.
- `GET /api/properties/locations`: Lấy danh sách các tỉnh/thành phố có cơ sở lưu trú.
- `GET /api/properties/amenities`: Lấy danh mục tiện ích toàn hệ thống.

### 3. Đặt phòng & Chuyến đi (`/api/bookings`)
- `POST /api/bookings`: Tạo đơn đặt phòng mới (kiểm tra trùng lịch, tính tiền phòng theo số đêm, dịch vụ bổ sung, áp dụng mã giảm giá).
- `GET /api/bookings`: Lấy danh sách lịch sử đặt phòng của khách hàng hiện tại.
- `GET /api/bookings/{id}`: Xem chi tiết đơn đặt phòng, bóc tách tiền phòng và phụ thu dịch vụ.
- `PUT /api/bookings/{id}/cancel`: Khách hủy đơn đặt phòng.
- `POST /api/bookings/{id}/refund`: Gửi yêu cầu hoàn tiền kèm thông tin ngân hàng thụ hưởng.

### 4. Thanh toán & Cổng VNPay (`/api/payments`)
- `POST /api/payments/create-url`: Khởi tạo URL thanh toán và mã QR qua cổng VNPay Sandbox.
- `GET /api/payments/vnpay-return`: Xử lý phản hồi thanh toán từ VNPay (kiểm tra chữ ký số SHA512).
- `GET /api/payments/status/{bookingId}`: Kiểm tra trạng thái thanh toán thời gian thực.

### 5. Kênh Chủ Cơ Sở Lưu Trú (`/api/owner`)
- `GET /api/owner/dashboard`: Thống kê tổng quan cơ sở, đơn đặt, doanh thu, đánh giá của chủ nhà.
- `POST /api/owner/properties`: Đăng ký thêm cơ sở lưu trú mới (kèm upload giấy tờ pháp lý).
- `PUT /api/owner/properties/{id}`: Cập nhật thông tin cơ sở lưu trú.
- `PUT /api/properties/{id}/toggle-active`: **Bật / Tắt trạng thái hoạt động** của cơ sở lưu trú (khi tắt sẽ ẩn khỏi trang tìm kiếm).
- `POST /api/owner/properties/{id}/rooms`: Thêm phòng mới vào cơ sở.
- `PUT /api/owner/rooms/{id}`: Sửa thông tin và giá phòng.
- `PUT /api/owner/rooms/{id}/toggle-active`: Bật / Tắt hoạt động của phòng.
- `POST /api/owner/services`: Thêm dịch vụ bổ sung cho cơ sở.
- `PUT /api/owner/bookings/{id}/approve`: Duyệt đơn đặt phòng của khách.
- `PUT /api/owner/bookings/{id}/reject`: Từ chối đơn đặt phòng.

### 6. Quản Trị Viên Sàn (`/api/admin`)
- `GET /api/admin/properties`: Quản lý danh sách toàn bộ cơ sở trên sàn (lọc theo `Pending`, `Approved`, `Rejected`).
- `POST /api/admin/properties/{id}/approve`: Phê duyệt cơ sở lưu trú.
- `POST /api/admin/properties/{id}/reject`: Từ chối cơ sở kèm lý do thẩm định.
- `GET /api/admin/users`: Quản lý người dùng, xem CCCD, STK ngân hàng.
- `PUT /api/admin/users/{id}/status`: Khóa hoặc mở khóa tài khoản người dùng.
- `GET /api/admin/revenue`: Báo cáo doanh thu toàn sàn (tách hoa hồng sàn 15% và tiền trả chủ nhà 85%).
- `POST /api/admin/payout`: Ghi nhận quyết toán chuyển khoản cho chủ nhà.
- `GET /api/admin/promotions` & `POST /api/admin/promotions`: Quản lý mã ưu đãi voucher.
- `GET /api/admin/audit-logs`: Xem nhật ký hoạt động hệ thống.

### 7. Bảo Trì & Sao Lưu CSDL (`/api/database`)
- `GET /api/database/status`: Kiểm tra trạng thái kỹ thuật SQL Server (kích thước Data/Log, số bảng).
- `GET /api/database/backups`: Xem danh sách các bản sao lưu hiện có.
- `POST /api/database/backup`: Tạo bản sao lưu ngay lập tức có nén dữ liệu.
- `GET /api/database/download-backup/{fileName}`: Tải file `.bak` về máy cá nhân từ xa.
- `POST /api/database/restore`: Phục hồi CSDL từ bản sao lưu.

---

## 🚀 Hướng Dẫn Cài Đặt & Chạy API

### 1. Yêu cầu môi trường
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server 2019/2022 hoặc Azure SQL Database.

### 2. Cấu hình chuỗi kết nối (`appsettings.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=HOMESTAY_DB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Issuer": "StaylyApi",
    "Audience": "StaylyClients",
    "Key": "StaylySecretKeyMustBeLongEnoughForHMACSHA256Security123456"
  }
}
```

### 3. Khởi tạo Cơ sở dữ liệu
Thực thi tệp [`HOMESTAY_DB.sql`](file:///d:/Homestay/HOMESTAY_DB.sql) trong SQL Server Management Studio (SSMS). Tệp đã được tích hợp sẵn 55+ khách sạn & homestay mẫu.

### 4. Khởi chạy API
```powershell
cd d:\Homestay\API
dotnet restore
dotnet run --launch-profile http
```
- Swagger UI tài liệu API: `http://localhost:5176/swagger`

---

## 👥 Tài Khoản Thử Nghiệm Mẫu

| Vai trò | Email | Mật khẩu | Quyền hạn |
| :--- | :--- | :--- | :--- |
| **Admin Quản trị viên** | `admin@gmail.com` | `123123123` | Toàn quyền quản trị, kiểm duyệt, quyết toán, sao lưu CSDL |
| **Admin Stayly** | `admin@stayly.com` | `Admin@123456` | Tài khoản quản trị dự phòng hệ thống |
| **Chủ nhà (Owner)** | `owner@gmail.com` | `123123123` | Quản lý cơ sở lưu trú, bật/tắt hoạt động, duyệt đơn |
| **Khách hàng (Guest)** | `guest@gmail.com` | `123123123` | Tìm kiếm, đặt phòng, thanh toán, đánh giá |

---
*© 2026 Stayly API Service. Đồ án môn học / Khóa luận cử nhân.*
