
# SRS — CATALOG: Catalog / Content

## 1. Module purpose & boundaries (in/out)

**Mục đích**
- Quản lý **Sản phẩm**, **Ngành hàng**, **Lô hàng/Lô kiểm duyệt** và **Tồn kho khả dụng** để phục vụ trải nghiệm đặt hàng của End User và vận hành nhập hàng từ NCC.
- Cung cấp dữ liệu sản phẩm đã được kiểm duyệt cho module **ORDERS** (tạo đơn), **PAYMENTS** (giá/bán), **REPORTING** (báo cáo doanh số, hoa hồng) và **ADMIN** (cấu hình). 

**Trong phạm vi (IN)**
- Quản lý danh sách Sản phẩm: tạo/cập nhật thông tin cơ bản (tên, mô tả, mã hàng/mã vạch, đơn vị tính, hình ảnh, giá bán hiển thị).
- Quản lý Ngành hàng: cấu trúc phân loại để tính hoa hồng và lọc sản phẩm.
- Quản lý Lô hàng/Lô kiểm duyệt: thông tin nhập hàng từ NCC, trạng thái kiểm duyệt (QC), tài liệu/ảnh chứng từ.
- Tính toán và cung cấp **tồn kho khả dụng** theo Sản phẩm (và, nếu được yêu cầu, theo Điểm nhận/Công ty – chi tiết TBD).

**Ngoài phạm vi (OUT)**
- Tính toán chi tiết hoa hồng theo ngành hàng và từng đơn (thuộc PAYMENTS/REPORTING).
- Quản lý hệ thống kho vật lý phức tạp, nhiều kho, nhiều vị trí (nếu có trong giai đoạn sau) – hiện chỉ cần mức tồn kho đủ dùng cho MVP.
- Quản lý nội dung marketing nâng cao (banner, landing page, khuyến mãi phức tạp) – chưa được mô tả trong Discovery.

## 2. Personas/roles involved

- **Tầng 3 (End User)**: 
	- Xem danh sách sản phẩm, chi tiết sản phẩm, tồn kho khả dụng (có/không còn hàng), giá bán.
- **Tầng 2**:
	- Xem danh sách sản phẩm để tư vấn cho Tầng 3 (giống view End User, có thể thêm thông tin ngành hàng/hoa hồng theo yêu cầu sau).
- **Tầng 1**:
	- Xem danh sách sản phẩm/nhóm ngành hàng liên quan đến mạng lưới của mình (MVP chủ yếu read-only).
- **QC (Admin)**:
	- Tạo/lập Lô hàng, kiểm duyệt Lô kiểm duyệt (review tài liệu, hình ảnh), chuyển trạng thái Lô.
- **Ops (Admin)**:
	- Sử dụng thông tin Lô/Sản phẩm để xuất kho, phối hợp với ORDERS.
- **NCC (qua Admin)**:
	- Không dùng hệ thống trực tiếp trong MVP; cung cấp thông tin sản phẩm và lô hàng qua Admin hoặc file import.

## 3. Functional requirements (FR-CATALOG-###)

### FR-CATALOG-001 — Quản lý danh sách Sản phẩm

**Mô tả**
- Hệ thống phải cho phép Admin quản lý danh sách Sản phẩm (tên, mã, mô tả, ngành hàng, NCC, hình ảnh cơ bản, giá bán hiển thị).

**Luồng chuẩn (Admin)**
1. Admin truy cập màn hình danh sách Sản phẩm.
2. Admin có thể tạo Sản phẩm mới với các trường tối thiểu: tên sản phẩm, mã sản phẩm/mã vạch (nếu có), ngành hàng, NCC, đơn vị tính, giá bán hiển thị.
3. Admin có thể cập nhật thông tin Sản phẩm (trừ các trường bị khoá theo Business-Rules như mã chính).
4. Sản phẩm phải gắn với ít nhất một ngành hàng hợp lệ.

**Điều kiện testable**
- Tạo mới Sản phẩm với dữ liệu hợp lệ → Sản phẩm xuất hiện trong danh sách cho End User (khi đã được gắn Lô kiểm duyệt/đã có tồn kho).
- Cập nhật Sản phẩm không được làm mất liên kết đến Lô hàng/đơn hàng trước đó.
- Không cho phép tạo 2 Sản phẩm cùng **mã định danh duy nhất** (ví dụ: mã sản phẩm/Mã vạch) trong cùng NCC (hoặc toàn hệ thống – TBD).

