
# SRS — AUTH: Authentication & Authorization

## 1. Module purpose & boundaries (in/out)

**Mục đích**
- Cung cấp cơ chế **đăng ký và đăng nhập** an toàn cho:
  - Công nhân KCN (Tầng 3), Tầng 2, Tầng 1 sử dụng Portal End User.
  - Các vai trò Admin (Super Admin, Ops, QC, Finance, Support) sử dụng Portal Admin.
- Giảm chi phí OTP ZNS bằng cách chỉ dùng OTP **ở lần đầu**, sau đó dùng **Session Token / Refresh Token** gắn với thiết bị/browser để đăng nhập tự động.

**Trong phạm vi (IN)**
- Đăng ký người dùng mới bằng **Số điện thoại (SĐT) + OTP ZNS**.
- Đăng nhập lần đầu bằng OTP ZNS.
- Đăng nhập Admin Portal bằng SĐT/Email + mật khẩu (và/hoặc OTP theo cấu hình).
- Cấp, làm mới và thu hồi **Session Token / Refresh Token** cho người dùng.
- Nhận diện thiết bị/browser để gắn phiên đăng nhập với thiết bị.
- Kiểm soát truy cập theo **vai trò** (Role) ở mức module (AUTH cung cấp thông tin role cho các module khác dùng).

**Ngoài phạm vi (OUT)**
- Chi tiết giao diện người dùng (UI) ngoài những gì mô tả ở Discovery.
- Tính toán tầng (Tầng 1/2/3), hoa hồng, hạn mức trả chậm (thuộc module USERS/PAYMENTS).
- Gửi ZNS (thuộc module NOTIFICATIONS – AUTH chỉ yêu cầu "gửi OTP" với API: TBD (Tech Lead)).
- Chi tiết chính sách mật khẩu, lock tài khoản, xác thực đa yếu tố khác ngoài OTP lần đầu (chưa được mô tả → Open Questions).

---

## 2. Personas / Roles liên quan

Theo Glossary và Discovery:

- **Tầng 3** – Công nhân KCN, người mua hàng cuối cùng.
- **Tầng 2** – Nhân sự giới thiệu, có thể kiêm **Shipper** và thu COD.
- **Tầng 1** – Quản lý mạng lưới, xem tổng quan & hoa hồng (read-only trong MVP).
- **Shipper** – Vai trò giao hàng tại điểm nhận, có thể là Tầng 2 hoặc user độc lập.
- **Admin Portal**:
  - **Super Admin** – quản trị cao nhất, phân quyền, duyệt hủy.
  - **Ops** – vận hành đơn hàng.
  - **QC** – kiểm duyệt lô hàng.
  - **Finance** – đối soát thanh toán, cấu hình trả chậm.
  - **Support** – hỗ trợ người dùng.

AUTH phục vụ **xác thực (AuthN)** cho tất cả các vai trò trên và cung cấp thông tin **vai trò (AuthZ)** cho các module khác.

---

## 3. Functional Requirements (FR-AUTH-###)

> Ghi chú: Mọi API gọi ra/nhận vào đều ghi dạng **API: TBD (Tech Lead)**, không liệt kê path cụ thể.

### FR-AUTH-001 – Đăng ký người dùng bằng SĐT + OTP ZNS (End User)

**Mô tả**
- Người dùng (Tầng 1/2/3) nhập thông tin **Họ tên, Công ty, SĐT** để đăng ký.
- Hệ thống gửi OTP qua ZNS tới SĐT này và yêu cầu người dùng nhập OTP để xác thực lần đầu.

**Điều kiện / Luồng chuẩn (happy path)**
1. Người dùng truy cập màn hình Đăng ký.
2. Nhập đầy đủ: Họ tên, chọn Công ty (từ danh sách công ty đã cấu hình), nhập SĐT.
3. Hệ thống kiểm tra SĐT có đúng định dạng (ví dụ: 10 số, bắt đầu bằng 0…) – chi tiết pattern sẽ được Tech Lead xác định.
4. Hệ thống gọi **API: TBD (Tech Lead)** tới module NOTIFICATIONS để gửi OTP ZNS.
5. Người dùng nhận OTP và nhập vào màn hình "Nhập OTP".
6. Nếu OTP hợp lệ và còn hiệu lực, hệ thống tạo tài khoản người dùng và gán tầng dựa trên danh sách nhân sự (USERS).

