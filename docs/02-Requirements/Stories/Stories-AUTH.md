
# Stories — AUTH: Authentication & Authorization

## Định dạng chung
- Mỗi story tuân theo format:
	- User story (As a / I want / So that).
	- Priority (P0/P1/P2) theo phạm vi MVP.
	- Definition of Ready (DoR) checklist.
	- Acceptance Criteria (AC) đo lường được.
	- Sample data (fake) theo dạng entity hoặc request/response, **không** trùng hoàn toàn dữ liệu thực tế.
	- Ít nhất 3 edge cases.
	- API mapping: `TBD (Tech Lead)` (không liệt kê path cụ thể).
	- Screens: danh sách màn hình nếu đã biết, hoặc `TBD`.

## Story list

---

### US-AUTH-001 — Đăng ký End User bằng SĐT + OTP ZNS
**As a** Công nhân KCN (Tầng 3)  
**I want** đăng ký tài khoản bằng số điện thoại và mã OTP ZNS  
**So that** tôi có thể sử dụng hệ thống để đặt hàng và theo dõi đơn

**Priority:** P0  
**Dependencies:**
- SRS-AUTH: FR-AUTH-001, BR-AUTH-001..004  
- SRS-USERS (match danh sách nhân sự, tầng)  
- NOTIFICATIONS (gửi OTP ZNS)

**Definition of Ready**
- [ ] Mẫu thiết kế UI màn hình Đăng ký được chốt (fields: Họ tên, SĐT, chọn Công ty, checkbox chấp nhận điều khoản…).
- [ ] Quy tắc định dạng SĐT được Tech Lead xác nhận.
- [ ] Luồng xử lý khi SĐT không có trong danh sách nhân sự (USERS) được BA/PO chốt (gán Tầng 3 mặc định hay chờ duyệt).
- [ ] Template nội dung OTP ZNS được NOTIFICATIONS cung cấp.
- [ ] Thông số thời gian sống OTP (TTL) và giới hạn số lần gửi lại OTP được thống nhất.

**Acceptance Criteria**
- AC1: Khi người dùng nhập đầy đủ Họ tên, chọn Công ty và SĐT hợp lệ, hệ thống gửi được OTP ZNS tới đúng SĐT và hiển thị thông báo “Đã gửi OTP”.
- AC2: Chỉ khi người dùng nhập đúng OTP còn hiệu lực, tài khoản mới được tạo và có thể dùng để đăng nhập lần sau.
- AC3: Nếu OTP sai hoặc hết hạn, hệ thống từ chối đăng ký và hiển thị thông báo lỗi rõ ràng, không tạo tài khoản.
- AC4: Hệ thống ghi nhận log đăng ký (thành công/thất bại) kèm correlationId, SĐT (ẩn/mask theo Data-Classification) và thời gian.

**Sample data (fake)**
- Input form:
	- Họ tên: "Trần Thị B"  
	- Công ty: "Công ty Điện tử KCN X"  
	- SĐT: "0912345678"  
	- Đồng ý điều khoản: true
- OTP gửi qua ZNS: "482913" (TTL 120s, giá trị minh họa)
- Kết quả tạo tài khoản (entity đơn giản hoá):
	- userId: 10001  
	- phone: "0912345678"  
	- fullName: "Trần Thị B"  
	- companyId: 501  
	- tier: 3  
	- status: "Active"

**Edge cases (min 3)**
- EC1: Người dùng nhập SĐT sai định dạng (ví dụ thiếu số, có ký tự chữ) → Hệ thống không cho gửi OTP, hiển thị validation error ngay trên form.
- EC2: Người dùng yêu cầu gửi lại OTP quá số lần cấu hình trong một khoảng thời gian (spam OTP) → Hệ thống chặn và hiển thị thông báo “Bạn đã yêu cầu quá số lần, vui lòng thử lại sau”.
- EC3: Dịch vụ ZNS bị lỗi tạm thời → Hệ thống hiển thị thông báo "Không thể gửi OTP, vui lòng thử lại sau" và **không** tạo bản ghi OTP dùng được.
- EC4: Người dùng đóng trình duyệt sau khi gửi OTP nhưng trước khi nhập → Lần truy cập lại trong thời gian OTP còn hiệu lực vẫn dùng được OTP cũ (hoặc bị vô hiệu theo policy được chốt).

