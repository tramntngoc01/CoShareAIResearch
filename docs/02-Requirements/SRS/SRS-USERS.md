
# SRS — USERS: User & Profile Management

## Purpose
- Quản lý danh tính và hồ sơ User (Tầng 1, Tầng 2, Tầng 3, Shipper) trong hệ thống.
- Quản lý mối quan hệ affiliate 3 tầng (Tầng 1 → Tầng 2 → Tầng 3) thông qua Ref Tier và cấu trúc tổ chức (KCN, Công ty, Điểm nhận).
- Quản lý dữ liệu KYC cơ bản (CCCD, ngày sinh, mã nhân viên) để phục vụ kiểm soát trả chậm, hoa hồng và vận hành.
- Cung cấp dữ liệu nền về User/tầng cho các module AUTH, ORDERS, PAYMENTS, ADMIN, REPORTING.

Phạm vi module USERS **không** bao gồm:
- Luồng đăng ký/đăng nhập, OTP, token (thuộc AUTH).
- Luồng đặt hàng, thanh toán, đối soát (thuộc ORDERS, PAYMENTS).

## Actors
- User (Tầng 3):
	- Chỉ xem và cập nhật một phần thông tin hồ sơ cá nhân (sau khi đã có tài khoản từ AUTH).
- Tầng 2:
	- Có thể xem danh sách Tầng 3 thuộc mạng lưới của mình (MVP: chỉ đọc), không tự chỉnh sửa dữ liệu KYC của Tầng 3.
- Tầng 1:
	- Có thể xem danh sách Tầng 2 và Tầng 3 thuộc mạng lưới của mình (MVP: chỉ đọc).
- Admin:
	- Thao tác import danh sách nhân sự từ đối tác (Công ty, Tầng 1, Tầng 2, Tầng 3, Shipper) theo file do BA/PO thiết kế.
	- Quản lý hồ sơ User và tầng (sửa thông tin, khoá/mở khoá, cập nhật mapping Ref Tier) theo quyền được phân bổ trong ADMIN.
- Super Admin:
	- Thực hiện các thao tác nhạy cảm: đổi Ref Tier hàng loạt, merge/split mạng lưới, khôi phục User đã khoá (nếu được cho phép trong policy).
- Hệ thống AUTH (module khác):
	- Đọc dữ liệu từ USERS để kiểm tra xem số điện thoại/mã nhân viên tồn tại và thuộc tầng nào khi user đăng ký/đăng nhập.
- Hệ thống PAYMENTS (module khác):
	- Đọc dữ liệu USERS để tính công nợ trả chậm, hoa hồng theo tầng.

## Functional requirements

### FR-USERS-001 — Import danh sách nhân sự từ Công ty / đối tác
- USERS phải cho phép Admin upload danh sách nhân sự từ file (ví dụ: CSV/Excel; format chi tiết TBD) để tạo/cập nhật User và cấu trúc tầng.
- Mỗi bản ghi import tối thiểu bao gồm các trường **không được suy đoán** ngoài Discovery/Glossary; các trường cụ thể sẽ được chốt trong tài liệu Sample-Data và Business-Rules (TBD).
- Với mỗi bản ghi hợp lệ, hệ thống phải:
	- Tạo mới bản ghi User nếu chưa tồn tại (theo khoá chính được định nghĩa trong BR-USERS-001).
	- Cập nhật bản ghi User hiện có nếu trùng khoá chính (ví dụ: mã nhân viên + Công ty; chi tiết trong BR-USERS-001).
	- Gán User vào đúng Công ty, Điểm nhận (theo dữ liệu file) và tầng (Tầng 1/2/3 hoặc Shipper), bao gồm xử lý Ref Tier nếu có.
- Hệ thống phải cung cấp kết quả import theo lô:
	- Tổng số bản ghi đọc được.
	- Số bản ghi tạo mới.
	- Số bản ghi cập nhật.
	- Số bản ghi lỗi, kèm lý do (ví dụ: thiếu trường bắt buộc, Ref Tier không tồn tại, mã Công ty không hợp lệ…).
- Import phải ghi log đầy đủ (Audit Log) bao gồm: người thực hiện, thời gian, file nguồn, kết quả tổng quan.

