
# SRS — ORDERS: Orders

## 1. Module purpose & boundaries (in/out)

**Mục đích**
- Quản lý toàn bộ vòng đời **Đơn hàng** của End User (Tầng 3) từ lúc đặt hàng đến khi **Đã nhận**, **Đã hủy** hoặc **Đổi trả**.
- Phối hợp với module **CATALOG** (sản phẩm, tồn kho), **USERS** (User/tầng/Công ty/Điểm nhận), **PAYMENTS** (COD, chuyển khoản, trả chậm), **NOTIFICATIONS** (thông báo), **ADMIN** (audit, cấu hình) để đảm bảo luồng nghiệp vụ nhất quán.

**Trong phạm vi (IN)**
- Tạo đơn hàng từ giỏ chọn sản phẩm của End User.
- Ghi nhận và quản lý trạng thái đơn hàng theo các trạng thái trong Glossary (Chờ xác nhận, Chờ thanh toán, Đã xác nhận, Đang giao, Sẵn sàng nhận, Đã nhận, Chờ duyệt hủy, Đã hủy, Đổi trả).
- Gắn Đơn hàng với **Điểm nhận**, **Công ty**, **User/Tầng**, **Shipper/Tầng 2**.
- Luồng yêu cầu hủy đơn và phê duyệt hủy bởi Super Admin.
- Luồng ghi nhận giao hàng (POD) và đổi/trả tại Điểm nhận (ở mức ghi nhận nghiệp vụ, chi tiết chính sách đổi trả thuộc Business-Rules).

**Ngoài phạm vi (OUT)**
- Tính toán chi tiết hoa hồng, công nợ và trả chậm (nằm ở **PAYMENTS/REPORTING**, nhưng ORDERS cung cấp dữ liệu đầu vào).
- Luồng chi tiết bố trí Shipper, điều phối giao hàng nội bộ NCC (nếu phức tạp hơn mô tả hiện tại).
- Logic UI cụ thể ngoài những gì được mô tả ở mức hành vi; chi tiết layout, màu sắc thuộc thiết kế Figma.

## 2. Personas/roles involved

- **Tầng 3 (End User)**:
	- Đặt đơn hàng, xem lịch sử đơn hàng, xem trạng thái đơn, yêu cầu hủy/đổi trả theo chính sách.
- **Tầng 2** (có thể kiêm **Shipper**):
	- Xem danh sách đơn hàng liên quan đến mạng lưới/ca làm của mình (nếu dùng làm Shipper).
	- Thu COD, giao hàng tại Điểm nhận, chụp POD, hỗ trợ đổi trả tại chỗ (chi tiết xử lý tiền ở PAYMENTS).
- **Tầng 1**:
	- Xem tổng quan đơn hàng thuộc mạng lưới (để tham chiếu hoa hồng; chủ yếu read-only trong MVP).
- **Shipper** (có thể là Tầng 2 hoặc user độc lập):
	- Nhận danh sách đơn cần giao, đánh dấu trạng thái giao hàng (Đang giao, Sẵn sàng nhận, Đã nhận), ghi nhận COD thu được, xử lý đổi trả tại Điểm nhận.
- **Ops (Admin)**:
	- Xác nhận đơn (Chờ xác nhận → Đã xác nhận/Chờ thanh toán), quản lý xuất kho, theo dõi trạng thái đơn.
	- Tạo yêu cầu hủy đơn (Chờ duyệt hủy).
- **Super Admin**:
	- Duyệt hoặc từ chối yêu cầu hủy đơn.
- **QC (Admin)**:
	- Gián tiếp liên quan thông qua việc chỉ cho phép đặt sản phẩm từ Lô đã kiểm duyệt.

## 3. Functional requirements (FR-ORDERS-###)

### FR-ORDERS-001 — Tạo đơn hàng từ lựa chọn sản phẩm của End User

**Mô tả**
- Cho phép End User (Tầng 3) tạo đơn hàng mới từ danh sách sản phẩm được hiển thị (CATALOG), gắn với Điểm nhận đã cấu hình cho Công ty.

