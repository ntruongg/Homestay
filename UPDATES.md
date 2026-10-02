# Báo Cáo Cập Nhật Hệ Thống (Updates Since Last Commit)

**Thời gian tổng hợp:** 02/10/2026  
**Commit cơ sở (Base commit):** `3250428 Fixed conflicts`  
**Nhánh (Branch):** `huy`  
**Tổng số tệp thay đổi:** 15 files (+3,282 lines / -135 lines)

---

## 1. Tổng Quan Thay Đổi (Executive Summary)

Đợt cập nhật này tập trung vào 4 nhóm tính năng và cải tiến trọng tâm:
1. **Xác thực & Trải nghiệm Đặt phòng (API & Web):**
   - Hỗ trợ đăng nhập linh hoạt bằng cả **Email** hoặc **Số điện thoại**.
   - Bổ sung thông tin cơ sở lưu trú (tên cơ sở, ảnh đại diện, danh sách số phòng) vào dữ liệu trả về của đơn đặt phòng.
   - Tự động hoàn trả trạng thái lịch phòng (`LichLuuTru`) về `"Trống"` khi khách hàng hủy đơn.
2. **Quản lý Đơn đặt phòng & Chuyến đi (My Trips):**
   - Nâng cấp giao diện trang `Trips.cshtml` hiển thị đầy đủ ảnh, tên homestay, số phòng.
   - Thêm tính năng **Hủy đơn** (`CancelBooking`) kèm xác nhận.
   - Thêm tính năng **Yêu cầu hoàn tiền** (`RequestRefund`) trước ngày nhận phòng tối thiểu 2 ngày.
   - Tích hợp modal **Đánh giá kỳ nghỉ** (5 sao + nhận xét) gửi trực tiếp qua AJAX sau khi hoàn tất lưu trú.
3. **Giao diện Người dùng & Trải nghiệm Hình ảnh (UI/UX):**
   - Chuẩn hóa tỷ lệ khung hình ảnh `16:10` trên toàn bộ thẻ phòng và chi tiết cơ sở (`site.css`, `Details.cshtml`, `Index.cshtml`).
   - Thêm cơ chế xử lý lỗi tải ảnh dự phòng (`onerror fallback`) ngăn chặn hiện tượng ảnh vỡ.
   - Tự động điều hướng tài khoản chủ nhà (`OWNER`) về trang Quản lý (`/Home/Dashboard`) sau khi đăng nhập.
4. **Đồng bộ Dữ liệu & Ứng dụng Quản trị WPF (WPF Desktop & Seed Data):**
   - Bổ sung tài khoản kiểm thử (`admin@stayly.com`, `admin@gmail.com`, `owner@gmail.com`, `guest@gmail.com`).
   - Sửa lỗi mapping JSON lịch sử duyệt cơ sở lưu trú (`ApiApprovalHistoryDto`) và xử lý danh sách rỗng trong `HttpAdminService.cs`.
   - Mở rộng cơ sở dữ liệu mẫu `HOMESTAY_DB.sql` và `DbInitializer.cs` với hơn 55+ cơ sở lưu trú chất lượng cao, các đơn đặt phòng, giao dịch thanh toán và đánh giá mẫu.

---

## 2. Chi Tiết Thay Đổi Theo Từng Thành Phần

### 2.1. Backend API (`API/`)