**Tiêu chí kiểm thử (testable)**
- Khi nhập SĐT hợp lệ, hệ thống **ghi nhận yêu cầu OTP** và trạng thái gửi (thành công/thất bại) trong log.
- OTP chỉ chấp nhận nếu **khớp giá trị** và **chưa hết thời gian hiệu lực** (thời gian hiệu lực cụ thể chưa được nêu → Open Question).
- Nếu OTP hợp lệ, tài khoản User mới được tạo duy nhất cho SĐT đó.
- Nếu OTP không hợp lệ, tài khoản không được tạo.

**Ví dụ dữ liệu (fake)**
- Họ tên: "Nguyễn Văn A"  
- Công ty: "Công ty May KCN A"  
- SĐT: "0900123456"  
- OTP: "123456"  
- Tầng (sau khi match USERS): 3

---

### FR-AUTH-002 – Đăng nhập lần đầu bằng OTP ZNS (End User)

**Mô tả**
- Người dùng đã đăng ký, đăng nhập lần đầu bằng SĐT + OTP ZNS.

**Điều kiện / Luồng chuẩn**
1. Người dùng nhập SĐT trên màn hình Đăng nhập.
2. Hệ thống kiểm tra SĐT đã tồn tại trong hệ thống.
3. Hệ thống gửi OTP ZNS tới SĐT (API: TBD (Tech Lead)).
4. Người dùng nhập OTP.
5. Nếu OTP hợp lệ, hệ thống:
	- Tạo **Session Token** và **Refresh Token**.
	- Gắn token với thiết bị/browser hiện tại.
	- Cho phép truy cập Portal End User với vai trò (Tầng 1/2/3) tương ứng.

**Tiêu chí test**
- Đăng nhập lần đầu **bắt buộc phải qua OTP** (không có mật khẩu).
- Sau khi OTP hợp lệ, token được cấp và lưu (conceptual, không mô tả cấu trúc chi tiết).
- Token được gắn với thông tin thiết bị/browser (ví dụ: user agent, fingerprint – chi tiết kỹ thuật do Tech Lead xác định).

**Ví dụ dữ liệu**
- SĐT: "0900123456"  
- OTP: "654321"  
- Session Token: "sess_fake_abc123" (fake)  
- Refresh Token: "ref_fake_xyz789" (fake)

---

### FR-AUTH-003 – Đăng nhập tự động bằng Session Token / Refresh Token (End User)

**Mô tả**
- Sau lần OTP đầu tiên, người dùng không phải nhập lại OTP cho những lần truy cập sau **trên cùng thiết bị/browser**, cho đến khi token hết hạn hoặc người dùng logout.

**Điều kiện / Luồng chuẩn**
1. Người dùng mở lại Portal End User trên cùng thiết bị/browser.
2. Trình duyệt gửi Session Token (ví dụ trong cookie/local storage – chi tiết kỹ thuật do Tech Lead xác định).
3. Hệ thống kiểm tra token:
	- Nếu còn hiệu lực → xác thực thành công, không yêu cầu OTP.
	- Nếu hết hạn nhưng còn Refresh Token hợp lệ → phát hành Session Token mới.
4. Nếu cả Session Token và Refresh Token không hợp lệ, hệ thống yêu cầu đăng nhập lại bằng OTP.

**Tiêu chí test**
- Trong thời gian token còn hiệu lực, người dùng **không bị yêu cầu OTP** lại.
- Khi token hết hạn, nhưng Refresh Token hợp lệ, người dùng vẫn đăng nhập được mà **không cần OTP**.
- Khi Refresh Token hết hạn/không hợp lệ, người dùng **phải nhập lại OTP**.

**Edge liên quan được mô tả chi tiết ở mục 6. Validations & edge cases.**

---

### FR-AUTH-004 – Đăng xuất & thu hồi token (End User)

**Mô tả**
- Người dùng có thể chủ động đăng xuất, hệ thống thu hồi token tương ứng để dừng đăng nhập tự động trên thiết bị đó.

**Điều kiện / Luồng chuẩn**
1. Người dùng nhấn "Đăng xuất" trên Portal End User.
2. Hệ thống:
	- Xoá/đánh dấu vô hiệu Session Token hiện hành.
	- (Tùy chính sách) có thể xoá/vô hiệu Refresh Token hoặc giữ lại – chi tiết chưa được nêu → Assumption.
