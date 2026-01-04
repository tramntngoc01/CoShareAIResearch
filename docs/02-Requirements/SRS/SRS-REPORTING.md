
# SRS — REPORTING: Reporting & Exports

## 1. Module purpose & boundaries (in/out)

**Mục đích**
- Cung cấp các **báo cáo vận hành** cho Admin Portal theo phạm vi MVP, cụ thể:
	- Báo cáo **doanh số** theo thời gian, Công ty, Điểm nhận, ngành hàng.
	- Báo cáo **hoa hồng** theo tầng (Tầng 1, Tầng 2) và ngành hàng.
	- Báo cáo **COD & đối soát** theo ca/Điểm nhận/Công ty.
	- Báo cáo **công nợ trả chậm** (trả chậm, quá hạn).
- Cung cấp dữ liệu cho **Dashboard** Admin (tổng quan đơn hôm nay, tỷ lệ nhận hàng, COD tồn, cảnh báo quá hạn...) dựa trên số liệu từ các module ORDERS, PAYMENTS, USERS, CATALOG.

**Trong phạm vi (IN)**
- Xem báo cáo dạng **bảng + tổng hợp** (aggregates) trên Admin Portal đối với:
	- Doanh số đơn hàng (số đơn, doanh thu, số lượng sản phẩm) theo ngày/tuần/tháng, theo Công ty, Điểm nhận, ngành hàng.
	- Hoa hồng theo tầng (Tầng 1/Tầng 2) và theo ngành hàng trong một khoảng thời gian.
	- COD đã thu, COD tồn, kết quả chốt ca và đối soát theo Ca Shipper/Tầng 2.
	- Công nợ trả chậm: số dư nợ, nợ trong hạn, nợ quá hạn theo Công ty, theo User.
- Cung cấp các **tổng số liệu** (KPIs) để hiển thị trên Dashboard Admin.
- Hỗ trợ **lọc và phân trang** cho báo cáo, và xuất dữ liệu ở mức cơ bản (ví dụ: export CSV) nếu được chốt trong giai đoạn thiết kế (chi tiết export chưa được mô tả rõ → Open Question).

**Ngoài phạm vi (OUT)**
- Hệ thống BI/Analytics đầy đủ (data warehouse, cube, báo cáo tài chính chuẩn IFRS...), chỉ dừng ở báo cáo vận hành mức MVP.
- Tích hợp trực tiếp với hệ thống báo cáo kế toán của đối tác (nếu có) – chưa được mô tả trong Discovery.
- Dashboard hoa hồng chi tiết nâng cao cho Tầng 1 (được mô tả là Giai đoạn mở rộng trong MVP-Scope).
- Báo cáo tài chính nâng cao (GĐ5 theo MVP-Scope), ngoài các báo cáo vận hành đã nêu (doanh số, hoa hồng, công nợ, COD).

---

## 2. Personas/roles involved

Theo Discovery và Glossary, REPORTING phục vụ các vai trò sau:

- **Super Admin**
	- Xem báo cáo tổng hợp toàn hệ thống (tất cả Công ty, KCN, ngành hàng, tầng).
	- Xem Dashboard tổng quan và các cảnh báo chính.

- **Ops**
	- Xem báo cáo vận hành đơn hàng: số đơn, tỷ lệ nhận hàng, trạng thái đơn theo Công ty/Điểm nhận.
	- Sử dụng số liệu để điều phối vận hành.

- **Finance**
	- Xem báo cáo COD (COD thu, COD tồn, sai lệch theo ca, theo Công ty).
	- Xem báo cáo công nợ trả chậm (trong hạn, quá hạn, theo Công ty/User).

- **QC**
	- Có thể xem một số báo cáo về doanh số theo ngành hàng/lô kiểm duyệt nếu được phân quyền (chi tiết quyền sẽ được chốt trong ADMIN).

- **Tầng 1 (read-only)**
	- Theo MVP-Scope, có quyền **xem tổng quan mạng lưới & hoa hồng** ở mức read-only; mức chi tiết field sẽ do khách hàng chốt.

- **Tầng 2**
	- Xem báo cáo/summary hoa hồng và doanh số cá nhân (MVP đề cập "Xem hoa hồng/chiết khấu cá nhân"), được hỗ trợ bởi REPORTING/ PAYMENTS.

