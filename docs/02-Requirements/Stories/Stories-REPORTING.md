
# Stories — REPORTING: Reporting & Exports

## Định dạng chung
- Mỗi story tuân theo format:
	- User story (As a / I want / So that).
	- Priority (P0/P1/P2) theo phạm vi MVP.
	- Definition of Ready (DoR) checklist.
	- Acceptance Criteria (AC) đo lường được.
	- Sample data (fake) theo dạng entity hoặc request/response.
	- Ít nhất 3 edge cases.
	- API mapping: "TBD (Tech Lead)" (không liệt kê path cụ thể).
	- Screens: danh sách màn hình nếu đã biết, hoặc `TBD`.

## Story list

---

### US-REPORTING-001 — Xem báo cáo doanh số theo thời gian và Công ty
**As a** Super Admin/Ops/Finance  
**I want** xem tổng hợp doanh số đơn hàng theo khoảng thời gian và Công ty  
**So that** tôi có thể đánh giá hiệu quả bán hàng và so sánh giữa các đơn vị

**Priority:** P0  
**Dependencies:**
- SRS-REPORTING: FR-REPORTING-001  
- SRS-ORDERS (trạng thái đơn, doanh thu)  
- SRS-CATALOG (ngành hàng)

**Definition of Ready**
- [ ] Danh sách trạng thái đơn được tính vào doanh thu (ví dụ: Đã nhận, Đã hoàn tất; loại trừ Đã hủy) được chốt trong ORDERS.
- [ ] Định nghĩa doanh thu tính theo giá bán hay giá sau chiết khấu được chốt trong Business-Rules (ORDERS/PAYMENTS).
- [ ] Thiết kế filter tối thiểu (fromDate, toDate, company, optional: pickupPoint, category) được phê duyệt.

**Acceptance Criteria**
- AC1: Khi chọn fromDate/toDate và một companyId hợp lệ, hệ thống hiển thị bảng doanh số với ít nhất các cột: ngày/khoảng, totalOrders, totalRevenue, totalItems.
- AC2: Tổng totalRevenue trên màn hình phải bằng tổng doanh thu của tất cả đơn thuộc phạm vi filter và trạng thái được chốt ở ORDERS.
- AC3: Khi thêm filter ngành hàng hoặc Điểm nhận, số liệu cập nhật tương ứng và không hiển thị đơn ngoài phạm vi filter.
- AC4: Danh sách chi tiết (nếu có) được phân trang theo page/pageSize, không tải toàn bộ dữ liệu nếu số bản ghi > pageSize.

**Sample data (fake)**
- Input filter:
	- fromDate: "2026-01-01"
	- toDate: "2026-01-07"
	- companyId: 100
	- pickupPointId: null
	- categoryId: null
- Output summary row:
	- period: "2026-01-01" → "2026-01-07"
	- totalOrders: 420
	- totalRevenue: 350_000_000
	- totalItems: 1_800

**Edge cases (min 3)**
- EC1: fromDate > toDate → Hệ thống trả về lỗi validation rõ ràng, không truy vấn dữ liệu.
- EC2: Không có đơn nào trong khoảng thời gian/filter → Hệ thống hiển thị 0 cho tất cả totals và bảng rỗng, không lỗi.
- EC3: Khoảng thời gian quá rộng (ví dụ > 1 năm) → Hệ thống cảnh báo/thực hiện giới hạn thời gian tối đa cho một truy vấn (policy TBD), không cho phép truy vấn gây nghẽn.
- EC4: CompanyId không tồn tại hoặc User không có quyền xem Công ty đó → Không trả về dữ liệu, trả lỗi 403/Not authorized theo chuẩn API.

**API mapping**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình "Báo cáo doanh số" trên Admin Portal)

---

### US-REPORTING-002 — Xem báo cáo doanh số theo ngành hàng
**As a** Ops/Finance  
**I want** xem doanh số phân theo ngành hàng trong một khoảng thời gian  
**So that** tôi có thể nhận biết ngành hàng nào bán tốt và ưu tiên hỗ trợ

