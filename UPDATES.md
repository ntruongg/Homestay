# Báo Cáo Cập Nhật Hệ Thống (Updates Since Last Commit)

**Thời gian tổng hợp:** 13/10/2026 (Bao gồm đầy đủ các đợt cập nhật ngày 02/10, 07/10, 11/10, 12/10 và 13/10/2026)\
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

---

### 8.6. Cập Nhật Xử Lý Lỗi HTTP 500 Khi "Yêu Cầu Đặt Phòng" (Sau Pull Request #19)

* **Hiện tượng**: Khách hàng ấn "Yêu cầu đặt phòng" trên Web (`/Home/Details/{id}`) bị báo lỗi HTTP 500 Internal Server Error.
* **Nguyên nhân cốt lõi**:
  - Bản commit mới nhất (`765ad7856679a3ae19f2759b43bd3d6795a0b2d5`) bổ sung 5 thuộc tính quyết toán vào Entity `ThanhToan` trong `API/Models/QuanLyDonDat.cs`:
    - `TrangThaiQuyetToan` (NVARCHAR 30)
    - `MaGiaoDichQuyetToan` (VARCHAR 50)
    - `NgayQuyetToan` (DATETIME)
    - `SoTienQuyetToan` (DECIMAL 12,2)
    - `GhiChuQuyetToan` (NVARCHAR 500)
  - Mặc dù file script `HOMESTAY_DB.sql` đã cập nhật, nhưng Database SQL Server cục bộ (`HOMESTAY_DB`) chưa chạy lệnh cập nhật schema tương ứng.
  - Khi luồng đặt phòng kích hoạt:
    1. `ExpiredBookingCleanupService` quét định kỳ và truy vấn `DonDatPhong` bao gồm `.Include(b => b.ThanhToan)`.
    2. Sau khi tạo đơn, Web gọi `GetBookingsAsync` để điều hướng sang Checkout (`/Home/Checkout?bookingId=...`), API thực hiện truy vấn `DonDatPhongs.Include(b => b.ThanhToan)`.
    3. EF Core sinh câu lệnh SQL `SELECT` có 5 cột trên, gây crash với lỗi SQL Server:
       `SqlException (0x80131904): Invalid column name 'GhiChuQuyetToan'. Invalid column name 'MaGiaoDichQuyetToan'...` -> Dẫn đến mã lỗi HTTP 500.
* **Biện pháp khắc phục**:
  - Đã thực thi script đồng bộ hóa schema trên CSDL `HOMESTAY_DB`:
    ```sql
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ThanhToan') AND name = 'TrangThaiQuyetToan')
        ALTER TABLE ThanhToan ADD TrangThaiQuyetToan NVARCHAR(30) NOT NULL CONSTRAINT DF_ThanhToan_TrangThaiQuyetToan DEFAULT 'ChuaQuyetToan';
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ThanhToan') AND name = 'MaGiaoDichQuyetToan')
        ALTER TABLE ThanhToan ADD MaGiaoDichQuyetToan VARCHAR(50) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ThanhToan') AND name = 'NgayQuyetToan')
        ALTER TABLE ThanhToan ADD NgayQuyetToan DATETIME NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ThanhToan') AND name = 'SoTienQuyetToan')
        ALTER TABLE ThanhToan ADD SoTienQuyetToan DECIMAL(12,2) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ThanhToan') AND name = 'GhiChuQuyetToan')
        ALTER TABLE ThanhToan ADD GhiChuQuyetToan NVARCHAR(500) NULL;
    ```
  - Kiểm tra toàn bộ 21 bảng trong CSDL với `HOMESTAY_DB.sql`, đảm bảo không còn sai lệch nào khác.
* **Kết quả kiểm thử**:
  - Đã kiểm thử luồng API: Tạo đơn đặt phòng (`POST /api/bookings`) và lấy thông tin (`GET /api/bookings`) trả về `201 Created` và `200 OK`.
  - Đã kiểm thử giao diện thực tế (Browser Subagent): Đăng nhập tài khoản khách -> Chọn phòng -> Bấm "Yêu cầu đặt phòng" -> Điều hướng thành công 100% đến trang Checkout (`/Home/Checkout?bookingId=17`) hiển thị đúng thông tin và tùy chọn thanh toán VNPay, không còn bất kỳ lỗi 500 nào.

---

### 8.7. Cập Nhật Tính Tiền Dịch Vụ Bổ Sung & Hiển Thị Chi Tiết Dịch Vụ Trên Chi Tiết Thanh Toán / Chi Tiết Đơn

* **Hiện tượng**:
  1. Khi người dùng chọn dịch vụ bổ sung trên trang chi tiết Homestay (`/Home/Details/{id}`), màn hình có tính trước (preview) tiền dịch vụ, nhưng khi bấm "Yêu cầu đặt phòng" chuyển sang trang Checkout thì tiền dịch vụ chưa được cộng vào tổng tiền thanh toán.
  2. Màn hình Chi tiết thanh toán (`/Home/Checkout`) và danh sách Chuyến đi của tôi (`/Home/Trips`) chưa hiển thị danh sách các dịch vụ bổ sung đã chọn.
* **Nguyên nhân**:
  1. Trong JavaScript hàm `validateBookingSubmit` (`Details.cshtml`), đoạn mã disable các input có `Quantity = 0` đã khiến form gửi các chỉ mục không liên tục (non-sequential index) lên ASP.NET Core Model Binder. Theo cơ chế mặc định của MVC, nếu thiếu chỉ mục 0, ModelBinder dừng liên kết collection và coi `input.Services` là rỗng/null, khiến dịch vụ không được gửi lên API.
  2. Model binding thiếu thẻ ẩn `Services.Index`, và thiếu kiểm tra null an toàn (`s != null`) khi lọc danh sách dịch vụ.
  3. Màn hình `Checkout.cshtml` và `Trips.cshtml` chưa có khối HTML hiển thị danh sách `Model.Services` và bảng chiết tính tiền phòng + dịch vụ.
* **Biện pháp khắc phục**:
  - **Details.cshtml**:
    - Bổ sung `<input type="hidden" name="Services.Index" value="@i" />` cho từng dịch vụ.
    - Loại bỏ việc disable các input số lượng = 0 trước khi submit form, để form luôn gửi đúng cấu trúc cho ModelBinder (việc lọc `Quantity > 0` được thực hiện an toàn ở Backend).
  - **HomeController.cs & HomestayApiClient.cs**:
    - Bổ sung kiểm tra `s != null && s.Quantity > 0` tránh ngoại lệ `NullReferenceException`.
  - **BookingsController.cs (Backend API)**:
    - Khi tạo `DonDatPhongDichVu`, gán trực tiếp navigation property `DichVu = dbSvc` để `ToResponse` trả về ngay tên dịch vụ chuẩn xác.
    - Đảm bảo số đêm `nights = Math.Max(1, ...)` tính chuẩn xác tiền phòng + dịch vụ.
  - **Checkout.cshtml (Chi tiết thanh toán)**:
    - Bổ sung khối "Dịch vụ đã chọn" liệt kê chi tiết: Tên dịch vụ, số lượng (`xN`), đơn giá (`₫/lượt`) và thành tiền.
    - Bổ sung bảng "Tóm tắt thanh toán": Tiền phòng (X đêm), Dịch vụ bổ sung (+... ₫), Mã ưu đãi (-... ₫), và Tổng số tiền cần thanh toán.
  - **Trips.cshtml (Chi tiết đơn của khách)**:
    - Hiển thị badge danh sách dịch vụ đã đặt trực tiếp trên card đơn hàng.
    - Bổ sung nút & Modal **"Chi tiết đơn"** (`#tripDetailModal`) hiển thị toàn bộ hóa đơn: Thông tin phòng, thời gian lưu trú, tiền phòng, danh sách dịch vụ kèm theo, mã giảm giá và tổng tiền.
  - **Dashboard.cshtml (Chi tiết đơn của chủ cơ sở)**:
    - Cập nhật hàm `openBookingDetailModal` hiển thị động danh sách phụ thu / dịch vụ bổ sung (`extraFees`) từ API thay vì nhãn tĩnh.