---

## 3. Functional requirements (FR-REPORTING-###)

> Ghi chú: REPORTING chủ yếu là module đọc/tổng hợp dữ liệu từ ORDERS, PAYMENTS, USERS, CATALOG; không thay đổi trạng thái nghiệp vụ.

### FR-REPORTING-001 — Báo cáo doanh số theo thời gian, Công ty, Điểm nhận, ngành hàng

**Mô tả**
- Hệ thống phải cung cấp báo cáo **doanh số đơn hàng** với các chỉ số cơ bản:
	- Số đơn.
	- Tổng doanh thu (theo đơn hàng đã "Đã nhận" hoặc trạng thái được chốt từ ORDERS).
	- Số lượng sản phẩm.
- Có thể lọc/nhóm theo:
	- Khoảng thời gian (ngày, tuần, tháng, khoảng tuỳ chọn).
	- Công ty, Điểm nhận.
	- Ngành hàng.

**Luồng chuẩn**
1. Admin (Super Admin/Ops/Finance) truy cập màn hình "Báo cáo doanh số".
2. Chọn filter: khoảng thời gian, Công ty, Điểm nhận, ngành hàng (tối thiểu thời gian + Công ty).
3. Hệ thống truy vấn số liệu từ nguồn (ORDERS, CATALOG) thông qua aggregation tại REPORTING.
4. Hệ thống hiển thị bảng + tổng kết (totals) và cho phép phân trang nếu danh sách chi tiết quá dài.

**Tiêu chí kiểm thử (testable)**
- AC1: Khi chọn một khoảng thời gian và Công ty cụ thể, báo cáo hiển thị đúng tổng số đơn và tổng doanh thu tương ứng với tập đơn ở trạng thái được quy định (ví dụ: chỉ tính đơn "Đã nhận" trừ đơn "Đã hủy"; rule chính xác sẽ được chốt trong ORDERS/Business-Rules).
- AC2: Khi thay đổi filter (ví dụ: thêm filter ngành hàng), số liệu cập nhật phù hợp (tổng doanh thu bằng tổng doanh thu của tất cả ngành hàng được filter).
- AC3: Báo cáo phải hỗ trợ phân trang cho danh sách chi tiết (nếu có) theo chuẩn page/pageSize, không tải toàn bộ dữ liệu một lần.

**Ví dụ dữ liệu (fake)**
- Input filter:
	- fromDate: "2026-01-01"
	- toDate: "2026-01-07"
	- companyId: 100
	- pickupPointId: null
	- categoryId: null
- Output summary:
	- totalOrders: 420
	- totalRevenue: 350_000_000
	- totalItems: 1_800

---

### FR-REPORTING-002 — Báo cáo hoa hồng theo tầng và ngành hàng

**Mô tả**
- Hệ thống phải cung cấp báo cáo hoa hồng cho **Tầng 1 và Tầng 2** theo ngành hàng và khoảng thời gian, dựa trên mô hình Affiliate 3 tầng.
- Báo cáo này được dùng cho:
	- Tầng 2 xem hoa hồng/chiết khấu cá nhân (khi được phép).
	- Tầng 1 xem tổng quan hoa hồng mạng lưới (read-only trong MVP).
	- Super Admin/Finance kiểm tra tổng hoa hồng phải trả.

**Luồng chuẩn**
1. Finance/Super Admin chọn khoảng thời gian và phạm vi (toàn hệ thống hoặc Công ty cụ thể).
2. Hệ thống tổng hợp hoa hồng theo rule tính hoa hồng được định nghĩa trong module PAYMENTS/Business-Rules (chi tiết tính % theo ngành hàng, lịch sử hiệu lực nằm ngoài REPORTING).
3. Hệ thống hiển thị bảng hoa hồng theo:
	- Người nhận hoa hồng (Tầng 1/Tầng 2).
	- Ngành hàng.
	- Kỳ/ khoảng thời gian.

**Tiêu chí kiểm thử (testable)**
- AC1: Tổng hoa hồng hiển thị trong báo cáo phải bằng tổng hoa hồng từ các giao dịch/đơn hàng trong khoảng thời gian đó, theo rule tính toán đã được xác nhận ở PAYMENTS.
- AC2: Khi filter theo Tầng 2 cụ thể, chỉ hiển thị hoa hồng thuộc về Tầng 2 đó, không bao gồm tầng khác.
- AC3: Khi filter theo ngành hàng, chỉ các bản ghi hoa hồng thuộc ngành hàng đó được tính vào tổng.

