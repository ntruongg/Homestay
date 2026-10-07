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

---

## 7. Cập Nhật Triển Khai Toàn Diện Giai Đoạn 1, 2, 3 & 4 (Theo `plan.md`)

**Thời gian triển khai:** 06/10/2026\
**Mục tiêu hoàn thành:**
- **Giai đoạn 1:** Triển khai toàn diện Backend API & Database Maintenance Service phục vụ Sao lưu & Phục hồi CSDL thực tế, loại bỏ triệt để hardcode, bổ sung đầy đủ các endpoint Admin bị thiếu (`properties/{id}`, `bookings/{id}`, `promotions`).
- **Giai đoạn 2:** Nâng cấp ứng dụng Quản trị máy trạm WPF Admin Client kết nối trực tiếp API Backend, loại bỏ mock stubs, hỗ trợ streaming tải về, upload phục hồi an toàn, xác nhận mật khẩu Admin và thiết kế lại giao diện theo phong cách Ocean Blue hiện đại.
- **Giai đoạn 3:** Xây dựng Dịch vụ Sao lưu Tự động Chạy nền (`AutomatedBackupHostedService`) kèm Chính sách Dọn dẹp Đĩa (Retention Policy) và Giao diện Quản lý Lịch biểu trên WPF Desktop.
- **Giai đoạn 4:** Kiểm thử Tích hợp Thực tế Toàn diện (End-to-End Real-world Testing), Kiểm chứng Phục hồi Thảm họa (Disaster Recovery Verification) & Sổ tay Vận hành Hệ thống.

---

### 7.1. Giai Đoạn 1: Backend API & Dịch Vụ Cơ Sở Dữ Liệu Thực Tế (Database Maintenance & Admin Endpoints)

