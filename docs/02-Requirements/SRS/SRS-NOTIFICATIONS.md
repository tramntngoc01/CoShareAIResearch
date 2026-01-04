
# SRS — NOTIFICATIONS: Notifications

## 1. Module purpose & boundaries (in/out)

**Mục đích**
- Cung cấp năng lực **quản lý template thông báo ZNS** và **gửi thông báo giao dịch** cho các module khác (AUTH, ORDERS, PAYMENTS, REPORTING).
- Tập trung vào **thông báo nghiệp vụ** trong MVP, bao gồm tối thiểu:
	- OTP ZNS cho đăng ký/đăng nhập (theo Discovery SRS-00-Overview và MVP Scope).
	- Thông báo/nhắc nhở liên quan đến đơn hàng và vận hành (ví dụ: nhắc nhận hàng tại Điểm nhận).
	- Thông báo/nhắc nhở liên quan đến thanh toán và công nợ (nhắc nợ trả chậm) và các cảnh báo vận hành cơ bản.
- Kiểm soát chi phí ZNS bằng việc cho phép **cấu hình template** và **giảm số lần gửi không cần thiết**, phù hợp với rủi ro R2 (chi phí ZNS cao) trong MVP-Scope.

**Trong phạm vi (IN)**
- Quản lý template ZNS phục vụ các luồng sau (danh sách ban đầu từ MVP Scope – mục "Thông báo/ZNS"):
	- OTP đăng ký/đăng nhập.
	- Nhắc nhận hàng (đơn ở trạng thái "Sẵn sàng nhận" nhưng chưa nhận).
	- Nhắc nợ trả chậm/công nợ quá hạn.
	- Thông báo thay đổi cấu hình hoa hồng (ví dụ: thay đổi % hoa hồng theo ngành hàng).
- Cung cấp API nội bộ (**API: TBD (Tech Lead)**) cho các module khác (AUTH, ORDERS, PAYMENTS, REPORTING, ADMIN) để:
	- Yêu cầu gửi một thông báo cụ thể cho 1 hoặc nhiều người nhận.
	- Truy vấn log thông báo đã gửi (theo quyền).
- Ghi nhận **log gửi thông báo** (thành công/thất bại), bao gồm correlationId và thông tin cần thiết để tra soát (không log nội dung nhạy cảm).
- Hỗ trợ **rate limit và chống spam cơ bản ở cấp ứng dụng** (ví dụ: giới hạn số lần gửi cùng một loại thông báo cho cùng một User trong một khoảng thời gian) – ngưỡng cụ thể sẽ được chốt sau (→ Open Questions).

**Ngoài phạm vi (OUT)**
- Gửi email, SMS non-Zalo, push notification mobile app, hay các kênh marketing nâng cao (campaign, A/B testing) – **không được mô tả trong Discovery MVP**, chỉ chừa cổng mở rộng về sau.
- Quản lý opt-in/opt-out chi tiết theo từng loại thông báo cho User (notification preferences) – chưa có yêu cầu rõ ràng, sẽ được xem là **Open Question**.
- Chi tiết tích hợp kỹ thuật với ZNS (SDK, chữ ký, webhook callback, SLA, retry policy chi tiết) – sẽ được đặc tả trong tài liệu thiết kế kỹ thuật và tài liệu tích hợp, không nằm trong SRS.
- Quy tắc kinh doanh chi tiết liên quan đến cách tính hoa hồng, công nợ, hay logic trạng thái đơn – thuộc các module REPORTING, PAYMENTS, ORDERS; NOTIFICATIONS chỉ nhận "sự kiện"/yêu cầu và gửi thông báo tương ứng.

---

## 2. Personas/roles involved

Theo Glossary và Discovery, module NOTIFICATIONS phục vụ hoặc tương tác với các vai trò sau:

- **Tầng 3 (End User)** – người nhận thông báo về:
	- OTP ZNS khi đăng ký/đăng nhập lần đầu.
	- Trạng thái đơn hàng (nhắc nhận hàng).
	- Nhắc nợ trả chậm/công nợ (nếu công ty bật trả chậm).
- **Tầng 2** – có thể nhận thông báo:
	- Liên quan đến đơn hàng mình phụ trách (nếu mô hình phân công đơn sử dụng ZNS – chi tiết mức độ dùng ZNS cho Tầng 2 chưa được nêu rõ → Open Question).
	- Thay đổi cấu hình hoa hồng hoặc thông tin mạng lưới.