**API mapping**
- TBD (Tech Lead)

**Screens**
- SC-AUTH-001 — End User Registration
- SC-AUTH-002 — OTP Verification

---

### US-AUTH-002 — Đăng nhập End User lần đầu bằng OTP
**As a** Công nhân KCN (Tầng 3)  
**I want** đăng nhập bằng số điện thoại và OTP ZNS trong lần sử dụng đầu tiên  
**So that** tôi có thể truy cập tài khoản đã đăng ký mà không cần mật khẩu

**Priority:** P0  
**Dependencies:**
- SRS-AUTH: FR-AUTH-002, BR-AUTH-001..004  
- NOTIFICATIONS (gửi OTP ZNS)

**Definition of Ready**
- [ ] Thiết kế UI màn hình Đăng nhập End User được chốt.
- [ ] Chính sách TTL và giới hạn số lần nhập sai OTP được xác nhận.
- [ ] Cách hiển thị thông báo lỗi khi SĐT chưa có tài khoản được thống nhất với UX.
- [ ] Cấu trúc cơ bản của Session Token / Refresh Token đã được Tech Lead phác thảo (ở mức conceptual).

**Acceptance Criteria**
- AC1: Khi người dùng nhập SĐT đã đăng ký, hệ thống gửi được OTP ZNS và hiển thị bước nhập OTP.
- AC2: Khi người dùng nhập OTP đúng và còn hiệu lực, hệ thống tạo Session Token + Refresh Token gắn với thiết bị/browser hiện tại và cho phép truy cập Portal End User.
- AC3: Nếu người dùng nhập SĐT chưa có tài khoản, hệ thống không gửi OTP và hiển thị thông báo phù hợp (ví dụ: "Tài khoản chưa được đăng ký").
- AC4: Sau khi đăng nhập thành công, người dùng nhìn thấy các thông tin tương ứng với tầng của mình (Tầng 1/2/3) theo dữ liệu từ USERS.

**Sample data (fake)**
- Input:
	- SĐT: "0912345678"  
	- OTP: "112233"
- Output conceptual:
	- sessionToken: "sess_abc123_fake"  
	- refreshToken: "ref_def456_fake"  
	- user: { userId: 10001, tier: 3, companyId: 501 }

**Edge cases (min 3)**
- EC1: Người dùng nhập SĐT đúng nhưng OTP sai 3 lần liên tiếp → Hệ thống từ chối và chặn OTP cho SĐT này trong khoảng thời gian cấu hình (TBD), log lại lý do.
- EC2: OTP hết hạn khi người dùng vừa nhấn "Xác nhận" → Hệ thống từ chối, hiển thị thông báo OTP hết hạn và cho phép gửi lại OTP nếu chưa vượt giới hạn.
- EC3: Người dùng nhập SĐT hợp lệ nhưng tài khoản đã bị khoá (Locked trong USERS) → Hệ thống từ chối đăng nhập, hiển thị thông báo chung (không tiết lộ lý do chi tiết), log trạng thái.
- EC4: Người dùng dùng trình duyệt ẩn danh (incognito) → Sau khi đóng tab, token không còn và lần sau phải đăng nhập lại theo chính sách được chốt.

**API mapping**
- TBD (Tech Lead)

**Screens**
- SC-AUTH-003 — End User Login (first time)
- SC-AUTH-002 — OTP Verification

---

