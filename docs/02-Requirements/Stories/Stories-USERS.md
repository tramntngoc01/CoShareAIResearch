
# Stories — USERS: User & Profile Management

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

### US-USERS-001 — Import danh sách nhân sự từ Công ty
**As a** Admin vận hành  
**I want** import danh sách nhân sự (Tầng 1/2/3, Shipper) từ file do Công ty cung cấp  
**So that** hệ thống có đầy đủ User và cấu trúc tổ chức để các module khác sử dụng

**Priority:** P0  
**Dependencies:**
- SRS-USERS: FR-USERS-001, BR-USERS-001..003  
- DB-USERS (thiết kế bảng User, Company, Pickup Point, Tier Relationship)

**Definition of Ready**
- [ ] Mẫu file chuẩn (CSV/Excel) được BA/PO chốt và được khách hàng duyệt (tên cột, kiểu dữ liệu, bắt buộc/tùy chọn).
- [ ] Rule xác định khoá định danh User (ví dụ: mã nhân viên + Công ty hoặc SĐT + Công ty) được thống nhất.
- [ ] Chính sách xử lý khi trùng khoá (overwrite trường nào, giữ trường nào) được mô tả trong Business-Rules.
- [ ] Giới hạn kích thước file (số dòng tối đa/lần import) được Tech Lead xác nhận.

**Acceptance Criteria**
- AC1: Admin có thể chọn và upload file nhân sự đúng định dạng; hệ thống đọc được toàn bộ file mà không lỗi kỹ thuật.
- AC2: Sau khi xử lý, hệ thống trả kết quả tổng quan: tổng bản ghi đọc được, số bản ghi tạo mới, số bản ghi cập nhật, số bản ghi lỗi.
- AC3: Các bản ghi hợp lệ tạo mới User với trạng thái và liên kết Công ty/Điểm nhận theo đúng file nguồn.
- AC4: Các bản ghi lỗi được liệt kê kèm lý do cụ thể (thiếu cột bắt buộc, sai định dạng, trùng khoá không cho phép...).
- AC5: Mỗi lần import được ghi vào audit log với thông tin người thực hiện, thời gian, tên file và thống kê kết quả.

**Sample data (fake)**
- Ví dụ 3 dòng dữ liệu file CSV (đơn giản hoá):
	- employee_code,full_name,phone,company_code,pickup_point_code,tier,ref_tier_code  
	- "CN001","Nguyễn Văn A","0900123456","CTY_A","PUP_A1",3,"T2_001"  
	- "T2_001","Trần Thị B","0912345678","CTY_A","PUP_A1",2,"T1_001"  
	- "T1_001","Phạm Văn C","0923456789","CTY_A","PUP_A1",1,""  
- Kết quả import (tóm tắt):
	- totalRows: 3  
	- created: 3  
	- updated: 0  
	- errors: []

**Edge cases (min 3)**
- EC1: File thiếu một hoặc nhiều cột bắt buộc (ví dụ thiếu company_code) → Hệ thống từ chối cả lô, trả về thông báo "Sai cấu trúc file" và không tạo/cập nhật User nào.
- EC2: Một dòng có giá trị tier không hợp lệ (không phải 1/2/3/shipper) → Dòng đó bị đánh lỗi, không tạo User, các dòng khác hợp lệ vẫn xử lý bình thường.
- EC3: File chứa 2 dòng trùng cùng khoá định danh (ví dụ cùng employee_code + company_code) nhưng dữ liệu khác nhau → Hệ thống xử lý theo rule đã chốt (ví dụ: chỉ nhận dòng đầu, đánh lỗi dòng sau).
- EC4: File quá lớn vượt giới hạn cấu hình → Hệ thống từ chối trước khi xử lý, hiển thị thông báo rõ ràng về giới hạn.

**API mapping**
- TBD (Tech Lead)

**Screens**
- Màn hình Import nhân sự (trong Admin Portal)
- Màn hình/section Kết quả import (hiển thị thống kê + danh sách lỗi)

---

### US-USERS-002 — Ánh xạ và quản lý Ref Tier (cấu trúc Tầng 1/2/3)
**As a** Super Admin  
**I want** xem và điều chỉnh quan hệ Ref Tier giữa Tầng 1, Tầng 2 và Tầng 3  
**So that** mạng lưới affiliate phản ánh đúng tổ chức thực tế và tính hoa hồng chính xác

**Priority:** P0  
**Dependencies:**
- SRS-USERS: FR-USERS-002, BR-USERS-002  
- REPORTING/PAYMENTS (phụ thuộc kết quả hoa hồng sau này)