- **Tầng 1** – nhận thông báo liên quan đến:
	- Thay đổi chính sách hoa hồng cho mạng lưới của mình (MVP nhắc tới thay đổi hoa hồng ở mức chung – chi tiết người nhận cần xác nhận → Open Question).
- **Shipper** (có thể trùng với Tầng 2) – có thể nhận thông báo vận hành (ví dụ: danh sách đơn giao trong ca, nhắc chốt ca) nếu quyết định sử dụng ZNS cho luồng này (hiện **chưa mô tả rõ** trong Discovery → Open Question).
- **Admin Portal roles (Super Admin, Ops, Finance, QC, Support)**:
	- Quản lý và xem ZNS template.
	- Xem log thông báo đã gửi để tra soát (ví dụ: kiểm tra lý do user nói không nhận được OTP/nhắc nhở).
	- Cấu hình bật/tắt một số loại thông báo theo công ty/module (chi tiết cần được chốt thêm).

- **Các module hệ thống khác (như actor kỹ thuật)**:
	- **AUTH** – yêu cầu gửi OTP ZNS.
	- **ORDERS** – yêu cầu gửi thông báo nhắc nhận hàng, thay đổi trạng thái đơn (nếu được chọn gửi qua ZNS).
	- **PAYMENTS** – yêu cầu gửi thông báo nhắc nợ, cảnh báo công nợ.
	- **REPORTING/ADMIN** – yêu cầu gửi thông báo khi cấu hình hoa hồng thay đổi (theo MVP Scope).

- **ZNS (Zalo Notification Service)** – hệ thống ngoài, được NOTIFICATIONS tích hợp để thực sự gửi tin.

---

## 3. Functional requirements (FR-NOTIFICATIONS-###)

> Ghi chú chung: Mọi API nội bộ được mô tả ở đây chỉ ghi **API: TBD (Tech Lead)**, không liệt kê path/HTTP verb cụ thể.

### FR-NOTIFICATIONS-001 — Quản lý template thông báo ZNS

**Mô tả**
- Hệ thống phải cho phép quản lý tập trung các **template thông báo ZNS** dùng cho OTP và các thông báo nghiệp vụ khác.
- Mỗi template tối thiểu bao gồm:
	- Mã template (code) – dùng cho các module khác tham chiếu.
	- Tên mô tả (description).
	- Nội dung template (message body) với placeholder cho dữ liệu động (ví dụ: {{otp}}, {{order_code}}, {{due_date}}…).
	- Loại thông báo (OTP, Nhắc nhận hàng, Nhắc nợ, Thay đổi hoa hồng, Khác...).
	- Ngôn ngữ (nếu có đa ngôn ngữ trong tương lai – hiện chỉ giả định tiếng Việt, cần xác nhận).
	- Trạng thái template (Active/Inactive).

**Luồng chuẩn**
1. Admin/Super Admin truy cập màn hình "Quản lý template ZNS".
2. Admin có thể thêm mới/nhân bản/cập nhật template với các trường trên.
3. Hệ thống kiểm tra các placeholder trong nội dung template có hợp lệ (không chứa ký hiệu ngoài danh sách placeholder được chấp nhận cho loại thông báo đó).
4. Hệ thống lưu template và ghi audit log thay đổi.
5. Các module khác khi yêu cầu gửi thông báo sẽ chỉ định mã template; NOTIFICATIONS dùng đúng template này để dựng nội dung gửi.

**Tiêu chí kiểm thử (testable)**
- Không thể tạo hai template với **cùng mã template** trong trạng thái Active.
- Khi template ở trạng thái Inactive, yêu cầu gửi thông báo với template đó phải bị từ chối hoặc chuyển sang trạng thái lỗi theo BR-NOTIFICATIONS (chi tiết ở mục 4 & 6).
- Khi nội dung template chứa placeholder không nằm trong tập cho phép của loại thông báo tương ứng, hệ thống từ chối lưu template và trả lỗi rõ ràng cho Admin.

**Ví dụ dữ liệu (fake)**
- Template OTP:
	- Code: `ZNS_OTP_LOGIN`
	- Type: OTP
	- Body: "Ma OTP cua ban la {{otp}}, hieu luc trong {{ttl}} phut. KHONG chia se ma nay voi bat ky ai."