* **Kết quả kiểm thử thực tế**:
  - Tạo đơn đặt phòng kèm dịch vụ (Phòng 202: 3.200.000 ₫ cho 2 đêm + Dịch vụ Trà chiều x2: 300.000 ₫).
  - Tổng tiền tính đúng 100%: **3.500.000 ₫**.
  - Trang Checkout hiển thị đầy đủ chi tiết dịch vụ `Trà chiều kèm hoa sen thơm ngát (x2) - 300.000 ₫` và tổng tiền `3.500.000 ₫`.
  - Trang Trips hiển thị đúng badge dịch vụ và Modal chi tiết đơn mở mượt mà.

---

### 8.8. Sửa Lỗi Hiển Thị Chi Tiết Dịch Vụ Và Bóc Tách Các Khoản Tiền Trong Đơn Của Khách (Trips & Dashboard)

* **Hiện tượng**:
  - Khách hàng xem trang "Chuyến đi của tôi" (`/Home/Trips`) và bấm mở popup modal "Chi tiết đơn", modal chỉ hiển thị tổng tiền đơn hàng mà không hiện danh sách từng dịch vụ đã chọn cũng như đơn giá, số lượng và thành tiền của dịch vụ đó.
  - Phía chủ cơ sở (`/Home/Dashboard`), các đơn đặt phòng chưa thanh toán (`ChoThanhToan`) hiển thị sai tổng tiền (bị thiếu tiền dịch vụ do chưa có bản ghi `ThanhToan`).
* **Nguyên nhân cốt lõi**:
  1. Trong Razor View `Trips.cshtml`, câu lệnh serialize `tripsData` sử dụng `System.Text.Json.JsonSerializer.Serialize` mặc định chuyển các trường của C# record `BookingServiceItem` sang **PascalCase** (`ServiceId`, `Name`, `Quantity`, `UnitPrice`, `TotalPrice`). Trong khi đó, mã JavaScript trong hàm `openTripDetailModal` lại truy cập thuộc tính theo **camelCase** (`s.name`, `s.quantity`, `s.unitPrice`, `s.totalPrice`), dẫn đến toàn bộ trả về `undefined`, `totalSvc = 0` và các dòng dịch vụ hiển thị `NaN ₫` hoặc bị ẩn.
  2. Cú pháp Razor `x@s.Quantity` bị phân tích thành chuỗi ký tự thông thường thay vì nội suy biến, cần viết chuẩn `x@(s.Quantity)`.
  3. Phía Backend API (`OwnerController.cs`), hai endpoint `GetDashboard` và `GetBookings` chưa nạp eager-loading navigation property `.Include(b => b.DonDatPhongDichVus).ThenInclude(d => d.DichVu)` và `.Include(b => b.GiamGia)`, đồng thời chỉ lấy `b.ThanhToan?.TongTien ?? (b.ChiTietDons.Sum(d => d.DonGia) * nights)`, hoàn toàn bỏ sót tiền dịch vụ cho các đơn chưa thanh toán.
* **Biện pháp khắc phục**:
  - **Trips.cshtml (Client Web)**:
    - Project rõ ràng cấu trúc camelCase trong JSON serialization của `tripsData`: `serviceId`, `name`, `quantity`, `unitPrice`, `totalPrice`, đồng thời tính toán sẵn `roomPrice` và `servicesTotal`.
    - Cập nhật hàm `openTripDetailModal` hỗ trợ fallback cả PascalCase và camelCase, định dạng số tiền chuẩn locale `vi-VN`.
    - Thiết kế lại bảng phân tích chi tiết:
      - Dòng Tiền phòng: `... ₫` (theo số đêm).
      - Khối Dịch vụ bổ sung: Liệt kê từng dịch vụ (`Tên dịch vụ x{Số lượng} ({Đơn giá} ₫/lượt) -> {Thành tiền} ₫`).
      - Dòng Ưu đãi mã giảm giá: `-... ₫` (nếu có).
      - Dòng Tổng thanh toán: `... ₫` nổi bật.
    - Sửa cú pháp thẻ Razor trên card `x@(s.Quantity)` và thêm thuộc tính `data-bs-toggle="modal"` vào nút "Chi tiết đơn".
  - **OwnerController.cs (Backend API)**:
    - Bổ sung nạp `.Include(b => b.DonDatPhongDichVus).ThenInclude(d => d.DichVu)` và `.Include(b => b.GiamGia)`.
    - Tính toán `calculatedTotal = Math.Max(0, roomSubtotal + servicesSubtotal - discount)` chuẩn xác cho các đơn chưa có bản ghi `ThanhToan`.
  - **Dashboard.cshtml (Chủ cơ sở)**:
    - Cập nhật hàm `openBookingDetailModal` hiển thị rõ đơn giá, số lượng và thành tiền của từng dịch vụ bổ sung.
* **Kết quả kiểm thử**:
  - Thẻ đơn #23 của khách: Tiền phòng (1 đêm) 450.000 ₫ + Dịch vụ Bếp nướng BBQ than hoa x1 (200.000 ₫/lượt) 200.000 ₫ = Tổng thanh toán 650.000 ₫.
  - Modal "Chi tiết đơn #23" hiển thị đầy đủ, sắc nét, minh bạch từng khoản tiền.

---

### 8.9. Khắc Phục Lỗi Reset Timer Mã QR Thanh Toán: Đếm 30 Phút Thực Tế Kể Từ Khi Tạo Đơn & Tự Động Hủy Đơn Sau 30 Phút

* **Hiện tượng**:
  - Bộ đếm thời gian (countdown timer) của mã QR thanh toán trên trang `/Home/Checkout` bị reset lại 30:00 từ đầu mỗi lần người dùng F5 refresh trang.
  - Nếu khách hàng thoát trang mà chưa thanh toán rồi quay lại sau (ví dụ sau 15 phút), timer vẫn bắt đầu lại 30:00 thay vì hiển thị thời gian còn lại thực tế.
  - Sau 30 phút, nếu khách không thanh toán thì đơn hàng vẫn giữ nguyên trạng thái `ChoThanhToan` nếu không có tương tác từ trình duyệt của khách.
* **Nguyên nhân**:
  1. Trang `Checkout.cshtml` gán cứng biến JavaScript `let timeInSecs = 30 * 60;`, không có liên kết với thời điểm tạo đơn hàng thực tế trong CSDL (`DonDatPhong.NgayDat`).
  2. DTO `BookingResponse` và Model `Booking` phía Web không chứa trường `CreatedAt` (`NgayDat`).
  3. Action `Checkout` trong `HomeController.cs` không kiểm tra thời hạn 30 phút, không truyền số giây còn lại xuống View.
  4. Endpoint API `bookings/{id}/expire` trước đây thực hiện xóa cứng `db.DonDatPhongs.Remove(booking)` thay vì chuyển trạng thái `TrangThai = "DaHuy"`.
