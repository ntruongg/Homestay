# Stayly Web Client — Phân Hệ Web Khách Hàng & Chủ Nhà

> **Nền tảng:** ASP.NET Core 8.0 MVC, Razor Views, C# 12  
> **Giao diện:** Bootstrap 5.3, Bootstrap Icons, Custom Ocean Blue CSS (`site.css`), Vanilla JavaScript (Fetch API, Chart.js)  
> **Kết nối:** `HomestayApiClient` tương tác trực tiếp với Backend RESTful API

---

## 📌 Giới Thiệu Tổng Quan

**Stayly Web Client** là phân hệ giao diện trực tuyến phục vụ 2 nhóm người dùng chính trên cùng một nền tảng Web hợp nhất:
1. **Khách hàng (Du khách / Guest):** Tìm kiếm homestay/khách sạn theo vị trí, tiện ích, giá tiền, xem chi tiết phòng, đặt phòng & dịch vụ bổ sung, thanh toán qua cổng VNPay-QR thời gian thực, quản lý lịch sử chuyến đi, yêu cầu hoàn tiền và viết đánh giá.
2. **Chủ cơ sở lưu trú (Host / Owner):** Kênh quản lý tập trung (Host Dashboard) để theo dõi tổng quan doanh thu, đăng ký cơ sở mới kèm hồ sơ pháp lý, quản lý danh sách phòng và bảng giá, bật/tắt trạng thái hoạt động của cơ sở (có thẻ nhận diện `status-unavailable`), tiếp nhận và phê duyệt đơn đặt phòng.

---

## 🌟 Các Tính Năng Nổi Bật

### 1. Khám Phá & Bộ Lọc Nâng Cao Chuẩn Du Lịch
- **Thanh tìm kiếm thông minh:** Tìm kiếm nhanh theo địa điểm/thành phố, ngày nhận phòng (`CheckIn`), ngày trả phòng (`CheckOut`) và số lượng khách.
- **Bộ lọc tiện ích tinh gọn:** Lọc nhanh theo 5 tiện ích quan trọng hàng đầu (Wifi tốc độ cao, Điều hòa nhiệt độ, Bể bơi, Bếp nấu ăn, Cho phép mang thú cưng).
- **Thanh lọc giá dạng trượt (Slider):** Lọc theo khoảng giá từ tối thiểu đến tối đa một cách trực quan, cập nhật danh sách ngay lập tức.
- **Giao diện card cơ sở chuẩn hóa:** Hình ảnh được cố định tỷ lệ chuẩn (`object-fit: cover`) chống vỡ bố cục khi ảnh quá cao, hiển thị đầy đủ xếp hạng sao, địa chỉ và giá phòng khởi điểm.

### 2. Trang Chi Tiết Cơ Sở & Đặt Phòng
- **Bộ sưu tập ảnh đa góc cạnh:** Ảnh khuôn viên cơ sở và ảnh chi tiết từng phòng.
- **Danh sách phòng còn trống:** Kiểm tra lịch trống kép, thông tin diện tích, sức chứa người lớn / trẻ em, tiện nghi phòng.
- **Dịch vụ bổ sung:** Chọn trước dịch vụ (BBQ, tiệc trà, giặt ủi, thuê xe...) với số lượng linh hoạt, tự động tính tiền minh bạch.
- **Áp dụng Mã giảm giá (Promo Code):** Tự động tính toán số tiền được giảm theo % và trần giảm tối đa.

### 3. Thanh Toán VNPay-QR & Bộ Đếm Thời Gian Thực Tế 30 Phút
- Tạo mã QR thanh toán trực tuyến qua cổng VNPay Sandbox.
- **Bộ đếm thời gian thực 30 phút:** Tính đúng thời gian kể từ thời điểm tạo đơn (`NgayDat`), **không bao giờ bị reset về 30:00 khi F5/refresh trang** hay khi người dùng đóng trình duyệt vào lại.
- **Tự động hủy đơn khi hết hạn:** Nếu quá 30 phút chưa thanh toán, đơn tự động hủy (`DaHuy`) và giải phóng phòng về trạng thái trống.
- **Polling thanh toán thời gian thực:** Tự động chuyển hướng ngay sang trang Chuyến đi khi phát hiện giao dịch thành công.

