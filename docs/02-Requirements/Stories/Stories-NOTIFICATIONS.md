
# Stories — NOTIFICATIONS: Notifications

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

### US-NOTIFICATIONS-001 — Quản lý template ZNS cho OTP và thông báo nghiệp vụ
**As a** Super Admin/Ops
**I want** quản lý các template ZNS dùng cho OTP và thông báo nghiệp vụ
**So that** hệ thống gửi đúng nội dung, đúng kênh, giảm rủi ro sai thông tin và dễ kiểm soát chi phí

**Priority:** P0  
**Dependencies:**
- SRS-NOTIFICATIONS: FR-NOTIFICATIONS-001, BR-NOTIFICATIONS-001, BR-NOTIFICATIONS-002

**Definition of Ready**
- [ ] Danh sách loại thông báo (OTP, Nhắc nhận hàng, Nhắc nợ, Thay đổi hoa hồng, Khác) được chốt.
- [ ] Quy tắc đặt mã template (templateCode) được thống nhất (ví dụ: ZNS_OTP_LOGIN, ZNS_REMIND_PICKUP...).
- [ ] Danh sách placeholder cho từng loại thông báo ({{otp}}, {{order_code}}, {{pickup_point}}, {{due_date}}...) được mô tả.

**Acceptance Criteria**
- AC1: Admin có thể tạo mới template ZNS với các trường tối thiểu: code, type, description, body, status, và hệ thống không cho phép trùng code ở trạng thái Active.
- AC2: Khi Admin cập nhật nội dung template, hệ thống kiểm tra placeholder hợp lệ; nếu template chứa placeholder không được định nghĩa, hệ thống không cho lưu và hiển thị lỗi.
- AC3: Khi template bị chuyển sang trạng thái Inactive, mọi yêu cầu gửi sử dụng templateCode đó bị từ chối và được log.

**Sample data (fake)**
- Template OTP:
	- templateCode: "ZNS_OTP_LOGIN"
	- type: "OTP"
	- description: "OTP đăng nhập lần đầu"
	- body: "Ma OTP cua ban la {{otp}}, hieu luc {{ttl}} phut. KHONG chia se ma nay voi bat ky ai."
	- status: "ACTIVE"
- Template nhắc nợ:
	- templateCode: "ZNS_DEBT_REMINDER"
	- type: "DEBT_REMIND"
	- body: "Cong no hien tai cua ban la {{debt_amount}} VND. Vui long thanh toan truoc {{due_date}}."
	- status: "ACTIVE"

**Edge cases (min 3)**
- EC1: Admin tạo template với code đã tồn tại và cùng trạng thái ACTIVE → hệ thống từ chối, hiển thị lỗi "templateCode đã tồn tại".
- EC2: Admin vô tình xóa/đặt Inactive template OTP đang sử dụng → các luồng AUTH gửi yêu cầu gửi OTP sẽ thất bại; hệ thống phải log rõ để Admin có thể khôi phục.
- EC3: Admin nhập body template có placeholder không nằm trong danh sách cho phép (ví dụ: {{unknown}}) → hệ thống không cho lưu, chỉ rõ placeholder sai.
- EC4: Admin cố gắng chỉnh sửa template đã được sử dụng nhiều lần; story này giả định việc sửa template được cho phép nhưng luôn có audit log (quy trình duyệt thay đổi có hay không là Open Question).

**API mapping (when available)**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình Admin Portal: Quản lý template ZNS)

---

### US-NOTIFICATIONS-002 — Gửi OTP ZNS cho đăng ký/đăng nhập lần đầu
**As a** AUTH module (đại diện cho End User)
**I want** yêu cầu gửi OTP ZNS đến SĐT của người dùng
**So that** người dùng có thể xác thực đăng ký/đăng nhập lần đầu một cách an toàn

**Priority:** P0  
**Dependencies:**
- SRS-NOTIFICATIONS: FR-NOTIFICATIONS-002, BR-NOTIFICATIONS-001, BR-NOTIFICATIONS-003
- SRS-AUTH (luồng OTP)

