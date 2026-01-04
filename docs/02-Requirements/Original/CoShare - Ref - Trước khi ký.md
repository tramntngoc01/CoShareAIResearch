# 

# 

# **![][image1]**

# 

# **E-Commerce Platform**

# **Tổng Quan Tính Năng & Màn Hình**

# 

### 

Phiên bản: 1.0.0  
Ngày: 15/10/2025  
Khởi tạo: Phong Võ

### 

# 

# **1\. BỐI CẢNH & NGUYÊN TẮC THIẾT KẾ**

* Kênh mua sắm trực tiếp NCC ↔ Công nhân KCN; nhận hàng **tại địa điểm trong doanh nghiệp**.  
* **Đăng ký/Đăng nhập** qua SĐT \+ **OTP ZNS** (chỉ **bắt buộc ở lần đầu** để xác thực danh tính); match danh sách **tầng 1–3** dựa vào danh sách nhân sự do Admin upload.  
* Sau khi OTP các thực lần đầu → hệ thống dùng **Session Token / Refresh Token** để **đăng nhập tự động** trong các lần truy cập tiếp theo **không cần OTP**, giúp **giảm tối đa chi phí ZNS.**  
* Token đăng nhập sẽ gắn với **thiết bị/browser**, tránh lạm dụng nhưng vẫn tiết kiệm chi phí.  
* **Thanh toán:** COD / Chuyển khoản. **Trả chậm đến ngày nhận lương** (enable theo công ty, có hạn mức).  
* **Affiliate 3 tầng** với **% hoa hồng theo từng sản phẩm** (có lịch sử hiệu lực áp dụng). Có hỗ trợ schedule set thay đổi %.  
* **Kiểm duyệt lô hàng**: bắt buộc lưu **tài liệu & ảnh** (audit trail).  
* **Tầng 2** có thể kiêm vai trò của **shipper/thu hộ COD** tại điểm nhận hàng.

# **2\. THÔNG TIN KIẾN TRÚC IA (INFORMATION ARCHITECTURE)**

**Admin**

1. Dashboard  
2. Người dùng & Tầng (Import/Match)  
3. Sản phẩm & Lô kiểm duyệt (Sản phẩm nên bao gồm Mã hàng/mã vạch SP \- Mục SP phân được nhóm hàng)  
4. Đơn hàng  
5. Điểm nhận & Lịch nhận  
6. Thanh toán CK & COD  
7. Trả chậm (Cấu hình theo công ty)  
8. Hoa hồng theo ngành hàng (Lịch sử hiệu lực)  
9. Báo cáo  
10. Đổi trả  
11. Thông báo/ZNS Templates  
12. Cấu hình hệ thống & Nhật ký (Audit)

**User** (Web): Trang chủ → Danh mục → Chi tiết SP (giá niêm yết \- mã giảm giá \- chiết khấu) → Giỏ hàng → Checkout → Trạng thái đơn → Mã nhận hàng (QR) → Đổi trả

* **Tầng 2** thêm các chức năng: Thấy được hoa hồng và chiết khấu của mình \- Giao nhận & Thu COD, Đối soát ca.  
* **Tầng 1** thêm các chức năng: Tổng quan mạng lưới & hoa hồng (read-only trong MVP)

Note: Cho phép admin chuyển tầng 3 qua tầng 2 khác. (quét mã của tầng 2 \-\> cần admin xác nhận, hoặc admin chuyển manual)

# 

# **3\. ADMIN – TÍNH NĂNG & MÀN HÌNH**

### **2.1 Đăng nhập *(Màn hình & chức năng)***

* Trường: SĐT/Email \+ mật khẩu (hoặc OTP ZNS dành cho Admin).  
* Quyền: Super Admin, Ops, QC (kiểm duyệt), Finance (đối soát), Support.

### **2.2 Dashboard**

* Widget: Đơn hôm nay, Tỉ lệ nhận hàng, Công nợ còn lại (trả chậm), COD tồn quỹ theo điểm, top ngành hàng bán chạy, cảnh báo quá hạn trả chậm.  
* Quick links: Import danh sách nhân sự, Tạo lô kiểm duyệt, Chốt ca COD.

### **2.3 Người dùng & Tầng (Import/Match)**

* Upload CSV/XLSX: cột tối thiểu **Company, Employee Code, FullName, Phone, Tier(1/2/3), Ref Tier (Mã nhân viên của tầng phụ thuộc)**.  
* Mapping preview & kiểm tra trùng (fuzzy theo tên/điện thoại/mã nhân viên).  
* Sau đăng ký của user: hệ thống **match tầng**; case **không có tên** → rule theo quyết định (tạm gán T3 / chờ duyệt); có thao tác **gán T2 cha** nếu cần.

### **2.4 Sản phẩm & Lô kiểm duyệt**