**Definition of Ready**
- [ ] Quy tắc một Tầng 3 chỉ thuộc một Tầng 2, và một Tầng 2 chỉ thuộc một Tầng 1 tại một thời điểm được xác nhận (hoặc rule khác nếu khách hàng yêu cầu).
- [ ] Chính sách áp dụng thay đổi Ref Tier cho đơn hàng cũ/mới được định nghĩa (chỉ áp dụng cho đơn mới hay retroactive một phần).
- [ ] Thiết kế UI màn hình xem cây mạng lưới và form chỉnh sửa Ref Tier được chốt.

**Acceptance Criteria**
- AC1: Super Admin có thể tìm kiếm một User bất kỳ và xem được Ref Tier hiện tại (cha trực tiếp) và tầng của User.
- AC2: Super Admin có thể đổi Ref Tier của User (ví dụ Tầng 3 chuyển từ Tầng 2 A sang Tầng 2 B) nếu không vi phạm rule đã thống nhất.
- AC3: Mỗi lần thay đổi Ref Tier đều được ghi nhận lịch sử: từ Ref Tier nào sang Ref Tier nào, ai thao tác, thời gian.
- AC4: Sau khi thay đổi Ref Tier, các module phụ thuộc (PAYMENTS/REPORTING) nhìn thấy thông tin mới cho các đơn hàng tạo sau thời điểm thay đổi.

**Sample data (fake)**
- Trước khi đổi:
	- userId: 10010 (Tầng 3)  
	- currentRefTier: "T2_001"  
	- currentParentName: "Nguyễn Văn Tầng 2A"
- Sau khi đổi:
	- newRefTier: "T2_005"  
	- newParentName: "Trần Thị Tầng 2B"
- Lịch sử:
	- { userId: 10010, oldRef: "T2_001", newRef: "T2_005", changedBy: "SA_001", changedAt: "2026-01-03T09:00:00Z" }

**Edge cases (min 3)**
- EC1: Super Admin chọn Ref Tier mới là chính User đó hoặc một User không thuộc tầng phù hợp (ví dụ: chọn Tầng 3 làm cha của Tầng 3) → Hệ thống từ chối, hiển thị thông báo lỗi.
- EC2: Ref Tier mới thuộc Công ty khác với Công ty của User Tầng 3 → Hệ thống chặn hoặc cảnh báo theo policy đã chốt (TBD), không cho tạo cấu trúc chéo ngoài quy định.
- EC3: Thao tác đổi Ref Tier bị gián đoạn (mất mạng) sau khi lưu một phần → Hệ thống phải đảm bảo atomic, hoặc rollback rõ ràng, không để trạng thái lửng.
- EC4: Cùng một User được hai Super Admin cố gắng đổi Ref Tier gần như đồng thời → Hệ thống áp dụng cơ chế lock/optimistic concurrency theo thiết kế, không tạo dữ liệu mâu thuẫn.

**API mapping**
- TBD (Tech Lead)

**Screens**
- Màn hình Quản lý mạng lưới/Tier (Admin Portal)
- Popup/section Chỉnh sửa Ref Tier cho một User

---

### US-USERS-003 — Quản lý hồ sơ KYC User (Admin)
**As a** Admin  
**I want** xem và chỉnh sửa thông tin hồ sơ/KYC của User trong phạm vi cho phép  
**So that** dữ liệu khách hàng luôn chính xác, phục vụ trả chậm và báo cáo

**Priority:** P0  
**Dependencies:**
- SRS-USERS: FR-USERS-003, BR-USERS-003  
- Data-Classification (quy định bảo vệ dữ liệu nhạy cảm)

**Definition of Ready**
- [ ] Danh sách trường KYC được chốt (ví dụ: CCCD, ngày sinh, mã nhân viên, địa chỉ liên lạc...).
- [ ] Phân loại trường nào do Công ty sở hữu (chỉ cập nhật qua import) và trường nào Admin có thể chỉnh sửa.
- [ ] Quy tắc mask/ẩn thông tin nhạy cảm trên UI được Security/Legal duyệt.

**Acceptance Criteria**
- AC1: Admin có thể mở màn hình chi tiết hồ sơ User và xem được đầy đủ thông tin trong phạm vi quyền hạn.
- AC2: Admin chỉ có thể chỉnh sửa những trường được cho phép; các trường do Công ty sở hữu (ví dụ: Công ty, Điểm nhận, mã nhân viên) bị khoá hoặc chỉ đọc.
- AC3: Mỗi lần chỉnh sửa trường KYC quan trọng (ví dụ: CCCD, ngày sinh) được ghi log audit với dữ liệu cũ/mới (ở mức không lộ PII đầy đủ theo quy định).
- AC4: Sau khi lưu, các thay đổi thể hiện khi module khác (PAYMENTS, REPORTING) truy vấn dữ liệu User.