### FR-USERS-002 — Quản lý cấu trúc tầng và Ref Tier
- USERS phải lưu trữ mối quan hệ affiliate 3 tầng (Tầng 1 → Tầng 2 → Tầng 3) và cho phép Admin/Super Admin cập nhật theo business rule.
- Hệ thống phải đảm bảo:
	- Mỗi User Tầng 2 phải thuộc đúng một Tầng 1 làm Ref Tier cha (nếu mô hình được yêu cầu như vậy trong BR toàn cục; chi tiết trong BR-USERS-002).
	- Mỗi User Tầng 3 phải thuộc đúng một Tầng 2 làm Ref Tier cha.
- Khi Admin thay đổi Ref Tier của một User (Tầng 2 hoặc Tầng 3), hệ thống phải:
	- Ghi nhận lịch sử thay đổi (ai, khi nào, từ Ref Tier nào sang Ref Tier nào).
	- Áp dụng thay đổi cho các đơn hàng tương lai; tác động tới đơn đã tồn tại sẽ được quy định trong Business-Rules (TBD, có thể chỉ áp dụng từ thời điểm thay đổi).
- Hệ thống phải cung cấp màn hình/tính năng cho phép Admin tìm kiếm User và chỉnh sửa Ref Tier (theo chính sách phân quyền từ ADMIN).

### FR-USERS-003 — Quản lý hồ sơ KYC User
- USERS phải lưu trữ thông tin KYC cơ bản của User để phục vụ:
	- Kiểm soát trả chậm (PAYMENTS).
	- Tính hoa hồng, báo cáo (REPORTING).
- Hệ thống phải cho phép Admin cập nhật các thông tin như: họ tên, mã nhân viên, dữ liệu định danh cơ bản (ví dụ: CCCD, ngày sinh) theo danh sách trường sẽ được chốt trong Business-Rules (TBD), với các ràng buộc từ pháp lý và bảo mật dữ liệu.
- User (Tầng 3) có thể xem hồ sơ của chính mình và chỉnh sửa một số trường được cho phép (ví dụ: địa chỉ giao nhận chi tiết, email; chi tiết trường cho phép TBD) nhưng không được tự ý thay đổi các thuộc tính do Công ty/quy định (ví dụ: Công ty, Điểm nhận, tầng, hạn mức trả chậm).
- Mọi thay đổi KYC quan trọng (ví dụ: CCCD, ngày sinh, Công ty) phải được ghi log audit kèm người thực hiện.

### FR-USERS-004 — Quản lý trạng thái User (active/inactive/locked)
- USERS phải hỗ trợ các trạng thái User cơ bản, ví dụ:
	- Active (được phép đăng nhập, đặt hàng; chi tiết trong States bên dưới).
	- Inactive (tài khoản đã tạo nhưng chưa dùng hoặc bị tạm ẩn trong UI; không cho đặt hàng).
	- Locked (bị khoá do vi phạm hoặc theo yêu cầu Công ty; không cho đăng nhập/đặt hàng; chi tiết xử lý cùng AUTH).
- Admin và/hoặc Super Admin (tuỳ phân quyền từ ADMIN) có thể:
	- Khoá User (chuyển sang trạng thái Locked) kèm lý do.
	- Mở khoá User trở lại (chuyển sang Active) nếu được chính sách cho phép.
- Khi User bị khoá, module USERS phải cung cấp tín hiệu/trạng thái rõ ràng cho AUTH, ORDERS, PAYMENTS để các module này từ chối thao tác liên quan (đăng nhập, đặt hàng, ghi nhận công nợ).

### FR-USERS-005 — Tra cứu & báo cáo danh sách User theo tầng
- USERS phải cung cấp khả năng tra cứu User cho các mục đích vận hành:
	- Admin: tìm kiếm theo nhiều tiêu chí (Công ty, Điểm nhận, tầng, trạng thái, mã nhân viên, số điện thoại, Ref Tier...).
	- Tầng 1: xem danh sách Tầng 2 và Tầng 3 thuộc mạng lưới của mình (MVP: read-only; chi tiết field hiển thị TBD).
	- Tầng 2: xem danh sách Tầng 3 thuộc mạng lưới của mình (MVP: read-only).
- Kết quả tra cứu phải hỗ trợ phân trang theo chuẩn API của hệ thống.
- Các báo cáo chi tiết/aggregated cho nghiệp vụ (doanh số theo tầng, hoa hồng, công nợ) sẽ thuộc REPORTING nhưng dựa trên dữ liệu USERS.

## Data requirements