**Priority:** P1  
**Dependencies:**
- SRS-REPORTING: FR-REPORTING-001  
- SRS-CATALOG (category hierarchy)  
- SRS-ORDERS (order items)

**Definition of Ready**
- [ ] Danh sách ngành hàng (category) và cách nhóm (cha/con) được chốt.
- [ ] Quy tắc tính doanh thu theo ngành hàng (theo dòng sản phẩm hay theo đơn) được mô tả high-level.

**Acceptance Criteria**
- AC1: Khi chọn fromDate/toDate và (optionally) companyId, hệ thống hiển thị bảng với ít nhất các cột: categoryName, totalOrders, totalItems, totalRevenue.
- AC2: Mỗi đơn có nhiều ngành hàng được phân bổ sang từng ngành tương ứng theo dòng sản phẩm (rule chi tiết thuộc ORDERS/CATALOG nhưng kết quả aggregate phải khớp).
- AC3: Tổng totalRevenue theo tất cả category trong báo cáo bằng totalRevenue của báo cáo doanh số cùng phạm vi filter (US-REPORTING-001).

**Sample data (fake)**
- Input filter:
	- fromDate: "2026-01-01"
	- toDate: "2026-01-31"
	- companyId: 100
- Output rows (sample):
	- { categoryName: "Thực phẩm khô", totalOrders: 200, totalItems: 900, totalRevenue: 120_000_000 }
	- { categoryName: "Đồ gia dụng", totalOrders: 150, totalItems: 600, totalRevenue: 90_000_000 }

**Edge cases (min 3)**
- EC1: Sản phẩm không gắn ngành hàng (categoryId null) → Hệ thống hoặc bỏ qua, hoặc gắn vào nhóm "Khác" theo policy, nhưng phải rõ ràng trong UI.
- EC2: Cấu trúc ngành hàng thay đổi trong kỳ (chuyển từ ngành A sang B) → Cách tính/hiển thị thuộc Business-Rules, story này giả định dữ liệu nguồn đã phản ánh đúng.
- EC3: Người dùng filter một ngành hàng cụ thể nhưng không có doanh thu trong kỳ → Hệ thống hiển thị bảng rỗng/0, không lỗi.

**API mapping**
- TBD (Tech Lead)

**Screens**
- TBD (tab/biểu đồ ngành hàng trong màn hình Báo cáo doanh số)

---

### US-REPORTING-003 — Xem báo cáo hoa hồng theo Tầng 2
**As a** Finance/Super Admin  
**I want** xem báo cáo hoa hồng phải trả cho từng Tầng 2 trong một kỳ  
**So that** tôi có thể đối soát tổng chi phí hoa hồng và chuẩn bị chi trả

**Priority:** P0  
**Dependencies:**
- SRS-REPORTING: FR-REPORTING-002  
- SRS-PAYMENTS (logic tính hoa hồng, kỳ hoa hồng)  
- SRS-USERS (profile Tầng 2)

**Definition of Ready**
- [ ] Kỳ hoa hồng (theo tháng/tuần/chu kỳ khác) được chốt ở PAYMENTS.
- [ ] Rule tính hoa hồng từng Tầng 2 (tỷ lệ, trần, sàn) được đặc tả ở Business-Rules, không nằm trong REPORTING.

**Acceptance Criteria**
- AC1: Khi chọn một kỳ hoa hồng, hệ thống hiển thị danh sách Tầng 2 với các chỉ số: totalRevenue, commissionRate (hoặc nhóm rule), commissionAmount.
- AC2: Tổng commissionAmount của tất cả Tầng 2 trong kỳ phải bằng tổng hoa hồng tính từ các giao dịch/đơn thuộc kỳ đó trong PAYMENTS.
- AC3: Khi filter theo một Tầng 2 cụ thể, chỉ hiển thị bản ghi thuộc Tầng 2 đó, không lẫn dữ liệu của người khác.

