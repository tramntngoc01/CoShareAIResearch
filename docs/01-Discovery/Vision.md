
# Vision

## Problem Statement (Vấn đề cần giải quyết)

**Bối cảnh:** Công nhân tại các Khu Công Nghiệp (KCN) gặp khó khăn trong việc tiếp cận hàng hóa chất lượng với giá cả hợp lý. Họ thường phải mua sắm sau giờ làm việc, tốn thời gian di chuyển và không có nhiều lựa chọn.

**Vấn đề chính:**
- Kênh mua sắm trực tiếp giữa Nhà Cung Cấp (NCC) và công nhân KCN chưa tồn tại hoặc không hiệu quả
- Công nhân thiếu thời gian và phương tiện để mua sắm ngoài giờ làm
- Không có cơ chế trả chậm phù hợp với chu kỳ nhận lương
- Thiếu hệ thống affiliate để khuyến khích giới thiệu sản phẩm trong cộng đồng công nhân

## Target Users & Personas (Đối tượng người dùng)

### Persona 1: Công nhân KCN (Tầng 3 - End User)
- **Đặc điểm:** Làm việc tại các doanh nghiệp trong KCN, thu nhập trung bình, nhận lương theo kỳ
- **Nhu cầu:** Mua hàng tiện lợi, nhận hàng tại nơi làm việc, có thể trả chậm đến ngày lương
- **Hành vi:** Sử dụng smartphone, quen với Zalo

### Persona 2: Nhân viên giới thiệu (Tầng 2 - Affiliate + Shipper)
- **Đặc điểm:** Có thể là công nhân có ảnh hưởng trong nhóm, kiêm vai trò giao hàng/thu COD
- **Nhu cầu:** Kiếm thêm thu nhập từ hoa hồng affiliate, quản lý giao nhận tại điểm
- **Hành vi:** Tích cực giới thiệu sản phẩm, hỗ trợ đồng nghiệp mua hàng

### Persona 3: Quản lý mạng lưới (Tầng 1 - Network Leader)
- **Đặc điểm:** Đứng đầu mạng lưới affiliate, có nhiều Tầng 2 dưới quyền
- **Nhu cầu:** Theo dõi tổng quan doanh số và hoa hồng mạng lưới
- **Hành vi:** Phân tích báo cáo, phát triển mạng lưới (read-only trong MVP)

### Persona 4: Admin vận hành (Ops/QC/Finance/Support)
- **Đặc điểm:** Nhân viên vận hành hệ thống
- **Nhu cầu:** Quản lý sản phẩm, đơn hàng, kiểm duyệt lô hàng, đối soát COD/công nợ
- **Hành vi:** Làm việc trên Admin Portal

### Persona 5: Super Admin
- **Đặc điểm:** Quản trị cấp cao nhất
- **Nhu cầu:** Duyệt hủy đơn, quản lý cấu hình hệ thống, phân quyền
- **Hành vi:** Kiểm soát toàn bộ hệ thống

## Value Proposition (Giá trị mang lại)

| Đối tượng | Giá trị |
|-----------|---------|
| Công nhân KCN | Mua hàng tiện lợi, nhận tại nơi làm, trả chậm theo lương, giá tốt từ NCC |
| Tầng 2/Tầng 1 | Thu nhập thêm từ hoa hồng 3 tầng theo ngành hàng |
| NCC | Kênh phân phối trực tiếp đến công nhân KCN, giảm chi phí trung gian |
| Doanh nghiệp trong KCN | Phúc lợi cho nhân viên, không tốn nguồn lực vận hành |

## Success Metrics (Chỉ số thành công)

| Metric | Mô tả | Target | Ghi chú |
|--------|-------|--------|---------|
| MAU (Monthly Active Users) | Số user hoạt động/tháng | → Open Question Q1 | Chưa có từ khách hàng |
| Conversion Rate | Tỷ lệ đặt hàng / lượt truy cập | → Open Question Q2 | Chưa có từ khách hàng |
| Tỷ lệ nhận hàng | Đơn hàng được nhận / Tổng đơn | → Open Question Q3 | Chưa có từ khách hàng |
| Concurrent Users | Số user đồng thời | ≥ 1,000 | Yêu cầu từ khách hàng |
| Thời gian checkout | Số bước đến "Đặt hàng thành công" | < 5 bước | Theo tài liệu |
| Tỷ lệ quá hạn trả chậm | % đơn trả chậm quá hạn | → Open Question Q4 | Chưa có từ khách hàng |

## Constraints (Ràng buộc)

| Loại | Chi tiết |
|------|----------|
| Timeline | ~16 tuần (4 tháng), chia 5 giai đoạn |
| Platform | Web-first, responsive trên mobile (không phát triển app trong GĐ1) |
| Admin Portal | Không cần responsive |
| Authentication | OTP qua ZNS (Zalo Notification Service) |
| Payment | COD, Chuyển khoản, Trả chậm (theo cấu hình công ty) |
| Performance | Hỗ trợ ≥ 1,000 concurrent users |
| Stack | .NET 9 + React + PostgreSQL (theo Stack Profile) |

## Assumptions (Giả định)

| ID | Giả định | Cần xác nhận |
|----|----------|--------------|
| A1 | Danh sách nhân sự được Admin upload từ file CSV/XLSX do công ty cung cấp | Có |
| A2 | ZNS đã được đăng ký và có template OTP sẵn sàng sử dụng | Có |
| A3 | Mỗi công ty có ít nhất 1 điểm nhận hàng mặc định | Có |
| A4 | Hoa hồng được tính theo % trên từng ngành hàng, không phải từng sản phẩm cụ thể | Có |
| A5 | Một lô hàng trong MVP giới hạn 1 ngành hàng (có thể mở rộng sau) | Có |
| A6 | Shipper được gán cho 1 công ty trong MVP | Có |

## Risks (Rủi ro)

| ID | Rủi ro | Impact | Mitigation |
|----|--------|--------|------------|
| R1 | Chi phí ZNS cao nếu gửi OTP thường xuyên | Cao | Token-based auto-login sau lần đầu |
| R2 | Dữ liệu nhân sự không khớp khi import | Trung bình | Fuzzy matching + workflow chờ duyệt |
| R3 | Quá hạn trả chậm gây mất vốn | Cao | Hạn mức + khóa mua khi quá hạn |
| R4 | Concurrent users vượt 1,000 gây quá tải | Cao | Load testing, horizontal scaling |
| R5 | Phức tạp trong tính hoa hồng 3 tầng với lịch sử hiệu lực | Trung bình | Thiết kế DB cẩn thận, test kỹ |