**Sample data (fake)**
- Trước chỉnh sửa:
	- userId: 10020  
	- fullName: "Lê Thị Cũ"  
	- cccdMasked: "0790******123"  
	- birthDate: "1995-05-10"  
	- email: "old.email@example.com"
- Sau chỉnh sửa (Admin đổi tên & email):
	- fullName: "Lê Thị Mới"  
	- email: "new.email@example.com"

**Edge cases (min 3)**
- EC1: Admin cố sửa trường không được phép (ví dụ: đổi Công ty của User) → Hệ thống từ chối và hiển thị thông báo quyền hạn.
- EC2: Trường KYC nhập sai định dạng (ví dụ: ngày sinh không hợp lệ, CCCD thiếu số) → Form hiển thị lỗi validation, không lưu dữ liệu.
- EC3: Admin chỉnh sửa hồ sơ cùng lúc file HR mới được import và cũng cập nhật User đó → Cần cơ chế xử lý xung đột (ưu tiên dữ liệu nào) theo Business-Rules.
- EC4: Admin huỷ thao tác giữa chừng sau khi đã thay đổi nhiều trường trên form → Không có thay đổi nào được lưu.

**API mapping**
- TBD (Tech Lead)

**Screens**
- Màn hình danh sách User (Admin Portal)
- Màn hình chi tiết hồ sơ User

---

### US-USERS-004 — End User xem và chỉnh sửa thông tin hồ sơ được phép
**As a** User Tầng 3  
**I want** xem hồ sơ cá nhân và chỉnh sửa một số thông tin liên lạc  
**So that** thông tin của tôi luôn cập nhật, phục vụ giao hàng và thông báo

**Priority:** P1  
**Dependencies:**
- SRS-USERS: FR-USERS-003  
- AUTH (đã đăng nhập)  
- Data-Classification

**Definition of Ready**
- [ ] Danh sách trường mà End User được phép chỉnh sửa (ví dụ: email, số điện thoại phụ, địa chỉ liên lạc chi tiết) được chốt.
- [ ] UI màn hình "Hồ sơ của tôi" được thiết kế.
- [ ] Chính sách xác thực lại (nếu có) khi đổi một số trường quan trọng được xác định.

**Acceptance Criteria**
- AC1: Sau khi đăng nhập, User có thể truy cập màn hình "Hồ sơ của tôi" và xem các thông tin hồ sơ cơ bản (một số trường có thể chỉ đọc).
- AC2: User có thể cập nhật các trường được phép (ví dụ: email, địa chỉ chi tiết) và lưu thành công nếu hợp lệ.
- AC3: Các trường không được phép chỉnh sửa (ví dụ: Công ty, Điểm nhận mặc định, tầng, hạn mức) được hiển thị nhưng không thể sửa.

**Sample data (fake)**
- Trước chỉnh sửa:
	- userId: 10030  
	- fullName: "Phạm Văn D"  
	- email: "pham.d.old@example.com"  
	- addressDetail: "Nhà trọ KCN X - đường 1"
- Sau chỉnh sửa:
	- email: "pham.d.new@example.com"  
	- addressDetail: "Nhà trọ KCN X - đường 2, phòng 305"

**Edge cases (min 3)**
- EC1: User cố gắng chỉnh sửa trường bị khoá (ví dụ qua dev tools) → Backend từ chối, giữ nguyên dữ liệu gốc.
- EC2: User nhập email sai định dạng → Form hiển thị lỗi, không lưu.
- EC3: User thay đổi thông tin trong lúc phiên đăng nhập sắp hết hạn → Cần xử lý đồng bộ với AUTH (nếu token hết hạn, yêu cầu đăng nhập lại rồi gửi lại dữ liệu).
- EC4: User không có địa chỉ chi tiết trước đó (null) → UI vẫn hiển thị form cho phép nhập mới mà không lỗi.

**API mapping**
- TBD (Tech Lead)

**Screens**
- Màn hình "Hồ sơ của tôi" (End User portal)

---

### US-USERS-005 — Quản lý trạng thái User (Active/Inactive/Locked)
**As a** Admin/Super Admin  
**I want** khoá/mở khoá hoặc chuyển trạng thái User theo yêu cầu Công ty  
**So that** chỉ những người dùng hợp lệ mới có thể tiếp tục sử dụng hệ thống

**Priority:** P0  
**Dependencies:**
- SRS-USERS: FR-USERS-004, BR-USERS-004  
- AUTH/ORDERS/PAYMENTS (tôn trọng trạng thái User)

**Definition of Ready**
- [ ] Danh sách trạng thái User (Draft, Active, Inactive, Locked, Deleted logic) được chốt.
- [ ] Rule chuyển đổi giữa các trạng thái (ai được phép chuyển, chuyển trong trường hợp nào) được định nghĩa.
- [ ] Quy tắc hiển thị trạng thái cho các module khác được thiết kế.