**Ví dụ dữ liệu (fake)**
- Kỳ báo cáo: 2026-01 (01/01/2026–31/01/2026)
- Bản ghi hoa hồng Tầng 2:
	- affiliateId: "T2_001"
	- affiliateName: "Nguyễn Văn T2A"
	- categoryName: "Thực phẩm khô"
	- totalRevenue: 50_000_000
	- commissionRate: 0.05
	- commissionAmount: 2_500_000

---

### FR-REPORTING-003 — Báo cáo COD & đối soát theo Ca

**Mô tả**
- Hệ thống phải cung cấp báo cáo tổng hợp **COD đã thu, COD tồn, sai lệch** theo Ca Shipper/Tầng 2, theo Công ty/Điểm nhận.
- Báo cáo phục vụ Finance và Ops trong việc kiểm soát COD.

**Luồng chuẩn**
1. Finance chọn khoảng thời gian và phạm vi (Công ty, Shipper/Tầng 2, Điểm nhận...).
2. Hệ thống lấy dữ liệu từ PAYMENTS (Shift, Receipt, Reconciliation) và ORDERS để tổng hợp:
	- expectedCod (tổng COD cần thu theo đơn).
	- collectedCod (tổng theo Phiếu thu).
	- difference (expectedCod - collectedCod).
3. Hệ thống hiển thị danh sách Ca với các số liệu trên, cùng trạng thái đối soát (đã đối soát, sai lệch...).

**Tiêu chí kiểm thử**
- AC1: Đối với một Ca cụ thể, expectedCod trong báo cáo phải bằng tổng COD cần thu của tất cả đơn COD thuộc Ca đó (theo PAYMENTS/ORDERS).
- AC2: collectedCod trong báo cáo phải bằng tổng amount của Phiếu thu (receipt) gắn với Ca đó.
- AC3: difference = expectedCod - collectedCod trùng khớp với difference được lưu trong module PAYMENTS.

**Ví dụ dữ liệu (fake)**
- Ca:
	- shiftId: 30001
	- shipperId: "shipper_200"
	- expectedCod: 1_500_000
	- collectedCod: 1_480_000
	- difference: -20_000
	- status: "DIFFERENCE"

---

### FR-REPORTING-004 — Báo cáo công nợ trả chậm (credit/debt)

**Mô tả**
- Hệ thống phải hiển thị báo cáo công nợ trả chậm trên phạm vi toàn hệ thống hoặc theo Công ty, bao gồm:
	- Tổng nợ trong hạn.
	- Tổng nợ quá hạn.
	- Chi tiết theo User (Tầng 3) nếu được phân quyền.
- Báo cáo dựa trên dữ liệu từ PAYMENTS (DebtBalance, DebtTransaction, CreditLimit) và USERS.

**Luồng chuẩn**
1. Finance/Super Admin chọn phạm vi (toàn hệ thống hoặc Công ty cụ thể) và ngày chốt báo cáo.
2. Hệ thống truy vấn snapshot công nợ tại thời điểm đó (hoặc tính toán dựa trên bút toán nợ/thu nợ đến thời điểm đó; chi tiết thiết kế do Tech Lead xác định).
3. Hệ thống hiển thị tổng nợ, nợ trong hạn, nợ quá hạn, và có thể drill down theo Công ty và/hoặc User.

**Tiêu chí kiểm thử**
- AC1: Với một User cụ thể, số nợ hiển thị trong báo cáo phải bằng DebtBalance trong PAYMENTS tại thời điểm báo cáo.
- AC2: Tổng nợ toàn hệ thống bằng tổng cộng tất cả DebtBalance của User nằm trong phạm vi filter.
- AC3: Phân loại trong hạn/quá hạn phải dựa trên ngày đến hạn được định nghĩa trong PAYMENTS (ví dụ: ngày lương); rule chi tiết không được định nghĩa ở REPORTING.

