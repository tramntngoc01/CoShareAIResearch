
# SRS — Overview

## Scope
- In scope:
	- Hệ thống E-Commerce kết nối **Nhà Cung Cấp (NCC)** với **công nhân Khu Công Nghiệp (KCN)** thông qua hai portal:
		- **Portal End User**: cho Tầng 1, Tầng 2, Tầng 3, Shipper.
		- **Portal Admin**: cho Super Admin, Ops, QC, Finance, Support.
	- Các luồng nghiệp vụ MVP đã được mô tả trong Discovery:
		- Đăng ký/Đăng nhập bằng **Số điện thoại + OTP ZNS** (OTP chỉ bắt buộc lần đầu), sau đó đăng nhập tự động bằng **Session Token / Refresh Token** gắn với thiết bị/browser.
		- Import & quản lý **nhân sự và tầng (Tầng 1/2/3)** theo công ty.
		- Quản lý **sản phẩm, ngành hàng, lô kiểm duyệt** (mỗi lô 1 ngành hàng trong MVP, có tài liệu & ảnh kiểm duyệt).
		- Đặt hàng, checkout < 5 bước, theo dõi trạng thái, nhận hàng tại **điểm nhận** bằng mã nhận hàng (QR/PIN), đổi trả.
		- Thanh toán bằng **COD / Chuyển khoản / Trả chậm đến ngày lương** (bật/tắt theo công ty, có hạn mức, khóa mua khi quá hạn).
		- Mô hình **Affiliate 3 tầng** với % hoa hồng theo ngành hàng (có lịch sử hiệu lực & schedule thay đổi).
		- Quản lý **điểm nhận & lịch nhận**, **báo cáo vận hành** (doanh số, hoa hồng, COD, công nợ, hàng hủy), **Dashboard vận hành**.
		- Gửi **OTP ZNS** và các thông báo vận hành cơ bản (nhắc nhận hàng, nhắc nợ, thay đổi hoa hồng) trong phạm vi MVP.
	- Khả năng phục vụ **≥ 1.000 người dùng đồng thời** theo yêu cầu khách hàng.
- Out of scope:
	- **App mobile native (iOS/Android)** trong Giai đoạn 1 (chỉ web-first, responsive tốt trên mobile cho End User; Admin không cần responsive).
	- **Lô hàng multi-category** (1 lô nhiều ngành hàng) – được ghi rõ là giai đoạn mở rộng.
	- **Overflow-assist cho Tầng 2** (mô tả chi tiết chưa có, nằm ở Giai đoạn 5 – tuỳ chọn).
	- **Dashboard hoa hồng chi tiết** & **một số báo cáo tài chính nâng cao** (Giai đoạn 5).
	- Tích hợp trực tiếp với **phần mềm quản lý hàng hóa kho** và **Payment Gateway** cụ thể: hiện chỉ nêu "chừa cổng" hoặc "chuyển khoản" mà chưa xác nhận tích hợp hệ thống thứ ba.

## System context
- Actors:
	- **Tầng 3** – Công nhân KCN, người mua hàng cuối cùng (User End User).
	- **Tầng 2** – Nhân sự giới thiệu (Affiliate), có thể kiêm **Shipper/thu COD**.
	- **Tầng 1** – Quản lý mạng lưới (read-only trong MVP về hoa hồng & mạng lưới).
	- **Shipper** – Vai trò giao hàng tại điểm nhận, có thể là Tầng 2 hoặc user độc lập.
	- **NCC** – Nhà Cung Cấp, cung cấp sản phẩm & lô hàng.
	- **Super Admin** – Quản trị cấp cao nhất, cấu hình hệ thống, phân quyền, duyệt hủy.
	- **Ops** – Vận hành đơn hàng (xuất kho, theo dõi trạng thái, yêu cầu hủy đơn).
	- **QC** – Kiểm duyệt lô hàng (tài liệu & ảnh), đảm bảo chất lượng.
	- **Finance** – Quản lý thanh toán, COD, trả chậm, công nợ.
	- **Support** – Hỗ trợ người dùng, xử lý vấn đề nghiệp vụ.