**Ví dụ dữ liệu (fake)**
- productId: 1001  
- productCode: "SP-MI-TOM-001"  
- barCode: "8931234567890"  
- name: "Mì gói siêu ngon"  
- category: "Mì ăn liền" (Ngành hàng)  
- supplierName: "NCC Thực phẩm A"  
- unitPrice: 6500  
- unit: "Gói"  
- status: "Active"

---

### FR-CATALOG-002 — Quản lý Ngành hàng

**Mô tả**
- Hệ thống phải cho phép Admin quản lý danh sách Ngành hàng để phân loại Sản phẩm và hỗ trợ tính hoa hồng.

**Luồng chuẩn (Admin)**
1. Admin xem danh sách Ngành hàng.
2. Admin có thể tạo mới Ngành hàng với tên, mã ngành và (nếu cần) mô tả.
3. Admin có thể cập nhật tên/mô tả ngành nhưng không được xoá Ngành hàng đang được dùng bởi Sản phẩm.

**Điều kiện testable**
- Ngành hàng mới được tạo có thể được gán cho Sản phẩm.
- Không thể xoá Ngành hàng nếu còn ít nhất một Sản phẩm đang tham chiếu.
- Thay đổi tên Ngành hàng không làm mất liên kết với Sản phẩm.

**Ví dụ dữ liệu (fake)**
- categoryId: 10  
- categoryCode: "FOOD-INSTANT"  
- name: "Mì ăn liền"  
- description: "Các loại mì/miến ăn liền"

---

### FR-CATALOG-003 — Quản lý Lô hàng và Lô kiểm duyệt

**Mô tả**
- Hệ thống phải cho phép QC/Admin tạo và quản lý thông tin Lô hàng nhập từ NCC và trạng thái kiểm duyệt trước khi mở bán.
- MVP: mỗi Lô chỉ chứa **một Ngành hàng** (theo Discovery).

**Luồng chuẩn (QC/Admin)**
1. QC/Admin tạo Lô mới với thông tin tối thiểu: NCC, Ngành hàng, danh sách Sản phẩm (hoặc tham chiếu sản phẩm đã có), số lượng nhập, ngày nhập, tài liệu/ảnh liên quan.
2. Lô ban đầu ở trạng thái "Chờ kiểm duyệt".
3. QC kiểm tra tài liệu, hình ảnh, thông tin sản phẩm; nếu đạt → chuyển Lô sang "Đã kiểm duyệt"; nếu không đạt → "Từ chối" hoặc yêu cầu bổ sung.
4. Chỉ khi Lô ở trạng thái "Đã kiểm duyệt", Sản phẩm trong Lô mới được phép hiển thị (và có tồn kho bán).

**Điều kiện testable**
- Lô mới phải ghi nhận được ngành hàng duy nhất và liên kết tới NCC.
- Không thể chuyển Lô sang trạng thái "Đã kiểm duyệt" nếu thiếu tài liệu bắt buộc (theo Business-Rules, TBD chi tiết danh mục tài liệu).
- Khi Lô ở trạng thái "Đã kiểm duyệt", tồn kho khả dụng cho các Sản phẩm trong Lô phải tăng tương ứng số lượng nhập (sau khi trừ các điều chỉnh khác nếu có).

**Ví dụ dữ liệu (fake)**
- batchId: 5001  
- supplierId: 300  
- categoryId: 10  
- status: "PENDING_REVIEW" / "APPROVED"  
- items: [ { productId: 1001, quantity: 1000 }, { productId: 1002, quantity: 800 } ]

---

### FR-CATALOG-004 — Cung cấp danh sách Sản phẩm cho End User

**Mô tả**
- Hệ thống phải cung cấp danh sách Sản phẩm để End User (Tầng 3) duyệt và chọn mua, với chỉ các Sản phẩm thuộc Lô đã kiểm duyệt và còn tồn kho.

**Luồng chuẩn (End User)**
1. User truy cập danh sách sản phẩm.
2. Hệ thống hiển thị các Sản phẩm theo bộ lọc (Ngành hàng, tên, v.v.) chỉ với Sản phẩm từ Lô "Đã kiểm duyệt" và tồn kho khả dụng > 0.
3. User có thể xem chi tiết sản phẩm (tên, mô tả, hình ảnh, giá, đơn vị, tình trạng có/không còn hàng).

**Điều kiện testable**
- Sản phẩm từ Lô chưa kiểm duyệt hoặc bị từ chối không hiển thị cho End User.
- Sản phẩm có tồn kho khả dụng = 0 phải được đánh dấu "Hết hàng" hoặc ẩn khỏi danh sách (policy hiển thị TBD, nhưng logic kỹ thuật vẫn đảm bảo không cho đặt).
- Kết quả danh sách hỗ trợ phân trang theo chuẩn toàn hệ thống.

