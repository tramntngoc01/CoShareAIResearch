
# User Journey

## Journey 1: Công nhân KCN (Tầng 3) - Đăng ký & Mua hàng lần đầu

### Happy Path

| Bước | Hành động | Màn hình | Dữ liệu thu thập | Ghi chú |
|------|-----------|----------|------------------|---------|
| 1 | Truy cập website | Landing page | - | Web responsive |
| 2 | Nhấn "Đăng ký" | Form đăng ký | Họ tên, Công ty, SĐT | Công ty là dropdown/autocomplete |
| 3 | Nhận & nhập OTP | OTP input | OTP 6 số | ZNS gửi qua Zalo |
| 4 | Xác thực thành công | Thông báo tầng | Tầng được gán (1/2/3) | Hệ thống match từ danh sách nhân sự |
| 5 | (Optional) KYC trả chậm | Form KYC | CCCD, ngày sinh, mã NV | Chỉ hiện nếu công ty bật trả chậm |
| 6 | Duyệt trang chủ | Homepage | - | Banner deal, danh mục |
| 7 | Chọn danh mục/sản phẩm | Category/Product list | - | Lọc theo tồn khả dụng |
| 8 | Xem chi tiết sản phẩm | Product detail | - | Ảnh, mô tả, video, giá, lượt bán |
| 9 | Thêm vào giỏ hàng | Cart (mini/full) | Sản phẩm, số lượng | - |
| 10 | Tiến hành checkout | Checkout page | Điểm nhận, người nhận, SĐT, ghi chú | Default điểm nhận theo công ty |
| 11 | Chọn phương thức thanh toán | Payment selection | COD/Chuyển khoản/Trả chậm | Trả chậm chỉ hiện nếu đủ điều kiện |
| 12 | Xác nhận đặt hàng | Order confirmation | Order ID, QR/PIN nhận hàng | Gửi ZNS xác nhận (optional) |
| 13 | Theo dõi trạng thái | Order status page | Timeline trạng thái | Cập nhật realtime |
| 14 | Nhận hàng tại điểm | Điểm nhận | Scan QR/nhập PIN | Shipper/Tầng 2 xác nhận |

### Edge/Exception Paths

| ID | Trường hợp | Xử lý | Màn hình/Thông báo |
|----|------------|-------|-------------------|
| E1.1 | SĐT không có trong danh sách nhân sự | Gán Tầng 3 mặc định HOẶC chờ duyệt (→ Open Question Q7) | Thông báo "Đang xác minh" |
| E1.2 | OTP hết hạn/sai | Cho phép gửi lại OTP (giới hạn lần → Open Question Q8) | "OTP không hợp lệ, gửi lại?" |
| E1.3 | Sản phẩm hết hàng khi checkout | Thông báo, yêu cầu cập nhật giỏ | "Sản phẩm X đã hết, vui lòng bỏ" |
| E1.4 | Vượt hạn mức trả chậm | Không cho chọn trả chậm | "Bạn đã đạt hạn mức trả chậm" |
| E1.5 | Quá hạn trả chậm (nợ cũ) | Khóa mua hàng mới | "Vui lòng thanh toán nợ trước" |
| E1.6 | Chuyển khoản không đúng cú pháp | Admin xác nhận thủ công | Trạng thái "Chờ xác nhận thanh toán" |

---

## Journey 2: Tầng 2 - Giao hàng & Thu COD

### Happy Path