**Acceptance Criteria**
- AC1: Admin có thể tìm kiếm User và xem trạng thái hiện tại.
- AC2: Admin có thể thực hiện hành động khoá/mở khoá hoặc chuyển sang Inactive nếu có quyền, kèm ghi chú lý do.
- AC3: Sau khi User bị khoá, mọi lần đăng nhập mới hoặc đặt đơn hàng mới của User đó bị chặn bởi AUTH/ORDERS.
- AC4: Mỗi lần thay đổi trạng thái được ghi vào audit log (người thực hiện, thời gian, trạng thái cũ/mới, lý do).

**Sample data (fake)**
- Trước khi khoá:
	- userId: 10040  
	- status: "Active"
- Sau khoá:
	- status: "Locked"  
	- lockedReason: "Yêu cầu từ Công ty do nghỉ việc"

**Edge cases (min 3)**
- EC1: Admin cố khoá một User đã ở trạng thái Locked → Hệ thống không thay đổi trạng thái, có thể hiển thị cảnh báo nhẹ.
- EC2: User đang có đơn hàng ở trạng thái đang xử lý khi bị khoá → Đơn hiện tại không tự động bị huỷ; ORDERS sẽ xử lý theo rule riêng, nhưng tất cả đơn mới bị chặn.
- EC3: Hai Admin cùng lúc thay đổi trạng thái cùng một User sang hai trạng thái khác nhau → Cần cơ chế tránh ghi đè không mong muốn (optimistic concurrency).
- EC4: Admin chuyển User sang Inactive do nghỉ việc rồi sau đó Công ty gửi file HR mới vẫn chứa User đó → Rule xử lý xung đột phải được mô tả trong Business-Rules (ví dụ: ưu tiên trạng thái từ HR mới).

**API mapping**
- TBD (Tech Lead)

**Screens**
- Màn hình danh sách User (Admin Portal)
- Popup/section Thay đổi trạng thái User

---

### US-USERS-006 — Tra cứu danh sách User theo nhiều tiêu chí
**As a** Admin/Ops  
**I want** tìm kiếm và lọc danh sách User theo Công ty, tầng, trạng thái, mã nhân viên, SĐT  
**So that** tôi có thể hỗ trợ vận hành, tra cứu nhanh thông tin khi cần

**Priority:** P0  
**Dependencies:**
- SRS-USERS: FR-USERS-005  
- SRS-00-Overview (quy tắc phân trang chuẩn)

**Definition of Ready**
- [ ] Danh sách tiêu chí lọc (filters) được chốt (Công ty, Điểm nhận, tầng, trạng thái, mã nhân viên, SĐT...).
- [ ] Thiết kế UI danh sách User với khu vực filter và kết quả phân trang được phê duyệt.
- [ ] Giới hạn pageSize mặc định và tối đa được xác định.

**Acceptance Criteria**
- AC1: Admin/Ops có thể nhập hoặc chọn các tiêu chí lọc và nhận được danh sách User phù hợp.
- AC2: Kết quả tra cứu hỗ trợ phân trang theo chuẩn { items, page, totalItems, totalPages } của hệ thống.
- AC3: Tốc độ trả kết quả ở quy mô dữ liệu mục tiêu (TBD, ví dụ vài chục nghìn User) vẫn trong ngưỡng chấp nhận được do khách hàng chốt.

**Sample data (fake)**
- Filter request conceptual:
	- companyCode: "CTY_A"  
	- tier: 3  
	- status: "Active"  
	- page: 1  
	- pageSize: 20
- Response conceptual:
	- items: [ { userId: 10001, fullName: "Nguyễn Văn A", phoneMasked: "0900***456", tier: 3, pickupPoint: "PUP_A1" }, ... ]  
	- page: 1  
	- totalItems: 135  
	- totalPages: 7

**Edge cases (min 3)**
- EC1: Admin nhập filter quá rộng (không filter gì) → Hệ thống vẫn trả kết quả theo phân trang, không bị timeout; có thể hiển thị cảnh báo nếu số bản ghi quá lớn.
- EC2: Admin nhập giá trị filter không tồn tại (ví dụ companyCode sai) → Trả về danh sách rỗng, không lỗi kỹ thuật.
- EC3: Kết hợp nhiều filter dẫn đến không có bản ghi → Trả về danh sách rỗng, hiển thị rõ "Không tìm thấy User phù hợp".
- EC4: Admin thay đổi nhanh filter trong khi trang trước chưa load xong → Hệ thống chỉ hiển thị kết quả tương ứng với filter mới nhất, không lẫn dữ liệu.

**API mapping**
- TBD (Tech Lead)

**Screens**
- Màn hình danh sách User với khu vực filter (Admin Portal)

