
# SRS — ADMIN: Admin Console

## 1. Module purpose & boundaries (in/out)

**Mục đích**
- Cung cấp **Admin Portal** và các chức năng cấu hình hệ thống trung tâm cho các vai trò Admin (Super Admin, Ops, QC, Finance, Support).
- Quản lý các **master data & cấu hình dùng chung** như: Công ty, Điểm nhận, cấu hình hệ thống (logo, pháp lý, điều khoản), cấu hình phân quyền Admin.
- Cung cấp **khả năng xem và tra cứu Audit Log** cho các thao tác nghiệp vụ quan trọng trên toàn hệ thống.

**Trong phạm vi (IN)**
- Khung **Admin Portal**: nhóm menu, điều hướng tới các module CATALOG, ORDERS, PAYMENTS, USERS, NOTIFICATIONS, REPORTING (mỗi module vẫn chịu trách nhiệm nghiệp vụ riêng).
- Quản lý **Công ty** và **Điểm nhận** ở mức cấu hình vận hành (theo MVP Scope mục "Quản lý điểm nhận"):
	- Tạo/cập nhật Công ty và danh sách Điểm nhận của từng Công ty.
	- Đánh dấu Điểm nhận mặc định cho mỗi Công ty.
- Quản lý **vai trò Admin** và phân quyền cơ bản:
	- Danh mục role Admin: Super Admin, Ops, QC, Finance, Support (theo MVP Scope).
	- Gán role cho tài khoản Admin (kết hợp với AUTH để đăng nhập).
- Quản lý **cấu hình hệ thống** ở mức high-level (theo MVP Scope mục "Cấu hình hệ thống"):
	- Logo, thông tin pháp lý, điều khoản sử dụng hiển thị trên Portal.
	- Một số cờ cấu hình toàn hệ thống (ví dụ: bật/tắt các module/báo cáo nhất định) – chi tiết cần làm rõ thêm.
- Quản lý và **tra cứu Audit Log** ở mức tổng quan, cho phép Super Admin xem lịch sử các thao tác nhạy cảm (hủy đơn, hủy hàng, cấu hình, phân quyền...) được các module khác ghi lại.

**Ngoài phạm vi (OUT)**
- Nghiệp vụ chi tiết của từng module:
	- Quản lý sản phẩm, ngành hàng, lô kiểm duyệt, tồn kho (thuộc CATALOG).
	- Đặt hàng, trạng thái đơn, đổi trả, workflow hủy đơn (thuộc ORDERS, CATALOG, PAYMENTS).
	- COD, chuyển khoản, trả chậm, công nợ, đối soát (thuộc PAYMENTS).
	- Nội dung & gửi thông báo ZNS (thuộc NOTIFICATIONS).
	- Báo cáo doanh số, hoa hồng, công nợ, COD (thuộc REPORTING).
- Luồng đăng nhập Admin, xác thực token (thuộc AUTH; ADMIN chỉ cấu hình role và quyền, không xử lý AuthN).
- Chi tiết thiết kế UI/UX và layout màn hình cụ thể (được mô tả trong tài liệu UI, không trong SRS).
- Thiết kế chi tiết của cơ chế RBAC trong code (.NET policy, attribute, v.v.) – được mô tả ở mức kiến trúc, không ở SRS.

---

## 2. Personas/roles involved

Theo Glossary và Discovery, ADMIN chủ yếu phục vụ các vai trò sau:

- **Super Admin**
	- Quản trị cấp cao nhất của hệ thống.
	- Quản lý phân quyền Admin, cấu hình hệ thống, xem toàn bộ Audit Log.
- **Ops**
	- Vận hành đơn hàng: xem, lọc, xác nhận xuất kho, đánh dấu đến điểm nhận, yêu cầu hủy đơn.
	- Sử dụng Admin Portal để truy cập các màn hình ORDERS/REPORTING liên quan.
- **QC**
	- Kiểm duyệt lô hàng: tạo lô, đính kèm tài liệu & ảnh.
	- Sử dụng Admin Portal để truy cập các màn hình CATALOG.
- **Finance**
	- Quản lý thanh toán, COD, trả chậm, công nợ, đối soát.
	- Sử dụng Admin Portal để truy cập các màn hình PAYMENTS/REPORTING và một phần cấu hình trả chậm.
- **Support**
	- Hỗ trợ người dùng: tra cứu đơn, kiểm tra trạng thái, hỗ trợ các vấn đề vận hành.
	- Có thể cần truy cập một phần Audit Log và dữ liệu tổng hợp để trả lời khách (mức độ quyền xem chi tiết cần được chốt).