**Ví dụ dữ liệu (fake)**
- Công ty A:
	- totalDebt: 200_000_000
	- inTermDebt: 150_000_000
	- overdueDebt: 50_000_000
- User level (nếu drill down):
	- userId: 10030
	- fullName: "Phạm Văn D"
	- companyName: "Công ty May KCN A"
	- debtBalance: 5_000_000
	- status: "OVERDUE"

---

### FR-REPORTING-005 — Cung cấp số liệu cho Dashboard Admin

**Mô tả**
- REPORTING phải cung cấp các số liệu tổng hợp cho Dashboard Admin Portal (xem SRS-ADMIN FR-ADMIN-004), bao gồm tối thiểu:
	- Đơn hôm nay, tỷ lệ nhận hàng.
	- COD tồn.
	- Công nợ trả chậm.
	- Top ngành hàng/bán chạy.
	- Cảnh báo quá hạn (đơn quá hạn nhận, nợ quá hạn...).

**Luồng chuẩn**
1. Admin truy cập Dashboard; ADMIN gọi các API: TBD (Tech Lead) của REPORTING.
2. REPORTING trả về các chỉ số tổng hợp theo khoảng thời gian mặc định (ví dụ: hôm nay, 7 ngày gần nhất) hoặc theo filter được truyền vào.
3. Dashboard hiển thị các chỉ số theo card/widget.

**Tiêu chí kiểm thử**
- AC1: Số liệu trên Dashboard (ví dụ tổng đơn hôm nay) phải khớp với số liệu cùng kỳ trên báo cáo chi tiết tương ứng (FR-REPORTING-001/003/004).
- AC2: Khi filter Dashboard theo Công ty, chỉ số hiển thị phải thay đổi tương ứng.
- AC3: Trong trường hợp lỗi khi lấy số liệu cho một widget, widget đó phải trả về thông tin lỗi phù hợp thay vì làm hỏng toàn bộ Dashboard.

**Ví dụ dữ liệu (fake)**
- Dashboard summary (hôm nay):
	- totalOrdersToday: 300
	- receivedRateToday: 0.9
	- totalCodOpen: 70_000_000
	- totalDebtOverdue: 60_000_000
	- topCategory: "Đồ gia dụng"

---

## 4. Business rules references (BR-###)

> Các BR tính toán chi tiết (hoa hồng, công nợ, chọn trạng thái đơn để tính doanh thu) thuộc các module nguồn (ORDERS, PAYMENTS, USERS, CATALOG); REPORTING chỉ đọc và tổng hợp dựa trên dữ liệu đó.

- **BR-REPORTING-001 — Không tự thay đổi logic nghiệp vụ nguồn**
	- REPORTING **không** tự đặt ra rule tính toán riêng, mà phải tuân thủ rule chính thức ở ORDERS/PAYMENTS (ví dụ: đơn nào được coi là doanh thu, cách tính hoa hồng, khi nào công nợ được coi là quá hạn).

- **BR-REPORTING-002 — Tránh đếm trùng doanh số và hoa hồng**
	- Khi tổng hợp doanh số, đơn "Đã hủy" hoặc đơn được đổi trả phải được xử lý theo rule toàn cục (ví dụ: loại khỏi doanh thu, điều chỉnh doanh thu kỳ trước...). Chi tiết rule này thuộc ORDERS/PAYMENTS; REPORTING phải tuân thủ.

- **BR-REPORTING-003 — Phạm vi dữ liệu theo quyền**
	- Tầng 1/Tầng 2 chỉ xem được báo cáo trong phạm vi mạng lưới/hoa hồng của mình (theo cấu trúc Ref Tier ở USERS).
	- Finance/Ops/Super Admin có thể xem phạm vi rộng hơn theo phân quyền ở ADMIN.

Chi tiết mã BR-xxx sẽ được liệt kê tập trung trong Business-Rules.md; REPORTING chỉ tham chiếu và không định nghĩa lại.

---

## 5. Data inputs/outputs (conceptual)

### 5.1. Nguồn dữ liệu (inputs)

- Từ **ORDERS**:
	- Thông tin đơn hàng (orderId, orderCode, companyId, pickupPointId, orderStatus, createdAt, confirmedAt, receivedAt...).
	- Số dòng sản phẩm (order items) với productId, quantity, price, categoryId.