3. Lần truy cập sau, người dùng bị yêu cầu đăng nhập lại (OTP hoặc cơ chế khác theo chính sách).

**Tiêu chí test**
- Sau logout, mọi request sử dụng token cũ đều bị từ chối (401/unauthenticated ở mức hệ thống – mapping HTTP sẽ được định nghĩa ở mức API, không thuộc AUTH SRS chi tiết).

---

### FR-AUTH-005 – Đăng nhập Admin Portal

**Mô tả**
- Admin (Super Admin, Ops, QC, Finance, Support) truy cập Portal Admin bằng SĐT/Email + mật khẩu (và/hoặc OTP ZNS nếu được bật cho Admin).

**Điều kiện / Luồng chuẩn**
1. Admin truy cập màn hình Đăng nhập Admin.
2. Nhập SĐT hoặc Email, và mật khẩu.
3. Hệ thống kiểm tra thông tin đăng nhập.
4. Nếu hợp lệ, hệ thống cấp Session Token Admin và gán **Role** tương ứng.
5. Nếu không hợp lệ, hiển thị thông báo lỗi chung (không tiết lộ mật khẩu đúng/sai chi tiết).

**Tiêu chí test**
- Admin hợp lệ có thể đăng nhập và thấy đúng các chức năng tương ứng với role (chi tiết quyền ở mục 5 & 6).
- Thử đăng nhập với user không có quyền Admin → bị từ chối.
- Số lần sai mật khẩu liên tiếp để khoá tài khoản **chưa được quy định** → Open Question.

**Ghi chú**
- Việc có yêu cầu OTP ZNS cho Admin hay không **không được mô tả rõ** trong tài liệu → Open Question.

---

## 4. Business rules references (BR-AUTH-###)

> Các BR dưới đây là **trích từ Discovery** và được giới hạn trong phạm vi module AUTH.

- **BR-AUTH-001** – OTP ZNS chỉ bắt buộc ở **lần đăng ký/đăng nhập đầu tiên** cho End User; các lần sau phải ưu tiên dùng **Session Token / Refresh Token gắn thiết bị/browser** để giảm chi phí ZNS.
- **BR-AUTH-002** – Token đăng nhập phải gắn với **thiết bị/browser** để tránh lạm dụng; nếu người dùng đổi thiết bị/browser, hệ thống có thể yêu cầu OTP lại (chi tiết chính sách chưa xác định → Open Question).
- **BR-AUTH-003** – Chỉ những người dùng có bản ghi hợp lệ trong danh sách nhân sự (import từ USERS) mới được gán Tầng 1/2/3 tự động; trường hợp không khớp phải xử lý theo rule đã được thống nhất ở mức toàn hệ thống (Q7).
- **BR-AUTH-004** – Mọi lần đăng ký/đăng nhập (thành công hoặc thất bại) phải được ghi nhận vào audit log với tối thiểu: thời gian, SĐT/Email, kết quả.

Các BR này cần được tổng hợp lại vào docs/02-Requirements/Business-Rules.md dưới dạng BR-xxx toàn hệ thống.

---

## 5. Data inputs/outputs (conceptual)

### 5.1. Thực thể chính (khái niệm)

- **User** (End User – Tầng 1/2/3):
  - Trường chính liên quan AUTH: Phone, FullName, CompanyId, Tier, RefTier (thông tin Tier nằm ở USERS, AUTH chỉ đọc).
- **AdminUser**:
  - SĐT hoặc Email, mật khẩu (hash), Role (Super Admin/Ops/QC/Finance/Support).
- **OtpRequest** (khái niệm):
  - Phone, OtpCode (mã OTP), ExpiryTime, Status (Pending/Used/Expired), CreatedAt.
- **SessionToken**:
  - TokenValue, UserId/AdminUserId, DeviceId/BrowserId (khái niệm), ExpiryTime, CreatedAt, Revoked (true/false).
- **RefreshToken**:
  - TokenValue, UserId/AdminUserId, DeviceId/BrowserId, ExpiryTime, Revoked.

### 5.2. Luồng dữ liệu chính

- **Đăng ký/Đăng nhập lần đầu (End User)**:
  - Input: Họ tên, Công ty, SĐT, OTP.
  - Output: Bản ghi User mới (nếu chưa tồn tại), Tier thông tin, Session Token + Refresh Token.
- **Đăng nhập tự động**:
  - Input: Session Token (và/hoặc Refresh Token) từ client.
  - Output: Quyết định chấp nhận/không chấp nhận, thông tin User/Tier/Role để các module khác dùng.