- **Các module hệ thống khác (tác nhân kỹ thuật)**
	- AUTH, USERS, CATALOG, ORDERS, PAYMENTS, NOTIFICATIONS, REPORTING ghi Audit Log và đọc cấu hình ADMIN (ví dụ: Công ty, Điểm nhận mặc định, cấu hình hệ thống) để vận hành đúng.

---

## 3. Functional requirements (FR-ADMIN-###)

> Mọi API nội bộ đều chỉ ghi "API: TBD (Tech Lead)", không liệt kê path/verb cụ thể.

### FR-ADMIN-001 — Quản lý Công ty và Điểm nhận

**Mô tả**
- ADMIN phải cho phép Super Admin/Ops quản lý danh sách **Công ty** (doanh nghiệp trong KCN) và **Điểm nhận** tương ứng.
- Thông tin này được dùng bởi các module khác (USERS, ORDERS, PAYMENTS, REPORTING) để gán User, đơn hàng, ca giao hàng theo đúng Công ty và Điểm nhận.

**Luồng chuẩn (happy path)**
1. Admin truy cập màn hình "Quản lý Công ty" trên Admin Portal.
2. Admin có thể tạo mới Công ty với các thông tin cơ bản (ví dụ: tên Công ty, mã Công ty, KCN liên kết; chi tiết trường sẽ được chốt trong DB-ADMIN).
3. Trong chi tiết một Công ty, Admin có thể quản lý danh sách Điểm nhận:
	- Tạo mới Điểm nhận.
	- Cập nhật tên/địa điểm Điểm nhận.
	- Gán/đổi Điểm nhận mặc định cho Công ty.
4. Hệ thống lưu thay đổi và ghi Audit Log cho các thao tác tạo/cập nhật/xoá logic.

**Tiêu chí kiểm thử (testable)**
- AC1: Không thể tạo 2 Công ty với cùng mã định danh (mã Công ty) trong trạng thái Active.
- AC2: Mỗi Công ty luôn có **tối đa 1 Điểm nhận mặc định** tại một thời điểm; nếu Admin đặt một Điểm nhận mới là mặc định, Điểm nhận trước đó phải bị bỏ cờ mặc định.
- AC3: Khi một Điểm nhận bị đánh dấu không còn sử dụng (inactive hoặc xoá logic), hệ thống không cho phép gán mới các User/đơn hàng vào Điểm nhận đó (các liên kết lịch sử vẫn được giữ cho tra cứu).

**Ví dụ dữ liệu (fake)**
- Công ty:
	- companyId: 100
	- companyCode: "CTY_MAY_A"
	- companyName: "Công ty May KCN A"
	- zone: "KCN A"
- Điểm nhận:
	- pickupPointId: 1001
	- companyId: 100
	- name: "Điểm nhận A - Cổng chính"
	- isDefault: true

---

### FR-ADMIN-002 — Quản lý vai trò Admin và phân quyền cơ bản

**Mô tả**
- ADMIN phải cho phép Super Admin quản lý danh sách **role Admin** (Super Admin, Ops, QC, Finance, Support…) và gán role cho các tài khoản Admin.
- Vai trò này được AUTH sử dụng để cấp quyền truy cập tương ứng trên Admin Portal.

**Luồng chuẩn**
1. Super Admin truy cập màn hình "Quản lý Admin & phân quyền".
2. Super Admin có thể:
	- Tạo tài khoản Admin mới hoặc liên kết tài khoản Admin với User sẵn có (chi tiết luồng lấy dữ liệu từ USERS/AUTH cần được chốt).
	- Gán một hoặc nhiều role cho mỗi Admin (ví dụ: vừa Finance vừa Support).
3. Hệ thống lưu thông tin role và đảm bảo các module khác (AUTH, API gateway) có thể tra cứu quyền khi xử lý request.
4. Mọi thay đổi phân quyền được ghi Audit Log.

**Tiêu chí kiểm thử**
- AC1: Chỉ **Super Admin** mới có quyền gán hoặc thu hồi role Admin cho tài khoản Admin khác.
- AC2: Một tài khoản Admin phải có ít nhất 1 role để đăng nhập Admin Portal; nếu không có role, truy cập sẽ bị từ chối.
- AC3: Sau khi role của Admin bị thay đổi, những lần truy cập mới phải áp dụng quyền mới (ví dụ: không còn thấy menu/báo cáo trước đây), theo cơ chế cache/refresh được thiết kế ở AUTH.