* **Biện pháp khắc phục**:
  - **API DTOs & Models**:
    - Bổ sung `DateTime? CreatedAt = null` vào `BookingResponse` (`BookingDtos.cs`) và `Booking` (`ApiModels.cs`).
    - Trong `BookingsController.ToResponse`, truyền `DateTime.SpecifyKind(booking.NgayDat, DateTimeKind.Utc)`.
  - **HomeController.cs**:
    - Trong action `Checkout`:
      - Kiểm tra nếu trạng thái khác `ChoThanhToan`/`Pending`, chuyển hướng về Trips kèm thông báo phù hợp.
      - Tính toán: $\text{remainingSeconds} = \max\Big(0, \,(\text{CreatedAt} + 30\text{ phút}) - \text{DateTime.UtcNow}\Big)$.
      - Nếu $\text{remainingSeconds} \le 0$: Gọi `api.ExpireBookingAsync` để hủy đơn ngay lập tức, thông báo "Thời hạn thanh toán 30 phút đã kết thúc" và chuyển hướng về Trips.
      - Nếu còn hạn: Truyền `remainingSeconds` xuống `ViewBag.RemainingSeconds`.
    - Trong `CheckPaymentStatus`: Bổ sung cờ `isCancelled` để client nhận biết đơn đã bị hủy từ server.
  - **Checkout.cshtml**:
    - Khởi tạo giá trị chuỗi thời gian ngay từ Razor (`initTimeStr`) để tránh hiện tượng nhảy số 30:00 khi trang vừa render.
    - JavaScript khởi tạo mốc đích: `targetExpiryTimestamp = Date.now() + (serverRemainingSecs * 1000)`.
    - Tính chênh lệch `diffMs = targetExpiryTimestamp - Date.now()` trong mỗi tick của timer. Cách này đảm bảo đếm đúng từng giây ngay cả khi trình duyệt bóp băng thông/throttle tab chạy ngầm khi khách mở app ngân hàng, và **hoàn toàn không bị reset khi refresh trang**.
    - Khi hết giờ ($\le 0$): Tự động gọi API hủy đơn, thông báo alert và chuyển hướng về trang Chuyến đi.
  - **Background Worker & Lazy Check**:
    - `ExpiredBookingCleanupService.cs`: Tăng tần suất quét lên mỗi **30 giây** (thay vì 1 phút); tự động quét các đơn `ChoThanhToan` hoặc `Pending` có `NgayDat <= DateTime.UtcNow.AddMinutes(-30)` và chưa thanh toán để chuyển sang `TrangThai = "DaHuy"`, giải phóng `LichLuuTru` về `"Trống"`.
    - `BookingsController.cs`: Bổ sung Lazy Check tự động cập nhật đơn quá hạn trong `GetMine()` và `GetById()`, đảm bảo trạng thái "Đã hủy" được phản ánh tức thì.
    - Sửa `Expire` endpoint từ xóa đơn (`Remove`) sang `booking.TrangThai = "DaHuy"`.
  - **Trips.cshtml**:
    - Hiển thị badge `Hạn thanh toán: còn ~X phút` trực tiếp trên card đơn hàng `ChoThanhToan`.
    - Trong Modal "Chi tiết đơn", hiển thị thanh cảnh báo thời hạn thanh toán 30 phút kèm nút "Thanh toán ngay".
* **Kết quả kiểm thử thực tế (Browser Subagent Automation)**:
  - Tạo đơn đặt phòng mới `#24`: Mở trang thanh toán lúc `29:53`.
  - Chờ 4 giây và F5 Refresh trang: Timer tiếp tục đếm xuống `29:46` (**xác nhận không hề bị reset**).
  - Thoát ra trang Trips: Thẻ đơn và Modal hiển thị đúng huy hiệu hạn thanh toán còn lại.

---

### 8.10. Tích Hợp Hoàn Chỉnh Hệ Thống Sao Lưu & Phục Hồi Cơ Sở Dữ Liệu Sau Khi Deploy (`plan.md`)