- Từ **PAYMENTS**:
	- Payment (paymentMethod, paymentStatus, amount...).
	- Receipt (Phiếu thu COD) theo đơn, theo Ca.
	- Shift (Ca COD) và Reconciliation (kết quả đối soát).
	- CreditLimit, DebtBalance, DebtTransaction.

- Từ **USERS**:
	- User (tier, refTier, companyId...).
	- Thông tin Tầng 1/Tầng 2/Tầng 3 để gắn hoa hồng và báo cáo mạng lưới.

- Từ **CATALOG**:
	- Product, Category, thông tin ngành hàng.

### 5.2. Đầu ra (outputs)

- **SalesReport** (khái niệm):
	- Chỉ số tổng hợp: totalOrders, totalRevenue, totalItems, breakdown theo dimension.

- **CommissionReport**:
	- Chỉ số: commissionAmount per affiliate (Tầng 1/Tầng 2), per category, per period.

- **CodShiftReport**:
	- expectedCod, collectedCod, difference, status per shift.

- **DebtReport**:
	- totalDebt, inTermDebt, overdueDebt per company, per user.

- **DashboardMetrics**:
	- Tổng hợp nhiều chỉ số để hiển thị trên Dashboard.

Chi tiết physical schema (bảng fact/dimension, index) sẽ được thiết kế trong DB-REPORTING theo DB-Design-Rules.

---

## 6. Validations & edge cases

### 6.1. Validations

- **V-REPORTING-001 – Khoảng thời gian truy vấn**
	- fromDate <= toDate.
	- Khoảng thời gian không vượt quá giới hạn tối đa cho phép trong một truy vấn (ví dụ: N tháng; N cụ thể sẽ do Tech Lead/PO chốt).

- **V-REPORTING-002 – Phân quyền phạm vi dữ liệu**
	- Người dùng chỉ được xem dữ liệu trong phạm vi Công ty/mạng lưới/tầng mà mình có quyền.

- **V-REPORTING-003 – Tham chiếu dữ liệu nguồn**
	- Mọi bản ghi hiển thị trong báo cáo phải tham chiếu tới thực thể nguồn còn tồn tại (đơn hàng, user, ca...). Nếu dữ liệu nguồn bị xoá logic, báo cáo vẫn dùng dữ liệu nhưng tuân thủ quy định hiển thị (ví dụ: ẩn bớt chi tiết, chỉ hiển thị mã định danh).

### 6.2. Edge cases

1. **E-REPORTING-001 – Số liệu không khớp do dữ liệu nguồn đang cập nhật**
	- Mô tả: Finance mở báo cáo trong khi một batch job hoặc quy trình đồng bộ vừa mới cập nhật dữ liệu công nợ/hoa hồng.
	- Mong đợi: Có thể chấp nhận chênh lệch nhỏ tạm thời; cần rule về thời điểm chốt dữ liệu báo cáo (end-of-day, end-of-period) – sẽ được xác định trong thiết kế/Business-Rules.

2. **E-REPORTING-002 – Đơn bị huỷ hoặc đổi trả sau khi báo cáo đã được xuất**
	- Mô tả: Một đơn đã được tính vào doanh thu kỳ trước nhưng sau đó bị huỷ/đổi trả.
	- Mong đợi: Cần rule nghiệp vụ rõ (ví dụ: tạo bút toán điều chỉnh kỳ sau); REPORTING phải hỗ trợ xem báo cáo sau điều chỉnh, nhưng chi tiết logic thuộc ORDERS/PAYMENTS.

3. **E-REPORTING-003 – Công nợ âm hoặc số dư hoa hồng âm**
	- Mô tả: Do điều chỉnh hoặc trả lại hoa hồng, một số User có số dư hoa hồng/công nợ âm.
	- Mong đợi: Báo cáo vẫn hiển thị số âm, nhưng cần rule rõ về cách hiểu và hiển thị (ví dụ: màu khác, cột riêng) – TBD.

4. **E-REPORTING-004 – Truy vấn với filter rất rộng**
	- Mô tả: Super Admin truy vấn doanh số toàn hệ thống trong khoảng thời gian dài (nhiều tháng/năm), dẫn tới tập dữ liệu lớn.
	- Mong đợi: Hệ thống áp dụng giới hạn (như cảnh báo hoặc cắt nhỏ theo trang) để tránh quá tải, theo NFR hiệu năng toàn hệ thống.