**Ví dụ dữ liệu (fake)**
- AdminUser:
	- adminId: 5001
	- loginId: "ops_01@example.com"
	- roles: ["OPS"]
- AdminUser khác:
	- adminId: 5002
	- loginId: "finance_support@example.com"
	- roles: ["FINANCE", "SUPPORT"]

---

### FR-ADMIN-003 — Cấu hình hệ thống (logo, pháp lý, điều khoản, cài đặt chung)

**Mô tả**
- ADMIN phải cho phép Super Admin cấu hình một số thông tin hệ thống chung:
	- Logo, tên hệ thống hiển thị trên Portal End User và Admin Portal.
	- Thông tin pháp lý cơ bản (tên pháp nhân, địa chỉ, thông tin liên hệ) để hiển thị ở chân trang.
	- Điều khoản sử dụng/Chính sách (link hoặc nội dung text) để hiển thị cho người dùng.
- Các module frontend (React) sẽ đọc cấu hình này để hiển thị, không hard-code.

**Luồng chuẩn**
1. Super Admin truy cập màn hình "Cấu hình hệ thống".
2. Super Admin cập nhật các trường cấu hình cho toàn hệ thống.
3. Hệ thống lưu cấu hình mới, ghi Audit Log, và các client lần tải trang sau sẽ hiển thị thông tin mới.

**Tiêu chí kiểm thử**
- AC1: Khi Super Admin thay đổi logo hoặc tên hệ thống, Portal End User và Admin Portal sử dụng giá trị mới sau khi refresh, không cần deploy lại code.
- AC2: Khi cập nhật điều khoản sử dụng, người dùng mới phải nhìn thấy phiên bản điều khoản mới; cách xử lý với người dùng hiện hữu (có yêu cầu re-accept hay không) là Open Question.
- AC3: Mọi thay đổi cấu hình hệ thống phải có log ghi lại: ai thay đổi, thay đổi trường gì, trước/sau (hoặc ít nhất là giá trị mới).

**Ví dụ dữ liệu (fake)**
- SystemConfig:
	- systemName: "CoShare KCN"
	- logoUrl: "https://cdn.example.com/assets/coshare-logo.png"
	- legalEntityName: "Công ty CP CoShare Việt Nam"
	- legalAddress: "Tầng 10, Tòa nhà X, Quận Y, TP. HCM"
	- termsUrl: "https://coshare.example.com/terms"

---

### FR-ADMIN-004 — Xem Dashboard tổng quan vận hành

**Mô tả**
- Theo MVP Scope, Admin Portal phải có Dashboard tổng quan: đơn hôm nay, tỷ lệ nhận hàng, công nợ, COD tồn, top ngành hàng, cảnh báo quá hạn.
- ADMIN chịu trách nhiệm **khung hiển thị dashboard và cấu hình widget**, còn số liệu chi tiết được lấy từ các module REPORTING, ORDERS, PAYMENTS, CATALOG.

**Luồng chuẩn (khung)**
1. Admin (Ops/Finance/Super Admin) truy cập màn hình Dashboard.
2. Hệ thống hiển thị các widget với số liệu lấy từ REPORTING/ORDERS/PAYMENTS/CATALOG thông qua API: TBD (Tech Lead).
3. (Tuỳ theo quyết định thiết kế) Super Admin có thể bật/tắt một số widget hoặc cấu hình tham số cơ bản (ví dụ: khung thời gian mặc định: hôm nay/7 ngày/... ).

**Tiêu chí kiểm thử**
- AC1: Dashboard hiển thị được tối thiểu các nhóm chỉ số được mô tả trong MVP Scope (ở mức high-level; chi tiết cột/số liệu thuộc REPORTING).
- AC2: Nếu một widget gặp lỗi khi gọi dữ liệu (API downstream lỗi), hệ thống vẫn load trang Dashboard và hiển thị thông báo phù hợp cho widget đó, không làm sập toàn bộ Dashboard.
- AC3: Nếu một widget bị Super Admin tắt, nó không còn hiển thị cho các Admin khác trên Dashboard.

**Ví dụ dữ liệu (fake)**
- Widget "Đơn hôm nay":
	- totalOrdersToday: 245
	- receivedRate: 0.92 (92%)
- Widget "COD tồn":
	- totalCodOpen: 58_000_000

---