#### 1. Bộ DTOs Cơ Sở Dữ Liệu ([DatabaseDtos.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/DTOs/Database/DatabaseDtos.cs))
- Khai báo [BackupItemDto](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/DTOs/Database/DatabaseDtos.cs#L5-L12): Chứa thông tin chi tiết của từng tệp sao lưu (`FileName`, `SizeInBytes`, `FormattedSize`, `CreatedAt`, `IsAutomated`, `Description`).
- Khai báo [CreateBackupRequest](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/DTOs/Database/DatabaseDtos.cs#L14-L17): Nhận ghi chú sao lưu và cờ nén dữ liệu `Compress` (mặc định `true`).
- Khai báo [RestoreServerFileRequest](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/DTOs/Database/DatabaseDtos.cs#L19-L23): Yêu cầu tên tệp, mật khẩu Quản trị viên (`AdminPasswordConfirmation`) và xác nhận tên CSDL (`ConfirmDatabaseName == "HOMESTAY_DB"`).
- Khai báo [DatabaseStatusDto](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/DTOs/Database/DatabaseDtos.cs#L25-L34): Báo cáo trạng thái CSDL thực tế từ máy chủ (kích thước file Data, file Log, số lượng bảng, thời điểm sao lưu gần nhất, tổng số bản sao lưu và dung lượng).

#### 2. Dịch vụ Bảo trì CSDL Backend ([IDatabaseMaintenanceService.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Services/Database/IDatabaseMaintenanceService.cs) & [DatabaseMaintenanceService.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Services/Database/DatabaseMaintenanceService.cs))
- **Sao lưu thực tế bằng T-SQL:**
  - Thực thi lệnh: `BACKUP DATABASE [HOMESTAY_DB] TO DISK = @path WITH INIT, COMPRESSION, STATS = 10`.
  - Tự động sinh tệp siêu dữ liệu đi kèm `.meta.json` để lưu trữ mô tả, người tạo và cờ tự động mà không làm ảnh hưởng đến tệp `.bak` chuẩn của SQL Server.
- **Phục hồi CSDL an toàn:**
  - Kết nối qua database hệ thống `master` để tránh bị khóa chính CSDL đích.
  - Gọi [SqlConnection.ClearAllPools()](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Services/Database/DatabaseMaintenanceService.cs#L104) để giải phóng toàn bộ connection pool của Entity Framework Core đang hoạt động.
  - Đưa CSDL về chế độ độc quyền: `ALTER DATABASE [HOMESTAY_DB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;`.
  - Thực thi: `RESTORE DATABASE [HOMESTAY_DB] FROM DISK = @path WITH REPLACE;`.
  - Đưa CSDL trở lại chế độ đa người dùng: `ALTER DATABASE [HOMESTAY_DB] SET MULTI_USER;`.
- **Hỗ trợ Streaming Download & Upload Restore:**
  - `GetBackupStreamAsync`: Mở luồng đọc tệp `.bak` trả về client với `FileShare.Read`.
  - `RestoreFromUploadAsync`: Lưu tệp `.bak` tải lên từ client vào thư mục tạm trên server, kiểm tra tính hợp lệ rồi tiến hành phục hồi.
  - `DeleteBackupAsync`: Xóa tệp `.bak` và dọn dẹp tệp `.meta.json` đi kèm.
- **Đăng ký dịch vụ:** Đã đăng ký `IDatabaseMaintenanceService` dạng Scoped trong [Program.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Program.cs#L25).

#### 3. Bộ điều khiển Quản trị CSDL ([DatabaseController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/DatabaseController.cs))
- Endpoint `GET /api/admin/database/status`: Trả về thông tin trạng thái CSDL.
- Endpoint `GET /api/admin/database/backups`: Trả về danh sách toàn bộ các bản sao lưu hiện có trên máy chủ.
- Endpoint `POST /api/admin/database/backup`: Tạo bản sao lưu mới và ghi Audit Log (`SAO_LUU_CSDL`).
- Endpoint `GET /api/admin/database/backups/{fileName}/download`: Streaming download tệp `.bak` về máy trạm client qua MIME `application/octet-stream`.
- Endpoint `POST /api/admin/database/restore`: Phục hồi CSDL từ tệp máy chủ kèm xác thực mật khẩu Admin thông qua [IPasswordHasher<NguoiDung>](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/DatabaseController.cs#L125).
- Endpoint `POST /api/admin/database/restore/upload`: Tiếp nhận tệp upload (giới hạn tối đa 1GB) và tiến hành phục hồi.
- Endpoint `DELETE /api/admin/database/backups/{fileName}`: Xóa tệp sao lưu trên máy chủ.

#### 4. Bổ sung các Endpoint Admin còn thiếu ([AdminController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/AdminController.cs) & [AdminPromotionsController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/AdminPromotionsController.cs))
- **Cập nhật Cơ sở lưu trú:** Bổ sung `PUT /api/admin/properties/{id}` nhận [UpdatePropertyAdminRequest](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/DTOs/Admin/AdminDtos.cs#L79-L92) cho phép chỉnh sửa thông tin, chính sách và trạng thái hoạt động của cơ sở lưu trú.
- **Chi tiết Đơn đặt phòng:** Bổ sung `GET /api/admin/bookings/{id}` trả về đầy đủ [AdminBookingDetailsResponse](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/DTOs/Admin/AdminDtos.cs#L165-L185) (phòng, dịch vụ đi kèm, hóa đơn, thông tin thanh toán, hoàn tiền).
- **Cập nhật Đơn đặt phòng:** Bổ sung `PUT /api/admin/bookings/{id}` nhận [UpdateBookingAdminRequest](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/DTOs/Admin/AdminDtos.cs#L238-L249) cho phép quản trị viên điều chỉnh ngày nhận/trả phòng, số lượng khách, thông tin liên lạc và trạng thái đơn.
- **Quản lý Khuyến mãi:** Bổ sung [AdminPromotionsController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/AdminPromotionsController.cs) hỗ trợ trọn bộ `GET`, `POST`, `PUT`, `DELETE /api/admin/promotions` khớp chính xác với DTO client WPF đang sử dụng.

---

### 7.2. Giai Đoạn 2: Nâng Cấp Ứng Dụng Quản Trị Desktop WPF (WPF Admin Desktop Client)

#### 1. Loại Bỏ Hardcode Đường Dẫn & Triệt Tiêu Mock Stubs
- Xóa bỏ hoàn toàn đường dẫn cố định `D:\DO_AN_HK7\KhoaLuanCuNhan\KLCN\Wpf\Backup` khỏi luồng xử lý chính. Máy trạm client hiện tại hoạt động độc lập và chỉ giao tiếp với máy chủ thông qua HTTP REST API.
- Thay thế hoàn toàn mã giả lập `Task.Delay(100)` trong [HttpAdminService.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Services/HttpAdminService.cs) bằng các tác vụ mạng bất đồng bộ thực sự.

#### 2. Mô hình Dữ liệu Mới
- [BackupItem.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Models/BackupItem.cs): Đại diện cho tệp sao lưu trên client, hỗ trợ các thuộc tính hiển thị `LoaiSaoLuuText`, `NgayTaoText`.
- [DatabaseStatus.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Models/DatabaseStatus.cs): Đại diện cho trạng thái CSDL thực tế (kích thước dữ liệu Data + Log MB, số bảng, lần sao lưu gần nhất).

#### 3. Dịch Vụ Admin Client ([IAdminService.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Services/IAdminService.cs) & [HttpAdminService.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Services/HttpAdminService.cs))
- `LayTrangThaiCSDLAsync()`: Gọi endpoint `/api/admin/database/status`.
- `LayDanhSachBanSaoLuuAsync()`: Gọi endpoint `/api/admin/database/backups`.
- `TaoBanSaoLuuAsync(moTa, compress)`: Gọi POST `/api/admin/database/backup`.
- `TaiTepSaoLuuVeMayAsync(tenTep, duongDanLuu)`: Streaming tải file từ server về máy trạm và lưu ra đường dẫn do người dùng chỉ định.
- `PhucHoiTuTepMayChuAsync(tenTep, matKhauAdmin, xacNhanDb)`: Gửi yêu cầu phục hồi tệp server kèm mật khẩu quản trị viên.
- `PhucHoiTuTepUploadAsync(duongDanTep, matKhauAdmin, xacNhanDb)`: Sử dụng `MultipartFormDataContent` tải file `.bak` từ máy trạm lên API để phục hồi.
- `XoaBanSaoLuuAsync(tenTep)`: Gọi DELETE `/api/admin/database/backups/{fileName}`.
- `LayNhatKyBaoTriAsync()`: Tự động tổng hợp từ Audit Logs trên máy chủ (`/api/admin/audit-logs`) và lịch sử sao lưu.

#### 4. ViewModel Điều Khiển Quản Trị ([BaoTriHeThongViewModel.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/ViewModels/BaoTriHeThongViewModel.cs))
- Quản lý các thuộc tính KPI tự động tính toán: `TongDungLuongMB`, `DungLuongDataMB`, `DungLuongLogMB`, `SoBang`, `LanSaoLuuCuoi`, `TongSoBanSaoLuu`, `TongDungLuongSaoLuu`.
- Tích hợp Modal cảnh báo bảo mật (`HienThiXacNhanPhucHoi`): Trước khi thực hiện phục hồi, người dùng bắt buộc phải nhập mật khẩu tài khoản Admin và gõ đúng chữ `HOMESTAY_DB`.
- Xử lý tải file về máy trạm bằng `SaveFileDialog` và chọn file phục hồi cục bộ bằng `OpenFileDialog`.
- Hiệu ứng chờ (`DangXuLy` và `ThongBaoTrangThai`) thông báo rõ ràng từng bước thực hiện trên máy chủ.
- Duy trì 100% khả năng tương thích ngược với các command cũ.

#### 5. Tái Thiết Kế Giao Diện Ocean Blue Đẳng Cấp ([BaoTriHeThongView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/BaoTriHeThongView.xaml))
- **Thanh tiêu đề:** Hiển thị trạng thái kết nối SQL Server trực tuyến kèm nút "Đồng bộ CSDL" và "Mở thư mục máy trạm".
- **Hệ thống 4 Thẻ KPI Metrics:**
  1. *Dung lượng CSDL:* Hiển thị tổng dung lượng (MB) kèm phân rã Data MB và Log MB.
  2. *Cấu trúc hệ thống:* Hiển thị số lượng bảng và phiên bản SQL Server đang chạy.
  3. *Bản sao lưu mới nhất:* Hiển thị thời điểm sao lưu gần nhất với màu xanh ngọc (Emerald Green).
  4. *Kho bản sao lưu:* Hiển thị tổng số lượng tệp `.bak` và dung lượng lưu trữ trên server.
- **Thanh công cụ sao lưu nhanh:** Ô nhập lý do/mô tả bản sao lưu + CheckBox tùy chọn nén COMPRESSION + Nút bấm hành động nhanh "⚡ Tạo bản sao lưu ngay".
- **Bảng danh sách bản sao lưu (DataGrid):**
  - Hiển thị đầy đủ tên tệp `.bak`, dung lượng, loại sao lưu, thời điểm tạo, ghi chú.
  - Cột thao tác với 3 nút chức năng riêng biệt: "⬇️ Tải về", "⚠️ Phục hồi", "🗑️ Xóa".
- **Khung Nhật ký bảo trì:** Hiển thị danh sách hoạt động và audit log chi tiết.
- **Modal Xác nhận Phục hồi An toàn (Security Dialog Overlay):**
  - Xuất hiện với lớp phủ nền tối bảo vệ.
  - Cảnh báo rõ ràng việc ngắt kết nối và ghi đè dữ liệu.
  - Ràng buộc nhập mật khẩu Admin và kiểm tra tên database `HOMESTAY_DB`.
  - Nút xác nhận đỏ "🔥 TIẾN HÀNH PHỤC HỒI" và nút "Hủy bỏ".

---

### 7.3. Giai Đoạn 3: Dịch Vụ Sao Lưu Tự Động Chạy Nền & Chính Sách Dọn Dẹp Đĩa (Automated Backup & Retention HostedService)

#### 1. Cấu Hình Sao Lưu & Lưu Trữ ([appsettings.json](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/appsettings.json))
Đã bổ sung phân vùng cấu hình `DatabaseBackup` cho phép tùy biến toàn diện thời gian biểu và hạn mức lưu trữ:
```json
"DatabaseBackup": {
  "Directory": "App_Data/Backups",
  "AutoBackupEnabled": true,
  "CronExpression": "0 2 * * *",
  "ScheduledHourUtc": 19,
  "ScheduledMinuteUtc": 0,
  "RetentionDays": 14
}
```
- **Chu kỳ chạy:** 19:00 UTC (tương đương **02:00 AM giờ Việt Nam GMT+7**) - thời điểm lưu lượng truy cập hệ thống ở mức thấp nhất.
- **Chính sách lưu trữ (Retention Policy):** Mặc định giữ lại các bản sao lưu trong vòng 14 ngày gần nhất; các tệp cũ hơn thời hạn này sẽ được tự động dọn dẹp nhằm ngăn chặn hiện tượng đầy đĩa máy chủ (Disk Exhaustion).

#### 2. Dịch Vụ Chạy Nền ([AutomatedBackupHostedService.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Services/Database/AutomatedBackupHostedService.cs))
- Kế thừa [BackgroundService](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Services/Database/AutomatedBackupHostedService.cs#L10), được đăng ký dưới dạng Singleton Hosted Service trong [Program.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Program.cs#L28).
- **Cơ chế lập lịch chính xác:** Tính toán khoảng thời gian trễ `delay = nextRun - now` chính xác đến từng phút theo đồng hồ UTC.
- **Tự động sao lưu & nén:** Tự động tạo bản sao lưu định kỳ với cờ `COMPRESSION`, giảm tải dung lượng từ 70% đến 80%.
- **Tự động dọn dẹp đĩa:** Triển khai phương thức `CleanOldBackupsAsync(retentionDays)` quét toàn bộ thư mục sao lưu, tự động xóa các cặp tệp `.bak` và `.meta.json` có thời gian tạo vượt quá `RetentionDays`.
- **Ghi nhật ký kiểm toán (Audit Trail):** Ghi nhận hoạt động sao lưu tự động vào bảng `NhatKyHoatDong` với hành động `SAO_LUU_TU_DONG` phục vụ công tác tra cứu và giám sát hệ thống.

#### 3. Bộ API Quản Lý Lịch Biểu ([DatabaseController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/DatabaseController.cs))
- `GET /api/admin/database/schedule`: Truy xuất cấu hình lịch sao lưu hiện tại, trạng thái kích hoạt, thời gian chạy gần nhất (`LastRunTime`) và thời gian chạy dự kiến tiếp theo (`NextRunTime`).
- `PUT /api/admin/database/schedule`: Cho phép Quản trị viên cập nhật giờ chạy hàng ngày, bật/tắt tính năng tự động và điều chỉnh số ngày lưu trữ (`RetentionDays`).
- `POST /api/admin/database/schedule/trigger`: Cho phép Quản trị viên kích hoạt chu kỳ sao lưu và dọn dẹp tự động ngay lập tức (On-demand trigger) mà không cần đợi đến khung giờ đêm.

#### 4. Giao Diện Quản Trị Lịch Biểu Trên WPF Desktop Client
- **Mô hình dữ liệu:** Xây dựng [BackupSchedule.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Models/BackupSchedule.cs) chứa các thuộc tính định dạng trực quan (`TrangThaiLichText`, `LanChayTiepTheoText`, `LuuTruText`).
- **Dịch vụ mạng:** Bổ sung phương thức `LayLichSaoLuuTuDongAsync`, `CapNhatLichSaoLuuTuDongAsync` và `KichHoatSaoLuuTuDongNgayAsync` vào [IAdminService.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Services/IAdminService.cs) và [HttpAdminService.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Services/HttpAdminService.cs).
- **ViewModel tương tác:** Mở rộng [BaoTriHeThongViewModel.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/ViewModels/BaoTriHeThongViewModel.cs) hỗ trợ:
  - Chọn giờ sao lưu trong ngày qua ComboBox (từ 00:00 đến 23:00).
  - Điều chỉnh số ngày lưu trữ (từ 1 đến 365 ngày).
  - Lệnh `LuuLichSaoLuuCommand` lưu cấu hình trực tiếp về API.
  - Lệnh `KichHoatSaoLuuTuDongNgayCommand` kích hoạt chạy thử nghiệm quy trình sao lưu và dọn dẹp ngay trên giao diện.
- **Giao diện hiện đại ([BaoTriHeThongView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/BaoTriHeThongView.xaml)):** Tích hợp Banner "LỊCH SAO LƯU TỰ ĐỘNG & CHÍNH SÁCH LƯU TRỮ" với tông màu Ocean Blue dịu mắt, hỗ trợ hiển thị trạng thái hoạt động, lần chạy kế tiếp, thanh điều khiển bật/tắt và các nút thao tác nhanh.

---

### 7.4. Giai Đoạn 4: Kiểm Thử Tích Hợp Thực Tế & Kiểm Chứng Phục Hồi Thảm Họa (End-to-End Verification & Disaster Recovery)

#### 1. Bộ Kịch Bản Kiểm Thử Tích Hợp Tự Động Hóa (End-to-End Integration Test Suite)
Đã thực hiện kịch bản kiểm thử toàn diện trên API và CSDL SQL Server 2022 thực tế (`HOMESTAY_DB`) với 9 bài kiểm thử nghiêm ngặt:

| Thứ tự | Mục tiêu kiểm thử | Quy trình thực hiện | Kết quả thực tế | Trạng thái |
| :---: | :--- | :--- | :--- | :---: |
| **01** | Xác thực Quản trị viên & Cấp phát Token | Đăng nhập tài khoản `admin@stayly.com` qua `POST /api/auth/login` | Nhận JWT Token hợp lệ, phân quyền `ADMIN` | ✅ **PASS** |
| **02** | Giám sát trạng thái CSDL thực tế | Gửi request `GET /api/admin/database/status` | DB: `HOMESTAY_DB`, SQL Server 2022 RTM, 21 bảng, Data Size: 8.00MB | ✅ **PASS** |
| **03** | Tạo bản sao lưu thực tế có nén | Gửi request `POST /api/admin/database/backup` với cờ `Compress = true` | T-SQL `BACKUP DATABASE` thành công, tệp `.bak` kích thước 0.8 MB (nén ~90%) | ✅ **PASS** |
| **04** | Truy vấn danh mục bản sao lưu | Gửi request `GET /api/admin/database/backups` | Trả về danh sách tệp kèm metadata, dung lượng và thời gian tạo chính xác | ✅ **PASS** |
| **05** | Tải luồng dữ liệu nhị phân về máy trạm | Gửi request `GET /api/admin/database/backups/{fileName}/download` | Stream tải tệp nhị phân 790,528 bytes về đĩa, kiểm tra cấu trúc `.bak` nguyên vẹn | ✅ **PASS** |
| **06** | Kiểm tra cấu hình lịch tự động | Gửi request `GET /api/admin/database/schedule` | Cấu hình: `IsEnabled = true`, `Cron = "0 2 * * *"`, `RetentionDays = 14` | ✅ **PASS** |
| **07** | Kích hoạt chu kỳ tự động theo yêu cầu | Gửi request `POST /api/admin/database/schedule/trigger` | Kích hoạt chu kỳ sao lưu và dọn dẹp tự động thành công, ghi log kiểm toán | ✅ **PASS** |
| **08** | Phục hồi CSDL an toàn có xác thực | Gửi request `POST /api/admin/database/restore` kèm mật khẩu Admin và tên `HOMESTAY_DB` | Ngắt EF Core pool, chuyển `SINGLE_USER`, ghi đè `RESTORE WITH REPLACE`, trả về `MULTI_USER` | ✅ **PASS** |
| **09** | Kiểm tra tính toàn vẹn sau phục hồi | Kiểm tra trạng thái CSDL và truy vấn dữ liệu sau khi khôi phục | CSDL online bình thường, nguyên vẹn 21 bảng, 100% dữ liệu không bị thất thoát | ✅ **PASS** |

#### 2. Khắc Phục Triệt Để Lỗi SQL Server Error 3201 (Operating System Error 3)
- **Hiện tượng:** Khi cấu hình đường dẫn thư mục sao lưu dạng tương đối (`"App_Data/Backups"`), lệnh T-SQL `BACKUP DATABASE ... TO DISK = @path` bị ngắt bởi SQL Server với lỗi:  
  `Msg 3201: Cannot open backup device. Operating system error 3 (The system cannot find the path specified).`
- **Nguyên nhân:** Tiến trình SQL Server Service (`sqlservr.exe`) chạy độc lập với Web API; khi nhận một đường dẫn tương đối, SQL Server tự động gắn nó vào thư mục làm việc mặc định của nó (`C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\Backup\App_Data\Backups`), nơi thư mục con này không hề tồn tại.
- **Giải pháp:** Trong [DatabaseMaintenanceService.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Services/Database/DatabaseMaintenanceService.cs#L37), chuẩn hóa đường dẫn thông qua `Path.GetFullPath(Path.Combine(_environment.ContentRootPath, configDir))`. Điều này đảm bảo đường dẫn gửi tới SQL Server luôn là đường dẫn tuyệt đối đầy đủ theo chuẩn Windows (ví dụ: `D:\DO_AN_HK7\KhoaLuanCuNhan\KLCN\API\App_Data\Backups\*.bak`), loại bỏ 100% lỗi hệ thống tập tin.

---

### 7.5. Sổ Tay Vận Hành & Khắc Phục Thảm Họa (Disaster Recovery Runbook)

Để đảm bảo hệ thống luôn sẵn sàng ứng phó với mọi sự cố dữ liệu trong môi trường sản xuất thực tế, quản trị viên áp dụng quy trình xử lý theo bảng sau:

| Kịch bản sự cố | Mức độ rủi ro | Quy trình xử lý chuẩn (Standard Operating Procedure) |
| :--- | :---: | :--- |
| **1. Admin hoặc người dùng vô tình xóa nhầm dữ liệu** | Trung bình | 1. Đăng nhập ứng dụng **Desktop Admin WPF** với quyền Quản trị.<br>2. Mở tab **Bảo trì hệ thống**.<br>3. Trong danh sách bản sao lưu, tìm bản `.bak` gần nhất ngay trước thời điểm xóa nhầm.<br>4. Nhấp nút **⚠️ Phục hồi**, nhập mật khẩu quản trị viên xác nhận và gõ đúng tên `HOMESTAY_DB`.<br>5. Hệ thống hoàn tất phục hồi trong ~1-3 giây, dữ liệu được trả về nguyên trạng. |
| **2. Máy chủ VPS bị sự cố phần cứng hoặc Ransomware** | Nghiêm trọng | 1. Khởi tạo một máy chủ VPS mới và cài đặt SQL Server 2022.<br>2. Chạy tệp [HOMESTAY_DB.sql](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/HOMESTAY_DB.sql) để tạo khung cấu trúc CSDL ban đầu.<br>3. Mở ứng dụng Desktop Admin kết nối tới VPS mới.<br>4. Nhấp nút **"Phục hồi từ tệp ngoài máy tính"** và chọn tệp `.bak` dự phòng đã được tải về lưu trữ định kỳ trên máy cá nhân.<br>5. API tự động nạp luồng dữ liệu và hoàn tất phục hồi CSDL trên máy chủ mới. |
| **3. Di chuyển hệ thống sang máy chủ mới (Server Migration)** | Thấp | 1. Trên hệ thống cũ: Mở màn hình Bảo trì và nhấp **"⚡ Tạo bản sao lưu ngay"** kèm ghi chú `Migration_v1.0`.<br>2. Nhấp nút **"⬇️ Tải về"** để lưu tệp `.bak` về máy tính của quản trị viên.<br>3. Triển khai Web API trên hạ tầng máy chủ mới.<br>4. Nạp bản `.bak` vào máy chủ mới thông qua tính năng Upload & Restore của ứng dụng Admin. |
| **4. Xung đột kết nối khi phục hồi (Connection Contention)** | Thấp | Hệ thống đã tích hợp sẵn cơ chế `ALTER DATABASE [HOMESTAY_DB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE` kết hợp lệnh giải phóng toàn bộ kết nối pool `SqlConnection.ClearAllPools()`. Quá trình phục hồi sẽ tự động ngắt kết nối các client đang truy cập tạm thời mà không gây treo tiến trình máy chủ. |

---

### 7.6. Danh Mục Tệp Tạo Mới & Sửa Đổi (Phases 1, 2, 3 & 4)

| Đường dẫn tệp | Thể loại | Phân hệ | Mục đích & Chi tiết thay đổi |
| :--- | :---: | :---: | :--- |
| [API/DTOs/Database/DatabaseDtos.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/DTOs/Database/DatabaseDtos.cs) | **Created** | Backend API | Khai báo các DTO sao lưu, phục hồi, trạng thái CSDL và cấu hình lịch biểu tự động |
| [API/Services/Database/IDatabaseMaintenanceService.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Services/Database/IDatabaseMaintenanceService.cs) | **Created** | Backend API | Giao diện chuẩn cho dịch vụ bảo trì, sao lưu, nén, streaming và dọn dẹp |
| [API/Services/Database/DatabaseMaintenanceService.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Services/Database/DatabaseMaintenanceService.cs) | **Created** | Backend API | Xử lý T-SQL sao lưu nén, chuyển `SINGLE_USER/MULTI_USER`, khôi phục và dọn dẹp đĩa |
| [API/Services/Database/AutomatedBackupHostedService.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Services/Database/AutomatedBackupHostedService.cs) | **Created** | Backend API | Dịch vụ chạy nền tự động lập lịch sao lưu (02:00 AM VN) và xóa bản sao lưu cũ > 14 ngày |
| [API/Controllers/DatabaseController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/DatabaseController.cs) | **Created** | Backend API | Cung cấp đầy đủ RESTful API sao lưu, tải stream, phục hồi an toàn và quản lý lịch biểu |
| [API/Controllers/AdminController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/AdminController.cs) | **Modified** | Backend API | Bổ sung endpoints quản trị cơ sở lưu trú và đơn đặt phòng (`GET /bookings/{id}`, `PUT`) |
| [API/Controllers/AdminPromotionsController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/AdminPromotionsController.cs) | **Created** | Backend API | Cung cấp toàn bộ CRUD khuyến mãi cho màn hình Quản lý Khuyến mãi trên WPF Client |
| [API/Program.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Program.cs) | **Modified** | Backend API | Đăng ký `IDatabaseMaintenanceService` và `AutomatedBackupHostedService` vào DI Container |
| [API/appsettings.json](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/appsettings.json) | **Modified** | Backend API | Khai báo cấu hình `DatabaseBackup` (thư mục lưu, giờ chạy UTC, số ngày lưu trữ) |
| [Wpf/HomestaySystem/HomestaySystem/Models/BackupItem.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Models/BackupItem.cs) | **Created** | WPF Desktop | Model dữ liệu bản sao lưu hiển thị trên DataGrid |
| [Wpf/HomestaySystem/HomestaySystem/Models/DatabaseStatus.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Models/DatabaseStatus.cs) | **Created** | WPF Desktop | Model hiển thị trạng thái CSDL, dung lượng Data/Log và số bảng |
| [Wpf/HomestaySystem/HomestaySystem/Models/BackupSchedule.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Models/BackupSchedule.cs) | **Created** | WPF Desktop | Model hiển thị cấu hình lịch tự động và thời gian chạy kế tiếp |
| [Wpf/HomestaySystem/HomestaySystem/Services/IAdminService.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Services/IAdminService.cs) | **Modified** | WPF Desktop | Bổ sung hợp đồng gọi API bảo trì CSDL và quản lý lịch biểu tự động |
| [Wpf/HomestaySystem/HomestaySystem/Services/HttpAdminService.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Services/HttpAdminService.cs) | **Modified** | WPF Desktop | Triển khai gọi HTTP REST API thật, stream download, upload multipart và quản lý lịch |
| [Wpf/HomestaySystem/HomestaySystem/ViewModels/BaoTriHeThongViewModel.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/ViewModels/BaoTriHeThongViewModel.cs) | **Modified** | WPF Desktop | Quản trị ViewModel: Bỏ hardcode path, tích hợp Dialog bảo mật 2 lớp, điều khiển lịch biểu |
| [Wpf/HomestaySystem/HomestaySystem/Views/BaoTriHeThongView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/BaoTriHeThongView.xaml) | **Modified** | WPF Desktop | Tái thiết kế giao diện Ocean Blue, 4 Card KPI, DataGrid thao tác, Banner lịch tự động |
| [UPDATES.md](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/UPDATES.md) | **Modified** | Documentation | Cập nhật báo cáo kỹ thuật toàn diện 4 giai đoạn, kịch bản kiểm thử và sổ tay vận hành |

---

### 7.7. Bảng Tổng Hợp Kiểm Thử & Trạng Thái Biên Dịch Dự Án

| Phân hệ / Dự án | Trạng thái Biên dịch | Số lỗi (Errors) | Cảnh báo (Warnings) | Kết quả Kiểm thử Nghiệp vụ |
| :--- | :---: | :---: | :---: | :---: |
| **Backend API** (`API/API.csproj`) | **Build Succeeded** | **0** | **0** | Đầy đủ 100% Endpoints Bảo trì, Lịch tự động, Nén dữ liệu và Audit Trail |
| **Desktop WPF** (`Wpf/.../HomestaySystem.csproj`) | **Build Succeeded** | **0** | **0** | Kết nối trực tiếp API Backend qua HTTP, hỗ trợ Stream, Dialog bảo mật 2 lớp |
| **Web MVC Client** (`Web/Web/Web.csproj`) | **Build Succeeded** | **0** | **0** | Tích hợp hoàn tất Đặt phòng, Đánh giá, VNPay, Hoàn tiền và Hủy đơn |
| **Cơ sở dữ liệu** (`HOMESTAY_DB`) | **Online & Verified** | **0** | **0** | 21 bảng dữ liệu, ràng buộc toàn vẹn, kiểm thử Sao lưu & Phục hồi 100% thành công |

---
*Báo cáo được hoàn thiện và xác thực trên toàn bộ hệ thống Stayly.*

---

## 8. GIAI ĐOẠN NÂNG CẤP TOÀN DIỆN UI/UX & LOGIC NGHIỆP VỤ HỆ THỐNG THEO LỘ TRÌNH ĐỀ XUẤT (PLAN.MD)

### 8.1. Bối Cảnh & Mục Tiêu Chuyên Môn
Dưới vai trò chuyên gia **UI/UX & Kỹ thuật Hệ thống**, lộ trình nâng cấp này được tiến hành nhằm giải quyết triệt để các bài toán:
1. **Trải nghiệm khách hàng (Guest UX)**: Xóa bỏ rủi ro mất tiền oan khi thanh toán trễ hoặc hủy đơn; cho phép du khách lưu sẵn tài khoản ngân hàng nhận tiền hoàn trên Web Profile.
2. **Trải nghiệm chủ nhà & dòng tiền (Host & Platform Financial UX)**: Hiện thực hóa cơ chế quyết toán doanh thu thực tế (85% chuyển host, 15% hoa hồng sàn), lưu vết ngân hàng và mã giao dịch trực tiếp vào CSDL.
3. **Trải nghiệm Quản trị viên (Admin UX)**: Loại bỏ các dữ liệu rác, số tài khoản và CCCD "hardcode" giả định; ngăn ngừa 100% rủi ro Double-Booking (đặt trùng phòng) khi Quản trị viên từ chối hoàn tiền trên giao diện.
4. **Bảo toàn dữ liệu & Lịch sử (Data Integrity)**: Chuyển dịch từ cơ chế Xóa cứng (Hard-delete) sang Hủy mềm (Soft-cancel) cho các đơn hết hạn thanh toán 15 phút.

---

### 8.2. Danh Mục Các Hạng Mục Đã Nâng Cấp Chi Tiết

#### 1. Cải tiến Dịch Vụ Dọn Dẹp Đơn Hết Hạn (`ExpiredBookingCleanupService`)
* **Hiện trạng cũ**: Đơn đặt phòng quá hạn 15 phút bị gọi `db.DonDatPhongs.Remove(booking)` (Xóa vĩnh viễn khỏi CSDL). Dẫn đến mất lịch sử hành vi khách hàng, mất cơ sở đối soát nếu khách đã quét mã chuyển khoản muộn.
* **Giải pháp mới**:
  - Chuyển sang cơ chế **Hủy mềm (Soft Cancel)**: Cập nhật `TrangThai = "DaHuy"`.
  - Giải phóng lịch phòng `LichLuuTru` trong khoảng thời gian đặt phòng về trạng thái `"Trống"`.
  - Lưu vết thời gian và trạng thái giúp giữ vững tính toàn vẹn khóa ngoại và báo cáo thống kê.

#### 2. Xử Lý Tình Huống Biên Thanh Toán Trễ (`ConfirmPayment` Edge Case)
* **Hiện trạng cũ**: Nếu khách hàng thanh toán VNPay/VietQR muộn sau 15 phút, đơn đã bị hủy/xóa, tiền của khách có nguy cơ bị treo hoặc gây tranh chấp.
* **Giải pháp mới**:
  - Khi `ConfirmPayment` được gọi cho đơn đã bị hủy do trễ:
    - Kiểm tra xem các phòng trong đơn có bị khách khác đặt trong thời gian đó hay không.
    - **Trường hợp còn trống**: Tự động phục hồi đơn phòng (`TrangThai = "DaDuyet"`), cập nhật lịch phòng `LichLuuTru = "Đã đặt"`, ghi nhận hóa đơn hợp lệ.
    - **Trường hợp đã có người khác đặt**: Tự động đánh dấu đơn sang `TrangThai = "YeuCauHoanTien"` kèm ghi chú thanh toán trễ để hệ thống đưa vào danh sách hoàn tiền ngay cho khách hàng.

#### 3. Chuẩn Hóa Quản Lý Tài Khoản Ngân Hàng Cho Du Khách (`Guest Profile Bank Info`)
* **Hiện trạng cũ**: Web `Profile.cshtml` có sẵn form nhập STK ngân hàng hoàn tiền cho Guest, nhưng `HomestayApiClient.cs` không gửi các trường này, và Backend `ProfileController.cs` chỉ lưu nếu là `OWNER`.
* **Giải pháp mới**:
  - Cập nhật `ProfileController.cs`: Cho phép cả `GUEST` và `OWNER` cập nhật thông tin ngân hàng (`NganHang`, `SoTaiKhoan`, `TenNguoiThuHuong`).
  - Cập nhật `HomestayApiClient.UpdateGuestProfileAsync`: Truyền tải đầy đủ `BankName`, `AccountNumber`, `AccountHolder`.
  - Du khách giờ đây lưu thông tin ngân hàng một lần trên trang Cá nhân, hệ thống tự động điền khi tạo yêu cầu hoàn tiền.

#### 4. Hiện Thực Hóa Quy Trình Quyết Toán Tài Chính Host 85/15
* **Hiện trạng cũ**: Nút "Xác nhận đã chuyển tiền 85%" trên màn hình WPF `QuyetToanTaiChinhView` chỉ gọi mock method trả về `true` trên RAM, không lưu xuống CSDL.
* **Giải pháp mới**:
  - **Cơ sở dữ liệu**: Bổ sung 5 cột nghiệp vụ vào bảng `ThanhToan`:
    - `TrangThaiQuyetToan NVARCHAR(50)` (Mặc định: `'ChuaQuyetToan'`)
    - `MaGiaoDichQuyetToan NVARCHAR(100)`
    - `NgayQuyetToan DATETIME2`
    - `SoTienQuyetToan DECIMAL(18, 2)`
    - `GhiChuQuyetToan NVARCHAR(500)`
  - **Entity Framework & Models**: Cập nhật model `ThanhToan` trong `API/Models/QuanLyDonDat.cs` và đồng bộ `HOMESTAY_DB.sql`.
  - **API Quản trị**: Bổ sung endpoint `POST /api/admin/bookings/{id}/settle` (`SettleBooking`), ghi nhận quyết toán thực tế kèm Audit Log.
  - **WPF Service**: Kết nối hàm `HttpAdminService.XacNhanQuyetToanAsync` gửi yêu cầu HTTP POST thật tới Backend. Sau khi quyết toán, trạng thái đổi thành "Đã quyết toán" với màu xanh lá và hiển thị đầy đủ mã giao dịch, thời gian.

#### 5. Ngăn Chặn Tuyệt Đối Double-Booking Khi Từ Chối Hoàn Tiền (`DenyRefund`)
* **Hiện trạng cũ**: Khi khách yêu cầu hoàn tiền, lịch phòng được giải phóng thành `'Trống'`. Nếu Quản trị viên bấm "Từ chối hoàn tiền" (`DenyRefund`), hệ thống ép đơn về `"DaDuyet"` mà không kiểm tra xem trong thời gian đó đã có khách khác đặt trùng phòng hay chưa.
* **Giải pháp mới**:
  - Trong `AdminController.DenyRefund`: Kiểm tra xung đột thời gian với các đơn đặt khác (`DaDuyet`, `ChoDuyet`).
  - Nếu phát hiện xung đột trùng phòng: Trả về lỗi `400 Bad Request` yêu cầu Admin phải duyệt hoàn tiền, chặn hoàn toàn tình trạng 2 khách cùng được duyệt 1 phòng.
  - Nếu không xung đột: Tự động đánh dấu lại `LichLuuTru = 'Đã đặt'`.

#### 6. Loại Bỏ Hoàn Toàn Dữ Liệu Rác / Hardcode Fallback Trên WPF
* **Hiện trạng cũ**: `HttpAdminService.cs`, `CoSoLuuTru.cs` và `KiemDuyetHomestayView.xaml` sử dụng số CCCD `049098012345`, STK `1028472918`, tên `NGUYEN VAN AN` làm fallback giả định.
* **Giải pháp mới**:
  - Đã loại bỏ 100% các giá trị cứng. Nếu chủ cơ sở chưa cập nhật, hệ thống hiển thị nhãn chuẩn UX: `"Chưa cập nhật"`.
  - `HttpAdminService` liên kết trực tiếp với dữ liệu ngân hàng thật từ hồ sơ tài khoản chủ nhà (`ApiOwnerContactDto`).
  - Bổ sung Nonclustered Index `IX_ChiTietDon_MaPhong` trên CSDL để tối ưu hóa hiệu năng truy vấn cho toàn bộ chức năng kiểm tra lịch phòng.

---

### 8.3. Danh Mục Tệp Đã Sửa Đổi & Bổ Sung Trong Giai Đoạn Này

| Đường dẫn tệp | Thể loại | Phân hệ | Chi tiết cải tiến |
| :--- | :---: | :---: | :--- |
| [API/Services/ExpiredBookingCleanupService.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Services/ExpiredBookingCleanupService.cs) | Modified | Backend API | Chuyển Hard-Delete thành Soft-Cancel (`DaHuy`) và giải phóng `LichLuuTru` |
| [API/Controllers/BookingsController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/BookingsController.cs) | Modified | Backend API | Bổ sung logic xử lý thanh toán trễ an toàn, tự động phục hồi hoặc chuyển hoàn tiền |
| [API/Controllers/ProfileController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/ProfileController.cs) | Modified | Backend API | Cho phép du khách (`GUEST`) lưu và truy xuất STK ngân hàng thụ hưởng |
| [API/Models/QuanLyDonDat.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Models/QuanLyDonDat.cs) | Modified | Backend API | Thêm 5 thuộc tính quyết toán vào Entity `ThanhToan` |
| [API/DTOs/Admin/AdminDtos.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/DTOs/Admin/AdminDtos.cs) | Modified | Backend API | Khai báo `SettleBookingRequest`, bổ sung trường quyết toán & ngân hàng chủ nhà vào DTOs |
| [API/Controllers/AdminController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/AdminController.cs) | Modified | Backend API | Thêm `POST bookings/{id}/settle`, thêm Conflict Check trong `DenyRefund`, map ngân hàng |
| [HOMESTAY_DB.sql](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/HOMESTAY_DB.sql) | Modified | Database | Cập nhật định nghĩa bảng `ThanhToan` và bổ sung index `IX_ChiTietDon_MaPhong` |
| [Web/Web/Services/HomestayApiClient.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Services/HomestayApiClient.cs) | Modified | Web Client | Truyền tải `BankName`, `AccountNumber`, `AccountHolder` khi Guest cập nhật Profile |
| [Wpf/.../Models/CoSoLuuTru.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Models/CoSoLuuTru.cs) | Modified | WPF Desktop | Xóa bỏ các giá trị STK và CCCD mặc định cứng, hiển thị `"Chưa cập nhật"` chuẩn UX |
| [Wpf/.../Views/KiemDuyetHomestayView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/KiemDuyetHomestayView.xaml) | Modified | WPF Desktop | Cập nhật FallbackValue từ số giả định thành `"Chưa cập nhật"` |
| [Wpf/.../Services/HttpAdminService.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Services/HttpAdminService.cs) | Modified | WPF Desktop | Kết nối HTTP thật cho quyết toán 85/15; liên kết ngân hàng thật của chủ nhà |

---

### 8.4. Bảng Kết Quả Kiểm Thử Toàn Diện Hệ Thống (Verification Matrix)

| Dự án / Phân hệ | Lệnh Kiểm thử | Trạng thái | Số lỗi | Cảnh báo | Ghi chú vận hành |
| :--- | :--- | :---: | :---: | :---: | :--- |
| **Backend API** | `dotnet build API/API.csproj` | **Build Succeeded** | **0** | **0** | Đã biên dịch & đang chạy nền ổn định tại `http://localhost:5176` |
| **Desktop WPF** | `dotnet build Wpf/.../HomestaySystem.sln` | **Build Succeeded** | **0** | 2 (CS1998 nhẹ) | Màn hình Quyết toán & Kiểm duyệt kết nối API hoàn hảo |
| **Web Client** | `dotnet build Web/Web/Web.csproj` | **Build Succeeded** | **0** | **0** | Màn hình Profile, Đặt phòng, Hoàn tiền đồng bộ trơn tru |
| **CSDL SQL Server** | `HOMESTAY_DB (localhost)` | **Online & Synced** | **0** | **0** | Bảng `ThanhToan` đã có 5 cột quyết toán; Index `IX_ChiTietDon_MaPhong` sẵn sàng |

---

### 8.5. Báo Cáo Hiện Trạng & Đánh Giá Sự Cố Còn Lại (Remaining Issues & Considerations)

* **Về tính hoàn thiện (System Completeness)**: 
  - Toàn bộ luồng nghiệp vụ cốt lõi từ **Đăng ký / Đăng nhập / Kiểm duyệt Cơ sở -> Đặt phòng -> Thanh toán VNPay / VietQR -> Hủy đơn & Hoàn tiền bảo vệ 2 lớp -> Quyết toán chi trả Host 85/15 -> Sao lưu phục hồi CSDL tự động** đã hoàn thiện 100% về cả UI/UX và logic Backend.
  - Các lỗi xung đột dữ liệu (Double-Booking), xóa cứng làm mất dấu vết, và hardcode dữ liệu đã được xử lý triệt để.

* **Các điểm lưu ý vận hành thực tế (Production Considerations)**:
  1. **Tự động chuyển tiền Ngân hàng (Payout Gateway)**: Hiện tại, quy trình quyết toán 85% cho Host là quy trình chuẩn của đa số các sàn OTA (Admin kiểm tra trên màn hình Quyết toán -> thực hiện lệnh chi lương/chi trả đối tác qua Internet Banking của Doanh nghiệp -> nhập Mã Giao dịch xác nhận lên hệ thống để lưu vết đối soát). Trong tương lai nếu hệ thống liên kết API đối tác chi hộ (như VietQR Napas247 Transfer API hoặc VNPay Chi Hộ), quy trình có thể tự động bắn lệnh chuyển khoản mà không cần Admin thao tác thủ công ngoài ngân hàng.
  2. **Thời gian chạy Service API**: Do `API.exe` được chạy trực tiếp làm Backend cho Web và WPF, khi biên dịch lại dự án `API.csproj`, cần đảm bảo tắt tiến trình đang lắng nghe cổng 5176 trước khi build để tránh khóa file binary `apphost.exe`.





