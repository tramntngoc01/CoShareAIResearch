
# MVP Scope

## MVP Goals (Mục tiêu MVP)

| ID | Mục tiêu | Đo lường |
|----|----------|----------|
| G1 | Cung cấp kênh mua sắm trực tiếp NCC → Công nhân KCN | Có thể đặt hàng, thanh toán, nhận hàng tại điểm |
| G2 | Hỗ trợ thanh toán COD, Chuyển khoản, Trả chậm | 3 phương thức hoạt động đầy đủ |
| G3 | Vận hành hệ thống affiliate 3 tầng | Tính hoa hồng chính xác theo ngành hàng |
| G4 | Quản lý lô hàng với kiểm duyệt đầy đủ | Lưu tài liệu & ảnh audit trail |
| G5 | Hỗ trợ ≥ 1,000 concurrent users | Load test đạt yêu cầu |

---

## In Scope - MVP (Phạm vi thực hiện)

### Portal Admin

| # | Tính năng | Mô tả |
|---|-----------|-------|
| 1 | Đăng nhập Admin | SĐT/Email + mật khẩu hoặc OTP ZNS |
| 2 | Phân quyền | Super Admin, Ops, QC, Finance, Support |
| 3 | Dashboard | Widget: đơn hôm nay, tỷ lệ nhận hàng, công nợ, COD tồn, top ngành hàng, cảnh báo quá hạn |
| 4 | Quản lý người dùng & Tầng | Import CSV/XLSX, fuzzy matching, gán tầng |
| 5 | Quản lý sản phẩm | CRUD sản phẩm, ngành hàng, giá/ưu đãi, chính sách đổi trả |
| 6 | Quản lý lô kiểm duyệt | Tạo lô (NCC, số lô, hạn dùng), đính kèm tài liệu & ảnh |
| 7 | Quản lý đơn hàng | Danh sách, lọc, xác nhận xuất kho, đánh dấu đến điểm, xác nhận nhận hàng |
| 8 | Workflow hủy đơn | Ops tạo yêu cầu → Super Admin duyệt → Hoàn tồn kho, điều chỉnh hoa hồng/công nợ |
| 9 | Quản lý hủy hàng | Phiếu hủy (hư hại/mất mát) → Super Admin duyệt → Trừ tồn, báo cáo |
| 10 | Quản lý điểm nhận | CRUD điểm nhận trong công ty, default điểm nhận |
| 11 | Thanh toán & COD | Phiếu thu điện tử, sổ quỹ theo điểm/ca, chốt ca, đối soát |
| 12 | Cấu hình trả chậm | Bật/tắt theo công ty, ngày lương, hạn mức (số đơn, tổng tiền) |
| 13 | Hoa hồng theo ngành hàng | Cấu hình % theo Category, lịch sử hiệu lực (effective from/to) |
| 14 | Báo cáo | Doanh số, hoa hồng theo tầng, tình trạng nhận hàng, công nợ, COD |
| 15 | Quản lý đổi trả | Tạo yêu cầu, ảnh chứng cứ, điều chỉnh tồn/doanh thu/hoa hồng/công nợ |
| 16 | Thông báo/ZNS | Quản lý template (OTP, nhắc nhận hàng, nhắc nợ, thay đổi hoa hồng) |
| 17 | Cấu hình hệ thống | Logo, pháp lý, điều khoản, phân quyền, audit log |
| 18 | Xuất PDF phiếu giao hàng | Format A5 |

### Portal End User (Web responsive)

| # | Tính năng | Mô tả |
|---|-----------|-------|
| 19 | Đăng ký/Đăng nhập | SĐT + OTP ZNS (lần đầu), auto-login bằng token sau đó |
| 20 | KYC trả chậm | CCCD, ngày sinh, mã NV (optional nếu công ty bật trả chậm) |
| 21 | Trang chủ | Banner deal theo công ty/ngành hàng, danh mục, lối tắt điểm nhận |
| 22 | Danh mục & Sản phẩm | Hiển thị theo kiểm duyệt & tồn khả dụng |
| 23 | Chi tiết sản phẩm | Ảnh, mô tả, video, giá/ưu đãi, chính sách đổi trả, lượt bán, số lượng tồn |
| 24 | Giỏ hàng | Thêm/xóa/sửa số lượng |
| 25 | Checkout | Chọn điểm nhận, thông tin người nhận, ghi chú, chọn phương thức thanh toán |
| 26 | Thanh toán | COD, Chuyển khoản (hiển thị thông tin TK + cú pháp), Trả chậm (nếu đủ điều kiện) |
| 27 | Trạng thái đơn hàng | Timeline trạng thái, mã nhận hàng QR/PIN |
| 28 | Đổi trả | Tạo yêu cầu tại điểm nhận với ảnh/ghi chú |
| 29 | Trợ lý AI | FAQ, tra cứu điểm nhận, tình trạng đơn, thu thập xu hướng người dùng |

### Tính năng Tầng 2 (bổ sung)