### Entities involved
- User:
	- Đại diện cho một người dùng hệ thống (Tầng 1, Tầng 2, Tầng 3, Shipper, Admin nếu dùng chung entity; chi tiết mô hình dữ liệu sẽ được thiết kế trong phần DB-USERS).
	- Thuộc một Công ty (đối với Tầng 3 và một số vai trò), có Điểm nhận mặc định.
	- Có một hoặc nhiều vai trò logic (Tầng 1/2/3, Shipper) theo module USERS.
- Company (tham chiếu từ module DB-ADMIN/DB-USERS):
	- Đơn vị nơi Tầng 3 làm việc, gắn với KCN, Điểm nhận và cấu hình trả chậm.
- Pickup Point (Điểm nhận):
	- Địa điểm nhận hàng, liên kết với Công ty.
- Tier Relationship / Referral:
	- Bảng biểu diễn quan hệ Tầng 1 → Tầng 2 → Tầng 3 thông qua Ref Tier.
- KYC Profile:
	- Tập thuộc tính KYC gắn với User (CCCD, ngày sinh, mã nhân viên, các trường pháp lý cần có).

Chi tiết tên bảng/cột, kiểu dữ liệu, index sẽ được chuẩn hoá trong DB-USERS theo quy tắc DB-Design-Rules của dự án.

### Ownership & lifecycle
- Dữ liệu User Tầng 3 được **đồng sở hữu** bởi Công ty (qua file HR/import) và hệ thống CoShare:
	- Công ty chịu trách nhiệm chính về độ chính xác thông tin nhân sự đầu vào.
	- CoShare chịu trách nhiệm về lịch sử thay đổi sau khi dữ liệu được dùng trong hệ thống (đơn hàng, trả chậm…).
- Dữ liệu Tầng 1/Tầng 2 có thể được:
	- Import từ file đối tác.
	- Hoặc tạo thủ công bởi Admin (chi tiết allowed flow TBD trong Business-Rules).
- Xoá User vật lý (hard delete) **không được phép** trong môi trường chạy thực tế; chỉ cho phép đánh dấu is_deleted và giữ lịch sử theo DB-Design-Rules.

## Permissions (AuthZ)
- Phân quyền chi tiết theo vai trò sẽ được điều khiển bởi module ADMIN, nhưng USERS phải định nghĩa các hành động và kiểm tra mức quyền tối thiểu:
	- User (Tầng 3):
		- Xem hồ sơ cá nhân.
		- Cập nhật các trường được cho phép (ví dụ: email, một số thông tin liên lạc bổ sung; danh sách trường cụ thể TBD).
	- Tầng 2:
		- Xem danh sách Tầng 3 thuộc mạng lưới của mình (chỉ đọc) để hỗ trợ tư vấn/bán hàng.
	- Tầng 1:
		- Xem danh sách Tầng 2 và Tầng 3 thuộc mạng lưới của mình (chỉ đọc).
	- Admin:
		- Import danh sách User.
		- Chỉnh sửa hồ sơ User trong phạm vi cho phép (không vượt quá policy của Công ty/đối tác).
		- Khoá/mở khoá User nếu có quyền tương ứng.
	- Super Admin:
		- Thực hiện các thao tác nhạy cảm: đổi Ref Tier hàng loạt, chuyển User giữa mạng lưới, khôi phục User khoá, tuỳ theo policy.
- Mọi thao tác ghi (create/update/delete logic) trong USERS phải được kiểm tra token và quyền từ AUTH/ADMIN, deny-by-default nếu không đủ quyền.

## Business rules (module-specific)

Lưu ý: Mã BR chi tiết sẽ được quản lý tập trung trong Business-Rules.md; ở đây chỉ tham chiếu ở mức khung, không tự tạo nội dung ngoài tài liệu Discovery.

- BR-USERS-001 — Khoá định danh User khi import:
	- Tập hợp trường dùng để xác định một User là duy nhất (ví dụ: mã nhân viên + Công ty; hoặc số điện thoại + Công ty) sẽ được chốt với khách hàng (TBD).
	- Nếu file import chứa bản ghi trùng khoá định danh, hệ thống phải cập nhật hồ sơ hiện có thay vì tạo User mới (chi tiết field nào overwrite, field nào giữ nguyên sẽ được định nghĩa sau).