- Template nhắc nhận hàng:
	- Code: `ZNS_REMIND_PICKUP`
	- Type: Nhắc nhận hàng
	- Body: "Don hang {{order_code}} da san sang tai diem nhan {{pickup_point}}. Vui long den nhan truoc {{pickup_deadline}}."

---

### FR-NOTIFICATIONS-002 — Gửi OTP ZNS theo yêu cầu từ AUTH

**Mô tả**
- Khi module AUTH yêu cầu gửi OTP cho một số điện thoại, NOTIFICATIONS phải:
	- Nhận yêu cầu với các tham số cần thiết (SĐT, OTP, thời gian hiệu lực, mã template…).
	- Dựng nội dung ZNS từ template tương ứng.
	- Gọi ZNS để gửi OTP.
	- Trả về kết quả gửi cho AUTH (thành công/thất bại) để AUTH quyết định tiếp theo.
- NOTIFICATIONS **không** quyết định logic OTP (độ dài, TTL, số lần gửi lại…) – phần này thuộc AUTH và Business-Rules toàn cục; NOTIFICATIONS chỉ đảm bảo gửi theo yêu cầu.

**Luồng chuẩn**
1. AUTH gọi **API: TBD (Tech Lead)** của NOTIFICATIONS với payload gồm:
	 - phone_number
	 - otp_code
	 - ttl_minutes
	 - correlationId
	 - template_code (ví dụ: `ZNS_OTP_LOGIN`)
2. NOTIFICATIONS kiểm tra:
	 - SĐT có định dạng hợp lệ.
	 - Template tồn tại và đang Active.
3. NOTIFICATIONS dựng nội dung ZNS dựa trên template và tham số.
4. NOTIFICATIONS gọi ZNS Service và nhận kết quả (success/failure + mã lỗi nếu có).
5. NOTIFICATIONS ghi log gửi (bao gồm correlationId, phone_number masked, template_code, status).
6. NOTIFICATIONS trả kết quả cho AUTH.

**Tiêu chí kiểm thử**
- Với SĐT hợp lệ và template Active, nếu ZNS trả kết quả thành công, NOTIFICATIONS phải trả trạng thái success cho AUTH và lưu log tương ứng.
- Nếu ZNS trả lỗi (ví dụ: sai cấu hình, quota vượt quá), NOTIFICATIONS phải trả trạng thái failure kèm mã lỗi nội bộ để AUTH có thể xử lý (ví dụ hiển thị "Không gửi được OTP, vui lòng thử lại").
- Nếu template không tồn tại hoặc Inactive, NOTIFICATIONS phải từ chối yêu cầu và trả lỗi rõ ràng.

**Ví dụ dữ liệu (fake)**
- Request từ AUTH:
	- phone_number: "0900123456"
	- otp_code: "123456"
	- ttl_minutes: 5
	- template_code: "ZNS_OTP_LOGIN"
	- correlationId: "corr-otp-0001"
- Kết quả ZNS: success, message_id: "zns-msg-9999".

---

### FR-NOTIFICATIONS-003 — Gửi thông báo/nhắc nhở nghiệp vụ theo sự kiện

**Mô tả**
- Khi các module ORDERS, PAYMENTS, REPORTING/ADMIN phát sinh **sự kiện nghiệp vụ**, họ có thể yêu cầu NOTIFICATIONS gửi thông báo ZNS tới User liên quan.
- Những nhóm sự kiện chính dựa trên Discovery/MVP-Scope:
	- Đơn hàng:
		- Đơn mới được tạo (tuỳ cấu hình có gửi hay không).
		- Đơn chuyển sang trạng thái "Sẵn sàng nhận" → nhắc nhận hàng.
	- Thanh toán / công nợ:
		- Gần tới hạn thanh toán trả chậm.
		- Đã quá hạn trả chậm.
	- Hoa hồng / chính sách:
		- Thay đổi cấu hình hoa hồng theo ngành hàng.

**Luồng chuẩn (khung)**
1. Module nguồn (ORDERS, PAYMENTS, REPORTING/ADMIN) phát sinh sự kiện và gửi yêu cầu tới **API: TBD (Tech Lead)** của NOTIFICATIONS với các thông tin tối thiểu:
	 - event_type (ví dụ: ORDER_READY_FOR_PICKUP, DEBT_DUE_REMINDER, COMMISSION_CONFIG_CHANGED...)
	 - recipient_phone_number (hoặc một tập SĐT)
	 - business_payload (order_code, pickup_point, due_date, new_commission_rate…)
	 - correlationId
