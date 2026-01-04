
# Stories — PAYMENTS: Payments

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

### US-PAYMENTS-001 — Ghi nhận hình thức và trạng thái thanh toán cho đơn hàng
**As a** hệ thống PAYMENTS (thay mặt End User & Ops)  
**I want** lưu được hình thức thanh toán và trạng thái thanh toán cho mỗi đơn hàng  
**So that** các bên nắm được đơn đã/ chưa được thanh toán, theo đúng nghiệp vụ COD/chuyển khoản/trả chậm

**Priority:** P0  
**Dependencies:**
- SRS-PAYMENTS: FR-PAYMENTS-001  
- SRS-ORDERS (tạo đơn)  
- SRS-USERS (cấu hình trả chậm theo Công ty)

**Definition of Ready**
- [ ] Danh sách giá trị hợp lệ cho paymentMethod (ví dụ: COD, TRANSFER, CREDIT) được chốt.
- [ ] Danh sách trạng thái thanh toán (UNPAID, PAID, PENDING_TRANSFER, CREDIT_DUE, v.v.) được định nghĩa rõ ràng.
- [ ] Quy tắc mapping trạng thái thanh toán với trạng thái đơn (ORDERS) được mô tả ở mức high-level.

**Acceptance Criteria**
- AC1: Khi ORDERS tạo đơn mới, PAYMENTS lưu được paymentMethod được chọn và khởi tạo trạng thái thanh toán ban đầu chính xác.
- AC2: Khi có sự kiện thanh toán (thu COD thành công, xác nhận chuyển khoản, ghi nhận trả chậm đến hạn), trạng thái paymentStatus của đơn được cập nhật đúng.
- AC3: Chỉ có một paymentMethod chính tại một thời điểm cho mỗi đơn; chuyển đổi hình thức thanh toán nếu được phép phải có rule riêng (không thuộc scope story này).

**Sample data (fake)**
- orderId: 70001  
- paymentMethod: "COD"  
- paymentStatus: "UNPAID" → "PAID"  
- amount: 43500

**Edge cases (min 3)**
- EC1: ORDERS gửi giá trị paymentMethod không nằm trong danh sách hợp lệ → PAYMENTS từ chối và log lỗi.
- EC2: Sự kiện thanh toán tới trễ (ví dụ chuyển khoản) sau khi đơn đã bị hủy → Cần rule xử lý (Open Question); story này giả định paymentStatus không chuyển sang PAID cho đơn đã hủy.
- EC3: Cùng một đơn nhận được hai sự kiện đánh dấu "PAID" (do retry) → Trạng thái thanh toán vẫn là PAID, không nhân đôi số tiền.
- EC4: Đơn trả chậm (CREDIT) nhưng sau đó khách quyết định thanh toán sớm → Cần rule cụ thể về cập nhật paymentStatus và công nợ (ngoài phạm vi chi tiết story này).

**API mapping**
- TBD (Tech Lead)

**Screens**
- N/A (logic nền giữa ORDERS và PAYMENTS, không có màn hình riêng)

---

### US-PAYMENTS-002 — Shipper ghi nhận Phiếu thu COD theo đơn
**As a** Shipper/Tầng 2  
**I want** tạo Phiếu thu COD khi thu tiền mặt của khách  
**So that** hệ thống ghi nhận chính xác số tiền tôi đã thu cho từng đơn

**Priority:** P0  
**Dependencies:**
- SRS-PAYMENTS: FR-PAYMENTS-002  
- SRS-ORDERS (trạng thái Đã nhận)  
- SRS-USERS (thông tin Shipper/Tầng 2)

**Definition of Ready**
- [ ] Thiết kế UI cho Shipper tạo Phiếu thu được chốt (trên mobile/web admin).
- [ ] Quy tắc bắt buộc/tuỳ chọn cho các trường của Phiếu thu (ghi chú, hình thức nộp lại tiền...) được xác định.
- [ ] Quy tắc khi nào cho phép tạo Phiếu thu (ví dụ: chỉ khi đơn ở trạng thái Đã nhận hoặc Sẵn sàng nhận) được chốt.

**Acceptance Criteria**
- AC1: Shipper có thể chọn một đơn thuộc ca/ phạm vi của mình và tạo Phiếu thu COD với số tiền bằng số tiền cần thu của đơn.
- AC2: Sau khi Phiếu thu được ghi nhận, paymentStatus của đơn chuyển sang trạng thái "PAID" (hoặc tương đương) nếu số tiền khớp.
- AC3: Hệ thống lưu lại thông tin: orderId, amount, collectedBy, collectedAt, shiftId.

**Sample data (fake)**
- Input:
	- orderId: 70001  
	- amount: 43500  
	- collectedBy: "shipper_200"  
	- shiftId: 30001
- Output (entity):
	- receiptId: 90001  
	- orderId: 70001  
	- amount: 43500  
	- collectedAt: "2026-01-05T03:05:00Z"

