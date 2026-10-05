# Báo Cáo Cập Nhật Hệ Thống (Updates Since Last Commit)

**Thời gian tổng hợp:** 02/10/2026\
**Commit cơ sở (Base commit):** `3250428 Fixed conflicts`\
**Nhánh (Branch):** `huy`\
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

- \[AuthController.cs\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/AuthController.cs#L214-L225)

  - Phương thức `Login`: Cho phép người dùng đăng nhập bằng cả địa chỉ **Email** hoặc **Số điện thoại** (`a.Email == email || a.DienThoai == input`).

- \[BookingsController.cs\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/BookingsController.cs#L145-L274)

  - Phương thức `GetMyBookings` & `GetById`: Eager load thêm thông tin cơ sở lưu trú và hình ảnh (`.ThenInclude(p => p.CoSoLuuTru).ThenInclude(c => c.HinhAnhs)`).
  - Phương thức `Cancel`: Cập nhật trạng thái đơn thành `"DaHuy"` đồng thời tự động tìm kiếm các bản ghi trong \[LichLuuTru\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Models/LichLuuTru.cs) tương ứng với các phòng trong khoảng thời gian `[NgayDen, NgayDi)` và cập nhật lại `TrangThai = "Trống"`.
  - Phương thức `MapToResponse`: Trả thêm `PropertyName`, `RoomNumbers`, và `PropertyImage` về client.

- \[BookingDtos.cs\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/DTOs/Bookings/BookingDtos.cs#L53-L57)

  - Cập nhật record `BookingResponse` với 3 trường mới:

    ```csharp
    string? PropertyName = null,
    IReadOnlyList<string>? RoomNumbers = null,
    string? PropertyImage = null
    ```

- \[DbInitializer.cs\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Data/DbInitializer.cs)

  - Thêm tài khoản mẫu: `guest@gmail.com` (Khách hàng) và `owner@gmail.com` (Chủ cơ sở).
  - Khởi tạo các cơ sở lưu trú mẫu: "An Nhiên Mountain Homestay Đà Lạt", "Phố Cổ Ancient Retreat Hà Nội" (trạng thái `ChoDuyet`).
  - Khởi tạo các đơn đặt phòng với các trạng thái khác nhau (`DaHoanTat`, `DaDuyet`, `ChoDuyet`), giao dịch thanh toán và đánh giá thực tế.

- \[Program.cs\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Program.cs#L43-L48)

  - Cấu hình bổ sung CORS origin `http://localhost:5280` và `https://localhost:5280` phục vụ Web MVC client kết nối tới API.

---

### 2.2. Web Khách Hàng & Chủ Nhà (`Web/`)

- \[HomeController.cs\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Controllers/HomeController.cs#L925-L1042)

  - `Book`: Nhận thêm `PropertyId`, đảm bảo khi phiên đăng nhập hết hạn hoặc xảy ra lỗi đặt phòng, hệ thống điều hướng chính xác về trang chi tiết homestay thay vì lỗi mã phòng.
  - `CancelBooking`: Action POST hủy đơn đặt phòng của khách hàng kèm thông báo `TempData`.
  - `RequestRefund`: Action POST gửi yêu cầu hoàn tiền và lý do lên ban quản trị xét duyệt.

- \[HomestayApiClient.cs\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Services/HomestayApiClient.cs#L213-L260)

  - Thêm phương thức `CancelBookingAsync(int bookingId, string token)` gọi API `PUT bookings/{id}/cancel`.
  - Thêm phương thức `RequestRefundAsync(int bookingId, string reason, string token)` gọi API `POST bookings/{id}/request-refund`.

- \[ApiModels.cs\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Models/ApiModels.cs#L38-L178)

  - Khai báo model `BookingServiceItem`.
  - Bổ sung `Services`, `PropertyName`, `RoomNumbers`, `PropertyImage` vào record `Booking`.
  - Bổ sung thuộc tính `PropertyId` vào class `BookingInput`.

- \[Trips.cshtml\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Home/Trips.cshtml)

  - Tái thiết kế giao diện danh sách chuyến đi: Card đơn đặt phòng hiển thị ảnh đại diện `16:10`, tên cơ sở, số hiệu phòng, khoảng thời gian và số lượng khách.
  - Hiển thị badge trạng thái chi tiết: `Chờ chủ duyệt`, `Đã xác nhận`, `Đang lưu trú`, `Đã hoàn tất`, `Đã huỷ`, `Chờ duyệt hoàn tiền`, `Đã hoàn tiền`.
  - Modal xác nhận **Hủy đơn** nhanh chóng.
  - Modal **Yêu cầu hoàn tiền** (kèm điều kiện trước ngày nhận phòng tối thiểu 2 ngày).
  - Modal **Đánh giá kỳ nghỉ** với bộ chọn 5 sao trực quan và gửi AJAX ngay trên trang.

- \[Details.cshtml\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Home/Details.cshtml) & \[Index.cshtml\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Home/Index.cshtml)

  - Khắc phục lỗi ảnh hỏng bằng thuộc tính `onerror` trỏ về placeholder Unsplash chất lượng cao.
  - Đồng bộ khung ảnh tỷ lệ cố định `16:10` trên cả desktop và thiết bị di động.
  - Đính kèm `PropertyId` vào form đặt phòng.

- \[\_Layout.cshtml\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Shared/\_Layout.cshtml#L685-L695)

  - Thêm logic kiểm tra vai trò sau khi đăng nhập qua popup modal: nếu người dùng là `OWNER`, chuyển hướng ngay tới `/Home/Dashboard`.

- \[site.css\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/wwwroot/css/site.css#L796-L968)

  - Chuẩn hóa khối thẻ `.room-card-horizontal`: thay thế chiều cao cứng bằng `height: auto` và `aspect-ratio: 16 / 10`.
  - Cải thiện khoảng cách đệm (padding), kiểu chữ (typography), và độ tương phản của nút bấm hành động.

---

### 2.3. Ứng Dụng Quản Trị WPF Desktop (`Wpf/`)

- \[HttpAdminService.cs\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Services/HttpAdminService.cs)
  - Mở rộng danh sách tài khoản quản trị viên tự động dò tìm: thêm `admin@stayly.com` và `admin@gmail.com`.
  - Sửa lỗi kiểm tra rỗng danh sách (`if (dtos != null)`) thay vì kiểm tra `Count > 0`, giúp trả về danh sách rỗng đúng chuẩn API thay vì kích hoạt chế độ fallback sai lệch.
  - Loại bỏ việc giả lập phê duyệt/từ chối ngầm khi gặp lỗi mạng (`catch { return false; }` và ghi log chẩn đoán `Debug.WriteLine`).
  - Hỗ trợ chuẩn hóa trạng thái đa ngôn ngữ: `"Approved" or "DaDuyet"`, `"Rejected" or "TuChoi"`.
  - Cập nhật thuộc tính `[JsonPropertyName]` cho `ApiApprovalHistoryDto` để ánh xạ chính xác với JSON API (`id`, `propertyId`, `status`, `rejectionReason`, `reviewerId`, `reviewerName`, `reviewedAt`).

---

### 2.4. Dữ Liệu SQL Database (`HOMESTAY_DB.sql`)

- \[HOMESTAY_DB.sql\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/HOMESTAY_DB.sql)
  - Chèn 55+ khách sạn và homestay thực tế trên khắp các tỉnh thành du lịch trọng điểm Việt Nam.
  - Bổ sung bộ sưu tập hình ảnh phòng và cơ sở lưu trú sắc nét từ Unsplash.
  - Chèn đơn đặt phòng, giao dịch thanh toán hoa hồng hệ thống (15%) và các đánh giá trải nghiệm thực tế theo chuẩn Traveloka.
  - Bổ sung ghi nhật ký hoạt động khởi tạo hệ thống (`NhatKyHoatDong`).

---

## 3. Danh Sách Tệp Đã Sửa Đổi (Changed Files)

| Đường dẫn tệp | Loại thay đổi | Mô tả tóm tắt |
| --- | --- | --- |
| \[API/Controllers/AuthController.cs\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/AuthController.cs) | Modified | Cho phép đăng nhập bằng cả Email hoặc Số điện thoại |
| \[API/Controllers/BookingsController.cs\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/BookingsController.cs) | Modified | Trả thêm thông tin cơ sở/ảnh/phòng và giải phóng lịch phòng khi hủy đơn |
| \[API/DTOs/Bookings/BookingDtos.cs\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/DTOs/Bookings/BookingDtos.cs) | Modified | Mở rộng DTO `BookingResponse` với tên cơ sở, số phòng, ảnh đại diện |
| \[API/Data/DbInitializer.cs\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Data/DbInitializer.cs) | Modified | Bổ sung seed dữ liệu người dùng mẫu, homestay, đơn đặt phòng và đánh giá |
| \[API/Program.cs\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Program.cs) | Modified | Thêm CORS port 5280 cho Web MVC Client |
| \[HOMESTAY_DB.sql\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/HOMESTAY_DB.sql) | Modified | Chèn 55+ homestay/khách sạn, hóa đơn, thanh toán và đánh giá chuẩn OTA |
| \[Web/Web/Controllers/HomeController.cs\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Controllers/HomeController.cs) | Modified | Thêm action Hủy đơn, Yêu cầu hoàn tiền và sửa redirect `PropertyId` |
| \[Web/Web/Models/ApiModels.cs\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Models/ApiModels.cs) | Modified | Thêm `BookingServiceItem`, cập nhật model `Booking` và `BookingInput` |
| \[Web/Web/Services/HomestayApiClient.cs\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Services/HomestayApiClient.cs) | Modified | Thêm hàm gọi API hủy đơn (`CancelBookingAsync`) và hoàn tiền (`RequestRefundAsync`) |
| \[Web/Web/Views/Home/Details.cshtml\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Home/Details.cshtml) | Modified | Thêm `onerror` fallback cho ảnh, chuẩn hóa tỷ lệ 16:10, thêm `PropertyId` vào form |
| \[Web/Web/Views/Home/Index.cshtml\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Home/Index.cshtml) | Modified | Thêm `onerror` fallback cho ảnh carousel, căn chỉnh layout card phòng |
| \[Web/Web/Views/Home/Trips.cshtml\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Home/Trips.cshtml) | Modified | Nâng cấp toàn diện giao diện Chuyến đi của tôi, tích hợp modal Hủy/Hoàn tiền/Đánh giá |
| \[Web/Web/Views/Shared/\_Layout.cshtml\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Shared/\_Layout.cshtml) | Modified | Điều hướng chủ nhà (`OWNER`) về `/Home/Dashboard` sau đăng nhập |
| \[Web/Web/wwwroot/css/site.css\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/wwwroot/css/site.css) | Modified | Tối ưu hóa CSS thẻ phòng ngang, khóa tỷ lệ ảnh 16:10 chống vỡ khung hình |
| \[Wpf/HomestaySystem/HomestaySystem/Services/HttpAdminService.cs\](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Services/HttpAdminService.cs) | Modified | Cập nhật tài khoản admin, sửa deserialize lịch sử duyệt và trạng thái phê duyệt |

---

# Báo Cáo Cập Nhật Hệ Thống - Giai Đoạn 2 (04/10/2026 - Sau Merge PR #15)

**Thời gian tổng hợp:** 04/10/2026  
**Commit cơ sở (Base commit):** `edb93d3 Merge pull request #15 from ntruongg/huy`  
**Commit mới nhất:** `de090b7 Thêm thanh toán bằng VNPay, đánh giá đơn từ khách và phản hồi đánh giá đơn của chủ, cho phép hủy đơn trước ngày nhận phòng 2 ngày`  
**Nhánh (Branch):** `ntruong`  

---

## 4. Các Tính Năng Đã Hoàn Thiện & Cập Nhật Mới

1. **Tích hợp Cổng thanh toán VNPay thật & Luồng tự động duyệt thanh toán:**
   - Xây dựng thư viện `VnPayLibrary.cs` tạo URL thanh toán bảo mật HMAC SHA512.
   - Thêm action `PaymentCallback` trong `HomeController.cs` và API `ConfirmPayment` trong `BookingsController.cs`.
   - Khi thanh toán VNPay thành công: tự động tạo bản ghi trong bảng `ThanhToan` (tính toán hoa hồng 15%, tiền thực nhận chủ cơ sở) và tự động cập nhật trạng thái đơn thành `'ChoDuyet'`.
2. **Dịch vụ chạy nền tự động dọn dẹp đơn quá hạn (`ExpiredBookingCleanupService`):**
   - Chạy nền chu kỳ mỗi 60 giây.
   - Tự động hủy đơn (`TrangThai = 'DaHuy'`) đối với các đơn ở trạng thái `'ChoThanhToan'` quá 15 phút chưa thanh toán, đồng thời giải phóng lịch phòng `LichLuuTru` về `'Trống'`.
3. **Quy trình Hủy đơn & Yêu cầu hoàn tiền (Refund Workflow):**
   - Khách được hủy và yêu cầu hoàn tiền ngay lập tức nếu đơn đang ở trạng thái `ChoDuyet`.
   - Nếu đơn đã duyệt (`DaDuyet` / `Confirmed`), yêu cầu hoàn tiền được chấp thuận nếu cách ngày nhận phòng tối thiểu 2 ngày.
   - Thu thập thông tin tài khoản ngân hàng thụ hưởng của khách (Ngân hàng, Số tài khoản, Tên người thụ hưởng), lưu vào hồ sơ khách hàng và kèm vào `LyDoHoanTien`.
   - Đồng bộ trạng thái `YeuCauHoanTien`, `DaHoanTien` trên toàn hệ thống (Web khách, Web chủ, WPF Admin).
4. **Hệ thống Đánh giá & Phản hồi đánh giá thực tế:**
   - **Đánh giá từ khách:** Sau khi hoàn tất kỳ nghỉ, khách hàng có thể gửi đánh giá điểm sao (1-5) và nhận xét.
   - **Phản hồi từ chủ nhà:** Chủ cơ sở lưu trú có thể phản hồi trực tiếp các đánh giá của khách trên Dashboard (`/Home/ReplyReview` $\rightarrow$ API `reviews/{id}/reply`). Hỗ trợ hiển thị và chỉnh sửa phản hồi đã có.
   - **Trang chi tiết phòng (`Details.cshtml`):** Loại bỏ hoàn toàn đánh giá mẫu, chỉ hiển thị đánh giá thật, phân trang tối đa 5 đánh giá/trang mượt mà.
   - **Trang chủ (`Index.cshtml`):** Bộ lọc tìm kiếm cho phép chọn 5 mục từ 1-5 sao, card homestay hiển thị điểm số thật từ database thay vì fix cứng 4.9, loại bỏ số lượng đánh giá trên card.

---

## 5. Những Thứ BẮT BUỘC Để Hệ Thống Chạy Được Như Bây Giờ (Setup Requirements)

Để hệ thống hoạt động đầy đủ và không gặp lỗi khi chạy trên máy khác hoặc môi trường mới, **bắt buộc phải thực hiện các bước sau**:

### 5.1. Cập nhật Cơ sở dữ liệu SQL Server (Bắt buộc)
Nếu dùng database có sẵn từ trước đợt merge này, **phải chạy đoạn script SQL sau** trên `HOMESTAY_DB`:

```sql
-- 1. Cập nhật Check Constraint và Default của trạng thái đơn đặt phòng
-- Lưu ý: Xóa check constraint cũ nếu có (tra cứu theo tên constraint trong DB)
ALTER TABLE DonDatPhong DROP CONSTRAINT [CK__DonDatPho__Trang__XXXXXXXX]; 
ALTER TABLE DonDatPhong ADD CONSTRAINT CK_DonDatPhong_TrangThai CHECK (TrangThai IN (
    'ChoThanhToan', 'ChoDuyet', 'TuChoi', 'DaDuyet', 'DaHoanTat', 'YeuCauHoanTien', 'DaHoanTien', 'DaHuy'
));
ALTER TABLE DonDatPhong ADD CONSTRAINT DF_DonDatPhong_TrangThai DEFAULT 'ChoThanhToan' FOR TrangThai;

-- 2. Bổ sung các cột hoàn tiền trong DonDatPhong nếu chưa có
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DonDatPhong') AND name = 'ThoiGianYeuCauHoan')
    ALTER TABLE DonDatPhong ADD ThoiGianYeuCauHoan DATETIME NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DonDatPhong') AND name = 'LyDoHoanTien')
    ALTER TABLE DonDatPhong ADD LyDoHoanTien NVARCHAR(500) NULL;

-- 3. Bổ sung các cột thông tin ngân hàng trong NguoiDung nếu chưa có
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('NguoiDung') AND name = 'NganHang')
    ALTER TABLE NguoiDung ADD NganHang NVARCHAR(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('NguoiDung') AND name = 'SoTaiKhoan')
    ALTER TABLE NguoiDung ADD SoTaiKhoan VARCHAR(30) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('NguoiDung') AND name = 'TenNguoiThuHuong')
    ALTER TABLE NguoiDung ADD TenNguoiThuHuong NVARCHAR(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('NguoiDung') AND name = 'CCCD')
    ALTER TABLE NguoiDung ADD CCCD VARCHAR(20) NULL;
```
*(Hoặc chạy lại toàn bộ file `HOMESTAY_DB.sql` để có cấu trúc và hơn 55+ cơ sở lưu trú mẫu mới nhất).*

### 5.2. Cấu hình VNPay trong `appsettings.json` (Web Client)
Trong tệp `Web/Web/appsettings.json`, cần đảm bảo cấu hình đúng thông số sandbox VNPay:
```json
"Vnpay": {
  "TmnCode": "YOUR_TMN_CODE",
  "HashSecret": "YOUR_HASH_SECRET",
  "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
  "ReturnUrl": "http://localhost:5280/Home/PaymentCallback"
}
```
*Lưu ý:* `ReturnUrl` phải khớp đúng scheme và port của ứng dụng Web (`http://localhost:5280` hoặc `https://localhost:7143`).

### 5.3. Đăng ký Background Service trong `API/Program.cs`
Dịch vụ dọn dẹp đơn quá hạn cần được đăng ký trong DI container:
```csharp
builder.Services.AddHostedService<ExpiredBookingCleanupService>();
```

---

## 6. Ghi Chú Lỗi & Danh Sách Cần Sửa Lần Sau (Known Issues & Tech Debt)

### ✅ Đã Sửa Xong:
- **[Đã Giải Quyết] Lỗi ràng buộc `UNIQUE` trên cột `CCCD`:** Đã sửa cả trong `HOMESTAY_DB.sql` và `HomestayDbContext.cs` bằng Filtered Index (`WHERE CCCD IS NOT NULL`). Người dùng đăng ký không nhập CCCD sẽ không còn bị lỗi trùng khóa `NULL`.
- **[Đã Giải Quyết] Thêm Migration EF Core mới nhất:** Đã sinh migration chính thức `20261004080250_AddRefundAndBankAccountFields` cho project `API` và đồng bộ vào bảng `[__EFMigrationsHistory]` trong `HOMESTAY_DB.sql`.
- **[Đã Giải Quyết] Đồng bộ giá trị mặc định `TrangThai`:** Trong model `QuanLyDonDat.cs` và `HomestayDbContext.cs` đã đồng bộ `DEFAULT 'ChoThanhToan'` khớp hoàn toàn với cơ sở dữ liệu.

- **[Đã Giải Quyết] Kiểm soát danh sách ngân hàng hợp lệ bằng Dropdown:** Toàn bộ các trường ngân hàng trong hệ thống (Hồ sơ người dùng/Chủ nhà, Modal yêu cầu hoàn tiền trong [Trips.cshtml](file:///d:/Homestay/Web/Web/Views/Home/Trips.cshtml), Đăng ký đối tác, và WPF Admin) đã được ràng buộc thành danh sách chọn Dropdown chứa các ngân hàng chính thức được VNPay hỗ trợ. Khách hàng đã lưu ngân hàng trong hồ sơ sẽ được tự động điền sẵn khi mở modal hoàn tiền; nếu chưa lưu, khách chọn từ danh sách và điền số tài khoản.
- **[Đã Giải Quyết] Tối ưu và tự động hóa hệ thống Email:**
  - Đã xóa bỏ 3 mẫu email không sử dụng (`PropertyApproved`, `PropertyRejected`, `PropertySubmissionReceived`).
  - Đã thêm template email mới [BookingConfirmed.cshtml](file:///d:/Homestay/API/Templates/Emails/BookingConfirmed.cshtml) (Xác nhận đặt phòng thành công) gửi tự động cho khách khi chủ cơ sở bấm phê duyệt đơn.
  - Tự động kích hoạt gửi mail: OTP đăng ký/đổi mật khẩu/đổi email, Xác nhận đặt phòng thành công (`BookingConfirmed`), Duyệt hoàn tiền (`RefundAccepted`), và Từ chối hoàn tiền (`RefundDenied`).
- **[Đã Giải Quyết] Loại bỏ hoàn toàn Mock Data & Chuẩn hóa Desktop WPF:**
  - Đã xóa bỏ hoàn toàn dịch vụ giả lập [DuLieuGiaLapAdminService.cs](file:///d:/Homestay/Wpf/HomestaySystem/HomestaySystem/Services/DuLieuGiaLapAdminService.cs), chuyển 100% sang `HttpAdminService` gọi trực tiếp API Backend.
  - Bổ sung 2 API endpoint quản trị người dùng: `POST /api/admin/users` (tạo tài khoản với PasswordHasher) và `PUT /api/admin/users/{id}` (cập nhật thông tin).
  - Thêm hiển thị thông báo lỗi `MessageBox.Show` chi tiết từ máy chủ khi thao tác dữ liệu thất bại.
  - Hỗ trợ đầy đủ trạng thái `DaHoanTat` (Đã hoàn tất / Check-out) trong bộ lọc đơn đặt phòng.
- **[Đã Giải Quyết] Bộ lọc Ajax, Phân trang 15 Card/Trang & Nhãn "Chưa có đánh giá":**
  - Đã tích hợp thanh trượt khoảng giá (Range Slider) kết hợp các nút chọn nhanh cùng bộ lọc sao 1-5 sao và "Chưa có đánh giá", lọc dữ liệu tức thì (Ajax) không tải lại trang.
  - Cố định phân trang **đúng 15 card phòng / trang** kèm thanh chuyển trang mượt mà.
  - Homestay chưa có đánh giá thực tế sẽ hiển thị nhãn **`"Chưa có đánh giá"`** thay vì điểm `0.0 ★` hoặc `"Mới"`.

### ⚠️ Các vấn đề cần cải tiến tiếp theo:

### ⚠️ Vấn đề 1 (Mức độ trung bình): Quy trình Hoàn tiền VNPay tự động (Automated VNPay Refund API)
- **Hiện trạng:** Việc hoàn tiền qua VNPay Sandbox yêu cầu đăng ký API hoàn tiền riêng, cấu hình chứng chỉ số RSA/IP tĩnh và thủ tục đối soát merchant phức tạp. Hệ thống hiện ghi nhận `YeuCauHoanTien` kèm thông tin tài khoản ngân hàng của khách đã được kiểm duyệt hợp lệ.
- **Hành vi:** Admin kiểm tra số tài khoản đã được chọn đúng ngân hàng VNPay hỗ trợ, duyệt hoàn tiền và hệ thống gửi email thông báo tự động `RefundAccepted` cho khách.
- **Giải pháp tiếp theo:** Tích hợp API `vnpay_refund` tự động gọi trực tiếp cổng thanh toán khi triển khai môi trường Production chính thức.