- BR-USERS-002 — Tính hợp lệ Ref Tier:
	- Mỗi bản ghi import có thể chứa Ref Tier (mã nhân viên/ID của Tầng cha) dùng để gán tầng:
		- Đối với Tầng 2: Ref Tier trỏ tới Tầng 1.
		- Đối với Tầng 3: Ref Tier trỏ tới Tầng 2.
	- Nếu Ref Tier không tồn tại trong hệ thống tại thời điểm import, bản ghi phải bị đánh lỗi và không tạo/cập nhật User, trừ khi có chính sách khác được khách hàng chấp thuận (TBD).
- BR-USERS-003 — Ràng buộc dữ liệu do Công ty cung cấp:
	- Một số trường (ví dụ: Công ty, Điểm nhận, tầng, mã nhân viên) chỉ được chỉnh sửa thông qua luồng do Admin thực hiện hoặc qua file do Công ty cung cấp, không cho User tự chỉnh sửa.
- BR-USERS-004 — Quản lý trạng thái User:
	- Định nghĩa chuyển đổi trạng thái (Active, Inactive, Locked) phải đồng bộ với quy tắc toàn cục trong SRS-00-Overview và Business-Rules.
	- Khi User bị khoá, mọi module khác phải tôn trọng trạng thái này và từ chối thao tác nghiệp vụ quan trọng (đặt hàng, thanh toán mới…).

Các business rule bổ sung (ví dụ: policy về merge User trùng nhau, xử lý khi Công ty thay đổi cấu trúc nhân sự) sẽ được bổ sung sau khi có quyết định từ khách hàng.

## States / status transitions

### State list (User)
- Draft (nếu dùng): User đã import/tạo nhưng chưa đủ thông tin KYC để sử dụng các tính năng trả chậm; chi tiết cần xác nhận (TBD).
- Active: User hợp lệ, được phép đăng nhập (nếu đã có tài khoản AUTH), đặt hàng, phát sinh giao dịch.
- Inactive: User không còn được sử dụng cho các giao dịch mới (ví dụ: đã nghỉ việc) nhưng vẫn được giữ để tra cứu lịch sử; cụ thể luồng kích hoạt trạng thái này TBD.
- Locked: User bị khoá do vi phạm hoặc theo yêu cầu đối tác; không được phép đăng nhập/đặt đơn mới.
- Deleted (logic): cờ is_deleted dùng cho các trường hợp cần ẩn User khỏi UI thông thường nhưng vẫn giữ trong DB.

### Transition rules (mô tả khung)
- Draft → Active:
	- Khi User được xác nhận đủ thông tin KYC theo tiêu chí do đối tác/CoShare chốt (TBD).
- Active → Inactive:
	- Khi Admin/Super Admin thao tác theo yêu cầu Công ty (ví dụ: nhân sự nghỉ việc) hoặc theo job đồng bộ dữ liệu từ file HR mới.
- Active → Locked:
	- Khi phát hiện vi phạm hoặc yêu cầu từ Công ty (chi tiết lý do, nghiệp vụ cụ thể TBD).
- Locked → Active:
	- Chỉ Super Admin (hoặc Admin có quyền cao) được phép sau khi xử lý xong nguyên nhân khoá.
- Mọi chuyển trạng thái phải được:
	- Ghi nhận người thực hiện, thời gian, lý do.
	- Đảm bảo các module khác (AUTH, ORDERS, PAYMENTS) đọc được trạng thái cập nhật.

## Edge cases
- E1 — User tồn tại nhiều lần trong file import:
	- Nếu file chứa nhiều dòng mô tả cùng một User theo khoá định danh (BR-USERS-001), hệ thống phải:
		- Từ chối import các dòng trùng lặp và đánh dấu lỗi.
		- Hoặc chỉ nhận một bản ghi và log cảnh báo; hành vi cụ thể TBD theo yêu cầu khách hàng.
- E2 — Ref Tier không tồn tại hoặc sai tầng:
	- Khi import, nếu Ref Tier không tồn tại hoặc thuộc tầng không phù hợp (ví dụ: Ref Tier được khai là Tầng 3 nhưng đang import bản ghi Tầng 3 cần cha là Tầng 2), hệ thống phải đánh lỗi bản ghi đó và không tạo/cập nhật User.