**Sample data (fake)**
- Kỳ: 2026-01  
- Sample row:
	- affiliateId: "T2_001"
	- affiliateName: "Nguyễn Văn T2A"
	- totalRevenue: 50_000_000
	- commissionRate: 0.05
	- commissionAmount: 2_500_000

**Edge cases (min 3)**
- EC1: Không có giao dịch nào tạo hoa hồng trong kỳ → Báo cáo vẫn hiển thị được (có thể bảng rỗng, tổng = 0).
- EC2: Một số giao dịch trong kỳ bị điều chỉnh/huỷ sau khi đã chốt kỳ hoa hồng → Cách xử lý (điều chỉnh kỳ sau hay cập nhật lại) thuộc PAYMENTS; story này giả định số liệu đã ổn định tại thời điểm báo cáo.
- EC3: Một Tầng 2 thay đổi trạng thái (ngưng hợp tác) giữa kỳ → Vẫn hiển thị hoa hồng đã phát sinh trong phạm vi kỳ đó, không mất dữ liệu.

**API mapping**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình "Báo cáo hoa hồng" trong Admin Portal)

---

### US-REPORTING-004 — Xem báo cáo COD & đối soát theo Ca
**As a** Finance  
**I want** xem các Ca COD đã chốt cùng số tiền cần thu, đã thu, chênh lệch  
**So that** tôi có thể kiểm soát việc thu COD và xử lý sai lệch

**Priority:** P0  
**Dependencies:**
- SRS-REPORTING: FR-REPORTING-003  
- SRS-PAYMENTS (Ca COD, Phiếu thu, đối soát)  
- SRS-ORDERS (COD theo đơn)

**Definition of Ready**
- [ ] Danh sách trạng thái Ca (OPEN, CLOSED, RECONCILED, DIFFERENCE, v.v.) được chốt ở PAYMENTS.
- [ ] Quy tắc khi nào một Ca được coi là đã đối soát/hoàn tất được mô tả high-level.

**Acceptance Criteria**
- AC1: Khi chọn fromDate/toDate và (optional) companyId/shipperId, hệ thống hiển thị mỗi Ca với: shiftId, shipper, expectedCod, collectedCod, difference, reconciliationStatus.
- AC2: expectedCod, collectedCod, difference trong báo cáo phải khớp với dữ liệu tương ứng trong PAYMENTS cho Ca đó.
- AC3: Finance có thể lọc danh sách Ca theo trạng thái (ví dụ: chỉ xem Ca DIFFERENCE) để tập trung xử lý.

**Sample data (fake)**
- Sample row:
	- shiftId: 30001
	- shipperId: "shipper_200"
	- expectedCod: 1_500_000
	- collectedCod: 1_480_000
	- difference: -20_000
	- reconciliationStatus: "DIFFERENCE"

**Edge cases (min 3)**
- EC1: Ca chưa được Shipper chốt (status OPEN) → Hoặc không hiển thị ở báo cáo này, hoặc hiển thị nhưng rõ trạng thái; không tính vào tổng COD đã chốt.
- EC2: Một Ca có số lượng đơn rất lớn (hàng ngàn đơn) → UI chỉ hiển thị tổng hợp Ca; chi tiết đơn có thể xem ở màn khác/khác story để tránh quá tải.
- EC3: Dữ liệu một Ca bị thiếu (thiếu Phiếu thu hoặc thiếu liên kết với đơn) do lỗi trước đó → Story này giả định dữ liệu đầu vào đã được đảm bảo; các lỗi dữ liệu sẽ được xử lý trong phạm vi khác.

**API mapping**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình "Báo cáo COD theo Ca" trong Admin Portal)

---

