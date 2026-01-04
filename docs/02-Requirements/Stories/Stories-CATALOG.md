
# Stories — CATALOG: Catalog / Content

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

### US-CATALOG-001 — Quản lý danh sách Sản phẩm (Admin)
**As a** Admin  
**I want** tạo và cập nhật danh sách Sản phẩm với thông tin cơ bản  
**So that** hệ thống có dữ liệu sản phẩm chính xác để hiển thị cho người dùng đặt hàng

**Priority:** P0  
**Dependencies:**
- SRS-CATALOG: FR-CATALOG-001, FR-CATALOG-004  
- DB-CATALOG (bảng Product, Category, Supplier)

**Definition of Ready**
- [ ] Danh sách trường tối thiểu của Sản phẩm được chốt (tên, mã sản phẩm, mã vạch, ngành hàng, NCC, đơn vị tính, giá hiển thị...).
- [ ] Quy tắc mã định danh duy nhất (product_code / barcode / kết hợp với NCC) được xác nhận.
- [ ] Thiết kế UI danh sách và form chi tiết Sản phẩm được duyệt.

**Acceptance Criteria**
- AC1: Admin có thể tạo mới một Sản phẩm với đầy đủ trường bắt buộc; sau khi lưu, Sản phẩm xuất hiện trong danh sách quản trị.
- AC2: Nếu Admin cố tạo Sản phẩm trùng mã định danh đã tồn tại, hệ thống chặn và hiển thị lỗi rõ ràng.
- AC3: Admin có thể chỉnh sửa một số trường cho phép (tên, mô tả, giá hiển thị...) mà không làm mất liên kết tới Lô hàng/đơn hàng cũ.
- AC4: Admin có thể đánh dấu Sản phẩm là "Inactive" để không còn hiển thị cho End User (logic hiển thị thuộc FR-CATALOG-004).

**Sample data (fake)**
- Input (tạo mới):
	- productCode: "SP-MI-TOM-001"  
	- barCode: "8931234567890"  
	- name: "Mì gói siêu ngon"  
	- categoryCode: "FOOD-INSTANT"  
	- supplierCode: "NCC_TP_A"  
	- unit: "Gói"  
	- unitPrice: 6500  
	- isActive: true
- Output (entity simple):
	- productId: 1001  
	- ... (các trường ở trên)

**Edge cases (min 3)**
- EC1: Thiếu trường bắt buộc (ví dụ không chọn Ngành hàng) → Form báo lỗi và không cho lưu.
- EC2: Admin nhập giá bán âm hoặc bằng 0 → Form hiển thị lỗi validation, không lưu.
- EC3: Admin cố đổi mã sản phẩm sang một mã đã tồn tại → Hệ thống từ chối, hiển thị thông báo "Mã sản phẩm đã tồn tại".
- EC4: Admin chỉnh sửa Sản phẩm đã gắn với nhiều Lô → Thay đổi chỉ ảnh hưởng tới thông tin hiển thị, không huỷ liên kết tới Lô hiện hữu.

**API mapping**
- TBD (Tech Lead)

**Screens**
- TBD (dựa trên thiết kế Figma cho danh sách + chi tiết Sản phẩm)

---

### US-CATALOG-002 — Quản lý Ngành hàng (Admin)
**As a** Admin  
**I want** tạo và quản lý danh sách Ngành hàng  
**So that** sản phẩm được phân loại đúng, hỗ trợ lọc và tính hoa hồng

**Priority:** P0  
**Dependencies:**
- SRS-CATALOG: FR-CATALOG-002  
- Business-Rules (quy tắc hoa hồng theo ngành hàng)

**Definition of Ready**
- [ ] Cấu trúc cây Ngành hàng (nếu có cấp cha-con) được xác nhận hoặc quyết định dùng danh sách phẳng.
- [ ] Quy tắc đặt mã ngành (category_code) được thống nhất (độc nhất/toàn hệ thống, pattern ký tự...).
- [ ] UI danh sách/cấu hình Ngành hàng được thiết kế.

**Acceptance Criteria**
- AC1: Admin có thể tạo mới một Ngành hàng với tên và mã ngành hợp lệ; mã ngành không trùng.
- AC2: Admin có thể cập nhật tên/mô tả của Ngành hàng đang dùng mà không ảnh hưởng tới liên kết Sản phẩm.
- AC3: Admin không thể xoá một Ngành hàng nếu vẫn còn Sản phẩm đang tham chiếu; hệ thống trả về lỗi thân thiện.

**Sample data (fake)**
- Input:
	- categoryCode: "FOOD-INSTANT"  
	- name: "Mì ăn liền"  
	- description: "Các loại mì/miến ăn liền"