* CRUD sản phẩm, ngành hàng, giá/ưu đãi.  
* Chính sách đổi trả cho riêng mỗi sản phẩm. (upload file/link google doc)  
* **Tạo lô đã kiểm duyệt**: nhập NCC, số lô, hạn dùng, đính kèm **tài liệu & ảnh** kiểm duyệt.

**Lưu ý:** Một **lô hàng** trong MVP nên **giới hạn 1 ngành hàng** để đơn giản kiểm duyệt & báo cáo. Hệ thống có thể **nâng cấp trong tương lai** cho phép lô chứa nhiều ngành hàng (multi-category) khi mở rộng mô hình kiểm duyệt & truy xuất nguồn gốc. \>\> có thể lên thành 3 \- 5 sản phẩm không để end users không thấy trang bán hàng bị đơn điệu

### **2.5 Đơn hàng**

* Danh sách đơn (lọc theo công ty/điểm nhận/trạng thái/theo sales/cùng một mã).  
* Thao tác: xác nhận xuất kho kèm ngày và thời gian, đánh dấu đến điểm nhận, xác nhận đã nhận/đổi trả.  
* Xem lịch sử sự kiện & nhật ký người thao tác (audit).  
* Hệ thống tạo ra phiếu thông tin phiếu giao hàng dán lên SP (xuất PDF A5).  
* Hủy đơn hàng:  
  * Admin vận hành (Ops) có thể **tạo yêu cầu hủy** đơn (có lý do, ảnh hoặc chứng cứ nếu cần).  
  * Đơn chuyển trạng thái **“Chờ duyệt hủy”**.  
  * **Super Admin** là người **duyệt yêu cầu hủy**.  
  * Sau khi SA duyệt → hệ thống:  
1. Cập nhật trạng thái đơn thành **Đã hủy**,  
2. Khôi phục tồn kho,  
3. Điều chỉnh hoa hồng (nếu có),  
4. Điều chỉnh công nợ COD/trả chậm (nếu có),  
5. Ghi audit log đầy đủ (người tạo yêu cầu → người duyệt → thời gian).

### **2.6 Hủy hàng (Hư hại / Mất mát / Lý do vận hành)**

* **Chức năng độc lập, không phụ thuộc vào trạng thái đơn hàng.**  
* Dùng khi **hàng hóa bị hư hại trong quá trình di chuyển**, lưu kho, giao nhận, hoặc **thiếu hụt thực tế**.  
* **Quy trình:**  
1. Ops tạo **phiếu hủy hàng**: chọn sản phẩm, số lượng, lô hàng, điểm nhận/kho liên quan.  
2. Ghi rõ **lý do hủy** (hư hại, bể vỡ, quá hạn, mất mát…), kèm **ảnh chứng cứ**. Phiếu chuyển sang trạng thái **“Chờ duyệt hủy hàng”**.  
3. **Super Admin duyệt**.  
4. Khi duyệt → hệ thống:  
   * Trừ tồn kho chính xác theo số lượng hủy.  
   * Ghi vào **báo cáo hàng hủy** (theo ngày, theo điểm nhận, theo nhân sự).  
   * Không ảnh hưởng tới hoa hồng/công nợ trừ khi hàng bị hủy thuộc đơn đã nhận.  
   * Ghi đầy đủ audit log.  
* **Báo cáo hỗ trợ:** tổng số hàng hủy, giá trị hủy, tỉ lệ hủy theo NCC/Lô/Điểm nhận/Nhân sự T2.

### **2.7 Điểm nhận & Lịch nhận**

* Quản lý **điểm nhận trong công ty** (tên, địa điểm).  
* **Default điểm nhận** cho công ty.

### **2.8 Thanh toán & COD**

* Phương thức: COD, Chuyển khoản (hiển thị thông tin tài khoản & cú pháp nội dung).  
* **COD**: phân quyền **Tầng 2** thu hộ; tạo **phiếu thu điện tử**, sổ quỹ theo **điểm nhận/ca**; tính năng **chốt ca**, nộp tiền, đối soát sai lệch.  
* Nhật ký thu/chi và tồn quỹ theo thời gian thực.

### **2.9 Trả sau (Cấu hình theo công ty)**

* Bật/tắt theo công ty; cấu hình theo **ngày nhận lương**; **hạn mức** (số đơn tối đa, tổng tiền tối đa).  
* Duyệt hồ sơ user (CCCD, ngày sinh, mã NV); khoá mua khi quá hạn.  
* Admin xác thực user trả xong và quản lý user còn nợ.

### **2.10 Hoa hồng theo ngành hàng**