### US-REPORTING-005 — Xem báo cáo công nợ trả chậm theo Công ty
**As a** Finance/Super Admin  
**I want** xem tổng công nợ trả chậm, nợ trong hạn, nợ quá hạn theo từng Công ty  
**So that** tôi có thể kiểm soát rủi ro tín dụng và lập kế hoạch thu nợ

**Priority:** P0  
**Dependencies:**
- SRS-REPORTING: FR-REPORTING-004  
- SRS-PAYMENTS (DebtBalance, DebtTransaction, CreditLimit)  
- SRS-USERS (User, Công ty)

**Definition of Ready**
- [ ] Cách xác định ngày chốt báo cáo (as-of date) được chốt.
- [ ] Rule phân loại trong hạn/quá hạn dựa trên ngày đến hạn được định nghĩa trong PAYMENTS.

**Acceptance Criteria**
- AC1: Với một asOfDate và phạm vi toàn hệ thống, báo cáo hiển thị mỗi Công ty với: totalDebt, inTermDebt, overdueDebt.
- AC2: Tổng totalDebt của tất cả Công ty phải bằng tổng tất cả DebtBalance thuộc phạm vi filter trong PAYMENTS tại thời điểm asOfDate.
- AC3: Khi chọn một Công ty cụ thể, người dùng có thể drill down để xem danh sách User và debtBalance tương ứng (nếu được phân quyền).

**Sample data (fake)**
- Company-level row:
	- companyId: 200
	- companyName: "Công ty May KCN A"
	- totalDebt: 200_000_000
	- inTermDebt: 150_000_000
	- overdueDebt: 50_000_000
- User-level row (drill down):
	- userId: 10030
	- fullName: "Phạm Văn D"
	- debtBalance: 5_000_000
	- status: "OVERDUE"

**Edge cases (min 3)**
- EC1: Công ty không có công nợ tại asOfDate → Vẫn hiển thị (tuỳ yêu cầu) với totalDebt = 0, hoặc không hiển thị trong danh sách; phải thống nhất cách thể hiện.
- EC2: Có User có debtBalance âm (do bút toán điều chỉnh) → Cách hiển thị/tính tổng cần tuân theo rule trong PAYMENTS (ví dụ tách thành cột Adjustments).
- EC3: Một số User thuộc Công ty đã ngưng hợp tác → Vẫn hiển thị công nợ nếu còn dư nợ, có thể kèm cờ "Inactive".

**API mapping**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình "Báo cáo công nợ" trong Admin Portal)

---

### US-REPORTING-006 — Cung cấp số liệu cho Dashboard Admin
**As a** Admin user (Super Admin/Ops/Finance)  
**I want** xem nhanh các chỉ số chính trên Dashboard (đơn hôm nay, tỷ lệ nhận hàng, COD tồn, nợ quá hạn, top ngành hàng)  
**So that** tôi có thể nắm tình hình tổng quan mà không cần mở từng báo cáo chi tiết

**Priority:** P0  
**Dependencies:**
- SRS-REPORTING: FR-REPORTING-005  
- SRS-ADMIN: FR-ADMIN-004 (Dashboard shell)  
- SRS-ORDERS, SRS-PAYMENTS, SRS-USERS, SRS-CATALOG (nguồn dữ liệu)

**Definition of Ready**
- [ ] Danh sách KPI tối thiểu trên Dashboard (tên, cách tính high-level) được chốt.
- [ ] Tần suất cập nhật số liệu (real-time, gần real-time, theo batch) được mô tả.

**Acceptance Criteria**
- AC1: Khi Admin mở Dashboard, hệ thống hiển thị ít nhất các chỉ số: totalOrdersToday, receivedRateToday, totalCodOpen, totalDebtOverdue, topCategoryName.
- AC2: totalOrdersToday và receivedRateToday khớp với số liệu chi tiết từ báo cáo doanh số cùng ngày (US-REPORTING-001) và trạng thái đơn ở ORDERS.
- AC3: totalCodOpen và totalDebtOverdue khớp với số liệu tổng từ báo cáo COD (US-REPORTING-004) và công nợ (US-REPORTING-005) trong cùng khoảng thời gian.
- AC4: Nếu một widget không lấy được dữ liệu (lỗi nguồn), widget đó hiển thị trạng thái lỗi rõ ràng (ví dụ: "Không tải được dữ liệu") nhưng các widget khác vẫn hoạt động.