- External systems:
	- **ZNS (Zalo Notification Service)** – được nêu rõ để gửi OTP và các thông báo (nhắc nhận, nhắc nợ, thay đổi hoa hồng). Chi tiết tích hợp kỹ thuật, SLA, rate limit chưa được cung cấp.
	- **Hệ thống ngân hàng/Payment Gateway** – **chưa được xác nhận** là có tích hợp trực tiếp; tài liệu chỉ nêu "Chuyển khoản" với hiển thị thông tin tài khoản & cú pháp. Do đó hiện tại ghi nhận là **"None confirmed"** cho tích hợp thanh toán, cần làm rõ sau.

## Non-functional requirements (NFR)
### Performance
- p95 latency:
	- **TBD** – khách hàng không đưa ra con số cụ thể.
	- Suggested default (needs confirmation): p95 thời gian phản hồi cho các API giao dịch chính (đặt hàng, đăng nhập, xem danh sách sản phẩm) **< 2–3 giây** dưới tải 1.000 user đồng thời.
- Throughput:
	- **TBD** – không có chỉ tiêu số giao dịch/giây cụ thể.
	- Suggested default (needs confirmation): hệ thống phải xử lý được **luồng duyệt sản phẩm + đặt hàng** cho 1.000 user đồng thời mà không suy giảm UX đáng kể (chi tiết kịch bản load test do Tech Lead định nghĩa).
- Batch jobs:
	- Có nhắc tới một số hoạt động dạng batch/hàng ngày trong Discovery (ví dụ: cập nhật lượt bán vào tối hôm trước, tính hoa hồng theo kỳ), nhưng **không có mô tả chi tiết** về thời điểm chạy & SLA.
	- Trạng thái: **TBD**, cần được làm rõ trong thiết kế chi tiết & Open Questions.

### Security
- AuthN/AuthZ:
	- Toàn hệ thống áp dụng **đăng ký/đăng nhập bằng SĐT + OTP ZNS lần đầu**, sau đó dùng **Session Token / Refresh Token** gắn với thiết bị/browser (cho End User).
	- Admin Portal dùng SĐT/Email + mật khẩu; việc bổ sung OTP/2FA cho Admin **chưa được xác nhận**.
	- Quyền truy cập dựa trên **Role** (Tầng 1/2/3, Shipper, Super Admin, Ops, QC, Finance, Support); chính sách phân quyền chi tiết sẽ nằm trong module-level SRS.
- Secrets handling:
	- Không được lưu mật khẩu dưới dạng plain text.
	- Không được log OTP đầy đủ (nếu cần log thì phải mask một phần) – chi tiết mức độ mask chưa được mô tả.
	- Tham số kết nối tới ZNS, DB, các dịch vụ ngoài được coi là secrets và phải được quản lý an toàn (kho secrets, biến môi trường… – chi tiết ở tầng triển khai, không trong SRS).
- Logging (no PII):
	- Logging phải **tránh log PII/PCI/PHI** như số CCCD, số tài khoản, thông tin thanh toán chi tiết.
	- Mọi log nghiệp vụ quan trọng phải kèm **correlationId** để truy vết xuyên module (theo guardrails chung).
	- Thông điệp lỗi gửi ra ngoài **không được lộ chi tiết nội bộ** (stacktrace, thông tin nhạy cảm).

### Availability & reliability
- Uptime target:
	- **TBD** – khách hàng chưa cung cấp chỉ tiêu uptime.
	- Suggested default (needs confirmation): uptime **99.5%** cho môi trường sản xuất.
- RPO/RTO:
	- **TBD** – chưa có yêu cầu cụ thể về Recovery Point Objective / Recovery Time Objective.
	- Suggested default (needs confirmation):
		- RPO: ≤ 15 phút dữ liệu giao dịch.
		- RTO: ≤ 60 phút cho các sự cố mức độ nghiêm trọng.