### FR-ADMIN-005 — Tra cứu Audit Log cho các thao tác nhạy cảm

**Mô tả**
- ADMIN phải cung cấp màn hình **tra cứu Audit Log** để Super Admin (và một số vai trò được phép) có thể kiểm tra lịch sử các thao tác quan trọng:
	- Đăng ký/đăng nhập.
	- Hủy đơn, hủy hàng.
	- Thay đổi cấu hình hệ thống, cấu hình trả chậm, hoa hồng.
	- Thay đổi phân quyền Admin.

**Luồng chuẩn**
1. Super Admin truy cập màn hình "Audit Log".
2. Super Admin lọc log theo một số tiêu chí (ví dụ: thời gian, loại hành động, module, userId/adminId, correlationId).
3. Hệ thống trả về danh sách log, phân trang theo chuẩn chung, với các trường không chứa PII/ dữ liệu nhạy cảm (theo Data-Classification).

**Tiêu chí kiểm thử**
- AC1: Mỗi thao tác quan trọng (như mô tả ở SRS-00-Overview) được thể hiện trong Audit Log với ít nhất: timestamp, actor (user/admin), actionType, module, correlationId, result (success/fail).
- AC2: Super Admin có thể tra cứu log theo correlationId để lần theo toàn bộ luồng xử lý một yêu cầu xuyên module.
- AC3: Nội dung log **không hiển thị trực tiếp dữ liệu nhạy cảm** (ví dụ: số CCCD, OTP), chỉ hiển thị mã định danh/đã mask theo Data-Classification.

**Ví dụ dữ liệu (fake)**
- AuditLog record:
	- logId: 200001
	- timestamp: "2026-01-04T03:15:00Z"
	- actorType: "AdminUser"
	- actorId: 5001
	- actionType: "CANCEL_ORDER_APPROVED"
	- module: "ORDERS"
	- correlationId: "corr-order-0001"
	- result: "SUCCESS"
	- metadata: {"orderCode": "ORD-2026-0001"}

---

## 4. Business rules references (BR-###)

> BR chi tiết sẽ được quản lý tập trung trong Business-Rules.md; dưới đây là khung các rule liên quan đến ADMIN.

- **BR-ADMIN-001 — Phân quyền dạng deny-by-default**
	- Mặc định, mọi chức năng trên Admin Portal đều **bị từ chối** nếu tài khoản không có role/permission phù hợp.
	- Chỉ Super Admin có thể gán/bỏ role Admin cho tài khoản khác.

- **BR-ADMIN-002 — Quản lý Công ty & Điểm nhận**
	- Mỗi User Tầng 3 thuộc đúng một Công ty và có một Điểm nhận mặc định (theo Assumption từ USERS/Discovery); ADMIN cung cấp dữ liệu Công ty/Điểm nhận cho các module khác sử dụng.
	- Không được xoá vật lý Công ty/Điểm nhận nếu đang được tham chiếu bởi User/đơn hàng; chỉ được đánh dấu inactive/is_deleted (chi tiết ràng buộc sẽ được thiết kế ở DB-ADMIN).

- **BR-ADMIN-003 — Audit Log bắt buộc**
	- Mọi thao tác cấu hình hệ thống, phân quyền, hoặc liên quan đến luồng hủy đơn/hủy hàng phải ghi Audit Log với correlationId.
	- ADMIN cung cấp khả năng tra cứu, nhưng trách nhiệm ghi log phân bổ cho từng module nghiệp vụ.

Các BR chi tiết (ví dụ: rule cụ thể khi đổi Công ty/Điểm nhận của User, khi merge Công ty, hoặc khi chỉnh sửa cấu hình trả chậm/hoa hồng) sẽ được bổ sung khi có quyết định từ khách hàng, và có thể thuộc module USERS/PAYMENTS/REPORTING hơn là ADMIN.

---

## 5. Data inputs/outputs (conceptual)

### 5.1. Thực thể chính

- **Company** (Công ty)
	- Đại diện cho doanh nghiệp trong KCN.
	- Thuộc một KCN (zone) và có nhiều Điểm nhận.
- **PickupPoint** (Điểm nhận)
	- Thuộc về một Công ty.
	- Có cờ is_default để đánh dấu Điểm nhận mặc định.
- **AdminUser**
	- Tài khoản đăng nhập Admin Portal (tham chiếu tới AUTH/USERS để xác định danh tính).
	- Có một hoặc nhiều role (SuperAdmin/Ops/QC/Finance/Support...).