**Luồng chuẩn (happy path)**
1. User đã đăng nhập (AUTH) duyệt danh sách sản phẩm (CATALOG) và chọn một hoặc nhiều sản phẩm, số lượng.
2. User xác nhận Điểm nhận (tự động lấy Điểm nhận mặc định từ USERS; nếu có nhiều, chọn trong danh sách – chi tiết tùy policy).
3. Hệ thống tính toán tạm tính (giá x số lượng), gửi yêu cầu tới PAYMENTS để kiểm tra hạn mức trả chậm (nếu User dùng trả chậm) và các ràng buộc thanh toán.
4. Hệ thống kiểm tra tồn kho khả dụng (CATALOG) cho từng sản phẩm.
5. Nếu tất cả điều kiện đều đạt, hệ thống tạo đơn hàng với trạng thái ban đầu (theo hình thức thanh toán – xem FR-ORDERS-002) và giữ tồn kho tương ứng.

**Điều kiện testable**
- Chỉ sản phẩm từ Lô đã kiểm duyệt và còn tồn kho khả dụng mới được tạo trong đơn.
- Đơn phải gắn với User, Tầng, Công ty, Điểm nhận một cách rõ ràng.
- Tổng tiền đơn hàng được tính chính xác từ giá sản phẩm + số lượng (không xử lý khuyến mãi phức tạp trong MVP trừ khi có business rule riêng).

**Ví dụ dữ liệu (fake)**
- orderId: 70001  
- userId: 10030 (Tầng 3)  
- companyId: 501  
- pickupPointId: 9001  
- items: [ { productId: 1001, quantity: 3, unitPrice: 6500 }, { productId: 1002, quantity: 2, unitPrice: 12000 } ]  
- totalAmount: 3×6500 + 2×12000 = 43500  
- paymentMethod: "COD" / "TRẢ_CHẬM" (giá trị cụ thể theo PAYMENTS)  
- status: "Chờ xác nhận" hoặc "Chờ thanh toán" theo chính sách thanh toán.

---

### FR-ORDERS-002 — Quản lý trạng thái đơn hàng theo luồng vận hành

**Mô tả**
- Quản lý trạng thái đơn theo các bước: 
	- Chờ xác nhận → (Chờ thanh toán) → Đã xác nhận → Đang giao → Sẵn sàng nhận → Đã nhận; 
	- và luồng Chờ duyệt hủy → Đã hủy; luồng Đổi trả.

**Luồng chuẩn (rút gọn)**
1. Đơn mới tạo vào **Chờ xác nhận** (hoặc **Chờ thanh toán** tuỳ hình thức thanh toán; chi tiết mapping ở PAYMENTS, nhưng ORDERS phải hỗ trợ).
2. Ops/ hệ thống xác nhận đơn đủ điều kiện (hàng, thanh toán) → chuyển sang **Đã xác nhận**.
3. Khi Shipper nhận hàng từ kho → **Đang giao**.
4. Khi hàng đến Điểm nhận → **Sẵn sàng nhận**.
5. Khi User nhận hàng thành công, Shipper chụp POD/thu COD → **Đã nhận**.

**Điều kiện testable**
- Không cho phép nhảy trạng thái không hợp lệ (ví dụ: từ Chờ xác nhận nhảy thẳng sang Đã nhận).
- Mọi thay đổi trạng thái phải ghi log với user thực hiện hoặc tiến trình hệ thống.
- Chuyển trạng thái phải tôn trọng các điều kiện từ module khác (ví dụ: chỉ chuyển sang Đang giao khi đã xuất kho; chi tiết trong thiết kế chi tiết sau).

**Ví dụ dữ liệu (fake)**
- stateHistory: [  
	{ status: "Chờ xác nhận", changedBy: "system", changedAt: "2026-01-04T08:00:00Z" },  
	{ status: "Đã xác nhận", changedBy: "ops_admin", changedAt: "2026-01-04T09:00:00Z" },  
	{ status: "Đang giao", changedBy: "shipper_200", changedAt: "2026-01-05T02:00:00Z" }  
]