* Cấu hình **% theo Category**; hỗ trợ **lịch sử hiệu lực (effective from/to)**.  
* Tạo **phiên bản mới** tỉ lệ → đặt ngày hiệu lực tương lai → gửi thông báo/ZNS.  
* Báo cáo tính hoa hồng theo **tầng** và **ngành hàng** dựa trên lịch sử.  
* **Hiển thị hoa hồng cá nhân cho Tầng 2 & Tầng 1 (read-only trong MVP cho T1):** tổng quan kỳ hiện tại, kỳ trước, đơn góp doanh thu, điều chỉnh do đổi trả.

### **2.11 Báo cáo**

* Doanh số theo KCN/công ty/ngành hàng/chiến dịch.  
* Hoa hồng theo tầng (áp dụng lịch sử tỉ lệ).  
* Tình trạng nhận hàng; công nợ trả chậm; **báo cáo COD** theo điểm/ca/nhân sự T2.

### **2.12 Đổi trả**

* Tạo yêu cầu đổi trả tại điểm nhận (ảnh chứng cứ, lý do, kết quả xử lý).  
* Tự động điều chỉnh tồn, doanh thu, hoa hồng & công nợ liên quan.

### **2.13 Thông báo/ZNS & Cấu hình hệ thống**

* Quản lý **template ZNS** (OTP, nhắc nhận hàng, nhắc nợ, thay đổi hoa hồng).  
* Cấu hình chung: logo, pháp lý, điều khoản, phân quyền, nhật ký (audit log).

# **3\. USER – TÍNH NĂNG & MÀN HÌNH (PHÂN TẦNG)**

### **3.1 Onboarding & Đăng nhập *(Màn hình & chức năng)***

* **Đăng ký**: Họ tên, Công ty, SĐT → OTP ZNS → xác thực → hệ thống báo **tầng** (1/2/3).  
* Nếu công ty bật **trả chậm**: hiển thị **KYC** (CCCD, ngày sinh, mã NV) để xin hạn mức. (Nếu không KYC thuộc user trả COD/Chuyển khoản) 

### **3.2 Trang chủ & Danh mục**

* Banner deal theo **công ty/ngành hàng**; lối tắt tới **điểm nhận mặc định**.  
* Danh mục ngành hàng; sản phẩm hiển thị theo kiểm duyệt & tồn khả dụng.

### **3.3 Chi tiết sản phẩm**

* Ảnh, mô tả, thuộc tính/ Video SP.  
* Giá/ưu đãi, chính sách đổi trả; gợi ý sản phẩm liên quan.  
* Lượt bán (hệ thống sẽ thống kê vào tối hôm qua)  
* Số lượng tồn (số lượng tồn kết chừa cổng kết nối phần mềm quản lý hàng hóa kho)

### **3.4 Giỏ hàng & Checkout**

* Chọn **điểm nhận** (default công ty). (có trường thông tin chi tiết người nhận hàng, có trường ghi chú lưu ý \-\> người mua ghi chú thông tin này).  
* Thông tin người nhận hàng:  
  * Họ tên người nhận (auto-fill theo user, cho phép sửa).  
  * Số điện thoại người nhận (auto-fill theo user, cho phép sửa nếu người khác nhận thay).  
* **Ghi chú giao hàng**: trường "Ghi chú" để người mua điền lưu ý đặc biệt (ví dụ: liên hệ trước, gửi tại phòng ban X…).  
* Phương thức: **COD** / **Chuyển khoản** / **Trả sau** (nếu đủ điều kiện & còn hạn mức).  
* Xem phí & điều khoản.

### **3.5 Trạng thái đơn & Nhận hàng**

* Trang **Đơn hàng**: trạng thái theo thời gian (đã xác nhận, đang giao đến điểm, sẵn sàng nhận…).  
* **Mã nhận hàng (QR/PIN)** để trình tại điểm; thông báo nhắc nhận qua ZNS (có thể configure để thông báo hoặc không).

### **3.6 Đổi trả & Hỗ trợ**

* Tạo yêu cầu đổi trả tại điểm nhận; kèm ảnh/ghi chú.  
* **Trợ lý AI** hỗ trợ:  
  * FAQ chính sách.  
  * Tra cứu điểm nhận / khung giờ nhận.  
  * Tình trạng đơn theo thời gian thực.  
  * **Thu thập nhu cầu & xu hướng người dùng** để đề xuất xây dựng **danh mục sản phẩm phù hợp** (AI phân tích từ câu hỏi, lịch sử tìm kiếm, loại sản phẩm người dùng quan tâm).

# **4\. ROLE SHIPPER \- TÍNH NĂNG CHI TIẾT**

**Shipper** là vai trò (role) có thể gán cho:

* Nhân sự **Tầng 2** (kiêm shipper), hoặc  
* Một **user độc lập** không thuộc các tầng mua hàng.

**Tính năng chính dành cho Shipper:**