### US-AUTH-003 — Đăng nhập tự động bằng Session Token / Refresh Token
**As a** người dùng End User đã đăng ký  
**I want** được tự động đăng nhập trên thiết bị đã dùng trước đó  
**So that** tôi không phải nhập OTP lại nhiều lần, tiết kiệm thời gian và chi phí OTP

**Priority:** P0  
**Dependencies:**
- SRS-AUTH: FR-AUTH-003, BR-AUTH-001..002

**Definition of Ready**
- [ ] Thời gian sống (TTL) cho Session Token và Refresh Token được chốt.
- [ ] Cơ chế lưu trữ token trên client (cookie hoặc local storage) được Tech Lead thiết kế sơ bộ.
- [ ] Chính sách khi đổi thiết bị/browser (bắt OTP lại hay không) được quyết định.

**Acceptance Criteria**
- AC1: Khi người dùng mở lại Portal trên cùng thiết bị trong thời gian Session Token còn hiệu lực, hệ thống tự động đăng nhập mà không yêu cầu OTP.
- AC2: Khi Session Token hết hạn nhưng Refresh Token còn hiệu lực, hệ thống cấp Session Token mới và cho phép truy cập, không yêu cầu OTP.
- AC3: Khi cả Session Token và Refresh Token đều không hợp lệ/hết hạn, hệ thống yêu cầu người dùng đăng nhập lại (SĐT + OTP), không cho truy cập silent.

**Sample data (fake)**
- Trạng thái trước:
	- sessionToken: "sess_abc123_fake" (hết hạn)  
	- refreshToken: "ref_def456_fake" (còn hạn)
- Sau refresh:
	- newSessionToken: "sess_new789_fake"  
	- user: { userId: 10001, tier: 3 }

**Edge cases (min 3)**
- EC1: Refresh Token đã bị revoke từ phía server (ví dụ người dùng reset bảo mật) nhưng client vẫn gửi → Hệ thống từ chối và yêu cầu đăng nhập lại.
- EC2: Client gửi Session Token bị chỉnh sửa (corrupted) → Hệ thống phát hiện không hợp lệ, không crash, từ chối truy cập và yêu cầu đăng nhập lại.
- EC3: Đồng hồ hệ thống và client lệch múi giờ → Việc kiểm tra hết hạn phải dựa trên thời gian server, không bị ảnh hưởng bởi giờ client.
- EC4: Người dùng có nhiều thiết bị, một thiết bị đăng xuất toàn bộ (nếu có tính năng này sau) → Token trên thiết bị khác cũng không còn hiệu lực (liên quan E-AUTH-006, cần rule rõ ràng).

**API mapping**
- TBD (Tech Lead)

**Screens**
- SC-AUTH-004 — End User Portal Shell/Header (logged-in state)

---

### US-AUTH-004 — Đăng xuất End User & thu hồi token trên thiết bị hiện tại
**As a** người dùng End User  
**I want** chủ động đăng xuất khỏi thiết bị đang dùng  
**So that** người khác không thể tiếp tục sử dụng tài khoản của tôi trên thiết bị đó

**Priority:** P0  
**Dependencies:**
- SRS-AUTH: FR-AUTH-004, BR-AUTH-004

**Definition of Ready**
- [ ] UI nút "Đăng xuất" được bố trí rõ ràng trên Portal.
- [ ] Chính sách thu hồi Refresh Token khi logout (chỉ session hiện tại hay tất cả) được xác nhận.

**Acceptance Criteria**
- AC1: Khi người dùng nhấn "Đăng xuất", Session Token trên thiết bị hiện tại bị vô hiệu hóa và không thể dùng cho bất kỳ API bảo vệ nào.
- AC2: Sau khi đăng xuất, tải lại trang Portal dẫn tới màn hình Đăng nhập, không còn dữ liệu người dùng cũ hiển thị.
- AC3: Log hệ thống ghi lại sự kiện logout kèm correlationId và userId.

**Sample data (fake)**
- Trước khi logout:
	- sessionToken: "sess_active_123"  
	- user: { userId: 10002, tier: 2 }