| Bước | Hành động | Màn hình | Dữ liệu thu thập | Ghi chú |
|------|-----------|----------|------------------|---------|
| 1 | Đăng nhập (auto-login nếu có token) | Login/Home | - | - |
| 2 | Xem danh sách đơn được phân công | Delivery list | Filter: công ty, điểm nhận, ca | - |
| 3 | Chọn đơn/Quét QR kiện hàng | Order detail | Order ID | - |
| 4 | Chụp ảnh trước giao | Camera capture | Ảnh trước giao | Bằng chứng tình trạng hàng |
| 5 | Giao hàng cho khách | - | - | Khách trình QR/PIN |
| 6 | Quét QR khách/nhập PIN | Verification | Xác thực đơn | - |
| 7 | Chụp ảnh POD | Camera capture | Ảnh POD | Kiện hàng + người nhận |
| 8 | (Nếu COD) Nhận tiền, ghi nhận | COD collection | Số tiền nhận | Sinh phiếu thu điện tử |
| 9 | Cập nhật "Đã giao thành công" | Confirm delivery | Trạng thái đơn | - |
| 10 | Cuối ca: Đối soát | Shift reconciliation | Tổng đơn, tổng COD, tiền nộp lại | - |
| 11 | Nộp tiền COD về quỹ | Cash handover | Số tiền nộp | Admin xác nhận |

### Edge/Exception Paths

| ID | Trường hợp | Xử lý | Màn hình/Thông báo |
|----|------------|-------|-------------------|
| E2.1 | Khách không nhận hàng | Đánh dấu "Không nhận", ghi lý do | Update status + notes |
| E2.2 | Hàng hư hại khi giao | Tạo phiếu hủy hàng với ảnh | Workflow hủy hàng |
| E2.3 | Khách yêu cầu đổi trả | Tạo yêu cầu đổi trả tại điểm | Return request form |
| E2.4 | Sai lệch tiền COD khi đối soát | Ghi nhận sai lệch, Admin review | Reconciliation report |
| E2.5 | Mất kết nối khi giao hàng | Lưu offline, sync khi có mạng (→ Open Question Q9) | - |

---

## Journey 3: Admin Ops - Xử lý đơn hàng

### Happy Path

| Bước | Hành động | Màn hình | Dữ liệu thu thập | Ghi chú |
|------|-----------|----------|------------------|---------|
| 1 | Đăng nhập Admin | Admin login | Email/SĐT + password | Hoặc OTP ZNS |
| 2 | Xem Dashboard | Dashboard | - | Widget tổng quan |
| 3 | Vào Quản lý đơn hàng | Order list | Filter: công ty, điểm, trạng thái | - |
| 4 | Xác nhận xuất kho | Order detail | Ngày, giờ xuất | Audit log |
| 5 | Xuất PDF phiếu giao | Print view | - | Format A5 |
| 6 | Đánh dấu "Đến điểm nhận" | Update status | - | Audit log |
| 7 | Xem báo cáo | Reports | - | Doanh số, hoa hồng, COD |

### Edge/Exception Paths

| ID | Trường hợp | Xử lý | Màn hình/Thông báo |
|----|------------|-------|-------------------|
| E3.1 | Cần hủy đơn | Ops tạo yêu cầu hủy → Super Admin duyệt | Cancel request form |
| E3.2 | Hàng hư hại tại kho | Tạo phiếu hủy hàng → Super Admin duyệt | Inventory write-off |
| E3.3 | Sai thông tin đơn hàng | Liên hệ khách qua Support | Manual update |

---

## Journey 4: Admin QC - Kiểm duyệt lô hàng

### Happy Path

| Bước | Hành động | Màn hình | Dữ liệu thu thập | Ghi chú |
|------|-----------|----------|------------------|---------|
| 1 | Đăng nhập Admin | Admin login | - | Role: QC |
| 2 | Vào Sản phẩm & Lô kiểm duyệt | Batch list | - | - |
| 3 | Tạo lô mới | Create batch form | NCC, số lô, hạn dùng, ngành hàng | 1 ngành hàng/lô trong MVP |
| 4 | Đính kèm tài liệu | File upload | PDF/Doc tài liệu | Bắt buộc |
| 5 | Đính kèm ảnh kiểm duyệt | Image upload | Ảnh chứng cứ | Bắt buộc |
| 6 | Lưu lô | Save | - | Audit trail |
| 7 | Gán sản phẩm vào lô | Product assignment | Product IDs | - |

### Edge/Exception Paths

| ID | Trường hợp | Xử lý | Màn hình/Thông báo |
|----|------------|-------|-------------------|
| E4.1 | Thiếu tài liệu/ảnh | Không cho lưu | Validation error |
| E4.2 | Lô quá hạn | Cảnh báo, không cho bán | Alert + status update |

