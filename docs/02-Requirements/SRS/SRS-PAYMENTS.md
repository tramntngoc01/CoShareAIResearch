
# SRS — PAYMENTS: Payments

## 1. Module purpose & boundaries (in/out)

**Mục đích**
- Quản lý **Thanh toán** cho đơn hàng, bao gồm:
	- **COD** (Cash On Delivery) do Shipper/Tầng 2 thu hộ.
	- **Chuyển khoản** (thanh toán qua ngân hàng).
	- **Trả chậm** (thanh toán sau, đến ngày nhận lương) và **Công nợ** của User.
- Hỗ trợ **Ca**, **Chốt ca**, **Đối soát COD** giữa Shipper và CoShare/Finance.

**Trong phạm vi (IN)**
- Ghi nhận **hình thức thanh toán** và **trạng thái thanh toán** cho từng Đơn hàng.
- Ghi nhận **Phiếu thu COD** theo từng đơn và tổng hợp theo **Ca** của Shipper.
- Quản lý **hạn mức trả chậm** và **công nợ** cho User Tầng 3 theo rule được khách hàng chốt.
- Hỗ trợ dữ liệu cho **đối soát COD** và **báo cáo công nợ** (bản chi tiết báo cáo thuộc REPORTING nhưng PAYMENTS cung cấp dữ liệu nguồn).

**Ngoài phạm vi (OUT)**
- Tích hợp chi tiết với Ngân hàng/Payment Gateway (nếu có trong tương lai) – hiện chỉ mô tả ở mức ghi nhận thông tin chuyển khoản, không mô tả API bên ngoài.
- Chi tiết logic tính hoa hồng Affiliate (dù dựa trên doanh thu đơn hàng) – thuộc REPORTING/Hoa hồng, không nằm trong PAYMENTS.

## 2. Personas/roles involved

- **User Tầng 3 (End User)**:
	- Chọn hình thức thanh toán (COD / Chuyển khoản / Trả chậm nếu được Công ty cho phép).
	- Theo dõi trạng thái thanh toán/ công nợ của mình (ở mức hiển thị, chi tiết báo cáo ở REPORTING).
- **Tầng 2 (có thể là Shipper)**:
	- Thu COD khi giao hàng.
	- Khởi tạo/ghi nhận Phiếu thu COD theo từng đơn hoặc từng lượt thu.
- **Shipper** (nếu là vai trò tách biệt):
	- Thu COD, tạo Phiếu thu, chốt ca.
- **Finance (Admin)**:
	- Cấu hình hạn mức trả chậm theo Công ty/User.
	- Theo dõi công nợ, duyệt/điều chỉnh các bút toán điều chỉnh nếu có (chi tiết rule sẽ được chốt).
	- Thực hiện đối soát COD theo ca với Shipper/Tầng 2.
- **Ops (Admin)**:
	- Xem trạng thái thanh toán để biết đơn đã đủ điều kiện để giao/hủy.

## 3. Functional requirements (FR-PAYMENTS-###)

### FR-PAYMENTS-001 — Ghi nhận hình thức và trạng thái thanh toán cho Đơn hàng

**Mô tả**
- Mỗi Đơn hàng phải có **hình thức thanh toán** (COD/Chuyển khoản/Trả chậm) và **trạng thái thanh toán** tương ứng.

**Luồng chuẩn (End User / hệ thống)**
1. Khi User tạo đơn hàng (ORDERS), User chọn hoặc hệ thống gợi ý hình thức thanh toán được phép (theo cấu hình Công ty và trạng thái công nợ).
2. PAYMENTS ghi nhận payment_method cho đơn và khởi tạo trạng thái thanh toán ban đầu (ví dụ: "Chưa thanh toán" hoặc "Chờ xác nhận chuyển khoản" hay "Công nợ" đối với trả chậm – mapping chi tiết sẽ được chốt).
3. Khi có sự kiện thanh toán (thu COD, xác nhận chuyển khoản, đến hạn trả chậm), PAYMENTS cập nhật trạng thái thanh toán tương ứng.

**Điều kiện testable**
- Mỗi đơn có đúng **một** hình thức thanh toán chính tại một thời điểm.
- Trạng thái thanh toán thay đổi theo các sự kiện nghiệp vụ xác định (thu tiền, hoàn tiền...).

**Ví dụ dữ liệu (fake)**
- orderId: 70001  
- paymentMethod: "COD"  
- paymentStatus: "UNPAID" / "PAID"  
- amount: 43500

---