---

### FR-ORDERS-003 — Yêu cầu hủy đơn và duyệt hủy

**Mô tả**
- Cho phép Ops tạo yêu cầu hủy đơn và Super Admin duyệt hoặc từ chối, theo luồng "Chờ duyệt hủy" → "Đã hủy".

**Luồng chuẩn**
1. Ops truy cập chi tiết đơn hàng ở trạng thái cho phép hủy (ví dụ: Chờ xác nhận, Đã xác nhận, Đang giao, Sẵn sàng nhận – cụ thể trạng thái nào được hủy sẽ do Business-Rules quyết định).
2. Ops tạo yêu cầu hủy, nhập lý do.
3. Đơn chuyển sang trạng thái **Chờ duyệt hủy**.
4. Super Admin xem danh sách đơn Chờ duyệt hủy và lý do.
5. Super Admin duyệt hoặc từ chối yêu cầu:
	 - Nếu duyệt → trạng thái đơn → **Đã hủy**, tồn kho và thanh toán được xử lý theo quy tắc với CATALOG/PAYMENTS.
	 - Nếu từ chối → đơn trở về trạng thái trước đó.

**Điều kiện testable**
- Chỉ những vai trò được phép (Ops, Super Admin) mới tạo và duyệt yêu cầu hủy.
- Đơn ở trạng thái không cho phép hủy (ví dụ: Đã nhận) không thể vào luồng Chờ duyệt hủy.
- Lịch sử hủy phải ghi nhận đầy đủ ai yêu cầu, ai duyệt, thời gian và lý do.

**Ví dụ dữ liệu (fake)**
- cancelRequest:  
	- orderId: 70001  
	- requestedBy: "ops_admin_01"  
	- reason: "NCC báo hết hàng"  
	- requestedAt: "2026-01-04T10:00:00Z"  
	- approvedBy: "super_admin_01"  
	- approvedAt: "2026-01-04T11:00:00Z"  
	- status: "Approved"

---

### FR-ORDERS-004 — Ghi nhận giao hàng, POD và đổi/trả tại Điểm nhận

**Mô tả**
- Hỗ trợ Shipper/Tầng 2 đánh dấu hàng đã giao (Đã nhận) kèm ảnh POD, và xử lý trường hợp đổi/trả theo chính sách tại Điểm nhận.

**Luồng chuẩn (POD)**
1. Shipper nhận danh sách đơn **Sẵn sàng nhận** tại Điểm nhận.
2. Khi khách đến nhận hàng, Shipper xác thực (bằng Mã nhận hàng, CCCD, hoặc phương thức được chốt; chi tiết TBD), giao hàng và thu COD (nếu có).
3. Shipper chụp ảnh POD qua ứng dụng, đánh dấu đơn **Đã nhận**.

**Luồng đổi/trả (khái niệm)**
1. Ngay tại Điểm nhận, nếu khách không hài lòng (hư hỏng, nhầm sản phẩm…), Shipper/Support ghi nhận yêu cầu **Đổi trả** cho đơn hoặc sản phẩm trong đơn.
2. Đơn/Item chuyển sang trạng thái "Đổi trả"; chi tiết hậu xử lý (nhập lại kho, xử lý tiền COD) thuộc PAYMENTS/CATALOG nhưng ORDERS phải lưu trạng thái.

**Điều kiện testable**
- Đơn chỉ có thể chuyển sang **Đã nhận** khi có ghi nhận POD (ít nhất 1 minh chứng, dạng link ảnh/file).
- Đơn ở trạng thái Đã nhận mới được phép vào luồng Đổi trả (hoặc theo rule chi tiết – TBD nếu cho phép đổi trả ở trạng thái trước đó).

**Ví dụ dữ liệu (fake)**
- deliveryProof:  
	- orderId: 70001  
	- deliveredBy: "shipper_200"  
	- deliveredAt: "2026-01-05T03:00:00Z"  
	- podImageUrl: "/pod/70001_1.jpg"  
	- receiverName: "Nguyễn Văn D" (nếu được phép lưu theo Data-Classification)