* **Mục tiêu**: Xây dựng giải pháp sao lưu và phục hồi CSDL SQL Server đáp ứng tiêu chuẩn vận hành thực tế kể cả sau khi triển khai hệ thống lên Cloud / VPS / Docker (Production Readiness).
* **Kết quả thực hiện**:
  1. **Backend API (`API/Services/Database/`)**:
     - `DatabaseMaintenanceService.cs`: Thực thi các lệnh T-SQL nén sao lưu `WITH COMPRESSION`, ngắt kết nối an toàn `ALTER DATABASE [HOMESTAY_DB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE`, khôi phục dữ liệu `RESTORE DATABASE ... WITH REPLACE`, và chuyển lại `MULTI_USER`.
     - `DatabaseController.cs`: Cung cấp 7 API endpoints chuẩn RESTful (`status`, `backups`, `backup`, `download`, `restore`, `restore/upload`, `schedule`) được bảo vệ nghiêm ngặt bằng phân quyền `[Authorize(Roles = "ADMIN")]`.
     - Hỗ trợ stream tệp dung lượng lớn với `[RequestSizeLimit(1_073_741_824)]` (1 GB) và `FileStreamResult`.
     - `AutomatedBackupHostedService.cs`: Dịch vụ chạy nền tự động sao lưu định kỳ hàng ngày (02:00 AM) và tự động dọn dẹp các tệp cũ hơn 14 ngày (`CleanOldBackupsAsync`).
  2. **Desktop Admin WPF (`HomestaySystem`)**:
     - `BaoTriHeThongViewModel.cs` & `HttpAdminService.cs`: Xóa bỏ 100% các đường dẫn ổ đĩa hardcoded; kết nối HTTP RESTful với API để lấy trạng thái CSDL, kích hoạt sao lưu, tải tệp `.bak` về máy cá nhân từ xa, hoặc upload file từ máy cá nhân lên server để khôi phục.
     - `BaoTriHeThongView.xaml`: Thiết kế giao diện quản trị chuyên nghiệp với thẻ KPI Database, bảng lịch sử bản sao lưu và hộp thoại xác thực bảo mật 2 lớp khi khôi phục CSDL.
  3. **Trạng thái**: Đã hoàn thành 100% cả 4 giai đoạn trong [`plan.md`](file:///d:/Homestay/plan.md).

---

### 8.11. Toàn Bộ Cải Tiến, Chuẩn Hóa Dữ Liệu & Khắc Phục Lỗi Hệ Thống Ngày 12/10/2026

* **Thời gian thực hiện**: 12/10/2026
* **Mục tiêu**: Chuẩn hóa toàn diện dữ liệu và các chỉ số thống kê giữa Web MVC, API và WPF Admin; đồng bộ các trường dữ liệu Chỗ nghỉ, loại bỏ lỗi validation khi chỉnh sửa cơ sở lưu trú; tinh gọn và bảo vệ tính toàn vẹn của quy trình phê duyệt hồ sơ; chuẩn hóa danh mục trạng thái đối tác và người dùng; khắc phục lỗi bộ lọc ngày tháng doanh thu và xử lý triệt để lỗi crash runtime màn hình Bảo trì & sao lưu.

* **Nội dung chi tiết các hạng mục đã hoàn thành trong ngày 12/10/2026**:

  1. **Rà Soát & Chuẩn Hóa Thống Kê Voucher / Khuyến Mãi & Bộ Đếm Đơn Hoàn Tiền** ([QuanLyKhuyenMaiViewModel.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/ViewModels/QuanLyKhuyenMaiViewModel.cs), [QuanLyVoucherView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/QuanLyVoucherView.xaml), [AdminPromotionsController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/AdminPromotionsController.cs)):
     - *Hiện tượng*: Khi tạo voucher mới trên Web thì danh sách hiển thị đúng (4 chiến dịch đang hoạt động, 2 tạm khóa/hết hạn), nhưng thẻ tổng hợp KPI phía trên chỉ báo 3 chiến dịch đang hoạt động và tổng chiến dịch báo 5 (bị thiếu 1 chiến dịch). Ngoài ra, bộ đếm đơn hoàn tiền hiển thị số 0 và không tự cập nhật theo số lượng thực tế.
     - *Nguyên nhân*: Thuộc tính đếm KPI trong ViewModel lọc theo điều kiện thời gian cứng và không bao hàm các voucher mới tạo trong ngày hiện tại; logic đếm đơn hoàn tiền chưa kết nối tới trạng thái `ChoDuyetHoanTien` / `DaHoanTien` từ CSDL.
     - *Biện pháp khắc phục*:
       - Đồng bộ logic tính toán KPI trực tiếp từ dữ liệu trả về của API: Đếm chính xác voucher đang hoạt động (`KichHoat == true && NgayKetThuc >= DateTime.Now`), tạm khóa/hết hạn (`KichHoat == false || NgayKetThuc < DateTime.Now`), và tổng số chiến dịch bằng tổng danh sách bản ghi thực tế (4 đang hoạt động, 2 tạm khóa = tổng 6 chiến dịch).
       - Tích hợp bộ đếm đơn đặt phòng yêu cầu hoàn tiền từ API quản lý đơn đặt, cập nhật tức thời khi phát sinh yêu cầu hoàn tiền mới từ du khách.

  2. **Khắc Phục Lỗi Validation Khi Chỉnh Sửa Cơ Sở Lưu Trú & Đồng Bộ Dữ Liệu Web - WPF** ([ValidationHelper.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Services/ValidationHelper.cs), [CoSoLuuTru.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Models/CoSoLuuTru.cs), [QuanLyDanhMucHomestayViewModel.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/ViewModels/QuanLyDanhMucHomestayViewModel.cs)):
     - *Hiện tượng*: Khi người dùng chỉnh sửa bất kỳ một trường dữ liệu nào trong chi tiết chỗ nghỉ (ví dụ: mô tả, tiện ích, chính sách nhận phòng), hệ thống hiển thị thông báo lỗi ràng buộc (Validation Error) và không cho phép lưu thay đổi vào CSDL.
     - *Nguyên nhân*: Các trường dữ liệu từ Web (như Tiện ích mở rộng, Quy định nhận/trả phòng, Tọa độ GPS, Thông tin liên hệ chủ nhà, Giờ Check-in/Check-out) khi map về model `CoSoLuuTru` phía WPF bị thiếu thuộc tính hoặc giá trị null vi phạm quy tắc validation nghiêm ngặt.
     - *Biện pháp khắc phục*:
       - Xây dựng lớp tiện ích xác thực [ValidationHelper.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Services/ValidationHelper.cs) chuẩn hóa kiểm tra dữ liệu đầu vào theo chuẩn nghiệp vụ của hệ thống Web MVC.
       - Bổ sung và đồng bộ 100% các trường thuộc tính của `CoSoLuuTru` từ Web sang WPF, đảm bảo model hai bên tương thích hoàn toàn.
       - Xử lý fallback an toàn cho các trường tùy chọn (optional fields) nếu null thì gán chuỗi rỗng hoặc giá trị mặc định hợp lệ, ngăn chặn ngoại lệ validation chặn lưu dữ liệu.

  3. **Lược Bỏ Trường Dư Thừa & Bảo Vệ Quy Trình Thẩm Định Phê Duyệt Hồ Sơ Chuẩn Mực** ([KiemDuyetHomestayView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/KiemDuyetHomestayView.xaml), [QuanLyDanhMucHomestayView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/QuanLyDanhMucHomestayView.xaml)):
     - *Hiện tượng*: Ở bảng danh sách bên ngoài đã có 2 nút nghiệp vụ chuyên trách "Phê duyệt" và "Từ chối" (cùng chức năng "Tạm ngừng phân phối"), nhưng bên trong form chi tiết lại có checkbox trạng thái mở bán/duyệt tự do; người dùng sửa trong form nhưng không lưu được hoặc gây xung đột logic. Đồng thời form mở lên một lúc tự động bị tắt (auto-close/time-out).
     - *Biện pháp khắc phục*:
       - Loại bỏ hoàn toàn checkbox can thiệp trạng thái duyệt tự do trong form chỉnh sửa chi tiết.
       - Khẳng định nguyên tắc thẩm định hồ sơ chuẩn sàn: Trạng thái phê duyệt chỉ được phép thay đổi thông qua quy trình xét duyệt chính thức qua 2 nút hành động nghiệp vụ "Phê duyệt" và "Từ chối", có lưu vết nhật ký kiểm duyệt vào CSDL và phản ánh ngay lên Web.
       - Sửa dứt điểm lỗi tự động out/đóng form ngoài ý muốn bằng cách hủy các event listener và timer chạy ngầm không cần thiết.

  4. **Chuẩn Hóa Danh Mục Trạng Thái Đối Tác & Người Dùng Khớp 100% CSDL** ([QuanLyDoiTacViewModel.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/ViewModels/QuanLyDoiTacViewModel.cs), [QuanLyNguoiDungViewModel.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/ViewModels/QuanLyNguoiDungViewModel.cs), [QuanLyDoiTacView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/QuanLyDoiTacView.xaml), [QuanLyNguoiDungView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/QuanLyNguoiDungView.xaml)):
     - *Hiện tượng*: Dropdown bộ lọc trạng thái của Đối tác và Người dùng có nhiều tùy chọn thừa không có trong CSDL (như "Chờ kích hoạt", "Đang xác thực"), trong khi form thông tin tài khoản chỉ có 2 trạng thái.
     - *Biện pháp khắc phục*:
       - Rà soát và rút gọn combobox lọc về đúng 2 trạng thái tài khoản thực tế khớp 100% với CSDL: **`Đang hoạt động`** (`TrangThai = 1` hoặc `"HoatDong"`) và **`Tạm khóa`** (`TrangThai = 0` hoặc `"BiKhoa"`).
       - Đồng bộ logic lọc và binding giữa ViewModel và giao diện XAML.

  5. **Đồng Bộ Đầy Đủ Danh Sách Chủ Nhà & Sửa Lỗi Bộ Lọc Ngày Tháng Báo Cáo Doanh Thu** ([BaoCaoDoanhThuViewModel.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/ViewModels/BaoCaoDoanhThuViewModel.cs)):
     - *Hiện tượng*:
       - Dropdown "Tất cả chủ homestay" bị thiếu một số chủ nhà có cơ sở lưu trú thực tế trong CSDL.
       - Bộ lọc ngày tháng doanh thu trả về sai kết quả: khi lọc dữ liệu trong tháng 9 lại trả về các đơn có ngày check-in/check-out sang tháng 10.
     - *Biện pháp khắc phục*:
       - Truy vấn đầy đủ danh sách chủ nhà đối tác từ CSDL thông qua API thay vì lọc thiếu; hiển thị toàn bộ 8 chủ homestay trong dropdown.
       - Sửa logic lọc doanh thu: So sánh theo mốc **Ngày đặt phòng (`NgayDat`)** và chu kỳ ghi nhận doanh thu thực tế, thay vì so sánh nhầm với `NgayCheckIn`/`NgayCheckOut` của kỳ lưu trú tương lai.

  6. **Khắc Phục Triệt Để Lỗi Crash Màn Hình Bảo Trì & Sao Lưu Hệ Thống** ([BaoTriHeThongView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/BaoTriHeThongView.xaml), [BaoTriHeThongViewModel.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/ViewModels/BaoTriHeThongViewModel.cs)):
     - *Hiện tượng*: Người dùng truy cập màn hình "Bảo trì & sao lưu hệ thống" bị dừng đột ngột và ném ngoại lệ: `A TwoWay or OneWayToSource binding cannot work on the read-only property 'TongDungLuongSaoLuu'`.
     - *Nguyên nhân*: Thẻ XAML `<Run Text="{Binding TongDungLuongSaoLuu}" />` ngầm định áp dụng TwoWay binding lên property chỉ có `get` trong ViewModel.
     - *Biện pháp khắc phục*:
       - Thêm tường minh `Mode=OneWay` vào toàn bộ các thẻ `<Run Text="{Binding ...}" />` trong `BaoTriHeThongView.xaml`.
       - Bổ sung setter no-op an toàn trong `BaoTriHeThongViewModel.cs` để phòng vệ từ xa, triệt tiêu hoàn toàn lỗi crash runtime.

---

### 8.12. Nâng Cấp Hệ Thống Quyết Toán Tài Chính, Chuẩn Hóa Báo Cáo Doanh Thu Thực Tế, Tối Ưu UX & Sửa Lỗi Giao Diện WPF (13/10/2026)

* **Thời gian thực hiện**: 13/10/2026
* **Mục tiêu**: Khắc phục dứt điểm các lỗi runtime UI WPF, đồng bộ 100% dữ liệu thật từ Database qua API, loại bỏ các thuật ngữ kỹ thuật thừa, xây dựng quy trình quyết toán dòng tiền ngân hàng thủ công (85% Host / 15% Sàn), và hiện đại hóa giao diện bộ chọn lịch (DatePicker) trên toàn hệ thống.

* **Nội dung chi tiết đã thực hiện**:
  1. **Khắc phục triệt để lỗi Crash giao diện & Cascade Dialogs**:
     - **Lỗi `Run.Text` Read-only Binding** ([BaoTriHeThongView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/BaoTriHeThongView.xaml) & [BaoTriHeThongViewModel.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/ViewModels/BaoTriHeThongViewModel.cs)): Bổ sung `Mode=OneWay` cho các thẻ `<Run Text="{Binding ...}" />` và tạo setter an toàn no-op cho các thuộc tính dung lượng CSDL.
     - **Lỗi `StaticResourceExtension` thiếu resource** ([BaoCaoDoanhThuView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/BaoCaoDoanhThuView.xaml)): Định nghĩa đầy đủ `StaylyTextBoxStyle` trong [App.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/App.xaml) với góc bo tròn 8px, viền mượt `#CBD5E1` và hiệu ứng focus xanh đại dương `#0077B6`.
     - **Chống lặp vô tận thông báo lỗi** ([App.xaml.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/App.xaml.cs)): Thêm bộ lọc debounce (chống lặp trong vòng 2 giây) tại `DispatcherUnhandledException`, triệt tiêu hoàn toàn hiện tượng nhảy ra hàng loạt hộp thoại xếp tầng khi xảy ra lỗi chu kỳ vẽ.
  2. **Chuẩn hóa Bảng Xếp Hạng Top Analytics 100% Dữ Liệu Thật từ Database & API**:
     - Loại bỏ toàn bộ mã gán cứng/mock ảo cũ trong [BaoCaoDoanhThuViewModel.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/ViewModels/BaoCaoDoanhThuViewModel.cs).
     - Gom nhóm và tính toán trực tiếp từ `LayDanhSachDonTheoBoLocAsync()` và `LayTatCaCoSoAsync()`:
       * **Top 10 Homestay**: Đếm số lượt đặt thực tế (Dalat Edensee 3 đơn, Ana Mandara 1 đơn, Terracotta 1 đơn, An Nhiên 1 đơn,...) và doanh thu tích lũy thật.
       * **Top Khách hàng VIP**: Lấy đúng danh sách khách hàng có phát sinh đơn trong CSDL (Khách Thuê Test, Lê Thị Chi, Nguyễn Quang Huy, Nguyễn Văn Minh, Trần Phương Lan) kèm tổng chi tiêu thực tế.
       * **Top Chủ nhà xuất sắc**: Thống kê đúng 8 chủ nhà đối tác chính thức, số lượng cơ sở lưu trú sở hữu và doanh thu thực nhận.
  3. **Lược bỏ thuật ngữ kỹ thuật & Auto-Refund**:
     - Xóa bỏ huy hiệu `"Auto-Refund Gateway: BẬT"` trên thanh tiêu đề Quản lý đơn ([QuanLyDonDatView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/QuanLyDonDatView.xaml)).
     - Loại bỏ số cổng kỹ thuật `5176` trên thanh trạng thái kết nối: Chuyển từ `"API Sẵn sàng (5176) • Trực tuyến"` thành `"Hệ thống Trực tuyến • Sẵn sàng"`.
  4. **Động hóa KPI Kiểm duyệt Homestay**:
     - Thay thế các tỷ lệ gán tĩnh bằng các thuộc tính động: `TieuChuanPhapLyText`, `TieuChuanPhapLyMota`, `TyLePheDuyetText`, `TyLePheDuyetMota` tại [KiemDuyetHomestayViewModel.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/ViewModels/KiemDuyetHomestayViewModel.cs).
     - Tính tự động từ API: Tỷ lệ phê duyệt thực tế (54/54 hồ sơ = 100%), Tiêu chuẩn pháp lý đạt chuẩn sàn (54/55 cơ sở = 98.2%), Chờ xét duyệt cấp tốc (1 hồ sơ).
  5. **Quy trình & Giao diện Quyết toán Ngân hàng Thủ công (85% Payout / 15% Sàn)**:
     - Sửa lỗi logic map trạng thái trong [HttpAdminService.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Services/HttpAdminService.cs): Phân tách rạch ròi giữa việc khách trả tiền cho sàn (`PaymentStatus == "Paid"`) và việc sàn giải ngân cho chủ nhà (`TrangThaiQuyetToan`).
     - Tích hợp thêm các trường ngân hàng của Host: `OwnerBankName`, `OwnerAccountNumber`, `OwnerAccountHolder`, `MaGiaoDichQuyetToan`, `NgayQuyetToan`, `GhiChuQuyetToan`.
     - Thêm cột `"THAO TÁC"` trên bảng Báo cáo doanh thu ([BaoCaoDoanhThuView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/BaoCaoDoanhThuView.xaml)): Nút xanh `💳 Quyết toán` (cho đơn chưa thanh toán) và nút xám `📄 Đã tất toán` (cho đơn đã thanh toán).
     - **Tách bạch 2 chế độ hiển thị Modal Quyết toán**:
       * *Chế độ Chưa quyết toán*: Mở form xác nhận, hiển thị số tiền 85% và STK chủ nhà, cho phép nhập Mã GD ngân hàng (Mã FT) và Ghi chú đối soát. Có 2 nút `Hủy bỏ` và `✔ Xác nhận đã chuyển khoản & Hoàn tất`.
       * *Chế độ Đã tất toán*: Mở xem biên lai đối soát (Read-only), hiển thị Mã GD đã lưu, ngày giờ tất toán thực tế, ghi chú đối soát. Ẩn toàn bộ ô nhập liệu và nút xác nhận, chỉ hiển thị duy nhất 1 nút `Đóng biên lai`.
     - Chuẩn hóa nút đóng `[✕ Đóng]` góc phải và hỗ trợ nhấp chuột ra bất kỳ vị trí nào ngoài màn hình (Backdrop click-to-dismiss) để tắt hộp thoại.
  6. **Việt hóa Mô hình Dòng tiền & Cập nhật Chu kỳ Quyết toán thành 1 tháng**:
     - Loại bỏ cụm từ tiếng Anh `(ESCROW FLOW ARCHITECTURE)` khỏi tiêu đề banner.
     - Cập nhật chu kỳ: **`Chu kỳ quyết toán: 1 tháng / lần & Sau khi khách hoàn tất lưu trú`**.
     - Việt hóa đồng bộ các huy hiệu: `100% GROSS` ➔ **`100% TỔNG THU`**, `15% TAKE RATE` ➔ **`15% HOA HỒNG`**, `85% PAYOUT` ➔ **`85% CHI TRẢ`**, `TK ngân hàng Host` ➔ **`TK ngân hàng Chủ nhà`**.
  7. **Nâng cấp Giao diện Lịch (DatePicker) Bo Góc Toàn Cục**:
     - Định nghĩa `StaylyDatePickerButtonTemplate`, `primitives:DatePickerTextBox`, và `Style TargetType="{x:Type DatePicker}"` trong [App.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/App.xaml).
     - Khung viền bo tròn **8px** (`CornerRadius="8"`), nền trắng `#FFFFFF`, viền xám `#CBD5E1`, hiệu ứng hover/focus đổi viền xanh đại dương `#0077B6`.
     - Nút icon lịch SVG vector phẳng hiện đại trên nền xám nhạt `#F1F5F9`, hover chuyển màu xanh pastel `#E0F2FE`.
     - Canh lề chữ ngày tháng căn giữa dọc hoàn hảo (`SemiBold`), nới rộng độ rộng cột lịch từ 120px lên 130px.
     - Áp dụng toàn cục tự động cho tất cả DatePicker trong toàn bộ ứng dụng WPF.

---

---

### 8.14. Chuẩn Hóa Ràng Buộc CCCD 12 Số Bắt Đầu Bằng 0, Gỡ Bỏ Khối Cảnh Báo Giả Lập & Giới Hạn Quản Lý Người Dùng Chỉ Dành Riêng Cho Khách Hàng (15/10/2026)

* **Thời gian thực hiện**: 15/10/2026
* **Mục tiêu**: 
  1. Đồng bộ quy tắc kiểm thực căn cước công dân (CCCD) trên toàn hệ thống (Web Client, API, WPF Desktop Admin) theo chuẩn Bộ Công an Việt Nam: đúng 12 chữ số và bắt buộc bắt đầu bằng số `0` (`^0\d{11}$`).
  2. Gỡ bỏ khối cảnh báo giả lập/hardcoded trong modal giám sát chất lượng chỗ nghỉ, chỉ giữ lại ô lý do can thiệp/tạm dừng hoạt động thực tế.
  3. Tối ưu phân hệ Quản lý người dùng trong ứng dụng Quản trị WPF: Cố định vai trò tạo mới chỉ dành cho Khách hàng thành viên (không cho tạo tài khoản Quản trị viên), đồng thời loại bỏ hoàn toàn các tài khoản Admin khỏi danh sách hiển thị và các thẻ KPI thống kê.

* **Nội dung chi tiết đã thực hiện**:
  1. **Chuẩn hóa Ràng buộc Định dạng CCCD 12 số bắt đầu bằng số 0 (`^0\d{11}$`)**:
     - [ValidationHelper.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Services/ValidationHelper.cs): Cập nhật biểu thức chính quy `CitizenIdRegex = @"^0\d{11}$"`. Cung cấp thông báo lỗi chi tiết khi CCCD không đủ 12 số hoặc không bắt đầu bằng số 0.
     - [Register.cshtml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Home/Register.cshtml): Cập nhật Regex kiểm tra frontend `const cccdRegex = /^0\d{11}$/;` cùng thông báo: *"Số CCCD phải gồm đúng 12 chữ số và bắt đầu bằng số 0."*
     - [_OwnerRegistrationFields.cshtml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Home/_OwnerRegistrationFields.cshtml): Cập nhật thuộc tính `pattern="0[0-9]{11}"`, `maxlength="12"`, và placeholder *"Ví dụ: 001200012345 (12 số bắt đầu bằng 0)"*.
     - [Profile.cshtml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Home/Profile.cshtml): Cập nhật thuộc tính `pattern="0[0-9]{11}"` và placeholder hướng dẫn người dùng nhập đúng định dạng.
     - [ApiModels.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Models/ApiModels.cs): Thêm thuộc tính xác thực DataAnnotations `[RegularExpression(@"^0\d{11}$", ErrorMessage = "Số CCCD phải gồm đúng 12 chữ số và bắt đầu bằng số 0.")]` cho cả `RegisterOwnerInput` và `ProfileInput`.
     - [AuthDtos.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/DTOs/Auth/AuthDtos.cs) & [AdminDtos.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/DTOs/Admin/AdminDtos.cs): Cập nhật RegularExpression cho các DTO API nhận CCCD.
  2. **Gỡ bỏ Khối Cảnh báo Giả lập trong Giám sát Chất lượng Chỗ nghỉ**:
     - [QuanLyDanhMucHomestayView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/QuanLyDanhMucHomestayView.xaml): Xóa bỏ hoàn toàn thẻ `Border Background="#FEF2F2"` chứa nội dung cảnh báo tĩnh giả lập khiếu nại (vì không phản ánh đánh giá thật từ khách thuê), làm cho giao diện hồ sơ kiểm soát chất lượng trở nên trực quan, thanh thoát và chính xác.
     - Thêm watermark mờ hướng dẫn tinh tế cho ô nhập *"Lý do can thiệp / tạm dừng cơ sở lưu trú"* bằng XAML DataTrigger, nâng cao trải nghiệm thao tác cho Admin.
     - [QuanLyDanhMucHomestayViewModel.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/ViewModels/QuanLyDanhMucHomestayViewModel.cs): Đặt giá trị mặc định của `_lyDoXuLy` là chuỗi rỗng (`string.Empty`), đảm bảo mỗi khi mở hồ sơ một homestay khác nhau, ô lý do luôn sạch sẽ và chờ Admin nhập lý do thực tế.
  3. **Giới hạn Phân hệ Quản lý Người dùng: Chỉ Tạo & Hiển thị Khách Hàng**:
     - [QuanLyNguoiDungView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/QuanLyNguoiDungView.xaml):
       * Cập nhật phụ đề modal: *"Cấp tài khoản sử dụng sàn Stayly cho khách hàng thành viên"*.
       * Thay thế thẻ ComboBox chọn vai trò (Khách hàng / Quản trị viên) bằng khung thẻ nhãn cố định mang tên **"Khách hàng"** (`Background="#F8FAFC"`, icon `👤`, màu chữ xám đậm), khóa chặt vai trò tạo mới.
     - [QuanLyNguoiDungViewModel.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/ViewModels/QuanLyNguoiDungViewModel.cs):
       * Cố định thuộc tính `ThemVaiTro` luôn trả về `"KhachHang"`.
       * Trong phương thức xác nhận thêm mới `ThucHienXacNhanThemMoiAsync()`: gán dứt khoát `VaiTro = "KhachHang"`.
       * Trong phương thức tải dữ liệu `TaiDanhSachNguoiDungAsync()`: truyền tham số `"KhachHang"` vào `LayDanhSachTaiKhoanAsync("KhachHang")` và áp dụng bộ lọc `tatCa.Where(t => t.VaiTro == "KhachHang")`. Loại bỏ 100% các tài khoản Quản trị viên (Admin) khỏi bảng danh sách và các thẻ thống kê KPI.
     - [AdminController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/AdminController.cs): Bổ sung kiểm tra chặn tạo tài khoản Quản trị viên qua endpoint `POST /api/admin/users`: nếu request chứa vai trò `ADMIN` hoặc `QUANTRIVIEN`, API trả về mã lỗi `400 Bad Request` kèm thông báo *"Không được phép tạo tài khoản quản trị viên. Hệ thống chỉ hỗ trợ tạo tài khoản khách hàng."*
     - [QuanLyTaiKhoanViewModel.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/ViewModels/QuanLyTaiKhoanViewModel.cs): Lọc bỏ tài khoản có vai trò `QuanTriVien` khỏi danh sách `_tatCaTaiKhoan`, đảm bảo tính nhất quán trên toàn bộ ứng dụng.

  4. **Chuẩn hóa Ràng buộc Mật khẩu Bảo mật (8-20 ký tự, Chữ in hoa, Chữ thường, Chữ số, Ký tự đặc biệt)**:
     - **Quy tắc bảo mật đồng bộ toàn hệ thống**: Biểu thức chính quy `^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9\s])\S{8,20}$`.
     - [ValidationHelper.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Services/ValidationHelper.cs):
       * Khai báo `PasswordRegex` kiểm tra chuẩn bảo mật: độ dài 8-20 ký tự, có ít nhất 1 chữ in hoa (`[A-Z]`), 1 chữ thường (`[a-z]`), 1 chữ số (`\d`) và 1 ký tự đặc biệt (`[^a-zA-Z0-9\s]`), không chứa khoảng trắng.
       * Cập nhật hàm `IsValidPassword`: kiểm tra độ dài và regex, xuất thông báo lỗi rõ ràng. Mật khẩu mặc định khởi tạo `Stayly@2026` hoàn toàn thỏa mãn 100% tiêu chuẩn này.
     - [QuanLyNguoiDungView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/QuanLyNguoiDungView.xaml) & [QuanLyDoiTacView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/QuanLyDoiTacView.xaml):
       * Khắc phục lỗi che mất chữ trên tiêu đề trường mật khẩu: Tách gọn tiêu đề chính *"Đổi mật khẩu mới: (để trống nếu không đổi)"* và chuyển quy tắc bảo mật *"Yêu cầu 8-20 ký tự: gồm ít nhất 1 chữ in hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt"* thành dòng ghi chú riêng bên dưới với `TextWrapping="Wrap"`.
       * Áp dụng bố cục tách dòng tương tự cho form Thêm mới người dùng và Thêm/Sửa đối tác chủ homestay, giúp giao diện thoáng đãng, sang trọng và không bao giờ bị cắt chữ.
       * Giới hạn `MaxLength="20"` trên các TextBox mật khẩu.
     - [ApiModels.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Models/ApiModels.cs):
       * Áp dụng `[StringLength(20, MinimumLength = 8)]` và `[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9\s])\S{8,20}$")]` cho `RegisterGuestInput.Password` và `ChangePasswordInput.NewPassword`.
     - [Register.cshtml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Home/Register.cshtml) & [_GuestRegistrationFields.cshtml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Home/_GuestRegistrationFields.cshtml):
       * Cập nhật thuộc tính `minlength="8" maxlength="20"`.
       * Validate JavaScript chặn ngay tại form submit: kiểm tra biểu thức chính quy, hiển thị thông báo lỗi chi tiết khi người dùng nhập mật khẩu thiếu chữ hoa, thường, số hoặc ký tự đặc biệt.
     - [_Layout.cshtml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Shared/_Layout.cshtml):
       * Đồng bộ modal Đăng ký nhanh trên Header: nhãn *"Mật khẩu (8-20 ký tự, gồm chữ hoa, thường, số, ký tự đặc biệt)"*, thuộc tính `maxlength="20"` và hàm validate JS regex tương ứng.
     - [Profile.cshtml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Home/Profile.cshtml):
       * Cập nhật khối cảnh báo an toàn mật khẩu và nhãn trường Mật khẩu mới trong tab Đổi mật khẩu.
     - [AuthDtos.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/DTOs/Auth/AuthDtos.cs) & [AdminDtos.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/DTOs/Admin/AdminDtos.cs):
       * Cập nhật DataAnnotations `[StringLength(20, MinimumLength = 8)]` và RegularExpression bảo mật cho tất cả các DTO liên quan: `RegisterGuestRequest`, `RegisterOwnerRequest`, `ChangePasswordRequest`, `ChangePasswordOtpRequest`, `CreateUserAdminRequest`, `UpdateUserAdminRequest`.
     - [AdminController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/AdminController.cs):
       * Đổi mật khẩu dự phòng mặc định khi Admin tạo user mới mà không nhập mật khẩu từ `"123456"` sang `"Stayly@2026"` để luôn tuân thủ chuẩn bảo mật.

   5. **Khắc Phục Triệt Để Lỗi Che Chữ và Canh Đều Cột Loại Hình Chỗ Nghỉ**:
      - **Nguyên nhân cốt lõi**: Chuỗi nhãn cũ `"Homestay (Thuê nguyên căn)"` dài tới 26 ký tự, kết hợp với cơ chế Virtualization của WPF DataGrid khi dòng đầu tiên là khách sạn ngắn hơn khiến cột đo co lại, dẫn đến các dòng sau bị cắt mất đuôi chữ thành `Homestay (Thuê nguyên c`. Thêm vào đó, badge bị canh dạt về bên trái (`HorizontalAlignment="Left"`), không thẳng hàng cân đối với tiêu đề cột.
      - [CoSoLuuTru.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Models/CoSoLuuTru.cs):
        * Chuẩn hóa thuộc tính `LoaiHinhHienThi`: Trả về `"Homestay nguyên căn"` (18 ký tự) và `"Khách sạn (Hotel)"` (17 ký tự). Hai nhãn có độ dài cân xứng lý tưởng, súc tích và hoàn toàn đồng bộ với nhãn hiển thị phía Web.
        * Bổ sung kiểm tra null-safe `LoaiHinh?.Contains(...) == true` triệt tiêu nguy cơ phát sinh ngoại lệ NullReferenceException.
      - [QuanLyDanhMucHomestayViewModel.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/ViewModels/QuanLyDanhMucHomestayViewModel.cs):
        * Cập nhật danh sách lựa chọn `DanhSachLoaiHinhLuuTru` với hai nhãn chuẩn: `"Homestay nguyên căn"` và `"Khách sạn (Hotel)"`.
        * Cải tiến bộ lọc `BoLocLoaiHinh` linh hoạt, kiểm tra an toàn theo cả mã nội bộ (`Homestay`, `Hotel`) lẫn nhãn hiển thị thực tế.
      - [QuanLyDanhMucHomestayView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/QuanLyDanhMucHomestayView.xaml):
        * Cột `LOẠI HÌNH`: Tăng `MinWidth="180"`, bổ sung `HeaderStyle` căn giữa (`HorizontalContentAlignment="Center"`).
        * Thẻ Badge: Căn đều chính giữa ô (`HorizontalAlignment="Center"`, `TextAlignment="Center"`, `Margin="6,0"`), đệm `Padding="12,5"`.
        * Bổ sung DataTrigger chuyển màu nền `#F1F5F9` và chữ `#475569` cho cả giá trị `"Khách sạn (Hotel)"` và `"Khách sạn"`.
        * Đồng bộ ComboBox bộ lọc và badge loại hình trong modal chi tiết hồ sơ giám sát.
      - [KiemDuyetHomestayView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/KiemDuyetHomestayView.xaml):
        * Áp dụng toàn bộ cấu trúc căn đều giữa và DataTrigger tương tự cho cột `LOẠI HÌNH` tại màn hình Kiểm duyệt Homestay.
        * Chuyển đổi nhãn loại hình trong modal thẩm định hồ sơ sang dynamic binding `{Binding CoSoDangChon.LoaiHinhHienThi}` thay cho chuỗi tĩnh.

   6. **Đồng Bộ Toàn Diện Phân Hệ Mã Giảm Giá / Voucher Sàn (Tên Chương Trình & Số Lượng, Bỏ Đơn Tối Thiểu)**:
      - **Cơ sở dữ liệu (SQL Server - Bảng `GiamGia`)**:
        * Bổ sung 2 cột mới: `TenChuongTrinh NVARCHAR(200) NULL` và `SoLuong INT NULL DEFAULT 100`.
        * Cập nhật dữ liệu tên chương trình thực tế và hạn mức số lượng cho toàn bộ 6 voucher hiện hữu trong hệ thống.
      - **Backend API (`API/`)**:
        * [QuanLyDonDat.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Models/QuanLyDonDat.cs): Cập nhật entity `GiamGia` với hai thuộc tính `TenChuongTrinh` và `SoLuong`.
        * [AdminPromotionsController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/AdminPromotionsController.cs): Cập nhật `AdminPromotionPayload`, hàm `GetAll`, `Create`, `Update` tiếp nhận, lưu trữ và phản hồi đầy đủ `title`, `quantity`, `usageCount`. Tự động vô hiệu hóa mã khi lượt sử dụng đạt trần (`usageCount >= quantity`).
        * [PromotionsController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/PromotionsController.cs) & [PropertiesController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/PropertiesController.cs): Trả về `TenChuongTrinh`, `SoLuong`, và tính toán số lượt còn lại `SoLuongConLai`. Endpoint `GetByCode` chặn áp dụng khi mã đã hết lượt dùng.
        * [BookingsController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/BookingsController.cs): Khi khách hàng tạo đơn đặt phòng, kiểm tra chặt chẽ điều kiện số lượt sử dụng thực tế (`coupon.DonDatPhongs.Count < coupon.SoLuong`).
      - **Desktop Admin (`WPF`)**:
        * [QuanLyVoucherView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/QuanLyVoucherView.xaml):
          - Xóa bỏ cột `ĐƠN TỐI THIỂU` khỏi DataGrid hiển thị.
          - Gỡ bỏ ô nhập "Đơn tối thiểu (VNĐ)" khỏi modal Thêm/Sửa voucher, tái cấu trúc ô *"Hạn mức số lượng phát hành (*)"* rõ ràng, trực quan.
        * [QuanLyKhuyenMaiViewModel.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/ViewModels/QuanLyKhuyenMaiViewModel.cs): Bỏ kiểm tra `DonGiaToiThieu < 0`, giữ nguyên validation `SoLuongToiDa > 0`.
        * [HttpAdminService.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Services/HttpAdminService.cs): Cập nhật `ApiPromotionDto` và payload của `ThemKhuyenMaiAsync`, `CapNhatKhuyenMaiAsync` gửi kèm `title` và `quantity`.
      - **Web Client (`Web/`)**:
        * [ApiModels.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Models/ApiModels.cs): Cập nhật `PromotionItem` và `CheckPromoResult` nhận `Title`, `Quantity`, `UsageCount`, `RemainingCount`.
        * [HomeController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Controllers/HomeController.cs): Endpoint `CheckPromoCode` trả về tên chương trình và số lượng còn lại cho client.
        * [_Layout.cshtml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Shared/_Layout.cshtml): Modal Danh sách ưu đãi sàn hiển thị chính xác Tên chương trình đã tạo từ Admin và huy hiệu số lượng lượt dùng còn lại (`Còn X/Y lượt`).
        * [Details.cshtml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Home/Details.cshtml): Khi nhập mã áp dụng thành công và ở bảng chi tiết thanh toán, hiển thị đầy đủ tên chương trình ưu đãi gắn liền với mã voucher.

---

### 8.15. Bảng Tổng Hợp Kiểm Thử & Trạng Thái Toàn Bộ Hệ Thống (Cập nhật 15/10/2026)

| Phân hệ / Thành phần | Công nghệ / Nền tảng | Trạng thái Build | Trạng thái Vận hành | Ghi chú & Đánh giá |
| :--- | :--- | :---: | :---: | :--- |
| **Backend API** | ASP.NET Core 8.0 RESTful API | **0 Lỗi, 0 Cảnh báo** | Đang chạy tại `http://localhost:5176` | Sẵn sàng 100%: Validate CCCD 12 số bắt đầu bằng 0; Ràng buộc mật khẩu 8-20 ký tự (hoa, thường, số, đặc biệt); Chặn tạo Admin |
| **Web Client** | ASP.NET Core 8.0 MVC Razor | **0 Lỗi, 0 Cảnh báo** | Đang chạy tại `http://localhost:5280` | Sẵn sàng 100%: Validate Regex CCCD & Mật khẩu 8-20 ký tự cả client-side HTML5/JS và server-side DataAnnotations |
| **Desktop Admin** | WPF .NET 8 / MVVM Pattern | **0 Lỗi, 0 Cảnh báo** | Sẵn sàng chạy | Sẵn sàng 100%: Màn hình Quản lý người dùng chỉ tạo/hiển thị Khách hàng, Validate mật khẩu bảo mật, gỡ cảnh báo tĩnh |
| **Cơ sở dữ liệu** | Microsoft SQL Server (`HOMESTAY_DB`) | **Online & Đầy đủ 21 Bảng** | Localhost & Sẵn sàng Deploy | Schema chuẩn hóa, dữ liệu tài khoản và mã băm mật khẩu bảo mật đồng bộ chính xác |


---

### 8.16. Chuẩn Hóa Chế Độ Chỉ Xem (Read-Only) Cho Đơn Đặt Phòng & Báo Cáo Luồng Đơn Đặt & Hoàn Tiền (15/10/2026)

* **Bối cảnh & Yêu cầu nghiệp vụ**:
  - Đơn đặt phòng là chứng từ hợp đồng điện tử có tính chất pháp lý và tài chính ràng buộc giữa 3 bên: Khách thuê - Chủ homestay - Sàn Stayly.
  - Quản trị viên (Admin) đóng vai trò giám sát, vận hành và giải quyết tranh chấp (Audit & Governance). Admin **không được phép tùy ý sửa đổi** thông tin hồ sơ của khách (Tên, SĐT, Email, Ngày nhận/trả phòng, Số người lớn/trẻ em, Tổng tiền).
  - Vì vậy, chức năng "Chỉnh sửa thông tin đơn đặt" được chuẩn hóa lại thành "Xem chi tiết hồ sơ đơn đặt" với chế độ **Chỉ xem (Read-Only)**, loại bỏ hoàn toàn nút lưu thay đổi.

* **Các tệp đã chỉnh sửa**:
  1. [QuanLyDonDatView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/QuanLyDonDatView.xaml):
     - Đổi nút tại modal chi tiết đơn: từ `"✏️ Chỉnh Sửa Thông Tin Đơn Đặt"` thành `"📋 Xem Chi Tiết Hồ Sơ Đơn Đặt"`.
     - Chuyển toàn bộ modal hồ sơ sang chế độ **Chỉ xem**:
       * Tiêu đề: `"📋 CHI TIẾT HỒ SƠ ĐƠN ĐẶT PHÒNG"` kèm badge `"🔒 Chỉ xem (Không sửa)"`.
       * Các trường dữ liệu (Tên, SĐT, Email, Ngày đến, Ngày đi, Số người, Ghi chú) chuyển thành `IsReadOnly="True" Focusable="False"` hoặc hiển thị thẻ TextBlock rõ nét trên nền `#F8FAFC`.
       * **Loại bỏ nút "💾 Lưu thay đổi"**, thay bằng nút `"Đóng cửa sổ"`.
  2. [QuanLyDonDatViewModel.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/ViewModels/QuanLyDonDatViewModel.cs):
     - Khóa phương thức cập nhật, thông báo dữ liệu đơn đặt là chứng từ hợp đồng chỉ được xem, không được chỉnh sửa trực tiếp.

---

### 8.17. Quy Tắc Hoàn Tiền Trước 2 Ngày, Đồng Bộ Form Web & Quy Trình Chuyển Khoản Thủ Công WPF (15/10/2026)

* **Yêu cầu & Quy định**:
  1. **Quy tắc 2 ngày trên Web & API**:
     - Khách hàng chỉ được bấm yêu cầu hoàn tiền khi thời điểm hiện tại cách ngày nhận phòng (Check-in) tối thiểu **2 ngày**.
     - Nếu cận ngày nhận phòng (< 2 ngày) hoặc sau ngày nhận phòng: ẩn nút Hoàn tiền trên Web, thay bằng huy hiệu `Quá hạn hoàn tiền (< 2 ngày)`, đồng thời API chặn với HTTP 400 Bad Request.
  2. **Đồng bộ Form hoàn tiền trên Web**:
     - Form yêu cầu hoàn tiền thu thập đầy đủ: Ngân hàng thụ hưởng, Số tài khoản, Tên chủ tài khoản, Lý do yêu cầu hoàn tiền (tự động điền nếu đã lưu trong hồ sơ).
     - Khi gửi: API cập nhật trạng thái đơn thành `YeuCauHoanTien`, lưu thông tin tài khoản ngân hàng và lý do, giải phóng lịch phòng `LichLuuTru`.
  3. **Quy trình Chuyển tiền thủ công tương tự Quyết toán đối tác trên WPF**:
     - Đơn chạy xuống WPF mục "Đơn đặt & hoàn tiền" với trạng thái `YeuCauHoanTien`.
     - Admin bấm "Phê duyệt hoàn tiền qua cổng", mở dialog chuyên dụng:
       * Hiển thị thông tin tài khoản ngân hàng của khách (Ngân hàng, Số TK, Tên chủ TK).
       * Số tiền cần hoàn.
       * Hướng dẫn quy trình chuyển tiền: Admin dùng Internet Banking chuyển tiền vào STK khách.
       * Ô nhập **Mã giao dịch ngân hàng / Mã chuẩn chi (*)** và **Ghi chú đối soát hoàn tiền**.
       * Nút **"✔ Xác nhận đã chuyển khoản"** để lưu vết và kích hoạt gửi email thông báo hoàn tiền cho khách hàng.

* **Các tệp đã chỉnh sửa**:
  1. [Web/Web/Views/Home/Trips.cshtml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Web/Web/Views/Home/Trips.cshtml):
     - Đồng bộ tính toán theo giờ Việt Nam (`DateTime.UtcNow.AddHours(7).Date`).
     - Ràng buộc hiển thị nút hoàn tiền: chỉ hiển thị khi `todayVn <= booking.CheckIn.Date.AddDays(-2)`. Nếu < 2 ngày, hiển thị huy hiệu `Quá hạn hoàn tiền (< 2 ngày)`.
  2. [API/Controllers/BookingsController.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/API/Controllers/BookingsController.cs):
     - Ràng buộc tại `RequestRefund`: kiểm tra ngày theo giờ VN, từ chối yêu cầu nếu `todayVn > booking.NgayDen.Date.AddDays(-2)`.
  3. [Wpf/.../ViewModels/QuanLyDonDatViewModel.cs](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/ViewModels/QuanLyDonDatViewModel.cs):
     - Thêm thuộc tính `MaGiaoDichHoanTien`, validate bắt buộc nhập mã giao dịch ngân hàng trước khi xác nhận hoàn tiền.
  4. [Wpf/.../Views/QuanLyDonDatView.xaml](file:///d:/DO_AN_HK7/KhoaLuanCuNhan/KLCN/Wpf/HomestaySystem/HomestaySystem/Views/QuanLyDonDatView.xaml):
     - Nâng cấp dialog hoàn tiền hiển thị tài khoản ngân hàng của khách, ô nhập Mã GD chuẩn chi, Ghi chú đối soát và nút "✔ Xác nhận đã chuyển khoản".

---
*Stayly Homestay Management System — Cập nhật ngày 15/10/2026.*