### FR-PAYMENTS-002 — Ghi nhận và quản lý Phiếu thu COD theo đơn

**Mô tả**
- Khi Shipper/Tầng 2 thu tiền mặt (COD) khi giao hàng, hệ thống phải ghi nhận **Phiếu thu** điện tử gắn với Đơn hàng.

**Luồng chuẩn (Shipper)**
1. Shipper giao hàng cho User (ORDERS chuyển đơn sang trạng thái Đã nhận).
2. Shipper thu COD đúng số tiền cần thu.
3. Shipper ghi nhận Phiếu thu trong hệ thống, bao gồm: số tiền, thời gian, đơn tham chiếu, Ca làm.
4. PAYMENTS cập nhật trạng thái thanh toán của đơn sang "Đã thanh toán" (hoặc trạng thái phù hợp nếu có tiền thiếu/thừa – rule chi tiết TBD).

**Điều kiện testable**
- Với mỗi đơn COD đã nhận, phải có ít nhất một Phiếu thu COD hoặc có lý do rõ ràng nếu không có (Open Question).
- Tổng số tiền COD phải thu trong một ca bằng tổng số tiền trên các Phiếu thu của các đơn thuộc ca đó (trước khi xử lý sai lệch/điều chỉnh nếu có).

**Ví dụ dữ liệu (fake)**
- receiptId: 90001  
- orderId: 70001  
- amount: 43500  
- collectedBy: "shipper_200"  
- collectedAt: "2026-01-05T03:05:00Z"  
- shiftId: 30001

---

### FR-PAYMENTS-003 — Quản lý Ca làm và Chốt ca COD của Shipper

**Mô tả**
- Hệ thống phải hỗ trợ Shipper chốt Ca làm (Ca) của mình, tổng hợp COD đã thu trong ca và hỗ trợ Finance đối soát.

**Luồng chuẩn (Shipper/Finance)**
1. Shipper bắt đầu Ca làm (tạo bản ghi Ca hoặc được hệ thống/ops tạo; chi tiết trigger tạo Ca TBD).
2. Trong Ca, Shipper giao hàng và tạo nhiều Phiếu thu COD cho các đơn.
3. Cuối Ca, Shipper thực hiện **Chốt ca**:
	 - Hệ thống tổng hợp số tiền COD phải thu (từ đơn hàng) và số tiền thực tế Shipper khai báo đã thu (tổng từ Phiếu thu) trong Ca.
	 - Ghi nhận chênh lệch (nếu có).
4. Finance sử dụng dữ liệu này để thực hiện đối soát (FR-PAYMENTS-004).

**Điều kiện testable**
- Một đơn COD chỉ thuộc về một Ca (ca của Shipper thực hiện giao hàng).
- Sau khi Chốt ca, trạng thái Ca phải chuyển sang "Đã chốt" và không thể sửa Phiếu thu trong Ca đó (trừ khi có quy trình điều chỉnh riêng được phê duyệt).

**Ví dụ dữ liệu (fake)**
- shiftId: 30001  
- shipperId: "shipper_200"  
- period: "2026-01-05T00:00:00Z" → "2026-01-05T08:00:00Z"  
- expectedCod: 1_500_000  
- collectedCod: 1_480_000  
- difference: -20_000 (thiếu)

---

### FR-PAYMENTS-004 — Đối soát COD giữa Shipper và Finance

**Mô tả**
- Hệ thống phải cung cấp dữ liệu và hỗ trợ quy trình **Đối soát** COD: so sánh số tiền Shipper đã khai báo/ nộp với số tiền hệ thống ghi nhận phải thu.

**Luồng chuẩn (Finance)**
1. Finance xem danh sách Ca đã chốt của các Shipper.
2. Finance đối chiếu số tiền COD theo hệ thống và số tiền thực tế Shipper nộp.
3. Nếu khớp → đánh dấu Ca "Đã đối soát".
4. Nếu chênh lệch → ghi nhận sai lệch (thiếu/dư) và (tuỳ chính sách) tạo bút toán điều chỉnh hoặc task xử lý.

**Điều kiện testable**
- Mỗi Ca đã chốt phải có trạng thái đối soát (Chưa đối soát / Đã đối soát / Sai lệch ... – chi tiết states sẽ được chốt).
- Sau khi đánh dấu "Đã đối soát", không thể thay đổi dữ liệu Phiếu thu trong Ca đó (trừ quy trình điều chỉnh có audit riêng).