**Definition of Ready**
- [ ] Đã chốt độ dài OTP và thời gian hiệu lực (TTL) ở AUTH.
- [ ] Template OTP ZNS (ZNS_OTP_LOGIN) đã tồn tại và ở trạng thái ACTIVE.
- [ ] Quy tắc masking SĐT trong log (ví dụ: 0900***456) đã được thống nhất.

**Acceptance Criteria**
- AC1: Khi AUTH gửi yêu cầu gửi OTP với SĐT hợp lệ và templateCode hợp lệ, NOTIFICATIONS gọi ZNS thành công và trả về kết quả success cho AUTH, đồng thời lưu một bản ghi NotificationLog với status = SUCCESS.
- AC2: Khi ZNS trả lỗi (ví dụ: timeout, lỗi cấu hình), NOTIFICATIONS trả về failure cho AUTH và lưu NotificationLog với status = FAILURE cùng errorCode nội bộ.
- AC3: Khi templateCode không tồn tại hoặc đang Inactive, NOTIFICATIONS không gọi ZNS, trả về lỗi rõ ràng cho AUTH và lưu NotificationLog với lý do template invalid.

**Sample data (fake)**
- Request từ AUTH:
	- phone_number: "0900123456"
	- otp_code: "123456"
	- ttl_minutes: 5
	- template_code: "ZNS_OTP_LOGIN"
	- correlationId: "corr-otp-0001"
- Kết quả ZNS (success):
	- providerStatus: "OK"
	- providerMessageId: "zns-msg-9999"
- NotificationLog:
	- notificationId: 100001
	- correlationId: "corr-otp-0001"
	- recipientMasked: "0900***456"
	- templateCode: "ZNS_OTP_LOGIN"
	- status: "SUCCESS"
	- providerMessageId: "zns-msg-9999"

**Edge cases (min 3)**
- EC1: SĐT không hợp lệ (sai định dạng) → NOTIFICATIONS từ chối yêu cầu và không gọi ZNS; AUTH nhận lỗi và hiển thị thông báo phù hợp.
- EC2: ZNS tạm thời không khả dụng (timeout) → NOTIFICATIONS đánh dấu FAILURE; policy retry (tự động/hay để AUTH retry) là Open Question, story này chỉ yêu cầu phản hồi rõ ràng.
- EC3: AUTH gửi hai yêu cầu OTP gần nhau cho cùng một SĐT và correlationId khác nhau → cả hai log đều được ghi nhận; giới hạn tần suất/anti-spam thuộc BR toàn cục, sẽ được cấu hình trong rate-limit.
- EC4: payload thiếu trường bắt buộc (ví dụ: otp_code hoặc ttl_minutes) → NOTIFICATIONS từ chối yêu cầu dưới dạng validation error, không gửi ZNS.

**API mapping (when available)**
- TBD (Tech Lead)

**Screens**
- N/A (backend integration giữa AUTH và NOTIFICATIONS, không có màn hình riêng)

---

### US-NOTIFICATIONS-003 — Gửi thông báo nhắc nhận hàng và nhắc nợ theo sự kiện
**As a** hệ thống ORDERS/PAYMENTS
**I want** gửi thông báo ZNS đến người dùng khi đơn sẵn sàng nhận hoặc sắp/đã đến hạn trả chậm
**So that** người dùng được nhắc kịp thời, giảm tỷ lệ quên nhận hàng và quá hạn công nợ

**Priority:** P0  
**Dependencies:**
- SRS-NOTIFICATIONS: FR-NOTIFICATIONS-003, BR-NOTIFICATIONS-002
- SRS-ORDERS (trạng thái đơn, đặc biệt "Sẵn sàng nhận")
- SRS-PAYMENTS (công nợ, hạn mức, ngày đến hạn)

**Definition of Ready**
- [ ] Danh sách event_type chính và mapping sang templateCode được chốt (ví dụ: ORDER_READY_FOR_PICKUP → ZNS_REMIND_PICKUP; DEBT_DUE_REMINDER → ZNS_DEBT_REMINDER).
- [ ] Đã chốt rule khi nào trigger từng loại sự kiện (ví dụ: nhắc nhận hàng ngay khi trạng thái chuyển sang "Sẵn sàng nhận"; nhắc nợ H-3, H0, H+X...).
- [ ] Cấu hình NotificationConfig cho từng Công ty đã có giá trị mặc định.