| # | Tính năng | Mô tả |
|---|-----------|-------|
| 30 | Xem hoa hồng/chiết khấu cá nhân | Kỳ hiện tại, kỳ trước, đơn góp doanh thu, điều chỉnh đổi trả |
| 31 | Giao nhận & Thu COD | Danh sách đơn phân công, quét QR, ảnh POD, phiếu thu |
| 32 | Đối soát ca | Tổng đơn, tổng COD, số tiền nộp lại |

### Tính năng Tầng 1 (bổ sung)

| # | Tính năng | Mô tả |
|---|-----------|-------|
| 33 | Tổng quan mạng lưới & hoa hồng | Read-only trong MVP |

---

## Out of Scope - MVP (Ngoài phạm vi MVP)

| # | Tính năng | Lý do | Giai đoạn dự kiến |
|---|-----------|-------|-------------------|
| O1 | Mobile App (iOS/Android) | Web-first trong GĐ1 | GĐ5+ |
| O2 | Admin responsive | Không cần theo yêu cầu | N/A |
| O3 | Lô hàng nhiều ngành (multi-category) | Đơn giản hóa MVP | GĐ5 |
| O4 | Overflow-assist cho Tầng 2 | Chưa rõ yêu cầu | GĐ5 |
| O5 | Dashboard hoa hồng chi tiết Tầng 1 | MVP chỉ read-only | GĐ5 |
| O6 | Báo cáo tài chính nâng cao | MVP chỉ báo cáo vận hành | GĐ5 |
| O7 | Shipper quản lý nhiều công ty | MVP giới hạn 1 công ty | GĐ5+ |
| O8 | Kết nối phần mềm quản lý kho bên thứ 3 | Chừa cổng, chưa tích hợp | GĐ5+ |

---

## Acceptance Criteria for MVP Completion (Tiêu chí hoàn thành MVP)

### Functional

| # | Tiêu chí | Pass/Fail |
|---|----------|-----------|
| AC1 | User đăng ký bằng OTP ZNS, hệ thống match tầng đúng | |
| AC2 | User duyệt sản phẩm, thêm giỏ hàng, checkout < 5 bước | |
| AC3 | Thanh toán COD/Chuyển khoản/Trả chậm hoạt động đúng | |
| AC4 | Tầng 2 thu COD, chốt ca, đối soát thành công | |
| AC5 | Hoa hồng tính đúng theo % ngành hàng và lịch sử hiệu lực | |
| AC6 | Workflow hủy đơn/hủy hàng với Super Admin duyệt | |
| AC7 | Lô kiểm duyệt lưu đầy đủ tài liệu & ảnh | |
| AC8 | Báo cáo doanh số, hoa hồng, COD, công nợ chạy đúng | |

### Non-Functional

| # | Tiêu chí | Target |
|---|----------|--------|
| NFR1 | Concurrent users | ≥ 1,000 |
| NFR2 | Page load time | < 3s (cần xác nhận → Open Question) |
| NFR3 | Uptime | 99.5% (cần xác nhận → Open Question) |
| NFR4 | Mobile responsive | Hoạt động trên iOS/Android browser |

---

## Dependencies (Phụ thuộc)

### Internal

| Dependency | Owner | Status |
|------------|-------|--------|
| OpenAPI contract hoàn chỉnh | Tech Lead | Pending |
| DB schema per module | Tech Lead | Pending |
| UI/UX wireframe & prototype | BA/Designer | GĐ1 |

### External

| Dependency | Provider | Status | Open Question |
|------------|----------|--------|---------------|
| ZNS OTP Service | Zalo | → Q5 | Cần xác nhận đã đăng ký? |
| Payment Gateway (nếu có) | → Q6 | Unknown | Chuyển khoản manual hay tích hợp? |
| Danh sách nhân sự công ty | Khách hàng | Per company | Format CSV/XLSX |

---

## Risks & Mitigations (Rủi ro & Giảm thiểu)

| ID | Rủi ro | Probability | Impact | Mitigation |
|----|--------|-------------|--------|------------|
| R1 | Dữ liệu nhân sự không chuẩn/trùng | Cao | Trung bình | Fuzzy matching + manual review |
| R2 | Chi phí ZNS cao | Trung bình | Cao | Token auto-login, giảm OTP |
| R3 | Tính toán hoa hồng 3 tầng phức tạp | Trung bình | Cao | Unit test kỹ, edge cases |
| R4 | Trả chậm quá hạn nhiều | Trung bình | Cao | Hạn mức, khóa mua, nhắc nợ ZNS |
| R5 | Load > 1,000 users | Thấp | Cao | Load testing GĐ3, horizontal scale |
| R6 | Go-live pilot thất bại | Thấp | Cao | UAT kỹ GĐ3, rollback plan |

---

## Milestones (Mốc thời gian)

| Milestone | Tuần | Deliverables |
|-----------|------|--------------|
| M1 - Khởi động & Thiết kế | 1-3 | Tài liệu scope, wireframe, prototype, kiến trúc |
| M2 - Phát triển MVP | 4-9 | Admin Portal + User Portal hoàn chỉnh |
| M3 - Testing & UAT | 10-11 | Staging deploy, pilot 1-2 công ty, fix bugs |
| M4 - Go-live Pilot | 12 | Production deploy công ty đầu tiên |
| M5 - Mở rộng (optional) | 13-16 | Multi-category lô, dashboard nâng cao |