**Ví dụ dữ liệu (fake)**
- reconciliationRecord:  
	- shiftId: 30001  
	- expectedCod: 1_500_000  
	- actualCod: 1_480_000  
	- difference: -20_000  
	- status: "DIFFERENCE"  
	- processedBy: "finance_01"  
	- processedAt: "2026-01-05T10:00:00Z"

---

### FR-PAYMENTS-005 — Quản lý hạn mức trả chậm và công nợ User

**Mô tả**
- PAYMENTS phải quản lý **Hạn mức trả chậm** và **Công nợ** cho User Tầng 3 theo cấu hình Công ty.

**Luồng chuẩn (End User / Finance)**
1. Finance/CoShare cấu hình hạn mức trả chậm cho Công ty hoặc cho từng User (tổng tiền tối đa/cả số đơn tối đa – chi tiết rule số lượng/bội số TBD).
2. Khi User đặt đơn với hình thức trả chậm, PAYMENTS kiểm tra:
	 - Số dư công nợ hiện tại + giá trị đơn mới có vượt hạn mức không.
3. Nếu không vượt hạn mức → cho phép tạo đơn, ghi nhận công nợ tăng.
4. Khi đến kỳ trả lương hoặc khi User trả nợ, PAYMENTS ghi nhận các khoản thanh toán công nợ, giảm số dư.

**Điều kiện testable**
- Không cho phép User tạo đơn trả chậm nếu sau khi cộng giá trị đơn mới **vượt quá hạn mức**.
- Công nợ của User được cập nhật đúng sau mỗi đơn trả chậm mới và mỗi lần thanh toán công nợ.

**Ví dụ dữ liệu (fake)**
- creditLimit: 2_000_000  
- currentDebt: 1_500_000  
- newOrderAmount: 400_000  
- result: 1_900_000 (<= 2_000_000 → cho phép)  
- debtAfterOrder: 1_900_000

## 4. Business rules references (BR-PAYMENTS-###)

> Mã BR đầy đủ sẽ được tập trung trong Business-Rules.md. Ở đây chỉ mô tả các khung quy tắc của module PAYMENTS.

- **BR-PAYMENTS-001 — Hạn mức trả chậm**:  
	- Mỗi User Tầng 3 thuộc Công ty có trả chậm phải có/hoặc được gán hạn mức; không cho phép công nợ vượt quá hạn mức.
- **BR-PAYMENTS-002 — Đối soát COD bắt buộc**:  
	- Mọi Ca có COD phải được đối soát; không được coi là hoàn tất cho đến khi trạng thái đối soát là "Đã đối soát" hoặc trạng thái sai lệch đã được xử lý.
- **BR-PAYMENTS-003 — Ghi nhận Phiếu thu**:  
	- Phiếu thu COD phải gắn với Đơn hàng và Ca; không cho phép sửa/xoá Phiếu thu sau khi Ca đã chốt, trừ luồng điều chỉnh đặc biệt.
- **BR-PAYMENTS-004 — Dữ liệu thanh toán là nguồn cho báo cáo**:  
	- Mọi thay đổi thanh toán (COD, chuyển khoản, trả chậm) phải đảm bảo thống nhất với dữ liệu báo cáo doanh thu/hoa hồng.

## 5. Data inputs/outputs (conceptual)

### 5.1. Thực thể chính
- **Payment** (thanh toán theo đơn):  
	- order_id, payment_method, amount, status, created_at, updated_at.
- **Receipt** (Phiếu thu COD):  
	- id, order_id, amount, collected_by (shipper), collected_at, shift_id.
- **Shift** (Ca làm của Shipper):  
	- id, shipper_id, start_time, end_time, status (OPEN/CLOSED/RECONCILED...), expected_cod, collected_cod, difference.
- **ReconciliationRecord** (Đối soát COD):  
	- id, shift_id, expected_cod, actual_cod, difference, status, processed_by, processed_at.
- **CreditLimit** (Hạn mức trả chậm):  
	- id, user_id/company_id, limit_amount, is_active, effective_from, effective_to.
- **DebtBalance** (Công nợ):  
	- id, user_id, current_debt, lastUpdatedAt.
- **DebtTransaction** (Giao dịch công nợ):  
	- id, user_id, order_id (nếu liên quan), amount_change (+ tăng nợ, - giảm nợ), created_at, note.

### 5.2. Luồng dữ liệu
- Input từ ORDERS: 
	- Thông tin đơn hàng, tổng tiền, trạng thái đơn, Shipper.
- Input từ USERS:  
	- Thông tin User, Tầng, Công ty, cấu hình trả chậm theo Công ty.
