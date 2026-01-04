
# Stories — ADMIN: Admin Console

## Định dạng chung
- Mỗi story tuân theo format:
	- User story (As a / I want / So that).
	- Priority (P0/P1/P2) theo phạm vi MVP.
	- Definition of Ready (DoR) checklist.
	- Acceptance Criteria (AC) đo lường được.
	- Sample data (fake) theo dạng entity hoặc request/response, **không** dùng dữ liệu thật.
	- Ít nhất 3 edge cases.
	- API mapping: "TBD (Tech Lead)" (không liệt kê path cụ thể).
	- Screens: danh sách màn hình nếu đã biết, hoặc `TBD`.

## Story list

---

### US-ADMIN-001 — Quản lý Công ty và Điểm nhận
**As a** Super Admin/Ops  
**I want** tạo và quản lý danh sách Công ty và Điểm nhận  
**So that** các module khác có dữ liệu chuẩn để gán User và đơn hàng đúng nơi

**Priority:** P0  
**Dependencies:**
- SRS-ADMIN: FR-ADMIN-001, BR-ADMIN-002
- SRS-USERS (User gắn với Công ty/Điểm nhận)
- SRS-ORDERS, SRS-PAYMENTS (đơn hàng gắn Công ty/Điểm nhận)

**Definition of Ready**
- [ ] Danh sách trường tối thiểu cho Company (code, name, zone, trạng thái...) được chốt.
- [ ] Danh sách trường tối thiểu cho PickupPoint (companyId, name, isDefault, trạng thái...) được chốt.
- [ ] Rule không được xoá vật lý Company/PickupPoint đang được tham chiếu được mô tả rõ trong Business-Rules/DB-ADMIN.

**Acceptance Criteria**
- AC1: Admin có thể tạo mới Company với mã không trùng và thêm ít nhất một PickupPoint cho Company đó.
- AC2: Admin có thể đặt/đổi PickupPoint mặc định cho một Company; sau thao tác, đúng một PickupPoint có isDefault = true.
- AC3: Khi Admin đánh dấu một PickupPoint là inactive, hệ thống không cho phép tạo mới User/đơn hàng gắn với PickupPoint đó (nhưng vẫn giữ liên kết lịch sử).

**Sample data (fake)**
- Company:
	- companyId: 100
	- companyCode: "CTY_MAY_A"
	- companyName: "Công ty May KCN A"
	- zone: "KCN A"
	- status: "ACTIVE"
- PickupPoint:
	- pickupPointId: 1001
	- companyId: 100
	- name: "Điểm nhận A - Cổng chính"
	- isDefault: true
	- status: "ACTIVE"

**Edge cases (min 3)**
- EC1: Tạo Company mới với companyCode đã tồn tại ở trạng thái ACTIVE → hệ thống từ chối, hiển thị lỗi "Mã Công ty đã tồn tại".
- EC2: Đặt 2 PickupPoint cùng lúc là mặc định (do 2 Admin thao tác gần nhau) → cơ chế concurrency phải đảm bảo sau commit chỉ có 1 isDefault = true, bản còn lại bị rollback hoặc cảnh báo.
- EC3: Đánh dấu Company là inactive trong khi vẫn còn User/đơn hàng/công nợ liên quan → hệ thống chặn hoặc yêu cầu xác nhận theo rule toàn cục (mặc định không cho phép xoá, chỉ cho phép inactive với cảnh báo rõ).
- EC4: Xoá logic một PickupPoint đang là mặc định → hệ thống yêu cầu chọn PickupPoint khác làm mặc định hoặc bỏ cờ mặc định trước khi cho phép.

**API mapping (when available)**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình Admin Portal: Quản lý Công ty & Điểm nhận)

---

### US-ADMIN-002 — Quản lý tài khoản Admin và phân quyền
**As a** Super Admin  
**I want** tạo và gán role cho các tài khoản Admin  
**So that** mỗi Admin chỉ truy cập được đúng phạm vi chức năng được phép

**Priority:** P0  
**Dependencies:**
- SRS-ADMIN: FR-ADMIN-002, BR-ADMIN-001
- SRS-AUTH (đăng nhập Admin)

**Definition of Ready**
- [ ] Danh sách role Admin (SUPER_ADMIN, OPS, QC, FINANCE, SUPPORT, ...) được chốt.
- [ ] Rule ai được phép tạo Admin mới (chỉ Super Admin hay cho phép delegate) được xác nhận.
- [ ] Thiết kế UI màn hình danh sách Admin và form gán role được duyệt.

**Acceptance Criteria**
- AC1: Super Admin có thể tạo tài khoản Admin mới (hoặc liên kết tới user hiện có) và gán ít nhất một role.
- AC2: Hệ thống không cho phép lưu AdminUser mà không có bất kỳ role nào.
- AC3: Khi Super Admin thu hồi một role khỏi AdminUser, các lần truy cập mới của Admin đó không còn thấy chức năng tương ứng (tuỳ theo cấu hình AUTH/Frontend), và hành động thu hồi được ghi Audit Log.