- **Đăng nhập Admin**:
  - Input: Email/SĐT, mật khẩu.
  - Output: Session Token Admin + Role.

**Lưu ý**: Cấu trúc chi tiết của bảng/tài liệu DB sẽ được định nghĩa ở tài liệu DB thiết kế (DB-AUTH/DB-USERS), không nằm trong SRS.

---

## 6. Validations & edge cases

### 6.1. Validations

- **V1 – SĐT hợp lệ**: SĐT phải:
  - Không rỗng.
  - Đáp ứng pattern hợp lệ (ví dụ: 10 số, bắt đầu bằng 0). Pattern cụ thể: **Open Question** cho Tech Lead.
- **V2 – OTP hợp lệ**:
  - Khớp với OTP đã gửi cho SĐT tương ứng, và trong thời gian hiệu lực.
  - Số lần nhập lại/gửi lại OTP không vượt quá giới hạn cấu hình (hiện là Assumption trong Open-Questions).
- **V3 – Unique User theo SĐT**:
  - Mỗi SĐT chỉ có **tối đa 1 User active**.
- **V4 – Admin credentials**:
  - Mật khẩu không được lưu dạng plain text (hash, nhưng chi tiết thuật toán không mô tả trong SRS).

### 6.2. Edge cases (liên quan AUTH)

1. **E-AUTH-001 – SĐT không có trong danh sách nhân sự**
	- Mô tả: Người dùng đăng ký với SĐT không tồn tại trong danh sách import.
	- Hành vi mong đợi: Hệ thống không thể match tầng; có 2 cách hiểu trong tài liệu (gán Tầng 3 mặc định hoặc chờ duyệt).
	- Xử lý cụ thể: **Open Question (Q7)**, không quyết định trong SRS-AUTH.

2. **E-AUTH-002 – OTP sai hoặc hết hạn**
	- Mô tả: Người dùng nhập OTP sai nhiều lần hoặc sau khi OTP hết hạn.
	- Hành vi mong đợi: Từ chối xác thực, không tạo/không cho đăng nhập; giới hạn số lần gửi lại/nhập lại OTP theo cấu hình.
	- Chi tiết giới hạn: hiện đang là **Assumption liên quan Q8**.

3. **E-AUTH-003 – Mất kết nối với dịch vụ ZNS**
	- Mô tả: Hệ thống không thể gửi OTP ZNS.
	- Hành vi mong đợi: Thông báo lỗi “Không thể gửi OTP, vui lòng thử lại sau”; không tạo/lưu OTPRequest.
	- Chi tiết retry/policy: chưa nêu → Open Question.

4. **E-AUTH-004 – Người dùng đổi thiết bị/browser**
	- Mô tả: User đang có token hợp lệ nhưng truy cập từ thiết bị/browser mới.
	- Hành vi mong đợi: Có thể yêu cầu OTP lại để đảm bảo an toàn.
	- Chi tiết chính sách: chưa nêu trong tài liệu → Open Question.

5. **E-AUTH-005 – Đăng nhập Admin sai mật khẩu nhiều lần**
	- Mô tả: Admin nhập sai mật khẩu liên tục.
	- Hành vi mong đợi: Có cơ chế khoá tạm thời hoặc captcha (chưa mô tả).
	- Xử lý chi tiết: **Open Question**.

6. **E-AUTH-006 – Thu hồi token toàn hệ thống**
	- Mô tả: Người dùng yêu cầu đăng xuất khỏi mọi thiết bị (hoặc nghi ngờ lộ token).
	- Hành vi mong đợi: Tất cả token liên quan bị vô hiệu.
	- Hiện chưa có mô tả cụ thể → Open Question.

---

## 7. Non-functional requirements (module-specific)

- **AUTH-NFR-01 – Hiệu năng OTP**: Thời gian từ lúc người dùng yêu cầu OTP đến khi nhận được ZNS phải ở mức chấp nhận được (ví dụ < X giây) – con số X **chưa được cung cấp** → cần xác nhận.
- **AUTH-NFR-02 – Khả năng chịu tải**: Module AUTH phải hỗ trợ luồng đăng ký/đăng nhập trong bối cảnh toàn hệ thống phục vụ **≥ 1.000 user đồng thời** (chi tiết kịch bản tải do Tech Lead xác định).
- **AUTH-NFR-03 – Audit log**: Tất cả yêu cầu liên quan đến đăng ký/đăng nhập/OTP phải được ghi log với mức đủ để truy vết (thời gian, user, loại thao tác, kết quả).
- **AUTH-NFR-04 – Bảo mật**: Mật khẩu Admin không được lưu dạng plain text; OTP không được log ở dạng full (nếu cần log thì mask); token không được lộ qua log.