**Ví dụ dữ liệu (fake)**
- Response conceptual:  
	- items: [  
		{ productId: 1001, name: "Mì gói siêu ngon", category: "Mì ăn liền", price: 6500, inStock: true },  
		{ productId: 1002, name: "Nước suối 500ml", category: "Nước giải khát", price: 3000, inStock: false }  
	]  
	- page: 1, totalItems: 2, totalPages: 1

---

### FR-CATALOG-005 — Tính và cung cấp tồn kho khả dụng (Available Stock)

**Mô tả**
- Hệ thống CATALOG phải tính toán tồn kho khả dụng cho từng Sản phẩm (và, nếu được yêu cầu, theo Điểm nhận/Công ty – TBD) dựa trên:
	- Số lượng nhập từ các Lô đã kiểm duyệt.
	- Trừ đi các số lượng đã được giữ bởi đơn hàng đang xử lý (ORDERS) theo quy tắc toàn cục.

**Luồng chuẩn (tính tồn kho)**
1. Khi Lô được chuyển sang "Đã kiểm duyệt", tồn kho khả dụng tăng.
2. Khi ORDERS giữ hàng cho đơn đặt mới, tồn kho khả dụng giảm tạm thời.
3. Khi đơn bị huỷ hoặc hoàn tất, tồn kho được điều chỉnh theo quy tắc ORDERS.

**Điều kiện testable**
- Tồn kho khả dụng của Sản phẩm không bao giờ âm.
- Khi một Lô bị đổi trạng thái từ "Đã kiểm duyệt" sang trạng thái khác (nếu cho phép) thì tồn kho phải được điều chỉnh tương ứng (rule chi tiết TBD, có thể không cho phép).
- ORDERS sử dụng giá trị tồn kho khả dụng từ CATALOG để quyết định cho phép/không cho phép đặt hàng.

**Ví dụ dữ liệu (fake)**
- initialStockFromApprovedBatches: 1000  
- reservedByOrders: 150  
- availableStock: 850

## 4. Business rules references (BR-CATALOG-###)

> Mã BR đầy đủ sẽ được tập trung trong Business-Rules.md; tại đây chỉ mô tả khung.

- **BR-CATALOG-001 — Mỗi Lô chỉ thuộc một Ngành hàng trong MVP**:  
	- Lô không được chứa sản phẩm từ nhiều Ngành hàng khác nhau.
	- Nếu file nhập hoặc thao tác tạo Lô vi phạm, hệ thống phải từ chối hoặc tách Lô (cách xử lý chi tiết TBD).

- **BR-CATALOG-002 — Không xoá cứng Sản phẩm/Ngành hàng**:  
	- Chỉ cho phép đánh dấu is_deleted hoặc trạng thái "Inactive"; dữ liệu vẫn tồn tại để phục vụ báo cáo.

- **BR-CATALOG-003 — Chỉ bán sản phẩm từ Lô đã kiểm duyệt**:  
	- Sản phẩm thuộc Lô chưa được QC duyệt không được hiển thị cho End User và không thể được thêm vào đơn hàng.

- **BR-CATALOG-004 — Tồn kho không âm**:  
	- Mọi thao tác thay đổi tồn kho phải đảm bảo kết quả không âm; nếu sắp sửa âm thì phải từ chối hoặc xử lý theo rule ORDERS.

## 5. Data inputs/outputs (conceptual)

### 5.1. Thực thể chính
- **Product (Sản phẩm)**:  
	- Trường chính: id, product_code, name, category_id, supplier_id, unit, unit_price, is_active, is_deleted.
- **Category (Ngành hàng)**:  
	- Trường chính: id, category_code, name, description, is_active.
- **Batch (Lô hàng/Lô kiểm duyệt)**:  
	- Trường chính: id, supplier_id, category_id, status, import_date, documents (link), images (link).
- **BatchItem**:  
	- Trường chính: batch_id, product_id, quantity_imported.
- **StockSnapshot/AvailableStock** (khái niệm):  
	- product_id, available_quantity (+ optional: pickup_point_id/company_id nếu được yêu cầu sau).

### 5.2. Luồng dữ liệu
- Input từ NCC/Admin:
	- File danh sách sản phẩm hoặc nhập tay.
	- Thông tin Lô hàng (NCC, ngành hàng, sản phẩm, số lượng, tài liệu/ảnh).