---

### FR-ORDERS-005 — Xem lịch sử đơn hàng của End User

**Mô tả**
- Cho phép End User xem danh sách các đơn hàng của chính mình, với trạng thái mới nhất và các thông tin chính.

**Luồng chuẩn**
1. User đăng nhập và vào mục "Đơn hàng của tôi".
2. Hệ thống hiển thị danh sách đơn theo thời gian (mới nhất trước), cho phép lọc theo trạng thái (Chờ xác nhận, Đang giao, Đã nhận, Đã hủy…).
3. User có thể mở chi tiết từng đơn để xem danh sách sản phẩm, tổng tiền, Điểm nhận, trạng thái, lịch sử thay đổi.

**Điều kiện testable**
- User chỉ xem được đơn hàng của chính mình (theo userId);
- Danh sách đơn hỗ trợ phân trang theo chuẩn hệ thống.

**Ví dụ dữ liệu (fake)**
- orders: [  
	{ orderId: 70001, status: "Đang giao", totalAmount: 43500, pickupPointName: "Điểm nhận KCN A-1" },  
	{ orderId: 70002, status: "Đã nhận", totalAmount: 120000, pickupPointName: "Điểm nhận KCN A-1" }  
]

## 4. Business rules references (BR-ORDERS-###)

> Mã BR chi tiết sẽ được tổng hợp trong Business-Rules.md. Ở đây chỉ ghi khung liên quan ORDERS.

- **BR-ORDERS-001 — Đơn chỉ chứa sản phẩm khả dụng**:  
	- Đơn chỉ được phép chứa sản phẩm có tồn kho khả dụng và thuộc Lô đã kiểm duyệt (theo BR-CATALOG-003/004).
- **BR-ORDERS-002 — Trạng thái đơn nhất quán**:  
	- Chỉ cho phép những chuyển trạng thái hợp lệ theo luồng đã thống nhất; các luồng khác phải bị chặn.
- **BR-ORDERS-003 — Hủy đơn theo phê duyệt**:  
	- Mọi hủy đơn sau một số trạng thái nhất định phải thông qua phê duyệt của Super Admin (trừ trường hợp auto-cancel theo rule thời gian, nếu có; hiện chưa mô tả chi tiết).
- **BR-ORDERS-004 — Đổi trả có ghi nhận chứng từ**:  
	- Mọi yêu cầu đổi/trả phải được ghi nhận lý do, ảnh chứng cứ (nếu được yêu cầu) và người thao tác.

## 5. Data inputs/outputs (conceptual)

### 5.1. Thực thể chính
- **Order (Đơn hàng)**:  
	- id, user_id, company_id, pickup_point_id, total_amount, payment_method, status, created_at, created_by, updated_at, updated_by.
- **OrderItem**:  
	- order_id, product_id, quantity, unit_price, line_amount.
- **OrderStatusHistory**:  
	- order_id, status, changed_by, changed_at, note.
- **OrderCancelRequest**:  
	- order_id, requested_by, requested_at, reason, approved_by, approved_at, status.
- **OrderDeliveryProof**:  
	- order_id, delivered_by, delivered_at, pod_url, note.
- **OrderReturnRequest** (khái niệm):  
	- order_id, items, reason, created_by, created_at, status.

### 5.2. Luồng dữ liệu
- Input từ CATALOG: sản phẩm, giá, tồn kho khả dụng.
- Input từ USERS: thông tin User, Công ty, Điểm nhận, Tầng.
- Input từ PAYMENTS: xác nhận phương thức thanh toán, hạn mức, trạng thái thanh toán.
- Output cho PAYMENTS: dữ liệu đơn hàng để đối soát thanh toán, công nợ, trả chậm.
- Output cho REPORTING: dữ liệu đơn hàng và trạng thái cho báo cáo doanh số, hoa hồng.

## 6. Validations & edge cases