2. NOTIFICATIONS ánh xạ event_type → template_code cấu hình sẵn (theo từng công ty/hệ thống, nếu có).
3. NOTIFICATIONS dựng nội dung từ template tương ứng.
4. NOTIFICATIONS kiểm tra rate-limit (ví dụ: không gửi quá N lần cùng loại thông báo đến cùng 1 số trong khung thời gian X – N/X sẽ được quy định sau → Open Question).
5. NOTIFICATIONS gửi ZNS, lưu log kết quả.

**Tiêu chí kiểm thử (testable)**
- Với event_type đã được cấu hình template và SĐT hợp lệ, NOTIFICATIONS phải gửi đúng nội dung, đúng người nhận.
- Nếu event_type chưa được cấu hình template (template missing), hệ thống phải:
	- Không gửi ZNS.
	- Ghi lại log lỗi để Admin có thể bổ sung template.
- Nếu rate-limit bị vượt (theo ngưỡng cấu hình), NOTIFICATIONS phải từ chối gửi thêm và ghi lại lý do.

**Ví dụ dữ liệu (fake)**
- Sự kiện nhắc nhận hàng:
	- event_type: "ORDER_READY_FOR_PICKUP"
	- recipient_phone_number: "0900123456"
	- business_payload: {"order_code": "ORD-2025-0001", "pickup_point": "Điểm nhận A - Công ty May KCN A", "pickup_deadline": "2025-06-30"}
	- Mapped template_code: "ZNS_REMIND_PICKUP"

---

### FR-NOTIFICATIONS-004 — Nhật ký gửi thông báo & tra cứu log

**Mô tả**
- Hệ thống phải lưu **nhật ký gửi thông báo** để hỗ trợ tra soát khi người dùng/đối tác phản ánh "không nhận được" hoặc có vấn đề liên quan đến ZNS.

**Luồng chuẩn**
1. Mỗi lần gửi ZNS (OTP hoặc thông báo nghiệp vụ), NOTIFICATIONS ghi một bản ghi log tối thiểu gồm:
	 - CorrelationId.
	 - RecipientPhone (mask một phần, ví dụ: 0900***456 – chi tiết mask do Tech Lead xác nhận).
	 - TemplateCode / EventType.
	 - Channel (ZNS).
	 - Status (Success/Failure/Pending nếu hỗ trợ async).
	 - ProviderMessageId (nếu ZNS trả về).
	 - Timestamp.
2. Admin/Super Admin có thể truy vấn log này theo filter:
	 - thời gian,
	 - SĐT (search theo pattern mask),
	 - template/event_type,
	 - status.

**Tiêu chí kiểm thử**
- Mọi yêu cầu gửi ZNS (kể cả thất bại) đều tạo một record log.
- Tìm kiếm log theo correlationId phải trả về đúng thông tin đã gửi.
- Log **không hiển thị toàn bộ nội dung tin nhắn** nếu có chứa dữ liệu nhạy cảm như OTP; ít nhất phải che mờ/mask phù hợp (mức độ mask cụ thể → Open Question/Data-Classification).

---

### FR-NOTIFICATIONS-005 — Cấu hình bật/tắt nhóm thông báo theo công ty (company-level configuration)

**Mô tả**
- Để kiểm soát chi phí ZNS (theo rủi ro R2 trong MVP-Scope), hệ thống cần cho phép cấu hình **bật/tắt một số nhóm thông báo** theo **Công ty** hoặc toàn hệ thống.
- Nhóm thông báo tối thiểu:
	- OTP (bắt buộc, nhưng có thể cấu hình template, TTL ở AUTH/NOTIFICATIONS).
	- Nhắc nhận hàng.
	- Nhắc nợ trả chậm.
	- Thông báo thay đổi hoa hồng.

**Luồng chuẩn (khung)**
1. Super Admin/Finance/Ops (tuỳ phân quyền) truy cập màn hình cấu hình thông báo.
2. Admin chọn Công ty (hoặc cấu hình mặc định toàn hệ thống).
3. Admin bật/tắt từng nhóm thông báo (trừ OTP nếu được yêu cầu bắt buộc – chi tiết chính sách cần xác nhận).
4. NOTIFICATIONS lưu cấu hình và áp dụng khi xử lý yêu cầu gửi từ các module khác (ví dụ: nếu công ty A tắt "Nhắc nhận hàng" thì event ORDER_READY_FOR_PICKUP của công ty A sẽ không tạo gửi ZNS).