### Audit & compliance
- Audit trails:
	- Bắt buộc phải có **audit log** cho các nghiệp vụ quan trọng nêu trong Discovery:
		- Đăng ký/đăng nhập (thành công/thất bại).
		- Import & cập nhật nhân sự/tầng.
		- Tạo & cập nhật **lô kiểm duyệt**, bao gồm tài liệu & ảnh.
		- Trạng thái đơn hàng (xác nhận xuất kho, đến điểm nhận, đã nhận, đổi trả).
		- **Yêu cầu & duyệt hủy đơn**, **phiếu hủy hàng** và đối soát COD.
	- Nội dung tối thiểu: thời gian, người thao tác (User/AdminUser), loại hành động, đối tượng bị ảnh hưởng, kết quả (success/fail).
- Data retention:
	- Thời gian lưu trữ log/audit, dữ liệu đơn hàng, công nợ, hoa hồng **chưa được khách hàng quy định rõ**.
	- Suggested default (needs confirmation):
		- Audit log nghiệp vụ: lưu ít nhất **2 năm**.
		- Dữ liệu đơn hàng & công nợ: lưu theo yêu cầu pháp lý nội địa (TBD với khách hàng/luật).

## Global business rules
- BR-001 – OTP ZNS chỉ bắt buộc cho **lần đăng ký/đăng nhập đầu tiên** của End User; các lần sau phải ưu tiên dùng **Session Token / Refresh Token** gắn với thiết bị/browser để giảm chi phí ZNS, trừ khi có rủi ro bảo mật cần yêu cầu OTP lại.
- BR-002 – **Trả chậm đến ngày nhận lương** chỉ được bật **theo công ty**, và phải gắn với **hạn mức** (số đơn tối đa, tổng tiền tối đa). Khi người dùng quá hạn trả, hệ thống phải **khóa khả năng mua mới** cho đến khi Admin xác nhận đã trả xong.
- BR-003 – Mỗi **lô hàng trong MVP** chỉ được gán **1 ngành hàng**, và phải lưu đủ **tài liệu & ảnh kiểm duyệt** trước khi hệ thống cho phép bán các sản phẩm thuộc lô đó.
- BR-004 – Mọi **hủy đơn** do Ops tạo yêu cầu và Super Admin duyệt phải kéo theo các hành động tự động: cập nhật trạng thái đơn, khôi phục tồn kho, điều chỉnh hoa hồng, điều chỉnh công nợ COD/trả chậm, ghi audit log đầy đủ.
- BR-005 – **Shipper/Tầng 2** khi thu COD phải chốt ca và đối soát cuối ca; mọi sai lệch phải được ghi nhận & báo cáo.

## Global states/status catalogs
(Define system-wide states that multiple modules reuse.)

> Chi tiết trạng thái đầy đủ cho từng module sẽ nằm trong SRS-<MODULE>; dưới đây là các nhóm trạng thái dùng chung toàn hệ thống.

### 1. Trạng thái đơn hàng (ORDERS)

Các trạng thái này đã được mô tả trong Discovery và Glossary:

- **Chờ xác nhận** – Đơn mới tạo, chưa được Ops/Admin xác nhận.
- **Chờ thanh toán** – Đơn dùng phương thức chuyển khoản, chờ Finance/Admin xác nhận đã nhận tiền.
- **Đã xác nhận** – Đơn đã được xác nhận, chờ xuất kho.
- **Đang giao** – Hàng đang được vận chuyển tới điểm nhận.
- **Sẵn sàng nhận** – Hàng đã đến điểm nhận, chờ Người mua đến nhận.
- **Đã nhận** – Người mua đã nhận hàng thành công (có POD).
- **Chờ duyệt hủy** – Ops tạo yêu cầu hủy, chờ Super Admin duyệt.
- **Đã hủy** – Đơn đã được hủy theo quy trình, tồn kho/hoa hồng/công nợ đã được điều chỉnh.
- **Đổi trả** – Đơn có yêu cầu đổi/trả, đang xử lý.