- Output (entity):
	- categoryId: 10  
	- ...

**Edge cases (min 3)**
- EC1: Thử tạo 2 Ngành hàng với cùng categoryCode → Bị chặn, thông báo lỗi "Mã ngành đã tồn tại".
- EC2: Thử xoá Ngành hàng đang được sử dụng bởi một hoặc nhiều Sản phẩm → Bị từ chối, hiển thị rõ lý do.
- EC3: Ngành hàng bị đánh dấu Inactive → Sản phẩm thuộc ngành đó có hiển thị hay không cần rule rõ ràng (Open Question), nhưng không được gây lỗi hệ thống.

**API mapping**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình cấu hình Ngành hàng trong Admin Portal)

---

### US-CATALOG-003 — Tạo và kiểm duyệt Lô hàng từ NCC
**As a** QC (Admin)  
**I want** tạo và kiểm duyệt các Lô hàng nhập từ NCC theo từng Ngành hàng  
**So that** chỉ hàng hoá đạt yêu cầu mới được bán cho công nhân

**Priority:** P0  
**Dependencies:**
- SRS-CATALOG: FR-CATALOG-003, BR-CATALOG-001, BR-CATALOG-003  
- DB-CATALOG (Batch, BatchItem)

**Definition of Ready**
- [ ] Danh sách tài liệu/ảnh bắt buộc cho mỗi Lô kiểm duyệt được khách hàng chốt (hóa đơn, phiếu nhập, ảnh sản phẩm...).
- [ ] Quy tắc trạng thái Lô (PENDING_REVIEW/APPROVED/REJECTED/...) và điều kiện chuyển đổi được mô tả.
- [ ] Mẫu file hoặc form nhập dữ liệu Lô (supplier, category, sản phẩm, số lượng) được định nghĩa.

**Acceptance Criteria**
- AC1: QC/Admin có thể tạo Lô mới, chọn đúng một Ngành hàng và gán nhiều Sản phẩm thuộc ngành đó với số lượng nhập cho mỗi Sản phẩm.
- AC2: Hệ thống không cho phép thêm sản phẩm không cùng Ngành hàng vào cùng một Lô.
- AC3: QC có thể đính kèm/ghi nhận đầy đủ tài liệu/ảnh bắt buộc trước khi chuyển Lô sang trạng thái "Đã kiểm duyệt".
- AC4: Khi Lô chuyển sang "Đã kiểm duyệt", tồn kho khả dụng của các Sản phẩm trong Lô được cập nhật tăng tương ứng.

**Sample data (fake)**
- Input (tóm tắt):
	- supplierCode: "NCC_TP_A"  
	- categoryCode: "FOOD-INSTANT"  
	- items: [ { productCode: "SP-MI-TOM-001", quantity: 1000 }, { productCode: "SP-MI-TOM-002", quantity: 800 } ]  
	- documents: ["/docs/bill_0123.pdf"]  
	- images: ["/images/lot_5001_1.jpg"]
- Output:
	- batchId: 5001  
	- status: "PENDING_REVIEW" → "APPROVED"

**Edge cases (min 3)**
- EC1: Gán vào Lô một Sản phẩm thuộc Ngành hàng khác → Hệ thống từ chối, hiển thị thông báo lỗi rõ ràng.
- EC2: Lô có sản phẩm nhưng thiếu tài liệu/ảnh bắt buộc khi QC cố gắng chuyển sang "Đã kiểm duyệt" → Bị chặn, hiển thị danh sách tài liệu còn thiếu.
- EC3: Sau khi Lô "Đã kiểm duyệt" mới phát hiện sai thông tin (ví dụ nhập sai số lượng) → Cần rule chỉnh sửa/điều chỉnh tồn kho (Open Question trong SRS-CATALOG), story này chỉ ghi nhận yêu cầu chặn hành vi sai lặp lại.
- EC4: Lô bị từ chối (REJECTED) sau khi đã nhập nhầm; đảm bảo tồn kho không bị cộng từ Lô đó.

**API mapping**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình danh sách Lô hàng + form chi tiết Lô, màn hình QC kiểm duyệt)

---

### US-CATALOG-004 — Hiển thị danh sách sản phẩm cho End User
**As a** User Tầng 3  
**I want** xem danh sách sản phẩm đang bán, theo Ngành hàng và tình trạng còn/hết hàng  
**So that** tôi có thể chọn mua sản phẩm phù hợp

**Priority:** P0  
**Dependencies:**
- SRS-CATALOG: FR-CATALOG-004, FR-CATALOG-005  
- SRS-ORDERS (tạo đơn từ sản phẩm)