**Edge cases (min 3)**
- EC1: Shipper nhập amount khác tổng tiền đơn (thiếu hoặc thừa) → Hệ thống cảnh báo/ chặn theo rule (Open Question: cho phép hay không và xử lý chênh lệch thế nào).
- EC2: Shipper cố gắng tạo Phiếu thu cho đơn không thuộc ca của mình → Bị từ chối do không đủ quyền.
- EC3: Shipper tạo 2 Phiếu thu cho cùng một đơn (do thao tác nhầm) → Hệ thống cần rule: chặn Phiếu thu thứ hai hoặc gộp, nhưng không được nhân đôi doanh thu (Open Question); story này giả định chỉ cho phép một Phiếu thu chính.
- EC4: Mạng lỗi trong lúc gửi thông tin Phiếu thu → Khi retry, hệ thống phải tránh tạo trùng (dựa trên idempotency key nếu có, TBD).

**API mapping**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình Shipper xem chi tiết đơn và tạo Phiếu thu COD)

---

### US-PAYMENTS-003 — Shipper chốt Ca COD của mình
**As a** Shipper  
**I want** chốt Ca làm việc và xem tổng số tiền COD tôi đã thu  
**So that** tôi có thể bàn giao tiền cho CoShare/Finance và kết thúc ca

**Priority:** P0  
**Dependencies:**
- SRS-PAYMENTS: FR-PAYMENTS-003  
- SRS-ORDERS (đơn thuộc ca)  
- SRS-USERS (thông tin Shipper)

**Definition of Ready**
- [ ] Cách xác định ca (shift) được chốt: thời gian cố định hay linh hoạt do Shipper chủ động "Bắt đầu ca".
- [ ] Thiết kế UI màn hình tổng hợp COD theo ca được phê duyệt.
- [ ] Rule về việc có cho phép chỉnh sửa/huỷ Phiếu thu trước khi chốt ca được xác định.

**Acceptance Criteria**
- AC1: Shipper có thể xem danh sách đơn COD thuộc Ca hiện tại và tổng số tiền cần thu (expectedCod).
- AC2: Sau khi kiểm tra, Shipper có thể nhấn "Chốt ca" để hệ thống tính toán và lưu collectedCod (tổng tiền theo Phiếu thu) và difference (expectedCod - collectedCod).
- AC3: Sau khi chốt ca, trạng thái Ca chuyển sang "Đã chốt" và Shipper không thể thêm/sửa/xoá Phiếu thu cho ca đó.

**Sample data (fake)**
- Ca trước khi chốt:
	- shiftId: 30001  
	- shipperId: "shipper_200"  
	- status: "OPEN"  
	- expectedCod: 1_500_000  
	- collectedCod: 0
- Sau chốt:
	- status: "CLOSED"  
	- collectedCod: 1_480_000  
	- difference: -20_000

**Edge cases (min 3)**
- EC1: Shipper cố chốt ca khi vẫn còn đơn COD chưa có Phiếu thu → Hệ thống cảnh báo rõ danh sách đơn còn thiếu, rule cho phép/không cho phép chốt được xác định.
- EC2: Shipper chốt nhầm ca (chưa kết thúc thực tế) → Cần quy trình mở lại/điều chỉnh (ngoài phạm vi story này, sẽ có story riêng nếu cần).
- EC3: Thời gian ca kéo dài nhiều ngày (qua 0h) → Cần rule cắt ca/ghi nhận; story này giả định ca được giới hạn trong một khoảng thời gian hợp lý (Assumption).
- EC4: Shipper làm nhiều ca trong ngày → Hệ thống phải phân biệt rõ từng ca, không gộp sai đơn vào ca khác.

**API mapping**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình Shipper xem tổng hợp COD và nút Chốt ca)

---

### US-PAYMENTS-004 — Finance đối soát COD theo Ca
**As a** Finance  
**I want** xem và đối soát các Ca COD của Shipper  
**So that** tôi có thể xác nhận số tiền thực thu và ghi nhận sai lệch nếu có

**Priority:** P0  
**Dependencies:**
- SRS-PAYMENTS: FR-PAYMENTS-004, BR-PAYMENTS-002  
- SRS-ORDERS (thông tin đơn)  
- SRS-USERS (thông tin Shipper)

**Definition of Ready**
- [ ] Các trạng thái đối soát (CHUA_DOI_SOAT, DA_DOI_SOAT, DIFFERENCE, v.v.) được chốt.
- [ ] Thiết kế UI danh sách Ca và chi tiết từng Ca được duyệt.
- [ ] Quy trình nghiệp vụ khi có sai lệch (ai xử lý, trong bao lâu) được mô tả ở mức high-level.