### 2. Trạng thái OTP / xác thực (AUTH – conceptual)

- **OTP-Pending** – Yêu cầu OTP đã được tạo, mã đang chờ người dùng nhập.
- **OTP-Used** – OTP đã được dùng thành công cho đăng ký/đăng nhập.
- **OTP-Expired** – OTP hết hạn, không còn giá trị.

### 3. Trạng thái lô kiểm duyệt (CATALOG)

- **Draft** – Lô đang được tạo, chưa đủ tài liệu/ảnh.
- **Approved** – Lô đã được QC kiểm duyệt, có đủ tài liệu & ảnh.
- **Expired** – Lô đã hết hạn dùng hoặc bị đánh dấu không còn hợp lệ.

### 4. Trạng thái công nợ trả chậm (PAYMENTS)

- **Đang trong hạn** – User có khoản trả chậm nhưng chưa tới ngày lương.
- **Quá hạn** – User chưa thanh toán khi đã qua ngày lương → có thể bị khóa quyền mua.
- **Đã tất toán** – Khoản nợ đã được Admin xác nhận là đã thanh toán.

### 5. Trạng thái phiếu COD & ca làm (PAYMENTS)

- **Ca mở** – Ca đang diễn ra, Shipper/Tầng 2 đang thu COD.
- **Chờ đối soát** – Ca đã kết thúc, chờ đối soát với Admin/Finance.
- **Đã đối soát** – COD đã được nộp và khớp số liệu.

## Open Questions & Assumptions (Overview)

- **OQ-OV-001** – Mục tiêu chi tiết cho các NFR (p95 latency, throughput, uptime, RPO/RTO) là bao nhiêu?  
	*Why it matters*: ảnh hưởng trực tiếp đến thiết kế kiến trúc, capacity planning và chi phí hạ tầng.  
	*Suggested default (needs confirmation)*: p95 < 2–3s, uptime 99.5%, RPO 15 phút, RTO 60 phút.

- **OQ-OV-002** – Có tích hợp chính thức với Payment Gateway hoặc hệ thống ngân hàng nào không, hay tất cả chuyển khoản được xử lý manual bởi Admin Finance?  
	*Why it matters*: quyết định phạm vi module PAYMENTS & NOTIFICATIONS, mức độ tự động hóa và rủi ro sai lệch.

- **OQ-OV-003** – Thời gian lưu trữ audit log, dữ liệu đơn hàng, công nợ & hoa hồng cần tuân theo quy định pháp lý/kiểm toán nào?  
	*Why it matters*: quyết định chính sách **Data retention** và dung lượng lưu trữ.

- **OQ-OV-004** – Chính sách chuẩn khi SĐT không nằm trong danh sách nhân sự (gán Tầng 3 mặc định hay chờ duyệt riêng)?  
	*Why it matters*: ảnh hưởng xuyên suốt các module AUTH/USERS/ORDERS.

- **OQ-OV-005** – Mức độ sử dụng OTP/2FA cho Admin Portal (chỉ mật khẩu hay thêm OTP ZNS/email)?  
	*Why it matters*: liên quan bảo mật, chi phí ZNS, UX cho Admin.

- **ASSUMP-OV-001** – Hệ thống sẽ không tích hợp trực tiếp với Payment Gateway trong MVP; **Chuyển khoản** được xử lý manual (Admin Finance xác nhận), và chỉ "chừa cổng" cho việc tích hợp sau.  
- **ASSUMP-OV-002** – NFR gợi ý (p95 latency, uptime, RPO/RTO, retention 2 năm) chỉ là **Suggested default (needs confirmation)** và sẽ được cập nhật lại sau khi khách hàng & Tech Lead thống nhất.