**Definition of Ready**
- [ ] Thiết kế UI danh sách sản phẩm (grid/list, filters) được chốt.
- [ ] Quy tắc hiển thị sản phẩm hết hàng (ẩn hẳn hay hiển thị "Hết hàng") được khách hàng quyết định.
- [ ] Quy tắc phân trang mặc định và tối đa cho danh sách sản phẩm được xác định.

**Acceptance Criteria**
- AC1: Khi User truy cập danh sách sản phẩm, hệ thống chỉ hiển thị sản phẩm thuộc Lô ở trạng thái "Đã kiểm duyệt" và còn tồn kho khả dụng > 0 (hoặc theo policy hiển thị hết hàng nếu được chốt).
- AC2: Bộ lọc theo Ngành hàng hoạt động đúng: khi chọn một Ngành hàng, chỉ sản phẩm thuộc ngành đó được hiển thị.
- AC3: Kết quả danh sách sản phẩm được trả về theo format phân trang chuẩn { items, page, totalItems, totalPages }.

**Sample data (fake)**
- Request filters conceptual:
	- categoryCode: "FOOD-INSTANT"  
	- searchText: "mì"  
	- page: 1  
	- pageSize: 20
- Response conceptual:
	- items: [ { productId: 1001, name: "Mì gói siêu ngon", price: 6500, inStock: true }, { productId: 1002, name: "Mì ly hải sản", price: 12000, inStock: false } ]  
	- page: 1, totalItems: 2, totalPages: 1

**Edge cases (min 3)**
- EC1: Không có sản phẩm nào thuộc Ngành hàng/keyword filter → Danh sách rỗng, hiển thị thông báo "Không có sản phẩm phù hợp".
- EC2: Sản phẩm vừa hết hàng trong lúc User đang xem danh sách → Khi User mở chi tiết hoặc thêm vào giỏ, ORDERS/ CATALOG phải kiểm tra tồn kho lại và từ chối nếu không đủ.
- EC3: Dữ liệu sản phẩm không có ảnh → UI vẫn hoạt động, hiển thị placeholder thay vì lỗi.
- EC4: Một số sản phẩm thuộc Lô chưa kiểm duyệt → Không hiển thị, nhưng không gây lỗi hệ thống.

**API mapping**
- TBD (Tech Lead)

**Screens**
- TBD (màn hình danh sách sản phẩm + màn hình chi tiết sản phẩm trong End User Portal)

---

### US-CATALOG-005 — Tính và cung cấp tồn kho khả dụng cho ORDERS
**As a** hệ thống ORDERS  
**I want** truy vấn được tồn kho khả dụng cho mỗi sản phẩm  
**So that** tôi có thể quyết định cho phép hay từ chối tạo đơn hàng

**Priority:** P0  
**Dependencies:**
- SRS-CATALOG: FR-CATALOG-005, BR-CATALOG-004  
- SRS-ORDERS (giữ/giải phóng tồn kho theo đơn)

**Definition of Ready**
- [ ] Rule tính tồn kho khả dụng (từ tổng nhập trừ đi số đã giữ cho đơn) được chốt.
- [ ] Cơ chế đồng bộ/locking khi nhiều đơn cùng đặt một sản phẩm được thiết kế ở mức high level.

**Acceptance Criteria**
- AC1: Khi ORDERS hỏi tồn kho khả dụng cho một sản phẩm, CATALOG luôn trả về số lượng >= 0.
- AC2: Nếu ORDERS yêu cầu đặt số lượng lớn hơn tồn kho khả dụng, CATALOG/ORDERS phải từ chối với thông báo phù hợp.
- AC3: Khi Lô mới được duyệt hoặc đơn bị huỷ/hoàn tất, tồn kho khả dụng được cập nhật và có hiệu lực cho các request tiếp theo.

**Sample data (fake)**
- Input:
	- productId: 1001
- Output conceptual:
	- availableQuantity: 850

**Edge cases (min 3)**
- EC1: Nhiều request đặt hàng gần như đồng thời cho cùng một sản phẩm → Cần đảm bảo không oversell (logic chi tiết sẽ được thiết kế trong ORDERS, nhưng CATALOG không cho phép tồn kho âm).
- EC2: Lô liên quan bị chuyển trạng thái (ví dụ từ APPROVED sang REJECTED) trong khi ORDERS đang gọi tồn kho → Rule xử lý phải đảm bảo consistency (Open Question).
- EC3: Không có Lô nào được duyệt cho sản phẩm đó → availableQuantity = 0, không lỗi.

**API mapping**
- TBD (Tech Lead)

**Screens**
- N/A (luồng kỹ thuật giữa CATALOG và ORDERS; không có màn hình riêng)