**Sample data (fake)**
- AdminUser A:
	- adminId: 5001
	- loginId: "ops_01@example.com"
	- roles: ["OPS"]
- AdminUser B:
	- adminId: 5002
	- loginId: "fin_sup@example.com"
	- roles: ["FINANCE", "SUPPORT"]

**Edge cases (min 3)**
- EC1: Super Admin cố gắng gán role không tồn tại (ví dụ: "DEVOPS") → hệ thống từ chối và hiển thị lỗi.
- EC2: Super Admin vô tình xoá hết role của một Admin và lưu → hệ thống từ chối (theo AC2), yêu cầu Admin phải có ít nhất một role.
- EC3: Hai Super Admin cùng lúc chỉnh sửa role của cùng một Admin (người A thêm role, người B xoá role) → cơ chế concurrency phải đảm bảo tránh mất thay đổi không mong muốn (optimistic locking hoặc merge rule).
- EC4: Một Admin tự gỡ quyền của chính mình (nếu được phép) dẫn đến mất quyền truy cập màn hình phân quyền → cần policy rõ ràng; story này giả định chỉ Super Admin khác mới có quyền đó.

**API mapping (when available)**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình Admin Portal: Quản lý Admin & phân quyền)

---

### US-ADMIN-003 — Cấu hình thông tin hệ thống (logo, tên hệ thống, pháp lý, điều khoản)
**As a** Super Admin  
**I want** cấu hình logo, tên hệ thống và thông tin pháp lý/điều khoản  
**So that** Portal hiển thị đúng brand và thông tin pháp lý của CoShare

**Priority:** P1  
**Dependencies:**
- SRS-ADMIN: FR-ADMIN-003, BR-ADMIN-003

**Definition of Ready**
- [ ] Danh sách key cấu hình hệ thống (systemName, logoUrl, legalEntityName, legalAddress, termsUrl, ...) được chốt.
- [ ] Quy ước nơi lưu cấu hình (DB, config service...) được thống nhất ở mức thiết kế kỹ thuật.
- [ ] Quy định Data-Classification cho các trường này (đa phần non-PII) được xác nhận.

**Acceptance Criteria**
- AC1: Super Admin có thể cập nhật logoUrl, systemName, legalEntityName, legalAddress, termsUrl và lưu thành công.
- AC2: Sau khi cập nhật, Portal End User và Admin Portal sử dụng giá trị mới (ví dụ: hiển thị logo mới) sau khi người dùng reload trang, không cần redeploy.
- AC3: Mỗi lần thay đổi cấu hình, hệ thống ghi Audit Log với thông tin: người thực hiện, thời gian, loại cấu hình được thay đổi.

**Sample data (fake)**
- Trước:
	- systemName: "CoShare"
	- logoUrl: "https://cdn.example.com/logo-old.png"
	- legalEntityName: "Công ty CP CoShare Việt Nam"
- Sau:
	- systemName: "CoShare KCN"
	- logoUrl: "https://cdn.example.com/logo-new.png"
	- legalEntityName: "Công ty CP CoShare Việt Nam"

**Edge cases (min 3)**
- EC1: Super Admin nhập logoUrl trỏ tới đường dẫn 404 → hệ thống không thể validate tất cả URL; UI có thể hiển thị logo lỗi, trách nhiệm kiểm tra nội dung thuộc Super Admin.
- EC2: Super Admin nhập termsUrl trống hoặc không hợp lệ → cần rule: có bắt buộc luôn có termsUrl hay không; story này giả định có validation cơ bản cho field bắt buộc.
- EC3: Hai Super Admin cùng sửa SystemConfig gần như đồng thời → cấu hình cuối cùng là lần lưu sau cùng; cần cân nhắc có khoá/phiên bản hoá hay không (Open Question).

**API mapping (when available)**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình Admin Portal: Cấu hình hệ thống)

---

### US-ADMIN-004 — Xem Dashboard tổng quan vận hành
**As a** Ops/Finance/Super Admin  
**I want** xem Dashboard tổng quan với các chỉ số chính  
**So that** tôi nắm được tình hình vận hành (đơn hàng, nhận hàng, COD, công nợ, top ngành hàng)

**Priority:** P1  
**Dependencies:**
- SRS-ADMIN: FR-ADMIN-004
- SRS-REPORTING, SRS-ORDERS, SRS-PAYMENTS, SRS-CATALOG (cung cấp số liệu)

**Definition of Ready**
- [ ] Danh sách widget tối thiểu cho Dashboard MVP được chốt (đơn hôm nay, tỷ lệ nhận hàng, COD tồn, công nợ, top ngành hàng, cảnh báo quá hạn).
- [ ] Quy tắc thống kê (ví dụ: "đơn hôm nay" tính theo created_at hay confirmed_at) được định nghĩa trong REPORTING.
- [ ] Thiết kế UI Dashboard (layout, card, filter thời gian) được phê duyệt.