- Output cho ORDERS:  
	- Trạng thái thanh toán của đơn (đã/ chưa thanh toán, công nợ, chờ chuyển khoản...).
- Output cho REPORTING:  
	- Dữ liệu thanh toán, COD theo Ca, công nợ User.

## 6. Validations & edge cases

### 6.1. Validations
- **V-PAYMENTS-001 — Số tiền hợp lệ**:  
	- Số tiền trên Phiếu thu, Payment, DebtTransaction phải >= 0 (trừ khi bút toán điều chỉnh âm được cho phép theo rule riêng).
- **V-PAYMENTS-002 — Gắn kết dữ liệu**:  
	- Phiếu thu phải gắn với Đơn hàng; Shift phải gắn với Shipper; DebtTransaction phải gắn với User (và order_id nếu là đơn trả chậm).
- **V-PAYMENTS-003 — Hạn mức trả chậm**:  
	- Trước khi ghi nhận đơn trả chậm mới, hệ thống phải kiểm tra và đảm bảo không vượt hạn mức.

### 6.2. Edge cases
- **E-PAYMENTS-001 — Shipper nộp thiếu/thừa tiền COD**:  
	- Khi đối soát phát hiện chênh lệch (difference ≠ 0), hệ thống phải ghi nhận sai lệch; cách xử lý tiếp theo (truy thu, bù, cảnh báo) là Open Question.
- **E-PAYMENTS-002 — Đơn chuyển từ COD sang trả chậm hoặc ngược lại**:  
	- Nếu khách đổi hình thức thanh toán sau khi tạo đơn, cần rule cụ thể; story này giả định chỉ đổi thông qua quy trình đặc biệt của Finance/Ops.
- **E-PAYMENTS-003 — User rời Công ty khi còn công nợ**:  
	- Cần rule xử lý (chuyển nợ, xoá nợ, xử lý riêng) – Open Question.
- **E-PAYMENTS-004 — Lỗi kỹ thuật khi ghi nhận Phiếu thu**:  
	- Nếu hệ thống lỗi sau khi thu tiền nhưng chưa ghi kịp Phiếu thu, cần cơ chế kiểm tra/điều chỉnh; yêu cầu chi tiết sẽ được xác định trong giai đoạn thiết kế chi tiết.

## 7. Non-functional requirements (module-specific)

- **Tính toàn vẹn dữ liệu tài chính**:  
	- Mọi thao tác ghi nhận thanh toán, công nợ, đối soát phải tuân thủ nguyên tắc không mất dữ liệu, không ghi trùng, và có audit log.
- **Hiệu năng & batch**:  
	- Các báo cáo/đối soát theo Ca và công nợ có thể chạy trên volume dữ liệu lớn; cần index phù hợp và có thể dùng batch job để tổng hợp;
- **Bảo mật & phân quyền**:  
	- Dữ liệu PAYMENTS phải chỉ được truy cập bởi các vai trò được phép (Finance, một phần Ops/Shipper), không lộ thông tin nhạy cảm cho End User ngoài phạm vi cần thiết.

## 8. Open Questions / Assumptions for PAYMENTS

**Open Questions**
- Q-PAYMENTS-001: Cách tính và phân bổ hạn mức trả chậm (mức chuẩn theo Công ty, theo seniority, theo mức lương...) cụ thể như thế nào?
- Q-PAYMENTS-002: Thời điểm chốt công nợ/thu nợ (theo kỳ lương, theo lịch cố định hay theo từng Công ty cấu hình riêng)?
- Q-PAYMENTS-003: Có tích hợp cổng thanh toán/chuyển khoản online không, hay chỉ ghi nhận thủ công?
- Q-PAYMENTS-004: Rule chi tiết khi xử lý chênh lệch COD (thiếu/dư) và workflow phê duyệt điều chỉnh là gì?

**Assumptions (cần xác nhận)**
- A-PAYMENTS-001: MVP chưa tích hợp trực tiếp với ngân hàng/cổng thanh toán; mọi thông tin chuyển khoản được nhập/đối soát thủ công.
- A-PAYMENTS-002: Mỗi đơn chỉ có **một** hình thức thanh toán chính; trường hợp thanh toán kết hợp (một phần COD, một phần chuyển khoản) sẽ được xử lý ở giai đoạn sau nếu khách hàng yêu cầu.
- A-PAYMENTS-003: Chênh lệch COD nhỏ sẽ được xử lý theo chính sách riêng (ví dụ: cho phép trong một ngưỡng), nhưng chính sách cụ thể chưa được chốt.