- Sau khi logout:
	- sessionToken: vô hiệu (revoked = true)

**Edge cases (min 3)**
- EC1: Người dùng mở nhiều tab trên cùng trình duyệt → Logout ở một tab phải khiến tất cả tab khác buộc quay về trạng thái chưa đăng nhập.
- EC2: Người dùng logout trong khi có request đang chạy nền (background) → Token đã bị revoke vẫn không được chấp nhận cho các request kết thúc sau đó.
- EC3: Mạng chập chờn khi người dùng nhấn logout → Hệ thống ưu tiên trạng thái an toàn (coi như đã logout nếu có nghi ngờ) theo thiết kế kỹ thuật.

**API mapping**
- TBD (Tech Lead)

**Screens**
- SC-AUTH-004 — End User Portal Shell/Header (logged-in state)

---

### US-AUTH-005 — Đăng nhập Admin Portal
**As a** Admin (Super Admin/Ops/QC/Finance/Support)  
**I want** đăng nhập vào Portal Admin bằng SĐT/Email và mật khẩu  
**So that** tôi có thể thực hiện các tác vụ vận hành theo vai trò được phân quyền

**Priority:** P0  
**Dependencies:**
- SRS-AUTH: FR-AUTH-005, validations & edge cases về Admin
- Module ADMIN (quản lý role & phân quyền chi tiết)

**Definition of Ready**
- [ ] Thiết kế UI màn hình Đăng nhập Admin (desktop-first) được chốt.
- [ ] Chiến lược lưu trữ mật khẩu (hash algorithm) được Tech Lead/Security Lead thống nhất.
- [ ] Chính sách khoá tài khoản Admin khi nhập sai nhiều lần (số lần, thời gian khoá) được quyết định.
- [ ] Quy tắc assign role Admin (Super Admin/Ops/QC/Finance/Support) từ dữ liệu USERS/ADMIN được định nghĩa.

**Acceptance Criteria**
- AC1: Admin với thông tin đăng nhập hợp lệ có thể đăng nhập và nhìn thấy dashboard tương ứng với vai trò của mình.
- AC2: Người dùng không có quyền Admin (End User) không thể đăng nhập Portal Admin dù nhập đúng SĐT/Email + mật khẩu.
- AC3: Mỗi lần đăng nhập (thành công hoặc thất bại) được ghi log đầy đủ (thời gian, userId/adminId, kết quả, correlationId).
- AC4: Thông báo lỗi khi đăng nhập thất bại không tiết lộ chi tiết (ví dụ: "Sai mật khẩu"), mà chỉ hiển thị thông báo chung đã được UX chốt.

**Sample data (fake)**
- Input:
	- email: "ops.admin@example.com"  
	- password: "P@ssw0rd!" (chỉ dùng minh hoạ; hệ thống lưu dạng hash)
- Output conceptual:
	- adminUserId: 9001  
	- roles: ["Ops"]  
	- adminSessionToken: "admin_sess_abc123"

**Edge cases (min 3)**
- EC1: Admin bị khoá tài khoản (do nhập sai nhiều lần trước đó) → Không đăng nhập được, hiển thị thông báo phù hợp, không tiết lộ lý do chi tiết.
- EC2: Tài khoản có nhiều role (ví dụ: vừa là Ops vừa là Support) → Giao diện hiển thị đầy đủ chức năng hợp lệ cho cả hai role, không bị xung đột.
- EC3: Thử đăng nhập bằng tài khoản End User vào Portal Admin → Bị từ chối với thông báo chung.
- EC4: Đồng bộ dữ liệu Admin từ hệ thống khác (nếu có) chậm vài phút → Story này chỉ xử lý logic đăng nhập, không chịu trách nhiệm về việc danh sách Admin có được cập nhật tức thời hay không.

**API mapping**
- TBD (Tech Lead)

**Screens**
- SC-AUTH-005 — Admin Login