- **SystemConfig**
	- Tập các cặp key/value hoặc cấu trúc cụ thể cho logo, systemName, thông tin pháp lý, link điều khoản...
- **AuditLog** (xem thêm SRS-00-Overview)
	- Bản ghi log cho các thao tác quan trọng.
	- Được ghi bởi các module khác, nhưng hiển thị qua ADMIN.

### 5.2. Luồng dữ liệu chính

- Quản lý Công ty/Điểm nhận:
	- Input (từ Admin Portal): thông tin Công ty mới/cập nhật, thông tin Điểm nhận.
	- Output: danh sách Công ty/Điểm nhận cho USERS, ORDERS, PAYMENTS, REPORTING; được truy vấn qua API: TBD (Tech Lead).

- Phân quyền Admin:
	- Input: danh sách role gán cho AdminUser.
	- Output: thông tin role để AUTH/Layer API sử dụng trong kiểm tra quyền.

- Cấu hình hệ thống:
	- Input: giá trị cấu hình do Super Admin cập nhật.
	- Output: giá trị config cho frontend/ các module khác đọc để hiển thị.

- Audit Log:
	- Input: log từ các module (ghi vào kho chung theo thiết kế DB/Logging).
	- Output: danh sách log đã lọc cho màn hình tra cứu Audit Log trên Admin Portal.

Chi tiết tên bảng, cột, kiểu dữ liệu, index sẽ được thiết kế trong DB-ADMIN theo chuẩn DB-Design-Rules.

---

## 6. Validations & edge cases

### 6.1. Validations

- **V-ADMIN-001 – Company**
	- companyName không được để trống.
	- companyCode (nếu dùng) phải là duy nhất trong trạng thái Active.

- **V-ADMIN-002 – PickupPoint**
	- PickupPoint phải tham chiếu tới một Công ty hợp lệ.
	- Chỉ một PickupPoint của mỗi Công ty được phép đánh dấu is_default = true.

- **V-ADMIN-003 – AdminUser & Role**
	- AdminUser phải tồn tại (tham chiếu tới entity người dùng đăng nhập hợp lệ từ AUTH/USERS).
	- Role chỉ được gán trong tập role đã định nghĩa (SuperAdmin, Ops, QC, Finance, Support; mở rộng sau nếu cần).

- **V-ADMIN-004 – SystemConfig**
	- Một key cấu hình chỉ được định nghĩa một lần (theo scope toàn hệ thống), trừ khi có khái niệm versioning/timeline được bổ sung sau.

- **V-ADMIN-005 – Audit Log query**
	- Thời gian tìm kiếm phải nằm trong khoảng hợp lệ (ví dụ: không truy vấn ngược quá xa nếu đã có policy retention; chi tiết khoảng thời gian tối đa cần chốt).

### 6.2. Edge cases

1. **E-ADMIN-001 – Xoá/disable Công ty có dữ liệu đang sử dụng**
	- Mô tả: Admin muốn disable hoặc đánh dấu is_deleted cho một Công ty mà vẫn còn User/đơn hàng/công nợ liên quan.
	- Mong đợi: Hệ thống không cho phép xoá vật lý; chỉ cho phép đánh dấu trạng thái (Inactive) và cảnh báo về dữ liệu liên quan. Chi tiết rule xử lý User/đơn hàng hiện có sẽ do USERS/ORDERS/PAYMENTS quy định.

2. **E-ADMIN-002 – Thay đổi Điểm nhận mặc định khi đã có đơn chờ nhận**
	- Mô tả: Admin đổi Điểm nhận mặc định của Công ty trong khi có nhiều đơn đang ở trạng thái "Sẵn sàng nhận" tại Điểm nhận cũ.
	- Mong đợi: Thay đổi chỉ áp dụng cho các đơn mới; các đơn hiện có vẫn giữ Điểm nhận đã chọn khi đặt hàng. Cụ thể sẽ được xác nhận ở ORDERS; ADMIN chỉ lưu config mới.

3. **E-ADMIN-003 – Gán sai role cho AdminUser**
	- Mô tả: Super Admin vô tình gán role nhiều hơn mong muốn (ví dụ: vừa Finance vừa Super Admin cho một user không thuộc team core).
	- Mong đợi: Có thể cần cơ chế phê duyệt/4-eyes cho một số role nhạy cảm; hiện chưa được mô tả → Open Question. SRS chỉ yêu cầu mọi thay đổi role được ghi Audit Log.