**Sample data (fake)**
- Dashboard snapshot (hôm nay):
	- totalOrdersToday: 300
	- receivedRateToday: 0.9
	- totalCodOpen: 70_000_000
	- totalDebtOverdue: 60_000_000
	- topCategoryName: "Đồ gia dụng"

**Edge cases (min 3)**
- EC1: Trong ngày chưa có đơn nào → totalOrdersToday = 0, receivedRateToday có thể hiển thị là 0 hoặc "N/A" (policy TBD), không lỗi.
- EC2: Một số nguồn dữ liệu chậm cập nhật (delay vài phút) → Dashboard có thể hiển thị thông tin "Cập nhật lần cuối lúc HH:MM" để tránh hiểu nhầm real-time.
- EC3: Người dùng filter Dashboard theo một Công ty không có dữ liệu → Tất cả KPI hiển thị 0 hoặc trạng thái rõ ràng, không lỗi.

**API mapping**
- TBD (Tech Lead)

**Screens**
- TBD (Dashboard Admin Portal — widget tổng quan)

---

### US-REPORTING-007 — Export dữ liệu báo cáo doanh số ở mức cơ bản
**As a** Finance/Ops  
**I want** tải xuống file CSV báo cáo doanh số theo filter đang chọn  
**So that** tôi có thể xử lý thêm (lọc, pivot) ngoài hệ thống khi cần

**Priority:** P2 (cân nhắc MVP)  
**Dependencies:**
- SRS-REPORTING: FR-REPORTING-001 (export là tính năng mở rộng, phụ thuộc quyết định scope)  
- Chính sách bảo mật dữ liệu và phân quyền ở ADMIN/SECURITY

**Definition of Ready**
- [ ] Quy định trường nào được export (không export PII nhạy cảm) được chốt.
- [ ] Giới hạn kích thước file/export (số dòng tối đa, cơ chế batch nếu cần) được xác định.

**Acceptance Criteria**
- AC1: Từ màn hình Báo cáo doanh số (US-REPORTING-001/002), người dùng có thể bấm "Export" để tải file CSV chứa dữ liệu theo đúng filter hiện tại.
- AC2: File CSV chỉ chứa các cột đã được phê duyệt (không bao gồm thông tin nhạy cảm như số điện thoại nếu bị hạn chế).
- AC3: Nếu số dòng dự kiến vượt giới hạn cho phép, hệ thống hiển thị cảnh báo/ từ chối export và gợi ý thu hẹp filter.

**Sample data (fake)**
- File CSV mẫu:
	- Columns: date, companyName, pickupPointName, categoryName, totalOrders, totalRevenue, totalItems
	- Row: 2026-01-01, "Công ty May KCN A", "Điểm nhận KCN A-1", "Thực phẩm khô", 30, 25_000_000, 120

**Edge cases (min 3)**
- EC1: Người dùng không có quyền export nhưng vẫn thấy nút → Nút export phải ẩn hoặc vô hiệu hoá, hoặc trả lỗi quyền nếu gọi API trực tiếp.
- EC2: Trình duyệt/người dùng huỷ tải file giữa chừng → Không ảnh hưởng đến dữ liệu trên hệ thống; lần sau vẫn export lại được.
- EC3: Server gặp lỗi trong quá trình tạo file export → Hệ thống log lỗi (không log dữ liệu nhạy cảm) và trả thông báo rõ ràng cho người dùng.

**API mapping**
- TBD (Tech Lead)

**Screens**
- TBD (nút "Export" trên màn hình Báo cáo doanh số)