Các NFR chi tiết hơn (SLA, rate limit cụ thể) nằm ngoài nội dung tài liệu khách hàng hiện tại → sẽ được định nghĩa trong tài liệu kiến trúc/NFR riêng.

---

## 8. Open Questions / Assumptions (AUTH-specific)

> Tham chiếu thêm docs/01-Discovery/Open-Questions.md; dưới đây là những ý liên quan trực tiếp đến AUTH.

### 8.1. Open Questions

1. **Q-AUTH-01** – Pattern SĐT hợp lệ (bao nhiêu số, prefix nào, có hỗ trợ số nước ngoài không)?  
	*Why it matters*: ảnh hưởng đến validation & khả năng mở rộng.  
	*Owner*: Tech Lead/BA.  
	*Status*: Open.

2. **Q-AUTH-02** – Thời gian hiệu lực của OTP là bao lâu (phút)?  
	*Why it matters*: ảnh hưởng UX & bảo mật.  
	*Owner*: BA/Tech Lead.  
	*Status*: Open.

3. **Q-AUTH-03** – Số lần gửi lại OTP tối đa và thời gian chờ giữa các lần là bao nhiêu?  
	*Why it matters*: chi phí ZNS & chống abuse.  
	*Owner*: BA/Tech Lead.  
	*Status*: Đang được ghi nhận như Assumption ở Discovery.

4. **Q-AUTH-04** – Xử lý chuẩn khi SĐT không có trong danh sách nhân sự (gán Tầng 3 mặc định hay chờ duyệt?)  
	*Why it matters*: ảnh hưởng logic match tầng và onboarding.  
	*Owner*: BA/Khách hàng.  
	*Status*: Open.

5. **Q-AUTH-05** – Admin Portal có sử dụng OTP ZNS bổ sung (2FA) hay chỉ dùng mật khẩu?  
	*Why it matters*: mức độ bảo mật & chi phí ZNS.  
	*Owner*: Tech Lead/Khách hàng.  
	*Status*: Open.

6. **Q-AUTH-06** – Policy khoá tài khoản Admin khi sai mật khẩu nhiều lần (bao nhiêu lần, thời gian khoá)?  
	*Why it matters*: bảo mật, chống brute-force.  
	*Owner*: Tech Lead/BA.  
	*Status*: Open.

7. **Q-AUTH-07** – Chính sách khi người dùng đổi thiết bị/browser: luôn yêu cầu OTP lại hay có ngoại lệ?  
	*Why it matters*: cân bằng giữa UX & bảo mật.  
	*Owner*: Tech Lead/Khách hàng.  
	*Status*: Open.

8. **Q-AUTH-08** – Có hỗ trợ "đăng xuất khỏi tất cả thiết bị" không? Nếu có, UX thế nào?  
	*Why it matters*: bảo mật khi nghi ngờ lộ token.  
	*Owner*: BA/Tech Lead.  
	*Status*: Open.

### 8.2. Assumptions (liên quan AUTH)

- **A-AUTH-01** – Số lần gửi lại OTP và khoảng thời gian chờ đã được tạm giả định ở Discovery (ví dụ 3 lần/10 phút, chờ 60s giữa 2 lần), nhưng **chỉ dùng cho mục đích ước tính** và sẽ phải được khách hàng xác nhận trước khi implement.
- **A-AUTH-02** – Trong MVP, đăng nhập Admin sử dụng **SĐT/Email + mật khẩu**, không bắt buộc OTP ZNS, trừ khi sau này khách hàng yêu cầu **2FA**.
- **A-AUTH-03** – Chính sách hết hạn của Session Token và Refresh Token được tạm thời coi theo khuyến nghị bảo mật chung (ví dụ: Session Token ~1h, Refresh Token ~30 ngày) nhưng **chưa được khách hàng xác nhận**, nên chỉ ghi ở Discovery dưới dạng giả định.

Các Assumption này phải được cập nhật lại trong docs/01-Discovery/Open-Questions.md khi trạng thái thay đổi.