- [AuthController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/AuthController.cs#L214-L225)
  - Phương thức `Login`: Cho phép người dùng đăng nhập bằng cả địa chỉ **Email** hoặc **Số điện thoại** (`a.Email == email || a.DienThoai == input`).
  
- [BookingsController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/BookingsController.cs#L145-L274)
  - Phương thức `GetMyBookings` & `GetById`: Eager load thêm thông tin cơ sở lưu trú và hình ảnh (`.ThenInclude(p => p.CoSoLuuTru).ThenInclude(c => c.HinhAnhs)`).
  - Phương thức `Cancel`: Cập nhật trạng thái đơn thành `"DaHuy"` đồng thời tự động tìm kiếm các bản ghi trong [LichLuuTru](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Models/LichLuuTru.cs) tương ứng với các phòng trong khoảng thời gian `[NgayDen, NgayDi)` và cập nhật lại `TrangThai = "Trống"`.
  - Phương thức `MapToResponse`: Trả thêm `PropertyName`, `RoomNumbers`, và `PropertyImage` về client.

- [BookingDtos.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/DTOs/Bookings/BookingDtos.cs#L53-L57)
  - Cập nhật record `BookingResponse` với 3 trường mới:
    ```csharp
    string? PropertyName = null,
    IReadOnlyList<string>? RoomNumbers = null,
    string? PropertyImage = null
    ```

- [DbInitializer.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Data/DbInitializer.cs)
  - Thêm tài khoản mẫu: `guest@gmail.com` (Khách hàng) và `owner@gmail.com` (Chủ cơ sở).
  - Khởi tạo các cơ sở lưu trú mẫu: "An Nhiên Mountain Homestay Đà Lạt", "Phố Cổ Ancient Retreat Hà Nội" (trạng thái `ChoDuyet`).
  - Khởi tạo các đơn đặt phòng với các trạng thái khác nhau (`DaHoanTat`, `DaDuyet`, `ChoDuyet`), giao dịch thanh toán và đánh giá thực tế.

- [Program.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Program.cs#L43-L48)
  - Cấu hình bổ sung CORS origin `http://localhost:5280` và `https://localhost:5280` phục vụ Web MVC client kết nối tới API.

---

### 2.2. Web Khách Hàng & Chủ Nhà (`Web/`)

- [HomeController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Controllers/HomeController.cs#L925-L1042)
  - `Book`: Nhận thêm `PropertyId`, đảm bảo khi phiên đăng nhập hết hạn hoặc xảy ra lỗi đặt phòng, hệ thống điều hướng chính xác về trang chi tiết homestay thay vì lỗi mã phòng.
  - `CancelBooking`: Action POST hủy đơn đặt phòng của khách hàng kèm thông báo `TempData`.
  - `RequestRefund`: Action POST gửi yêu cầu hoàn tiền và lý do lên ban quản trị xét duyệt.

- [HomestayApiClient.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Services/HomestayApiClient.cs#L213-L260)
  - Thêm phương thức `CancelBookingAsync(int bookingId, string token)` gọi API `PUT bookings/{id}/cancel`.
  - Thêm phương thức `RequestRefundAsync(int bookingId, string reason, string token)` gọi API `POST bookings/{id}/request-refund`.

- [ApiModels.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Models/ApiModels.cs#L38-L178)
  - Khai báo model `BookingServiceItem`.
  - Bổ sung `Services`, `PropertyName`, `RoomNumbers`, `PropertyImage` vào record `Booking`.
  - Bổ sung thuộc tính `PropertyId` vào class `BookingInput`.

- [Trips.cshtml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Home/Trips.cshtml)
  - Tái thiết kế giao diện danh sách chuyến đi: Card đơn đặt phòng hiển thị ảnh đại diện `16:10`, tên cơ sở, số hiệu phòng, khoảng thời gian và số lượng khách.
  - Hiển thị badge trạng thái chi tiết: `Chờ chủ duyệt`, `Đã xác nhận`, `Đang lưu trú`, `Đã hoàn tất`, `Đã huỷ`, `Chờ duyệt hoàn tiền`, `Đã hoàn tiền`.
  - Modal xác nhận **Hủy đơn** nhanh chóng.
  - Modal **Yêu cầu hoàn tiền** (kèm điều kiện trước ngày nhận phòng tối thiểu 2 ngày).
  - Modal **Đánh giá kỳ nghỉ** với bộ chọn 5 sao trực quan và gửi AJAX ngay trên trang.

- [Details.cshtml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Home/Details.cshtml) & [Index.cshtml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Home/Index.cshtml)
  - Khắc phục lỗi ảnh hỏng bằng thuộc tính `onerror` trỏ về placeholder Unsplash chất lượng cao.
  - Đồng bộ khung ảnh tỷ lệ cố định `16:10` trên cả desktop và thiết bị di động.
  - Đính kèm `PropertyId` vào form đặt phòng.

- [_Layout.cshtml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Shared/_Layout.cshtml#L685-L695)
  - Thêm logic kiểm tra vai trò sau khi đăng nhập qua popup modal: nếu người dùng là `OWNER`, chuyển hướng ngay tới `/Home/Dashboard`.

- [site.css](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/wwwroot/css/site.css#L796-L968)
  - Chuẩn hóa khối thẻ `.room-card-horizontal`: thay thế chiều cao cứng bằng `height: auto` và `aspect-ratio: 16 / 10`.
  - Cải thiện khoảng cách đệm (padding), kiểu chữ (typography), và độ tương phản của nút bấm hành động.

---

### 2.3. Ứng Dụng Quản Trị WPF Desktop (`Wpf/`)

- [HttpAdminService.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Services/HttpAdminService.cs)
  - Mở rộng danh sách tài khoản quản trị viên tự động dò tìm: thêm `admin@stayly.com` và `admin@gmail.com`.
  - Sửa lỗi kiểm tra rỗng danh sách (`if (dtos != null)`) thay vì kiểm tra `Count > 0`, giúp trả về danh sách rỗng đúng chuẩn API thay vì kích hoạt chế độ fallback sai lệch.
  - Loại bỏ việc giả lập phê duyệt/từ chối ngầm khi gặp lỗi mạng (`catch { return false; }` và ghi log chẩn đoán `Debug.WriteLine`).
  - Hỗ trợ chuẩn hóa trạng thái đa ngôn ngữ: `"Approved" or "DaDuyet"`, `"Rejected" or "TuChoi"`.
  - Cập nhật thuộc tính `[JsonPropertyName]` cho `ApiApprovalHistoryDto` để ánh xạ chính xác với JSON API (`id`, `propertyId`, `status`, `rejectionReason`, `reviewerId`, `reviewerName`, `reviewedAt`).

---

### 2.4. Dữ Liệu SQL Database (`HOMESTAY_DB.sql`)

- [HOMESTAY_DB.sql](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/HOMESTAY_DB.sql)
  - Chèn 55+ khách sạn và homestay thực tế trên khắp các tỉnh thành du lịch trọng điểm Việt Nam.
  - Bổ sung bộ sưu tập hình ảnh phòng và cơ sở lưu trú sắc nét từ Unsplash.
  - Chèn đơn đặt phòng, giao dịch thanh toán hoa hồng hệ thống (15%) và các đánh giá trải nghiệm thực tế theo chuẩn Traveloka.
  - Bổ sung ghi nhật ký hoạt động khởi tạo hệ thống (`NhatKyHoatDong`).

---

## 3. Danh Sách Tệp Đã Sửa Đổi (Changed Files)

| Đường dẫn tệp | Loại thay đổi | Mô tả tóm tắt |
| :--- | :--- | :--- |
| [API/Controllers/AuthController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/AuthController.cs) | Modified | Cho phép đăng nhập bằng cả Email hoặc Số điện thoại |
| [API/Controllers/BookingsController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/BookingsController.cs) | Modified | Trả thêm thông tin cơ sở/ảnh/phòng và giải phóng lịch phòng khi hủy đơn |
| [API/DTOs/Bookings/BookingDtos.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/DTOs/Bookings/BookingDtos.cs) | Modified | Mở rộng DTO `BookingResponse` với tên cơ sở, số phòng, ảnh đại diện |
| [API/Data/DbInitializer.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Data/DbInitializer.cs) | Modified | Bổ sung seed dữ liệu người dùng mẫu, homestay, đơn đặt phòng và đánh giá |
| [API/Program.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Program.cs) | Modified | Thêm CORS port 5280 cho Web MVC Client |
| [HOMESTAY_DB.sql](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/HOMESTAY_DB.sql) | Modified | Chèn 55+ homestay/khách sạn, hóa đơn, thanh toán và đánh giá chuẩn OTA |
| [Web/Web/Controllers/HomeController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Controllers/HomeController.cs) | Modified | Thêm action Hủy đơn, Yêu cầu hoàn tiền và sửa redirect `PropertyId` |
| [Web/Web/Models/ApiModels.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Models/ApiModels.cs) | Modified | Thêm `BookingServiceItem`, cập nhật model `Booking` và `BookingInput` |
| [Web/Web/Services/HomestayApiClient.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Services/HomestayApiClient.cs) | Modified | Thêm hàm gọi API hủy đơn (`CancelBookingAsync`) và hoàn tiền (`RequestRefundAsync`) |
| [Web/Web/Views/Home/Details.cshtml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Home/Details.cshtml) | Modified | Thêm `onerror` fallback cho ảnh, chuẩn hóa tỷ lệ 16:10, thêm `PropertyId` vào form |
| [Web/Web/Views/Home/Index.cshtml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Home/Index.cshtml) | Modified | Thêm `onerror` fallback cho ảnh carousel, căn chỉnh layout card phòng |
| [Web/Web/Views/Home/Trips.cshtml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Home/Trips.cshtml) | Modified | Nâng cấp toàn diện giao diện Chuyến đi của tôi, tích hợp modal Hủy/Hoàn tiền/Đánh giá |
| [Web/Web/Views/Shared/_Layout.cshtml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Shared/_Layout.cshtml) | Modified | Điều hướng chủ nhà (`OWNER`) về `/Home/Dashboard` sau đăng nhập |
| [Web/Web/wwwroot/css/site.css](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/wwwroot/css/site.css) | Modified | Tối ưu hóa CSS thẻ phòng ngang, khóa tỷ lệ ảnh 16:10 chống vỡ khung hình |
| [Wpf/HomestaySystem/HomestaySystem/Services/HttpAdminService.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Services/HttpAdminService.cs) | Modified | Cập nhật tài khoản admin, sửa deserialize lịch sử duyệt và trạng thái phê duyệt |