**Acceptance Criteria**
- AC1: Khi ORDERS phát sinh sự kiện ORDER_READY_FOR_PICKUP cho một đơn thuộc Công ty mà nhóm "Nhắc nhận hàng" đang bật, NOTIFICATIONS gửi đúng nội dung nhắc nhận hàng cho SĐT của User, và lưu NotificationLog.
- AC2: Khi PAYMENTS phát sinh sự kiện DEBT_DUE_REMINDER cho User có công nợ, và nhóm "Nhắc nợ" đang bật, NOTIFICATIONS gửi thông báo chứa thông tin debt_amount và due_date.
- AC3: Nếu event_type không có templateCode cấu hình hoặc nhóm thông báo tương ứng bị tắt cho Công ty đó, NOTIFICATIONS không gửi ZNS, log lý do (missing template/disabled config) và trả về trạng thái phù hợp cho module gọi.

**Sample data (fake)**
- Sự kiện nhắc nhận hàng:
	- event_type: "ORDER_READY_FOR_PICKUP"
	- recipient_phone_number: "0900123456"
	- business_payload: {"order_code": "ORD-2026-0001", "pickup_point": "Điểm nhận A - Công ty May KCN A", "pickup_deadline": "2026-01-10"}
	- companyId: 100
	- correlationId: "corr-order-0001"
- Sự kiện nhắc nợ:
	- event_type: "DEBT_DUE_REMINDER"
	- recipient_phone_number: "0900456789"
	- business_payload: {"debt_amount": 1_200_000, "due_date": "2026-01-25"}
	- companyId: 200
	- correlationId: "corr-debt-0001"

**Edge cases (min 3)**
- EC1: event_type gửi lên chưa được mapping sang templateCode → NOTIFICATIONS không gửi, log lỗi "template not configured".
- EC2: Company A đã tắt nhóm "Nhắc nhận hàng" → sự kiện ORDER_READY_FOR_PICKUP từ Company A không tạo bất kỳ gửi ZNS nào; log ghi rõ "notification group disabled".
- EC3: business_payload thiếu trường cần thiết (ví dụ: thiếu order_code) → NOTIFICATIONS không gửi, log lỗi validation.
- EC4: Cùng một event (cùng correlationId/eventId) được gửi lại nhiều lần do retry ở ORDERS/PAYMENTS → hành vi tránh gửi trùng lặp sẽ phụ thuộc BR (TBD); story này yêu cầu ít nhất log cho phép phân biệt các lần gửi.

**API mapping (when available)**
- TBD (Tech Lead)

**Screens**
- N/A (sự kiện nền giữa ORDERS/PAYMENTS và NOTIFICATIONS)

---

### US-NOTIFICATIONS-004 — Tra cứu log gửi thông báo để xử lý khiếu nại
**As a** Support/Super Admin
**I want** tra cứu lịch sử gửi thông báo ZNS
**So that** tôi có thể kiểm tra khi người dùng phản ánh "không nhận được OTP/thông báo"

**Priority:** P1  
**Dependencies:**
- SRS-NOTIFICATIONS: FR-NOTIFICATIONS-004, BR-NOTIFICATIONS-003

**Definition of Ready**
- [ ] Đã xác định rõ các filter cho màn hình log (thời gian, template/event_type, status, recipientMasked, correlationId...).
- [ ] Rule về mức độ hiển thị nội dung/thông tin nhạy cảm (mask số điện thoại, không hiển thị OTP) được chốt với Security/Data-Classification.

**Acceptance Criteria**
- AC1: Support có thể tìm kiếm log thông báo theo correlationId và xem được ít nhất: thời gian gửi, templateCode/eventType, status, recipientMasked, providerMessageId (nếu có).
- AC2: Hệ thống không hiển thị đầy đủ SĐT và nội dung OTP trong log, chỉ hiển thị dạng đã mask/ẩn theo quy định.
- AC3: Khi không tìm thấy bất kỳ log phù hợp với tiêu chí tìm kiếm, hệ thống hiển thị kết quả rỗng rõ ràng, không lỗi.

**Sample data (fake)**
- Truy vấn:
	- searchBy: "correlationId"
	- value: "corr-otp-0001"