- Output cho End User/Tầng 2/Tầng 1:
	- Danh sách sản phẩm hiển thị theo ngành hàng, tồn kho khả dụng, trạng thái.
- Output cho ORDERS/PAYMENTS/REPORTING:
	- Giá bán hiện tại của sản phẩm.
	- Trạng thái sản phẩm (active/inactive).
	- Tồn kho khả dụng theo sản phẩm.

## 6. Validations & edge cases

### 6.1. Validations
- **V-CATALOG-001 — Mã sản phẩm/Ngành hàng hợp lệ**:  
	- Không rỗng, không trùng nhau theo rule duy nhất đã chốt.
- **V-CATALOG-002 — Lô chỉ có một Ngành hàng**:  
	- Khi tạo/cập nhật Lô, tất cả sản phẩm trong Lô phải thuộc cùng một category_id.
- **V-CATALOG-003 — Quantity hợp lệ**:  
	- Số lượng nhập và tồn kho phải là số dương, không cho phép số âm.

### 6.2. Edge cases
- **E-CATALOG-001 — Cập nhật Sản phẩm đang có trong nhiều Lô**:  
	- Đổi tên hoặc mô tả sản phẩm không được làm mất liên kết đến các Lô; cần xác nhận với khách hàng về hiển thị lịch sử (TBD).
- **E-CATALOG-002 — Lô bị từ chối sau khi đã tạo**:  
	- Nếu Lô bị đổi sang trạng thái "Từ chối" sau khi nhập, phải đảm bảo không có tồn kho khả dụng nào được cộng từ Lô này.
- **E-CATALOG-003 — Lô nhập sai Ngành hàng**:  
	- Nếu phát hiện sau khi tạo Lô rằng ngành hàng gán sai, cần policy: chỉnh sửa Lô hay tạo Lô mới; hiện là Open Question.
- **E-CATALOG-004 — Sản phẩm hết hàng giữa lúc User đang đặt**:  
	- Khi ORDERS gửi yêu cầu đặt hàng, nếu tồn kho khả dụng không đủ, CATALOG/ORDERS phải từ chối và hiển thị thông báo phù hợp.

## 7. Non-functional requirements (module-specific)

- Hiệu năng:
	- Truy vấn danh sách Sản phẩm theo ngành hàng và từ khoá phải đáp ứng thời gian phản hồi phù hợp với NFR hệ thống (số liệu cụ thể TBD). 
	- Các truy vấn tồn kho nên được tối ưu bằng index/caching phù hợp.
- Logging & audit:
	- Mọi thao tác tạo/cập nhật/xoá logic Sản phẩm, Ngành hàng, Lô phải ghi log kèm correlationId.
- Bảo mật dữ liệu:
	- Không lưu PII nhạy cảm trong CATALOG; chỉ xử lý dữ liệu sản phẩm/NCC.

## 8. Open Questions / Assumptions for CATALOG

**Open Questions**
- Q-CATALOG-001: Mã định danh duy nhất của Sản phẩm là gì (product_code, barcode, hay kết hợp với NCC)?
- Q-CATALOG-002: Khi sản phẩm hết hàng, có muốn tiếp tục hiển thị với nhãn "Hết hàng" hay ẩn hoàn toàn khỏi danh sách?
- Q-CATALOG-003: Có cần quản lý tồn kho theo từng Điểm nhận/Công ty hay chỉ theo tổng kho trung tâm trong MVP?
- Q-CATALOG-004: Danh mục tài liệu/ảnh bắt buộc cho mỗi Lô kiểm duyệt là gì (hóa đơn, phiếu nhập, ảnh sản phẩm...) và có tiêu chí kích thước/định dạng file không?
- Q-CATALOG-005: Có cho phép chỉnh sửa Ngành hàng của Lô sau khi đã "Đã kiểm duyệt" không, và nếu có thì xử lý tồn kho/đơn hàng như thế nào?

**Assumptions (cần xác nhận)**
- A-CATALOG-001: MVP chỉ cần tồn kho ở mức tổng, chưa cần tách theo từng Điểm nhận/Công ty.
- A-CATALOG-002: Sản phẩm thuộc Lô bị từ chối sẽ không được hiển thị và không có tồn kho khả dụng.
- A-CATALOG-003: Không có yêu cầu về versioning chi tiết thông tin sản phẩm (tên/mô tả) cho hiển thị lịch sử đơn hàng; chỉ cần lưu giá/bản snapshot ở ORDERS/PAYMENTS.
