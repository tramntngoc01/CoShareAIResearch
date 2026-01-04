
# Stories — ORDERS: Orders

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

### US-ORDERS-001 — End User tạo đơn hàng mới
**As a** User Tầng 3  
**I want** tạo đơn hàng từ các sản phẩm tôi đã chọn  
**So that** tôi có thể mua hàng và nhận tại Điểm nhận của công ty

**Priority:** P0  
**Dependencies:**
- SRS-ORDERS: FR-ORDERS-001  
- SRS-CATALOG (danh sách sản phẩm, tồn kho khả dụng)  
- SRS-USERS (User, Công ty, Điểm nhận)  
- SRS-PAYMENTS (kiểm tra hạn mức, phương thức thanh toán)

**Definition of Ready**
- [ ] Thiết kế UI giỏ hàng và màn hình xác nhận đơn được chốt.
- [ ] Rule chọn Điểm nhận (mặc định hay cho phép chọn trong nhiều Điểm nhận) được xác nhận.
- [ ] Rule kiểm tra hạn mức trả chậm (nếu có) được định nghĩa ở PAYMENTS.
- [ ] Rule xử lý khi không đủ tồn kho cho một số sản phẩm được mô tả rõ.

**Acceptance Criteria**
- AC1: Khi User đã đăng nhập và chọn ít nhất một sản phẩm hợp lệ (còn tồn kho, thuộc Lô đã kiểm duyệt), hệ thống cho phép tiếp tục bước xác nhận đơn.
- AC2: Đơn hàng được gắn với đúng User, Công ty, Điểm nhận và tổng tiền được tính chính xác từ sản phẩm x số lượng.
- AC3: Nếu bất kỳ sản phẩm nào trong đơn không còn đủ tồn kho tại thời điểm xác nhận, hệ thống từ chối tạo đơn và hiển thị thông báo rõ ràng.
- AC4: Nếu User dùng trả chậm nhưng vượt hạn mức (theo PAYMENTS), hệ thống từ chối tạo đơn hoặc yêu cầu đổi phương thức thanh toán.

**Sample data (fake)**
- Request conceptual:
	- userId: 10030  
	- pickupPointId: 9001  
	- items: [ { productId: 1001, quantity: 3 }, { productId: 1002, quantity: 2 } ]  
	- paymentMethod: "COD"
- Response conceptual:
	- orderId: 70001  
	- status: "Chờ xác nhận"  
	- totalAmount: 43500

**Edge cases (min 3)**
- EC1: User không chọn sản phẩm nào nhưng cố nhấn "Đặt hàng" → Hệ thống không cho phép, hiển thị thông báo yêu cầu chọn sản phẩm.
- EC2: Một trong các sản phẩm bị Admin đánh dấu Inactive sau khi User đã thêm vào giỏ → Khi xác nhận, hệ thống không cho phép đặt sản phẩm đó và yêu cầu cập nhật giỏ hàng.
- EC3: User thay đổi Điểm nhận trong khi tạo đơn (nếu được phép) → Đơn phải gắn với Điểm nhận cuối cùng được chọn; cần rule rõ nếu tồn kho sau này tách theo Điểm nhận.
- EC4: Mạng chập chờn, request tạo đơn bị gửi lặp lại → Hệ thống phải tránh tạo trùng đơn (logic idempotency phối hợp với PAYMENTS/ORDERS, chi tiết sau).

**API mapping**
- TBD (Tech Lead)

**Screens**
- TBD (giỏ hàng + màn hình xác nhận đơn trong End User Portal)

---

### US-ORDERS-002 — Ops/Ops+Shipper cập nhật trạng thái đơn theo luồng vận hành
**As a** Ops/Shipper  
**I want** cập nhật trạng thái đơn hàng theo từng bước vận hành  
**So that** mọi bên nhìn thấy trạng thái đơn chính xác (Chờ xác nhận, Đã xác nhận, Đang giao, Sẵn sàng nhận, Đã nhận)