### 4. Quản Lý Chuyến Đi (Guest Trips)
- Theo dõi đơn đặt phòng theo từng trạng thái: *Chờ thanh toán, Chờ chủ nhà duyệt, Đã duyệt, Đã hoàn tất, Đã hủy, Yêu cầu hoàn tiền*.
- Xem chi tiết đơn bóc tách đầy đủ: Tiền phòng, số đêm, dịch vụ kèm theo, ưu đãi voucher và tổng thanh toán.
- Gửi yêu cầu hủy đơn & hoàn tiền (nhập số tài khoản ngân hàng thụ hưởng để Admin xử lý).
- Đánh giá xếp hạng sao (1 - 5 sao) và nhận xét trải nghiệm sau chuyến đi.

### 5. Kênh Chủ Cơ Sở Lưu Trú (Host Dashboard)
- **Thẻ trạng thái cơ sở (`property-status-pill`):**
  - Hiển thị rõ ràng trạng thái: **Đang hoạt động** (`status-approved`), **Tạm dừng hoạt động** (`status-unavailable`), **Đang chờ duyệt** (`status-pending`), **Bị từ chối** (`status-rejected`).
  - Khi cơ sở bị từ chối, hiển thị lý do thẩm định từ Admin và nút *"Sửa & Gửi lại"* để nộp lại hồ sơ nhanh chóng.
- **Công tắc Bật/Tắt hoạt động (Active Toggle Switch):**
  - Cho phép chủ nhà chủ động tạm dừng đón khách bất cứ lúc nào.
  - Hiển thị nhãn trực quan: **"Đang đón khách"** hoặc **"Tạm dừng"**.
  - Tự động đổi màu thẻ trạng thái và nhãn ngay tức thì qua JavaScript mà không cần tải lại trang.
- **Quản lý phòng:** Xem danh sách phòng, thêm phòng mới, cập nhật giá và tiện ích phòng.
- **Quản lý đơn khách:** Tiếp nhận và duyệt hoặc từ chối các đơn đặt phòng mới.
- **Thống kê tài chính:** Báo cáo tổng doanh thu, số đơn đã hoàn thành, tiền thực nhận 85% từ sàn.

---

## 🚀 Hướng Dẫn Cài Đặt & Khởi Chạy

### 1. Yêu cầu môi trường
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Backend API Stayly đang chạy tại `http://localhost:5176` (hoặc cấu hình URL Cloud).

### 2. Cấu hình kết nối API (`appsettings.json`)
Trong tệp `Web/Web/appsettings.json`:
```json
{
  "HomestayApi": {
    "BaseUrl": "http://localhost:5176/api/"
  },
  "VnPay": {
    "TmnCode": "YOUR_VNP_TMNCODE",
    "HashSecret": "YOUR_VNP_HASHSECRET",
    "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "Command": "pay",
    "CurrCode": "VND",
    "Version": "2.1.0",
    "Locale": "vn"
  }
}
```

### 3. Khởi chạy ứng dụng Web
```powershell
cd d:\Homestay\Web\Web
dotnet restore
dotnet run --launch-profile http
```
- Mở trình duyệt và truy cập: `http://localhost:5280`

---

## 👥 Tài Khoản Đăng Nhập Mẫu

| Nhóm người dùng | Email đăng nhập | Mật khẩu | Quyền hạn & Chức năng |
| :--- | :--- | :--- | :--- |
| **Chủ nhà (Owner)** | `owner@gmail.com` | `123123123` | Vào Host Dashboard, quản lý homestay, bật/tắt đón khách, duyệt đơn |
| **Khách thuê (Guest)** | `guest@gmail.com` | `123123123` | Tìm kiếm, đặt phòng, thanh toán VNPay-QR, xem chuyến đi, đánh giá |
| **Admin hệ thống** | `admin@gmail.com` | `123123123` | Quản trị toàn sàn |

---
*© 2026 Stayly Web Application. Đồ án môn học / Khóa luận cử nhân.*