- Kết quả:
	- notificationId: 100001
	- correlationId: "corr-otp-0001"
	- recipientMasked: "0900***456"
	- templateCode: "ZNS_OTP_LOGIN"
	- status: "SUCCESS"
	- sentAt: "2026-01-04T02:00:00Z"
	- providerMessageId: "zns-msg-9999"

**Edge cases (min 3)**
- EC1: Support tìm kiếm bằng SĐT đầy đủ nhưng hệ thống chỉ lưu recipientMasked → cần quy tắc tìm kiếm (ví dụ: nhập 3-4 số cuối, hoặc chỉ cho tìm theo correlationId/eventId); story này giả định tìm theo correlationId là kênh chính.
- EC2: Log rất lớn (nhiều bản ghi) → hệ thống phải phân trang và giới hạn số bản ghi mỗi lần truy vấn (chi tiết sẽ do Tech Lead thiết kế, story này chỉ nêu yêu cầu phân trang).
- EC3: Support không có quyền xem log template loại nhạy cảm (nếu phân quyền chi tiết theo loại) → story này giả định tối thiểu Super Admin luôn xem được, Support xem được theo policy sẽ được mô tả trong ADMIN.

**API mapping (when available)**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình Admin Portal: Tra cứu log thông báo)

---

### US-NOTIFICATIONS-005 — Cấu hình bật/tắt nhóm thông báo theo Công ty
**As a** Super Admin/Finance/Ops
**I want** bật/tắt các nhóm thông báo ZNS cho từng Công ty
**So that** có thể kiểm soát chi phí ZNS và phù hợp chính sách từng Công ty

**Priority:** P1  
**Dependencies:**
- SRS-NOTIFICATIONS: FR-NOTIFICATIONS-005, BR-NOTIFICATIONS-002
- SRS-USERS/ADMIN (Công ty)

**Definition of Ready**
- [ ] Đã chốt danh sách nhóm thông báo (OTP, Nhắc nhận hàng, Nhắc nợ, Thay đổi hoa hồng...).
- [ ] Đã xác định rule: OTP có thể tắt theo Công ty hay luôn bắt buộc toàn hệ thống (Open Question nếu chưa rõ).
- [ ] Thiết kế UI cấu hình theo Công ty được phê duyệt.

**Acceptance Criteria**
- AC1: Admin có thể chọn một Công ty và bật/tắt từng nhóm thông báo; cấu hình này được lưu và hiển thị lại chính xác khi mở lại.
- AC2: Khi nhóm "Nhắc nhận hàng" bị tắt cho Công ty A, mọi yêu cầu gửi event ORDER_READY_FOR_PICKUP cho đơn thuộc Công ty A phải **không gửi ZNS**, và NOTIFICATIONS log lý do "disabled by config".
- AC3: Khi bật lại nhóm "Nhắc nhận hàng" cho Công ty A, các sự kiện mới ORDER_READY_FOR_PICKUP được xử lý bình thường (nếu không vi phạm rate-limit).

**Sample data (fake)**
- NotificationConfig cho Công ty 100:
	- companyId: 100
	- enableOtp: true
	- enablePickupReminder: true
	- enableDebtReminder: false
	- enableCommissionChange: true
- NotificationConfig cho Công ty 200:
	- companyId: 200
	- enableOtp: true
	- enablePickupReminder: false
	- enableDebtReminder: true
	- enableCommissionChange: true

**Edge cases (min 3)**
- EC1: Admin tắt/ bật lại một nhóm thông báo nhiều lần trong ngày → NOTIFICATIONS luôn lấy cấu hình mới nhất khi quyết định có gửi hay không.
- EC2: Cấu hình cho Công ty chưa tồn tại (Công ty mới tạo) → hệ thống dùng cấu hình mặc định toàn hệ thống (global default) hoặc yêu cầu tạo cấu hình trước; policy cụ thể sẽ được chốt sau.
- EC3: Hai Admin cùng chỉnh cấu hình cho cùng một Công ty gần như đồng thời → cần cơ chế tránh ghi đè "mất thay đổi" (ví dụ: optimistic locking); story này yêu cầu ít nhất behavior rõ ràng về phiên bản cuối cùng.

**API mapping (when available)**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình Admin Portal: Cấu hình thông báo theo Công ty)