4. **E-ADMIN-004 – Thay đổi cấu hình hệ thống trong giờ cao điểm**
	- Mô tả: Super Admin thay đổi một cấu hình quan trọng (ví dụ: logo, tiêu đề, link điều khoản) khi có nhiều người dùng online.
	- Mong đợi: Thay đổi được áp dụng từ lần tải trang tiếp theo; không làm gián đoạn giao dịch. Nếu cấu hình không hợp lệ (ví dụ link 404), trách nhiệm kiểm tra nội dung thuộc Super Admin (không có validation sâu trong hệ thống ngoài dạng cơ bản).

5. **E-ADMIN-005 – Tra cứu Audit Log với khoảng thời gian rất lớn**
	- Mô tả: Super Admin chọn khoảng thời gian nhiều năm khiến truy vấn log rất nặng.
	- Mong đợi: Hệ thống áp dụng giới hạn (ví dụ chỉ cho phép tìm trong tối đa N ngày/1 truy vấn, có phân trang); thông số N cụ thể cần chốt trong thiết kế hiệu năng.

---

## 7. Non-functional requirements (module-specific)

- **Audit & bảo mật**
	- Màn hình Admin Portal chỉ truy cập được thông qua AuthN/AuthZ từ AUTH với JWT/Session Token hợp lệ và role phù hợp.
	- Không log PII/ dữ liệu nhạy cảm trong Audit Log UI; tuân thủ Data-Classification.

- **Hiệu năng & khả năng mở rộng**
	- Truy vấn danh sách Công ty/Điểm nhận và AdminUser cần được phân trang theo chuẩn chung (page/pageSize).
	- Truy vấn Audit Log phải sử dụng index phù hợp (theo thời gian, module, actionType) để không ảnh hưởng hiệu năng chung.

- **Khả dụng**
	- Sự cố ở Admin Portal (ví dụ lỗi Dashboard) không được làm ảnh hưởng trực tiếp đến luồng End User (Portal End User vẫn phải hoạt động nếu API backend còn hoạt động).

- **Ghi log & quan sát**
	- Mọi API quan trọng của ADMIN phải log kèm correlationId.
	- Lỗi trong Dashboard hoặc Audit Log phải được log với mức cảnh báo phù hợp để DevOps có thể phát hiện.

---

## 8. Open Questions / Assumptions for this module

**Câu hỏi mở**

- **Q-ADMIN-001** – Chi tiết bộ trường cho entity Công ty và Điểm nhận là gì (mã, địa chỉ chi tiết, thông tin liên hệ, cấu hình lịch nhận hàng...)?
- **Q-ADMIN-002** – Có cần workflow phê duyệt cho các thay đổi cấu hình quan trọng (ví dụ: thay đổi cấu hình trả chậm, hoa hồng, quyền Super Admin) hay chỉ Super Admin thao tác trực tiếp?
- **Q-ADMIN-003** – Có cần versioning cho SystemConfig (lưu nhiều phiên bản điều khoản/điều kiện) và cơ chế bắt người dùng re-accept khi điều khoản thay đổi hay không?
- **Q-ADMIN-004** – Mức độ chi tiết của Audit Log mà Support được xem là tới đâu (toàn bộ hay chỉ một subset, ví dụ không xem được thay đổi phân quyền)?
- **Q-ADMIN-005** – Có yêu cầu quản lý đa-tenant phức tạp hơn (một Admin quản lý nhiều Công ty với giới hạn rõ ràng) hay giả định mỗi Admin gắn với một tổ chức/khách hàng duy nhất?

**Giả định (cần xác nhận)**

- **A-ADMIN-001** – Mỗi User Tầng 3 chỉ thuộc đúng **một** Công ty và một Điểm nhận mặc định tại một thời điểm (theo các giả định trong SRS-USERS và Discovery).
- **A-ADMIN-002** – Super Admin là role duy nhất có quyền thay đổi phân quyền Admin và cấu hình hệ thống quan trọng.
- **A-ADMIN-003** – Admin Portal sẽ được triển khai dưới dạng web không cần responsive mạnh (theo MVP Scope), nên các yêu cầu về UX mobile cho Admin không nằm trong phạm vi module ADMIN.
- **A-ADMIN-004** – Policy retention cho Audit Log (lưu bao lâu, mức chi tiết nào) tuân theo quyết định toàn cục trong SRS-00-Overview; ADMIN chỉ hiển thị những gì còn trong kho log.