1. **Danh sách đơn giao được phân công** (theo công ty, điểm nhận, ca làm).  
2. **Bắt đầu giao hàng**:  
   1. Chọn đơn hoặc quét QR trên kiện hàng.  
   2. Chụp **ảnh trước giao** (bằng chứng tình trạng hàng).  
3. **Hoàn tất giao hàng (POD – Proof of Delivery)**:  
   1. Chụp **ảnh POD** (kiện hàng \+ người nhận hoặc điểm nhận).  
   2. Cập nhật trạng thái “Đã giao thành công”.  
4. **Thu tiền COD** (nếu đơn COD):  
   1. Ghi nhận số tiền nhận.  
   2. Sinh **phiếu thu điện tử**.  
5. **Đối soát ca**:  
   1. Tổng số đơn đã giao.  
   2. Tổng tiền COD thu được.  
   3. Số tiền phải nộp lại quỹ.  
6. **Hỗ trợ đổi trả** tại điểm nhận (theo phân công của Admin).  
7. **Giới hạn công ty**: mỗi shipper được gán cho **1 công ty** (hoặc nhiều công ty trong tương lai nếu mở rộng).

# **5\. GHI CHÚ TRIỂN KHAI UI/UX (MVP)**

1. Thiết kế **web-first**, **responsive tốt trên mobile** (không phát triển app/mobile-first ở GĐ1). ***Lưu ý: Admin sẽ không cần chức năng responsive***.  
2. Giữ \< 5 bước tới "Đặt hàng thành công".  
3. Pre-fill **điểm nhận mặc định** theo công ty. Có các thông tin chi tiết người nhận hàng (hoặc shipper) và những ghi chú lưu ý.  
4. Với Tầng 2, cung cấp **màn hình quét QR/nhập mã đơn** & **chốt ca** đơn giản, rõ ràng.

# **6\. KẾ HOẠCH TRIỂN KHAI & THANH TOÁN THEO GIAI ĐOẠN**

| Giai đoạn | Thời gian dự kiến | Mục tiêu / Kết quả bàn giao |
| ----- | ----- | ----- |
| **Giai đoạn 1 – Khởi động & Thiết kế hệ thống** | Tuần 1–3 | \- Tài liệu xác nhận phạm vi & menu chức năng.  \- **Thiết kế wireframe & prototype mẫu cho cả portal Admin và End User.**  \- Xác nhận kiến trúc hệ thống (backend/frontend, hạ tầng server). |
| **Giai đoạn 2 – Phát triển lõi hệ thống (MVP)** | Tuần 4–9 | **Phân tách 2 portal:**  **(a) Portal Admin**:  \- Module quản lý user & tầng (import/match, phân tầng, xác thực ZNS).  \- Module sản phẩm, ngành hàng, lô kiểm duyệt (1 ngành/lô).  \- Quản lý đơn hàng, giao nhận Assigned-only, chốt ca COD.  \- Cấu hình trả chậm, báo cáo vận hành, ZNS template.   **(b) Portal End User**:  \- Đăng ký OTP ZNS & match tầng.  \- Xem sản phẩm, giỏ hàng, đặt hàng, thanh toán COD/CK, trả chậm (nếu có).  \- Quản lý đơn hàng, QR nhận hàng, đổi trả, dashboard cá nhân. \- Responsive web-first, tối ưu UX đơn giản 5 bước checkout.   **Kiến trúc:** Backend & Frontend triển khai **trên 2 hệ thống server riêng biệt** (API Gateway & Web Client). |
| **Giai đoạn 3 – Kiểm thử & UAT khách hàng** | Tuần 10–11 | \- Triển khai môi trường staging (backend \+ frontend).  \- Kiểm thử với 1–2 công ty pilot.  \- Sửa lỗi, tối ưu UX/UI. |
| **Giai đoạn 4 – Go-live Pilot & Đánh giá sau triển khai** | Tuần 12 | \- Go-live bản chính thức tại công ty/KCN đầu tiên.  \- Theo dõi vận hành thực tế, fix minor bug.  \- Báo cáo tổng hợp giai đoạn pilot. |
| **Giai đoạn 5 – Mở rộng & Tối ưu nâng cao (tuỳ chọn)** | Tuần 13–16 | \- Cho phép lô hàng nhiều ngành nếu muốn (multi-category).  \- Mở rộng Overflow-assist cho Tầng 2 (Nếu có). \- Dashboard hoa hồng chi tiết.  \- Một số báo cáo tài chính. |

**Tổng thời gian dự kiến:** \~16 tuần (4 tháng), chia 5 giai đoạn.

**Cách thanh toán:** Chuyển khoản sau khi bên A xác nhận biên bản bàn giao từng giai đoạn. Có thể chia nhỏ hơn (20–30–30–10–10%) nếu cần bảo đảm tiến độ.

[image1]: <data:image/png;base64,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>