**Tiêu chí kiểm thử (testable)**
- Khi một nhóm thông báo bị tắt cho một công ty, mọi yêu cầu tương ứng từ các module khác phải **không gửi ZNS** và được log rõ lý do.
- Khi bật lại nhóm thông báo, các sự kiện mới phải được xử lý bình thường.

---

## 4. Business rules references (BR-###)

> Phần này trích/khung hoá các Business Rule liên quan đến NOTIFICATIONS dựa trên Discovery; mã BR cụ thể sẽ được quản lý trung tâm trong Business-Rules.md.

- **BR-NOTIFICATIONS-001 — OTP ZNS & giảm chi phí**
	- OTP ZNS chỉ được dùng cho **lần đăng ký/đăng nhập đầu tiên** (theo BR toàn cục ở SRS-00-Overview); các lần sau ưu tiên Session Token/Refresh Token để **giảm tần suất gửi OTP**.
	- NOTIFICATIONS phải hỗ trợ việc gửi OTP đúng theo yêu cầu từ AUTH, nhưng **không chủ động tăng tần suất** ngoài yêu cầu.

- **BR-NOTIFICATIONS-002 — Sử dụng ZNS cho nhắc nhở vận hành**
	- Một số thông báo (nhắc nhận hàng, nhắc nợ, thay đổi hoa hồng) được thực hiện qua ZNS theo cấu hình trong MVP Scope.
	- Việc kích hoạt/tắt từng nhóm thông báo và tần suất gửi phải có cơ chế cấu hình, nhằm cân bằng **trải nghiệm người dùng** và **chi phí ZNS**.

- **BR-NOTIFICATIONS-003 — Không log dữ liệu nhạy cảm trong nội dung thông báo**
	- Nội dung OTP hoặc dữ liệu nhạy cảm khác không được lưu đầy đủ trong log; chỉ lưu phần cần thiết để tra soát (ví dụ: loại thông báo, template, trạng thái, thời gian, recipient mask).
	- Mức độ mask, ẩn/mã hoá phải tuân theo Data-Classification và Security-Baseline của dự án (chi tiết chưa được mô tả → tham chiếu tài liệu bảo mật).

Các BR chi tiết khác (như tối đa số thông báo/ngày cho 1 User, hành vi khi ZNS không đáp ứng SLA) sẽ được bổ sung sau khi làm rõ với khách hàng/Tech Lead.

---

## 5. Data inputs/outputs (conceptual)

### 5.1. Thực thể chính (khái niệm)

- **NotificationTemplate**
	- Mã template.
	- Loại thông báo (OTP, Nhắc nhận hàng, Nhắc nợ, Thay đổi hoa hồng...).
	- Nội dung template và placeholder.
	- Ngôn ngữ (TBD nếu đa ngôn ngữ).
	- Trạng thái (Active/Inactive).

- **NotificationConfig** (cấu hình nhóm thông báo theo Công ty/hệ thống)
	- Scope: Toàn hệ thống hoặc theo Công ty.
	- Cờ bật/tắt cho từng nhóm thông báo.
	- Thông tin liên kết tới Công ty (CompanyId) nếu cấu hình theo công ty.

- **NotificationLog**
	- NotificationId.
	- CorrelationId.
	- Recipient (SĐT, dạng đã mask hoặc tách trường raw/masked theo Data-Classification).
	- TemplateCode / EventType.
	- Channel (ZNS).
	- Status (Success/Failure/... theo thiết kế).
	- ProviderMessageId.
	- CreatedAt.

- **External dependency: ZNS Provider**
	- NOTIFICATIONS gửi payload tới ZNS theo format do Zalo quy định (chi tiết ở tài liệu tích hợp).
	- Nhận lại kết quả gửi (success/failure) và ID thông điệp.

### 5.2. Luồng dữ liệu chính

- **Luồng OTP từ AUTH → NOTIFICATIONS → ZNS → User**
	- Input (từ AUTH): phone_number, otp_code, ttl_minutes, template_code, correlationId.
	- Output (từ NOTIFICATIONS tới AUTH): status (success/failure), error_code nội bộ nếu có.
	- Side-effect: bản ghi NotificationLog.