5. **E-REPORTING-005 – Thiếu dữ liệu liên kết (orphan)**
	- Mô tả: Một số bản ghi Payment hoặc Order có dữ liệu không nhất quán (thiếu liên kết đến User hoặc Company) do lỗi nhập liệu/đồng bộ.
	- Mong đợi: REPORTING có thể loại trừ các bản ghi này khỏi báo cáo tổng hợp, hoặc gom vào nhóm "Dữ liệu lỗi"; rule chi tiết sẽ do Business-Rules quy định.

---

## 7. Non-functional requirements (module-specific)

- **Hiệu năng**
	- Các báo cáo thường xuyên dùng (trang Dashboard, báo cáo doanh số theo ngày) phải trả kết quả trong khoảng thời gian hợp lý (ví dụ: p95 < 3 giây dưới tải bình thường; con số chính xác lấy theo NFR toàn hệ thống).
	- Truy vấn trên tập dữ liệu lớn phải sử dụng index và/hoặc pre-aggregation (materialized view, bảng fact) theo thiết kế DB-REPORTING.

- **Khả dụng**
	- Lỗi ở REPORTING không được ảnh hưởng tới các luồng giao dịch chính (đặt hàng, thanh toán). Nếu báo cáo không truy vấn được, người dùng vẫn có thể thực hiện giao dịch.

- **Bảo mật & phân quyền**
	- Áp dụng RBAC; người dùng chỉ xem được báo cáo trong phạm vi quyền.
	- Không hiển thị PII không cần thiết (ví dụ: CCCD, thông tin KYC nhạy cảm) trong báo cáo; chỉ hiển thị mã hoặc thông tin đã mask theo Data-Classification.

- **Audit & logging**
	- Các truy vấn báo cáo lớn hoặc export dữ liệu phải được log với correlationId và thông tin actor để phục vụ audit.
	- Không log chi tiết dữ liệu nhạy cảm trong log hệ thống.

---

## 8. Open Questions / Assumptions for this module

**Câu hỏi mở**

- **Q-REPORTING-001** – Các chỉ số chính xác cần có trong từng báo cáo (cột, công thức tính) cho từng vai trò (Super Admin, Finance, Ops, Tầng 1, Tầng 2) là gì? (hiện mới có mô tả high-level trong MVP-Scope).
- **Q-REPORTING-002** – Có yêu cầu export dữ liệu báo cáo ra CSV/Excel/PDF trực tiếp từ hệ thống không? Nếu có, giới hạn dung lượng/file là bao nhiêu?
- **Q-REPORTING-003** – Chu kỳ kết toán/đóng sổ cho công nợ và hoa hồng là theo tháng lịch, theo chu kỳ lương hay theo cấu hình khác?
- **Q-REPORTING-004** – Đối với đơn bị huỷ/đổi trả sau kỳ báo cáo, khách hàng muốn xử lý điều chỉnh như thế nào trong báo cáo (retroactive hay điều chỉnh kỳ sau)?
- **Q-REPORTING-005** – Có yêu cầu báo cáo chi tiết cho Tầng 1/Tầng 2 trên portal riêng (ngoài Admin Portal) hay chỉ một số chỉ số tổng quan được embedding?

**Giả định (Assumptions)**

- **A-REPORTING-001** – REPORTING là module **chỉ đọc** dữ liệu từ các module khác; mọi thay đổi nghiệp vụ (huỷ đơn, điều chỉnh hoa hồng, điều chỉnh công nợ) đều thực hiện tại ORDERS/PAYMENTS/USERS.
- **A-REPORTING-002** – Các báo cáo MVP tập trung vào vận hành (doanh số, hoa hồng, COD, công nợ) như mô tả trong MVP-Scope; các báo cáo tài chính nâng cao và dashboard chi tiết Tầng 1 sẽ được xử lý ở giai đoạn sau.
- **A-REPORTING-003** – Tất cả số liệu báo cáo sử dụng cùng timezone và currency với hệ thống giao dịch (timestamptz UTC lưu trong DB, hiển thị theo timezone khách hàng được quyết định ở tầng UI/Backend chung, không riêng REPORTING).