### 6.1. Validations
- **V-ORDERS-001 — Quyền truy cập đơn hàng**:  
	- End User chỉ được truy cập đơn của chính mình; Admin/Tầng 2/Tầng 1 chỉ xem được đơn thuộc phạm vi mạng lưới/quyền hạn.
- **V-ORDERS-002 — Kiểm tra tồn kho khi tạo đơn**:  
	- Trước khi tạo đơn, phải kiểm tra tồn kho khả dụng với CATALOG; không cho phép đặt nếu không đủ.
- **V-ORDERS-003 — Kiểm tra trạng thái trước khi hủy**:  
	- Chỉ các trạng thái nằm trong danh sách cho phép (sẽ được business chốt) mới được tạo yêu cầu hủy.

### 6.2. Edge cases
- **E-ORDERS-001 — Sản phẩm hết hàng trong lúc đặt**:  
	- User bấm xác nhận đơn nhưng trong khoảng thời gian đó sản phẩm hết tồn kho; hệ thống phải từ chối tạo đơn và trả thông báo phù hợp.
- **E-ORDERS-002 — Không đồng bộ trạng thái giữa ORDERS và PAYMENTS**:  
	- Thanh toán bị lỗi sau khi ORDERS đã chuyển trạng thái đơn; cần cơ chế xử lý hoà giải (chi tiết thuộc PAYMENTS/REPORTING, ORDERS phải hỗ trợ cập nhật trạng thái phù hợp).
- **E-ORDERS-003 — Shipper quên chụp POD**:  
	- Nếu cho phép chuyển sang Đã nhận mà chưa upload POD, cần rule rõ ràng; yêu cầu mặc định là phải có POD (Assumption), nếu không thì giữ ở trạng thái Sẵn sàng nhận.
- **E-ORDERS-004 — Hủy đơn sau khi đã giao một phần** (nếu có mô hình giao nhiều lần):  
	- Hiện chưa được mô tả trong Discovery; nếu phát sinh yêu cầu, cần story và rule riêng.

## 7. Non-functional requirements (module-specific)

- **Hiệu năng**:  
	- Lịch sử đơn hàng của một User có thể dài (nhiều đơn); truy vấn phải được phân trang và tối ưu index theo user_id, created_at.
- **Audit & logging**:  
	- Mọi thay đổi trạng thái đơn, hủy, đổi trả, phải được ghi nhận trong audit log với correlationId, actor và timestamp.
- **Tính toàn vẹn dữ liệu**:  
	- Các thao tác tạo đơn và điều chỉnh tồn kho, thanh toán liên quan cần được thiết kế transactional để tránh lệch trạng thái (chi tiết implementation do Tech Lead quyết định).

## 8. Open Questions / Assumptions for ORDERS

**Open Questions**
- Q-ORDERS-001: Những trạng thái cụ thể nào được phép yêu cầu hủy đơn, và ở trạng thái nào thì chỉ có quyền hủy từ phía hệ thống (auto-cancel) thay vì theo yêu cầu?
- Q-ORDERS-002: Thời gian giữ tồn kho tạm thời cho một đơn "Chờ xác nhận" là bao lâu trước khi tự động hủy/giải phóng tồn kho?
- Q-ORDERS-003: Chính sách đổi trả chi tiết (thời gian cho phép, điều kiện sản phẩm, mức hoàn tiền) là gì?
- Q-ORDERS-004: Có mô hình giao hàng nhiều lần cho một đơn không, hay mỗi đơn luôn giao một lần duy nhất trong MVP?

**Assumptions (cần xác nhận)**
- A-ORDERS-001: MVP xử lý đơn hàng với một lần giao duy nhất; không có chia nhỏ nhiều giao/đợt.
- A-ORDERS-002: Mỗi đơn hàng gắn với đúng một Điểm nhận và một Công ty tại một thời điểm.
- A-ORDERS-003: Để đánh dấu Đã nhận, hệ thống yêu cầu ít nhất một minh chứng POD (ảnh hoặc xác nhận số hoá); policy chi tiết sẽ được refine trong giai đoạn thiết kế UX.