- **Luồng sự kiện nghiệp vụ → NOTIFICATIONS → ZNS → User**
	- Input (từ ORDERS/PAYMENTS/REPORTING/ADMIN): event_type, recipient_phone(s), business_payload, correlationId.
	- Output (từ NOTIFICATIONS về module gọi): kết quả gửi tổng quan (ví dụ: thành công với X số, thất bại với Y số; chi tiết biểu diễn do Tech Lead thiết kế).
	- Side-effect: các bản ghi NotificationLog.

Chi tiết bảng/column, index sẽ được mô tả trong tài liệu DB-NOTIFICATIONS theo chuẩn DB-Design-Rules.

---

## 6. Validations & edge cases

### 6.1. Validations

- **V-NOTI-001 – Định dạng SĐT**
	- SĐT trong mọi yêu cầu gửi thông báo phải thoả điều kiện định dạng hợp lệ (pattern cụ thể sẽ được Tech Lead xác định, ví dụ: 10 số, bắt đầu bằng 0).

- **V-NOTI-002 – Template tồn tại và Active**
	- Mã template trong yêu cầu gửi phải tồn tại.
	- Trạng thái template phải là Active; nếu Inactive, hệ thống từ chối gửi.

- **V-NOTI-003 – Placeholder hợp lệ**
	- Khi dựng nội dung từ template, mọi placeholder trong template phải có giá trị tương ứng trong payload; nếu thiếu, hệ thống phải từ chối gửi và log lỗi.

- **V-NOTI-004 – Kiểm tra cấu hình nhóm thông báo (NotificationConfig)**
	- Trước khi gửi, hệ thống phải kiểm tra xem nhóm thông báo tương ứng có đang được bật cho scope (Công ty/toàn hệ thống) hay không.

- **V-NOTI-005 – Rate limit cơ bản**
	- Hệ thống phải kiểm tra các rule rate-limit được cấu hình (ví dụ: tối đa X thông báo cùng loại/ngày cho 1 số).
	- Nếu vượt quá, hệ thống từ chối gửi thêm và log lại lý do.

### 6.2. Edge cases

1. **E-NOTI-001 – ZNS không khả dụng / lỗi hệ thống ngoài**
	 - Mô tả: Khi gọi ZNS, NOTIFICATIONS nhận lỗi (timeout, network error, provider down...).
	 - Mong đợi:
		 - Mark thông báo là Failure trong NotificationLog với mã lỗi phù hợp.
		 - Trả failure cho module gọi (AUTH, ORDERS, PAYMENTS...).
		 - Chính sách retry tự động (nếu có) sẽ được xác định trong thiết kế kỹ thuật (hiện để Open Question).

2. **E-NOTI-002 – Template bị xoá hoặc Inactive trong khi module khác vẫn dùng**
	 - Mô tả: Admin vô hiệu hoá một template nhưng AUTH/ORDERS vẫn gửi yêu cầu với template_code đó.
	 - Mong đợi:
		 - Yêu cầu gửi bị từ chối.
		 - Log lỗi rõ ràng để Admin nhận biết.

3. **E-NOTI-003 – Payload thiếu dữ liệu để render template**
	 - Mô tả: Yêu cầu gửi nhắc nhận hàng thiếu order_code hoặc pickup_point.
	 - Mong đợi:
		 - Hệ thống không gửi ZNS.
		 - Log lỗi "missing placeholder data".

4. **E-NOTI-004 – Gửi trùng lặp do retry từ module nguồn**
	 - Mô tả: ORDERS/PAYMENTS gửi lại cùng một sự kiện nhiều lần (ví dụ do retry logic), nhưng business muốn chỉ gửi 1 thông báo cho User.
	 - Mong đợi:
		 - NOTIFICATIONS có khả năng nhận diện trùng lặp cơ bản thông qua correlationId hoặc một trường idempotency khác (ví dụ: event_id) nếu được module nguồn cung cấp.
		 - Hành vi cụ thể (cho phép gửi lại hay bỏ qua) sẽ do Business-Rules quyết định (TBD), nhưng SRS ghi nhận nhu cầu tránh spam.

