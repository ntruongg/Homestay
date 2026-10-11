# Stayly Web Client

Ứng dụng web hiện đại xây dựng trên nền tảng **ASP.NET Core 8 MVC**, sử dụng Razor Views, Bootstrap 5.3, Bootstrap Icons và kết nối với Backend qua `HomestayApiClient`.

---

## 🌟 Các Chức Năng Chính

1. **Khám phá & Tìm kiếm:**
   - Tìm kiếm cơ sở lưu trú theo địa điểm, khu vực, khoảng giá và các tiện ích (Hồ bơi, BBQ, Bếp riêng, Cho phép thú cưng...).
   - Xem chi tiết Homestay: Bộ sưu tập ảnh Cloudinary, thông tin phòng trống, bản đồ, nội quy và đánh giá thực tế từ khách hàng.

2. **Đặt phòng & Dịch vụ bổ sung:**
   - Chọn ngày nhận phòng (`CheckIn`) và trả phòng (`CheckOut`) có kiểm tra lịch trống và sức chứa.
   - Chọn các **dịch vụ bổ sung** (Bếp nướng BBQ, tiệc trà, giặt ủi, xe đưa đón...) với đơn giá và số lượng linh hoạt.
   - Nhập **Mã giảm giá (Promo Code)** có xác thực thời hạn và trần giảm tối đa.

3. **Thanh toán VNPay & QR Code thời gian thực:**
   - Tạo liên kết thanh toán VNPay và hiển thị mã QR thanh toán tức thì.
   - **Bộ đếm thời gian thực tế 30 phút:** Tính đúng thời gian kể từ lúc đặt phòng (`NgayDat`), chống reset khi F5/refresh trang.
   - **Tự động hủy đơn quá hạn:** Chuyển sang trạng thái `DaHuy` và giải phóng phòng sau 30 phút chưa thanh toán.
   - Polling trạng thái thanh toán thời gian thực: Tự động chuyển hướng sang Chuyến đi ngay khi quét mã thành công.

4. **Quản lý chuyến đi (Khách hàng):**
   - Danh sách đơn đặt phòng phân loại theo trạng thái rõ ràng.
   - Modal **Chi tiết đơn** bóc tách đầy đủ: Tiền phòng theo số đêm, danh sách dịch vụ bổ sung, ưu đãi giảm giá và tổng thanh toán.
   - Huy hiệu thời hạn thanh toán còn lại cho các đơn đang chờ thanh toán.
   - Hủy đơn và gửi yêu cầu hoàn tiền minh bạch (kèm thông tin STK ngân hàng thụ hưởng).
   - Đánh giá xếp hạng sao và gửi nhận xét sau khi hoàn tất chuyến đi.

5. **Kênh chủ cơ sở lưu trú (Host Dashboard):**
   - Đăng ký và quản lý các cơ sở homestay, danh sách phòng và bảng giá.
   - Tiếp nhận và duyệt/từ chối đơn đặt phòng của khách.
   - Cấu hình danh mục dịch vụ bổ sung riêng cho từng cơ sở.
   - Bảng theo dõi doanh thu và trạng thái quyết toán chi trả 85% từ sàn.

---

## 🚀 Khởi Chạy Ứng Dụng

1. Đảm bảo Backend API đang chạy tại `http://localhost:5176`.
2. Chuyển vào thư mục `Web/Web` và chạy:

```powershell
cd d:\Homestay\Web\Web
dotnet run --launch-profile http
```

3. Mở trình duyệt và truy cập: `http://localhost:5280`.

---

## ⚙️ Cấu Hình Hệ Thống (`appsettings.json`)

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