**Acceptance Criteria**
- AC1: Finance có thể xem danh sách Ca đã chốt, với các trường chính: shipper, expectedCod, collectedCod, difference, status.
- AC2: Finance có thể đánh dấu một Ca là "Đã đối soát" nếu số tiền khớp (difference = 0).
- AC3: Nếu difference ≠ 0, Finance có thể đánh dấu Ca ở trạng thái "Sai lệch" và ghi chú lý do; dữ liệu này được lưu lại để xử lý sau.
- AC4: Sau khi đánh dấu "Đã đối soát", hệ thống không cho phép chỉnh Phiếu thu trong Ca đó.

**Sample data (fake)**
- Ca sau đối soát:
	- shiftId: 30001  
	- expectedCod: 1_500_000  
	- collectedCod: 1_480_000  
	- difference: -20_000  
	- status: "DIFFERENCE"  
	- processedBy: "finance_01"  
	- processedAt: "2026-01-05T10:00:00Z"  
	- note: "Shipper báo khách thiếu 20k, sẽ thu bổ sung kỳ sau"

**Edge cases (min 3)**
- EC1: Finance phát hiện Phiếu thu bị nhập sai (số tiền quá lớn) sau khi Shipper đã chốt ca nhưng trước khi đối soát → Cần quy trình hiệu chỉnh; story này chỉ đảm bảo ghi được trạng thái "Sai lệch" và note.
- EC2: Có nhiều người Finance cùng mở chi tiết một Ca và thao tác đối soát gần như cùng lúc → Hệ thống phải tránh ghi đè trạng thái sai (optimistic locking).
- EC3: Một Ca có difference nhỏ trong ngưỡng cho phép theo policy (ví dụ ±5.000đ) → Policy xử lý (bỏ qua hay vẫn ghi nhận) là Open Question.
- EC4: Ca chưa được Shipper chốt nhưng Finance đã xem trong danh sách → Hệ thống hiển thị rõ trạng thái để tránh nhầm lẫn; không cho đối soát trước khi chốt ca.

**API mapping**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình Finance xem danh sách Ca và chi tiết Ca để đối soát)

---

### US-PAYMENTS-005 — Kiểm tra hạn mức trả chậm và cập nhật công nợ User
**As a** hệ thống PAYMENTS / Finance  
**I want** kiểm tra hạn mức trả chậm và cập nhật công nợ cho User khi đặt đơn  
**So that** công nợ không vượt quá mức cho phép và dữ liệu nợ được ghi nhận chính xác

**Priority:** P0  
**Dependencies:**
- SRS-PAYMENTS: FR-PAYMENTS-005, BR-PAYMENTS-001  
- SRS-ORDERS (tạo đơn trả chậm)  
- SRS-USERS (User, Công ty, cấu hình trả chậm)

**Definition of Ready**
- [ ] Rule tính và lưu hạn mức trả chậm theo Công ty/User được chốt (limitAmount, hiệu lực từ/đến).
- [ ] Quy trình/chu kỳ thu hồi công nợ (theo kỳ lương hay cấu hình khác) được mô tả.

**Acceptance Criteria**
- AC1: Khi ORDERS yêu cầu tạo đơn với hình thức trả chậm, PAYMENTS kiểm tra: currentDebt + orderAmount <= creditLimit; nếu vượt, từ chối cho phép trả chậm.
- AC2: Nếu đơn trả chậm được chấp nhận, công nợ của User (DebtBalance) được cập nhật tăng đúng bằng orderAmount.
- AC3: Khi Finance ghi nhận User trả nợ (qua luồng riêng), công nợ giảm tương ứng, không âm nếu không có bút toán điều chỉnh đặc biệt.

**Sample data (fake)**
- Trước khi đặt đơn:
	- creditLimit: 2_000_000  
	- currentDebt: 1_500_000
- Đơn mới:
	- newOrderAmount: 400_000
- Kết quả:
	- allowed: true  
	- debtAfterOrder: 1_900_000

**Edge cases (min 3)**
- EC1: creditLimit = 0 (Công ty không bật trả chậm) nhưng ORDERS vẫn cố gửi yêu cầu trả chậm → Hệ thống từ chối, trả về lỗi "Không được phép trả chậm".
- EC2: currentDebt không đồng bộ do lỗi trước đó (ví dụ thiếu một bút toán) → Cần cơ chế kiểm tra/điều chỉnh; story này giả định số liệu DebtBalance là nguồn tin cậy.
- EC3: User thuộc Công ty thay đổi chính sách hạn mức (limitAmount giảm) trong khi vẫn còn công nợ vượt quá mức mới → Cần policy rõ ràng (Open Question) về xử lý trường hợp này.
- EC4: Có nhiều đơn trả chậm tạo gần như đồng thời cho cùng User → Hệ thống cần đảm bảo không để tổng công nợ vượt limit do race condition.

**API mapping**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình Finance cấu hình hạn mức và xem công nợ; End User có thể xem công nợ ở một màn hình khác nếu được yêu cầu)