**Priority:** P0  
**Dependencies:**
- SRS-ORDERS: FR-ORDERS-002  
- SRS-CATALOG (xuất kho/tồn kho)  
- SRS-PAYMENTS (trạng thái thanh toán)

**Definition of Ready**
- [ ] Bản đồ trạng thái đơn hàng và các chuyển tiếp hợp lệ được chốt.
- [ ] Phân quyền rõ vai trò nào được đổi trạng thái nào (Ops, Shipper, hệ thống).
- [ ] Thiết kế UI bảng điều khiển đơn hàng cho Ops/Shipper được duyệt.

**Acceptance Criteria**
- AC1: Ops có thể chuyển đơn từ "Chờ xác nhận" sang "Đã xác nhận" (hoặc "Chờ thanh toán" tùy chính sách) khi đủ điều kiện.
- AC2: Shipper có thể đánh dấu đơn từ "Đã xác nhận" sang "Đang giao", rồi "Sẵn sàng nhận", rồi "Đã nhận" theo luồng hợp lệ.
- AC3: Hệ thống không cho phép nhảy trạng thái không hợp lệ (ví dụ từ "Chờ xác nhận" sang "Đã nhận").
- AC4: Mỗi lần chuyển trạng thái đều được lưu vào lịch sử trạng thái với thời gian, người thao tác và ghi chú (nếu có).

**Sample data (fake)**
- Trước cập nhật:
	- orderId: 70001  
	- status: "Đã xác nhận"
- Hành động:
	- changedBy: "shipper_200"  
	- newStatus: "Đang giao"  
	- note: "Đã nhận hàng từ kho lúc 02:00"
- Sau cập nhật:
	- stateHistory thêm bản ghi mới với newStatus "Đang giao".

**Edge cases (min 3)**
- EC1: Shipper cố cập nhật trạng thái đơn không thuộc ca làm hoặc mạng lưới của mình → Hệ thống từ chối, hiển thị lỗi quyền hạn.
- EC2: Hai người dùng/tiến trình cùng lúc cố cập nhật trạng thái cùng một đơn → Cần cơ chế xử lý xung đột (optimistic locking) để tránh trạng thái không nhất quán.
- EC3: Hệ thống mất kết nối tạm thời sau khi ghi log nhưng trước khi trả response → Cần đảm bảo idempotency khi client retry, không ghi trùng.
- EC4: Đơn đang ở trạng thái Chờ duyệt hủy → Có cho phép đổi sang trạng thái khác không? (phụ thuộc Business-Rules; story này mặc định không cho phép, trừ khi rule khác được chốt).

**API mapping**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình danh sách đơn cho Ops, màn hình Shipper xem/đổi trạng thái đơn)

---

### US-ORDERS-003 — Ops tạo và Super Admin duyệt yêu cầu hủy đơn
**As a** Ops / Super Admin  
**I want** tạo và duyệt yêu cầu hủy đơn khi có lý do chính đáng  
**So that** hệ thống ghi nhận rõ ràng các hủy đơn và ảnh hưởng tới tồn kho, thanh toán

**Priority:** P0  
**Dependencies:**
- SRS-ORDERS: FR-ORDERS-003, BR-ORDERS-003  
- SRS-CATALOG (điều chỉnh tồn kho)  
- SRS-PAYMENTS (hoàn tiền/điều chỉnh công nợ)

**Definition of Ready**
- [ ] Danh sách trạng thái đơn cho phép tạo yêu cầu hủy được xác nhận.
- [ ] Quy tắc khi nào hủy auto (hết hạn) và khi nào cần phê duyệt được mô tả.
- [ ] Thiết kế UI màn hình chi tiết đơn (có nút "Yêu cầu hủy") và màn hình danh sách đơn chờ duyệt hủy cho Super Admin.