5. **E-NOTI-005 – Chi phí ZNS vượt ngân sách dự kiến**
	 - Mô tả: Tần suất gửi thông báo cao hơn dự kiến (ví dụ: nhắc nhận hàng quá thường xuyên).
	 - Mong đợi (khung):
		 - Admin có thể giảm tần suất bằng cách tắt/bật nhóm thông báo hoặc chỉnh lại cấu hình.
		 - Cần dashboard/báo cáo đơn giản về số lượng thông báo theo loại/công ty để theo dõi (chi tiết thuộc REPORTING hoặc ADMIN, không nằm trọn trong NOTIFICATIONS; ở đây chỉ ghi nhu cầu nghiệp vụ).

---

## 7. Non-functional requirements (module-specific)

- **Rate limits & bảo vệ hệ thống**
	- NOTIFICATIONS phải áp dụng rate limit ở mức service để bảo vệ bản thân và dịch vụ ZNS (chi tiết ngưỡng do Tech Lead xác định).
	- Cần cơ chế backoff/circuit-breaker khi ZNS thường xuyên lỗi để tránh làm tê liệt hệ thống.

- **Audit logs**
	- Mọi thay đổi template và cấu hình NotificationConfig phải được ghi audit log (ai, khi nào, thay đổi gì).
	- Mọi yêu cầu gửi ZNS (kể cả thất bại) phải được log với correlationId.

- **Performance hotspots**
	- Gửi hàng loạt thông báo (ví dụ cho nhiều User cùng lúc) có thể là điểm nóng; cần thiết kế cơ chế batch/queue hợp lý trong giai đoạn kiến trúc.
	- Truy vấn NotificationLog cần được index phù hợp theo thời gian, template/event_type, status để tránh chậm.

- **Bảo mật & dữ liệu nhạy cảm**
	- Không lưu đầy đủ OTP trong log.
	- Không log số điện thoại ở dạng full trong kênh log ứng dụng (chỉ mask hoặc log ở kênh kiểm soát chặt hơn nếu thật sự cần thiết, tuân theo Data-Classification).

---

## 8. Open Questions / Assumptions for this module

**Câu hỏi mở (cần làm rõ với khách hàng/Tech Lead)**

- **Q-NOTI-001** – Ngoài OTP, nhắc nhận hàng, nhắc nợ, thay đổi hoa hồng, còn loại thông báo nghiệp vụ nào bắt buộc phải dùng ZNS trong MVP?
- **Q-NOTI-002** – Có hỗ trợ đa kênh (email, SMS khác, app push) trong giai đoạn MVP không, hay chỉ ZNS?
- **Q-NOTI-003** – Có yêu cầu cho phép User cấu hình opt-in/opt-out hoặc tần suất nhận một số loại thông báo (ngoài OTP bắt buộc) không?
- **Q-NOTI-004** – Ngưỡng rate limit cụ thể cho từng loại thông báo (OTP, nhắc nhận hàng, nhắc nợ...) là bao nhiêu?
- **Q-NOTI-005** – Chính sách lưu retention cho NotificationLog (bao lâu, mức chi tiết dữ liệu được giữ lại)?
- **Q-NOTI-006** – Các vai trò nào trên Admin Portal được phép xem nội dung gần đầy đủ của thông báo (ví dụ Support khi xử lý khiếu nại), và phải mask đến mức nào?
- **Q-NOTI-007** – Có yêu cầu gửi thông báo cho Tầng 2/Shipper qua ZNS về danh sách đơn trong ca, nhắc chốt ca hay không, hay chỉ dùng web UI?

**Giả định (Assumptions – cần xác nhận)**

- **A-NOTI-001** – Trong MVP, kênh duy nhất được sử dụng cho NOTIFICATIONS là **ZNS**, không có email/app push chính thức.
- **A-NOTI-002** – OTP nội dung chi tiết và TTL do AUTH quyết định; NOTIFICATIONS chỉ nhận giá trị đã được sinh và gửi đi.
- **A-NOTI-003** – Mặc định tất cả User đều opt-in cho thông báo nghiệp vụ (nhắc nhận hàng, nhắc nợ...), trừ khi sau này có yêu cầu cụ thể về opt-out.
- **A-NOTI-004** – Rate limit cơ bản sẽ được cấu hình ở mức toàn hệ thống (global) trước khi có yêu cầu tuỳ biến chi tiết per company/per user.
- **A-NOTI-005** – NotificationLog sẽ được lưu tối thiểu trong 1–2 năm để phục vụ tra soát, trừ khi yêu cầu pháp lý khác được thống nhất.