---

## Journey 5: Super Admin - Duyệt hủy đơn

### Happy Path

| Bước | Hành động | Màn hình | Dữ liệu thu thập | Ghi chú |
|------|-----------|----------|------------------|---------|
| 1 | Nhận thông báo yêu cầu hủy | Notification | - | - |
| 2 | Xem chi tiết yêu cầu | Cancel request detail | Lý do, ảnh chứng cứ | - |
| 3 | Duyệt/Từ chối | Approval action | Quyết định + ghi chú | - |
| 4 | (Nếu duyệt) Hệ thống tự động: | - | - | - |
| 4a | - Cập nhật trạng thái "Đã hủy" | - | - | - |
| 4b | - Khôi phục tồn kho | - | - | - |
| 4c | - Điều chỉnh hoa hồng | - | - | - |
| 4d | - Điều chỉnh công nợ COD/trả chậm | - | - | - |
| 5 | Ghi audit log đầy đủ | Audit log | Người tạo, người duyệt, thời gian | - |

---

## Data Touchpoints (Điểm thu thập dữ liệu)

| Hành trình | Dữ liệu chính | Module liên quan |
|------------|---------------|------------------|
| Đăng ký | Họ tên, SĐT, Công ty, Tầng | AUTH, USERS |
| KYC | CCCD, ngày sinh, mã NV | USERS, PAYMENTS |
| Đặt hàng | Sản phẩm, số lượng, điểm nhận, người nhận | ORDERS, CATALOG |
| Thanh toán | Phương thức, số tiền, trạng thái | PAYMENTS |
| Giao hàng | Ảnh POD, thời gian, shipper | ORDERS |
| COD | Số tiền thu, phiếu thu, đối soát ca | PAYMENTS |
| Hoa hồng | % theo ngành hàng, doanh số, tầng | REPORTING |
| Kiểm duyệt lô | NCC, số lô, tài liệu, ảnh | CATALOG |

---

## Integrations (Tích hợp)

| Hệ thống | Mục đích | Trạng thái |
|----------|----------|------------|
| ZNS (Zalo) | OTP, thông báo nhắc nhận hàng, nhắc nợ | → Open Question Q5 |
| Payment Gateway | Thanh toán online (nếu có) | → Open Question Q6 |
| Hệ thống quản lý kho | Đồng bộ tồn kho | Out of scope MVP, chừa cổng |

---

## Permissions/Roles per Step (Phân quyền)

| Hành động | Tầng 3 | Tầng 2 | Tầng 1 | Ops | QC | Finance | Support | Super Admin |
|-----------|--------|--------|--------|-----|-----|---------|---------|-------------|
| Đăng ký/Đăng nhập | ✓ | ✓ | ✓ | - | - | - | - | - |
| Mua hàng | ✓ | ✓ | ✓ | - | - | - | - | - |
| Xem hoa hồng cá nhân | - | ✓ | ✓ (read-only) | - | - | - | - | - |
| Giao hàng/Thu COD | - | ✓ | - | - | - | - | - | - |
| Đối soát ca | - | ✓ | - | - | - | - | - | - |
| Quản lý đơn hàng | - | - | - | ✓ | - | - | - | ✓ |
| Tạo yêu cầu hủy | - | - | - | ✓ | - | - | - | - |
| Duyệt hủy đơn/hủy hàng | - | - | - | - | - | - | - | ✓ |
| Kiểm duyệt lô | - | - | - | - | ✓ | - | - | ✓ |
| Cấu hình hoa hồng | - | - | - | - | - | - | - | ✓ |
| Cấu hình trả chậm | - | - | - | - | - | ✓ | - | ✓ |
| Xem báo cáo | - | - | - | ✓ | ✓ | ✓ | - | ✓ |
| Quản lý ZNS template | - | - | - | - | - | - | - | ✓ |
| Phân quyền | - | - | - | - | - | - | - | ✓ |