**Acceptance Criteria**
- AC1: Ops chỉ có thể tạo yêu cầu hủy cho các đơn ở trạng thái cho phép (ví dụ: Chờ xác nhận, Đã xác nhận, Đang giao, Sẵn sàng nhận – danh sách chính xác TBD).
- AC2: Khi yêu cầu hủy được tạo, trạng thái đơn chuyển sang "Chờ duyệt hủy" và không thể bị chỉnh sửa/đổi trạng thái bởi luồng khác cho đến khi được duyệt hoặc từ chối.
- AC3: Super Admin có thể duyệt hoặc từ chối yêu cầu; nếu duyệt, đơn chuyển sang "Đã hủy" và các module CATALOG/PAYMENTS được thông báo để điều chỉnh tồn kho/thanh toán.
- AC4: Lịch sử hủy đơn lưu đầy đủ ai tạo yêu cầu, ai duyệt, thời gian và lý do.

**Sample data (fake)**
- Trước khi yêu cầu:
	- orderId: 70001  
	- status: "Đã xác nhận"
- Yêu cầu hủy:
	- requestedBy: "ops_admin_01"  
	- reason: "NCC hết hàng"  
	- requestedAt: "2026-01-04T10:00:00Z"
- Duyệt:
	- approvedBy: "super_admin_01"  
	- approvedAt: "2026-01-04T11:00:00Z"  
	- finalStatus: "Đã hủy"

**Edge cases (min 3)**
- EC1: Ops cố tạo yêu cầu hủy cho đơn đã ở trạng thái Đã hủy/Đã nhận → Hệ thống từ chối, hiển thị lỗi.
- EC2: Có nhiều yêu cầu hủy trùng cho cùng một đơn (do gửi lặp) → Chỉ một yêu cầu được ghi nhận; các yêu cầu sau bị từ chối hoặc gộp theo rule.
- EC3: Super Admin từ chối yêu cầu hủy → Đơn quay lại trạng thái trước đó, và Ops phải thấy rõ lý do từ chối.
- EC4: Trong lúc chờ duyệt hủy, đơn đồng thời được cập nhật trạng thái giao hàng bởi Shipper → Cần rule ưu tiên; mặc định story này giả định trạng thái bị "đóng băng" khi Chờ duyệt hủy (Assumption).

**API mapping**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình chi tiết đơn cho Ops với nút Yêu cầu hủy, màn hình danh sách đơn chờ duyệt cho Super Admin)

---

### US-ORDERS-004 — Shipper ghi nhận giao hàng & POD, xử lý đổi/trả tại Điểm nhận
**As a** Shipper (có thể là Tầng 2)  
**I want** đánh dấu đơn đã giao kèm POD và ghi nhận đổi/trả nếu có  
**So that** trạng thái đơn phản ánh chính xác việc giao/nhận và hỗ trợ các module khác xử lý tiếp

**Priority:** P0  
**Dependencies:**
- SRS-ORDERS: FR-ORDERS-004, BR-ORDERS-004  
- SRS-PAYMENTS (COD, ghi nhận thu hộ)  
- SRS-CATALOG (tồn kho khi đổi/trả)

**Definition of Ready**
- [ ] Thiết kế UI cho Shipper xem danh sách đơn theo Điểm nhận/ca làm được chốt.
- [ ] Chuẩn POD (số ảnh, dung lượng, định dạng) được Security/Legal xác nhận.
- [ ] Rule đổi/trả (cho phép đổi/trả toàn bộ hay từng item, trong khoảng thời gian nào) được mô tả.

**Acceptance Criteria**
- AC1: Shipper có thể chọn một đơn ở trạng thái "Sẵn sàng nhận", xác thực khách (theo phương thức được chốt) và đánh dấu đơn "Đã nhận" kèm ít nhất một minh chứng POD.
- AC2: Nếu khách yêu cầu đổi/trả ngay tại Điểm nhận, Shipper có thể ghi nhận yêu cầu đổi/trả, đơn hoặc các dòng trong đơn chuyển sang trạng thái "Đổi trả".
- AC3: Sau khi Shipper ghi nhận "Đã nhận" hoặc "Đổi trả", hệ thống ghi log đầy đủ: người thao tác, thời gian, ảnh POD, lý do đổi/trả.