- E3 — Người dùng đổi Công ty hoặc Điểm nhận:
	- Trường hợp Công ty gửi file HR mới với thông tin User chuyển từ Công ty A sang Công ty B hoặc đổi Điểm nhận mặc định, hệ thống phải cập nhật thông tin này theo rule được khách hàng chốt (TBD) và đảm bảo không làm mất liên kết lịch sử đơn hàng cũ.
- E4 — User bị khoá nhưng vẫn có đơn treo:
	- Khi User bị chuyển sang Locked trong khi vẫn còn:
		- Đơn hàng ở trạng thái đang xử lý.
		- Công nợ chưa thanh toán.
	- Hệ thống phải **không** huỷ các đơn/công nợ hiện tại một cách tự động; cách xử lý sẽ theo rule của ORDERS/PAYMENTS nhưng phải dựa trên trạng thái User.
- E5 — Import sai cấu trúc file:
	- Nếu file import không đúng định dạng mẫu (ví dụ: thiếu cột bắt buộc), hệ thống phải từ chối cả lô và trả về thông báo lỗi rõ ràng, không tạo/cập nhật User nào.

## Non-functional notes (module-specific)
- Rate limits:
	- Các API tra cứu danh sách User phải được bảo vệ bằng rate limit hợp lý (chi tiết sẽ được xác định trong giai đoạn thiết kế kỹ thuật) để tránh lạm dụng.
	- Chức năng import file được giới hạn kích thước/lần thực hiện (tối đa số dòng/lần TBD) để không ảnh hưởng hiệu năng hệ thống.
- Audit logs:
	- Bắt buộc ghi log cho các thao tác:
		- Import danh sách User.
		- Thay đổi Ref Tier.
		- Thay đổi các trường KYC quan trọng.
		- Thay đổi trạng thái User (Active/Inactive/Locked).
	- Log phải bao gồm correlationId, người thao tác, thời gian, dữ liệu chính liên quan (không bao gồm PII nhạy cảm theo quy định Data-Classification).
- Performance hotspots:
	- Tra cứu danh sách User theo nhiều tiêu chí và join theo tầng/Công ty có thể là điểm nóng hiệu năng; cần index phù hợp (theo DB-Design-Rules).
	- Import số lượng lớn User cần được xử lý theo lô (batch) và có thể chạy bất đồng bộ, nhưng vẫn đảm bảo kết quả cuối cùng rõ ràng cho người dùng vận hành.

## Open questions & assumptions (USERS-specific)
- Câu hỏi mở (trích từ Open-Questions Discovery, không tự tạo mới ngoài phạm vi):
	- Q-USERS-001: Trường nào là khoá định danh chính của User trong file HR (mã nhân viên, số điện thoại, hay kết hợp)?
	- Q-USERS-002: Khách hàng muốn cho phép Tầng 1/Tầng 2 chỉnh sửa thông tin nào của Tầng 3 (nếu có), hay hoàn toàn read-only?
	- Q-USERS-003: Luồng tạo mới Tầng 1/Tầng 2 do ai khởi tạo (CoShare hay Công ty/NCC), có được tạo trực tiếp trên Admin Portal hay chỉ qua import?
	- Q-USERS-004: Có cho phép một User thuộc nhiều Công ty hoặc nhiều Điểm nhận không, hay bắt buộc 1-1 tại một thời điểm?
	- Q-USERS-005: Chính sách dữ liệu khi nhân sự nghỉ việc (bao lâu thì chuyển Inactive, có xoá logic không?).
- Giả định (cần xác nhận với khách hàng):
	- A-USERS-001: Mỗi User Tầng 3 chỉ thuộc đúng **một** Công ty và một Điểm nhận mặc định tại một thời điểm.
	- A-USERS-002: Mỗi User Tầng 3 chỉ có **một** Tầng 2 cha, và mỗi Tầng 2 chỉ có **một** Tầng 1 cha trong mô hình affiliate 3 tầng.
	- A-USERS-003: Tất cả thông tin nhân sự ban đầu được cung cấp thông qua file từ Công ty/đối tác; không có luồng tự đăng ký Tầng 3 thuần tuý từ ngoài hệ thống trong MVP.
	- A-USERS-004: Merge hai User bị trùng (do sai sót import) là thao tác hiếm; chi tiết luồng merge sẽ được thiết kế sau nếu khách hàng yêu cầu.
	- A-USERS-005: Các trường KYC nhạy cảm (CCCD, ngày sinh...) sẽ được mã hoá/ẩn một phần khi hiển thị theo quy định bảo mật của khách hàng.