**Acceptance Criteria**
- AC1: Khi Admin truy cập Dashboard, các widget tối thiểu hiển thị được số liệu giả lập/hard-coded ban đầu; khi tích hợp REPORTING, hiển thị số liệu thật theo quy tắc đã chốt.
- AC2: Nếu một widget không lấy được dữ liệu (API downstream lỗi), Dashboard vẫn load được, widget đó hiển thị trạng thái lỗi thân thiện (ví dụ: "Không tải được dữ liệu").
- AC3: Nếu Super Admin tắt một widget trong cấu hình (nếu tính năng bật/tắt widget được chốt), widget đó không còn xuất hiện với bất kỳ Admin nào.

**Sample data (fake)**
- Widget "Đơn hôm nay":
	- totalOrdersToday: 245
	- receivedRate: 0.92
- Widget "COD tồn":
	- totalCodOpen: 58_000_000
- Widget "Công nợ trả chậm":
	- totalDebtOpen: 320_000_000

**Edge cases (min 3)**
- EC1: Một trong các dịch vụ REPORTING/PAYMENTS không trả lời kịp (timeout) → chỉ widget đó báo lỗi, không chặn Dashboard.
- EC2: Không có dữ liệu cho khoảng thời gian được chọn (ví dụ: ngày chưa có đơn) → các widget hiển thị 0 hoặc trạng thái "Không có dữ liệu" thay vì lỗi.
- EC3: Admin không có quyền xem một số chỉ số (ví dụ: Finance chỉ xem được COD/công nợ, Ops chỉ xem đơn/tỷ lệ nhận hàng) → Dashboard phải ẩn hoặc vô hiệu hoá widget theo role (phối hợp với AUTH/ADMIN RBAC).

**API mapping (when available)**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình Admin Portal: Dashboard tổng quan)

---

### US-ADMIN-005 — Tra cứu Audit Log cho các thao tác nhạy cảm
**As a** Super Admin (và các vai trò được phép)  
**I want** tra cứu Audit Log theo thời gian, loại hành động và correlationId  
**So that** tôi có thể kiểm tra lại các thao tác quan trọng và xử lý khiếu nại

**Priority:** P1  
**Dependencies:**
- SRS-ADMIN: FR-ADMIN-005, BR-ADMIN-003
- SRS-00-Overview (yêu cầu Audit Log toàn cục)

**Definition of Ready**
- [ ] Cấu trúc tối thiểu của AuditLog record được chốt (timestamp, actorType, actorId, actionType, module, correlationId, result, metadata...).
- [ ] Chính sách phân quyền xem Audit Log (ai được xem loại log nào) được xác nhận với Security/PO.
- [ ] Giới hạn thời gian tìm kiếm/log retention (ví dụ: tối đa N ngày/1 truy vấn) được Tech Lead/PO chốt.

**Acceptance Criteria**
- AC1: Super Admin có thể lọc Audit Log theo tối thiểu: khoảng thời gian, module, actionType, actorId, correlationId.
- AC2: Kết quả trả về phân trang, hiển thị được các trường chính mà không chứa PII/ dữ liệu nhạy cảm (CCCD, OTP...).
- AC3: Khi không có bản ghi nào phù hợp filter, hệ thống hiển thị rõ "Không có dữ liệu" thay vì lỗi.

**Sample data (fake)**
- Truy vấn:
	- from: "2026-01-01T00:00:00Z"
	- to: "2026-01-04T23:59:59Z"
	- module: "ORDERS"
	- actionType: "CANCEL_ORDER_APPROVED"
- Một bản ghi kết quả:
	- logId: 200001
	- timestamp: "2026-01-04T03:15:00Z"
	- actorType: "AdminUser"
	- actorId: 5001
	- actionType: "CANCEL_ORDER_APPROVED"
	- module: "ORDERS"
	- correlationId: "corr-order-0001"
	- result: "SUCCESS"
	- metadata: {"orderCode": "ORD-2026-0001"}

**Edge cases (min 3)**
- EC1: Super Admin chọn khoảng thời gian rất lớn (ví dụ: 2 năm) → hệ thống áp dụng giới hạn (ví dụ: tối đa N ngày/1 truy vấn) hoặc cảnh báo, tránh query quá nặng.
- EC2: Support (không phải Super Admin) truy cập màn hình Audit Log nhưng chỉ được xem một subset actionType/module → cần rule phân quyền rõ; story này giả định tối thiểu Support không xem được log phân quyền/cấu hình hệ thống.
- EC3: Trong cùng một khoảng thời gian có rất nhiều bản ghi → hệ thống phải phân trang và có chỉ báo hiệu năng chấp nhận được (thời gian phản hồi trong ngưỡng NFR toàn hệ thống).

**API mapping (when available)**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình Admin Portal: Audit Log)