**Sample data (fake)**
- Delivery action:
	- orderId: 70001  
	- deliveredBy: "shipper_200"  
	- deliveredAt: "2026-01-05T03:00:00Z"  
	- podImageUrl: "/pod/70001_1.jpg"  
	- receiverName: "Nguyễn Văn D"
- Return action (conceptual):
	- orderId: 70001  
	- items: [ { productId: 1002, quantity: 1 } ]  
	- reason: "Bao bì rách"  
	- createdBy: "shipper_200"

**Edge cases (min 3)**
- EC1: Shipper cố đánh dấu "Đã nhận" mà không upload POD (khi policy yêu cầu bắt buộc POD) → Hệ thống từ chối, yêu cầu upload.
- EC2: Khách không tới nhận hàng trong thời gian ca → Cần rule về auto-cancel hoặc chuyển trạng thái đặc biệt; chưa mô tả chi tiết (Open Question), story này chỉ ghi nhận không cho phép đặt "Đã nhận" nếu không có sự kiện giao thực.
- EC3: Shipper ghi sai đơn (đánh dấu nhầm đơn khác là "Đã nhận") → Cần quy trình chỉnh sửa/rollback do Ops/Super Admin; sẽ được quy định trong Business-Rules, không xử lý trong story này.
- EC4: Đổi/trả nhiều lần cho cùng đơn → Cần policy rõ ràng (Open Question); story này giả định số lần đổi/trả được giới hạn.

**API mapping**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình danh sách đơn cho Shipper + màn hình chi tiết đơn với nút Đã nhận/Đổi trả)

---

### US-ORDERS-005 — End User xem lịch sử đơn hàng của mình
**As a** User Tầng 3  
**I want** xem lịch sử các đơn hàng tôi đã đặt  
**So that** tôi biết trạng thái hiện tại và tra cứu lại thông tin khi cần

**Priority:** P1  
**Dependencies:**
- SRS-ORDERS: FR-ORDERS-005  
- AUTH (đăng nhập)  
- USERS (thông tin Điểm nhận, Công ty)

**Definition of Ready**
- [ ] Thiết kế UI màn hình "Đơn hàng của tôi" (danh sách + chi tiết) được chốt.
- [ ] Quy tắc lọc/sắp xếp (mặc định sắp theo thời gian mới nhất trước) được thống nhất.

**Acceptance Criteria**
- AC1: Sau khi đăng nhập, User có thể vào mục "Đơn hàng của tôi" và nhìn thấy các đơn thuộc userId của mình, theo thứ tự từ mới đến cũ.
- AC2: User có thể lọc danh sách theo trạng thái (ví dụ: Đang giao, Đã nhận, Đã hủy).
- AC3: User có thể mở chi tiết đơn để xem sản phẩm, số lượng, tổng tiền, Điểm nhận và lịch sử trạng thái.

**Sample data (fake)**
- Response conceptual:
	- items: [  
		{ orderId: 70001, status: "Đang giao", totalAmount: 43500, createdAt: "2026-01-04T08:00:00Z", pickupPointName: "Điểm nhận KCN A-1" },  
		{ orderId: 70002, status: "Đã nhận", totalAmount: 120000, createdAt: "2025-12-30T09:30:00Z", pickupPointName: "Điểm nhận KCN A-1" }  
	]  
	- page: 1, totalItems: 2, totalPages: 1

**Edge cases (min 3)**
- EC1: User chưa có đơn hàng nào → Danh sách rỗng, hiển thị thông điệp thân thiện (ví dụ: "Bạn chưa có đơn hàng nào").
- EC2: User cố truy cập đơn của người khác (sửa id trong URL) → Hệ thống từ chối (403/404 tuỳ thiết kế), không lộ thông tin.
- EC3: Số lượng đơn rất lớn → Danh sách phải phân trang, không làm chậm UI; pageSize tối đa do Tech Lead chốt.
- EC4: Một số đơn rất cũ có trạng thái/thuộc tính đã bị deprecate → UI vẫn hiển thị trạng thái mapped hợp lý, không lỗi.

**API mapping**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình "Đơn hàng của tôi" trong End User Portal)

