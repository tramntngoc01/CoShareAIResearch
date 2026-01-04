
# Glossary (Bảng thuật ngữ)

## Quy tắc sử dụng
- Một thuật ngữ = một nghĩa duy nhất
- Tránh dùng từ đồng nghĩa; chọn một thuật ngữ chuẩn và dùng xuyên suốt
- Nếu có từ khác nhau trong tài liệu gốc, ghi vào cột "Từ tránh dùng"

---

## Người dùng & Vai trò

| Thuật ngữ | Định nghĩa | Ghi chú | Từ tránh dùng |
|-----------|------------|---------|---------------|
| User | Người dùng cuối của hệ thống (Tầng 1, 2, 3) | Không bao gồm Admin | Khách hàng, Customer |
| Tầng 1 | Quản lý mạng lưới affiliate, đứng đầu mạng lưới Tầng 2 | Read-only dashboard trong MVP | Tier 1, T1 |
| Tầng 2 | Nhân viên giới thiệu, có thể kiêm Shipper/thu COD | Thuộc mạng lưới của Tầng 1 | Tier 2, T2, Affiliate |
| Tầng 3 | Công nhân KCN, người mua hàng cuối cùng | Thuộc mạng lưới của Tầng 2 | Tier 3, T3, End user |
| Shipper | Vai trò giao hàng tại điểm nhận, có thể là Tầng 2 hoặc user độc lập | Thu COD, chụp POD | Người giao hàng |
| Admin | Nhân viên vận hành hệ thống trên Admin Portal | Bao gồm nhiều role | Quản trị viên |
| Super Admin | Quản trị cấp cao nhất, có toàn quyền | Duyệt hủy đơn, phân quyền | SA |
| Ops | Admin vận hành đơn hàng | Xác nhận xuất kho, tạo yêu cầu hủy | Operations |
| QC | Admin kiểm duyệt lô hàng | Quality Control | Kiểm duyệt viên |
| Finance | Admin đối soát thanh toán, công nợ | Cấu hình trả chậm | Kế toán |
| Support | Admin hỗ trợ khách hàng | - | CSKH |
| NCC | Nhà Cung Cấp, đối tác cung cấp sản phẩm | - | Supplier, Vendor |

---

## Địa điểm & Tổ chức

| Thuật ngữ | Định nghĩa | Ghi chú | Từ tránh dùng |
|-----------|------------|---------|---------------|
| KCN | Khu Công Nghiệp | Nơi các doanh nghiệp đặt trụ sở | Industrial Zone |
| Công ty | Doanh nghiệp trong KCN, nơi công nhân làm việc | Mỗi công ty có cấu hình riêng (trả chậm, điểm nhận) | Company, Enterprise |
| Điểm nhận | Địa điểm trong công ty nơi khách nhận hàng | Mỗi công ty có ≥ 1 điểm nhận mặc định | Pickup Point |

---

## Sản phẩm & Lô hàng

| Thuật ngữ | Định nghĩa | Ghi chú | Từ tránh dùng |
|-----------|------------|---------|---------------|
| Sản phẩm | Mặt hàng bán trên hệ thống | Có mã hàng/mã vạch, thuộc ngành hàng | Product, SP |
| Ngành hàng | Danh mục phân loại sản phẩm | Dùng để tính % hoa hồng | Category, Nhóm hàng |
| Lô hàng | Đơn vị nhập hàng từ NCC, có thông tin kiểm duyệt | MVP: 1 ngành hàng/lô | Batch |
| Lô kiểm duyệt | Lô hàng đã được QC duyệt với đầy đủ tài liệu & ảnh | Bắt buộc trước khi bán | Inspection Batch |
| Tồn kho | Số lượng sản phẩm khả dụng để bán | Có thể đồng bộ với hệ thống kho (future) | Inventory, Stock |

---

## Đơn hàng & Trạng thái

| Thuật ngữ | Định nghĩa | Ghi chú | Từ tránh dùng |
|-----------|------------|---------|---------------|
| Đơn hàng | Yêu cầu mua hàng của User | Có Order ID duy nhất | Order |
| Mã nhận hàng | QR code hoặc PIN để xác thực khi nhận hàng | Hiển thị sau khi đặt hàng thành công | Pickup Code |
| POD | Proof of Delivery, ảnh chứng cứ giao hàng thành công | Shipper chụp | Ảnh giao hàng |
| Phiếu giao hàng | Tài liệu in dán lên sản phẩm khi giao | Format PDF A5 | Shipping Label |

### Trạng thái đơn hàng

| Trạng thái | Định nghĩa | Transition từ | Transition đến |
|------------|------------|---------------|----------------|
| Chờ xác nhận | Đơn mới tạo, chưa xử lý | - | Đã xác nhận, Hủy |
| Chờ thanh toán | Đơn chuyển khoản chờ xác nhận tiền | Chờ xác nhận | Đã xác nhận, Hủy |
| Đã xác nhận | Đơn đã xác nhận, chuẩn bị xuất kho | Chờ xác nhận, Chờ thanh toán | Đang giao, Hủy |
| Đang giao | Hàng đang trên đường đến điểm nhận | Đã xác nhận | Sẵn sàng nhận, Hủy |
| Sẵn sàng nhận | Hàng đã đến điểm nhận, chờ khách | Đang giao | Đã nhận, Hủy |
| Đã nhận | Khách đã nhận hàng thành công | Sẵn sàng nhận | Đổi trả |
| Chờ duyệt hủy | Ops yêu cầu hủy, chờ Super Admin | Mọi trạng thái trừ Đã hủy | Đã hủy, trạng thái cũ |
| Đã hủy | Đơn bị hủy | Chờ duyệt hủy | - |
| Đổi trả | Khách yêu cầu đổi/trả hàng | Đã nhận | - |

---

## Thanh toán & Công nợ

| Thuật ngữ | Định nghĩa | Ghi chú | Từ tránh dùng |
|-----------|------------|---------|---------------|
| COD | Cash On Delivery, thanh toán khi nhận hàng | Tầng 2/Shipper thu hộ | Tiền mặt |
| Chuyển khoản | Thanh toán qua ngân hàng | Hiển thị thông tin TK + cú pháp nội dung | Bank Transfer, CK |
| Trả chậm | Thanh toán sau, đến ngày nhận lương | Bật/tắt theo công ty, có hạn mức | Deferred Payment, Trả sau |
| Hạn mức | Giới hạn tối đa cho trả chậm | Số đơn tối đa + tổng tiền tối đa | Credit Limit |
| Công nợ | Số tiền User còn nợ (trả chậm) | Khóa mua khi quá hạn | Outstanding |
| Phiếu thu | Chứng từ điện tử khi Shipper thu COD | - | Receipt |
| Ca | Đơn vị thời gian làm việc của Shipper | Dùng để đối soát COD | Shift |
| Chốt ca | Kết thúc ca, tổng hợp COD đã thu | Shipper thực hiện | End Shift |
| Đối soát | So khớp số tiền thu với số tiền phải thu | Phát hiện sai lệch | Reconciliation |

---

## Hoa hồng & Affiliate

| Thuật ngữ | Định nghĩa | Ghi chú | Từ tránh dùng |
|-----------|------------|---------|---------------|
| Hoa hồng | % doanh thu trả cho Tầng 1/2 khi có đơn hàng | Tính theo ngành hàng | Commission |
| Affiliate 3 tầng | Mô hình giới thiệu: Tầng 1 → Tầng 2 → Tầng 3 | Mỗi tầng nhận % khác nhau | 3-tier Affiliate |
| Lịch sử hiệu lực | Bản ghi thay đổi % hoa hồng theo thời gian | effective_from, effective_to | Effective History |
| Ref Tier | Mã nhân viên của tầng phụ thuộc (cha) | Dùng khi import danh sách | Parent Tier |

---

## Hủy & Đổi trả

| Thuật ngữ | Định nghĩa | Ghi chú | Từ tránh dùng |
|-----------|------------|---------|---------------|
| Hủy đơn | Hủy đơn hàng theo yêu cầu | Cần Super Admin duyệt | Order Cancellation |
| Hủy hàng | Hủy sản phẩm do hư hại/mất mát | Không phụ thuộc đơn hàng | Inventory Write-off |
| Phiếu hủy hàng | Chứng từ ghi nhận hàng bị hủy | Kèm ảnh chứng cứ | - |
| Đổi trả | Yêu cầu đổi hoặc trả sản phẩm đã nhận | Tại điểm nhận, kèm ảnh | Return/Exchange |

---

## Hệ thống & Kỹ thuật

| Thuật ngữ | Định nghĩa | Ghi chú | Từ tránh dùng |
|-----------|------------|---------|---------------|
| ZNS | Zalo Notification Service | Gửi OTP, thông báo | - |
| OTP | One-Time Password, mã xác thực 1 lần | Gửi qua ZNS | Mã xác thực |
| Session Token | Token đăng nhập, gắn với thiết bị/browser | Dùng sau lần OTP đầu tiên | - |
| Refresh Token | Token làm mới Session Token | Kéo dài phiên đăng nhập | - |
| KYC | Know Your Customer, xác minh danh tính | CCCD, ngày sinh, mã NV | Xác thực danh tính |
| Audit Log | Nhật ký ghi lại mọi thao tác quan trọng | Bắt buộc cho hủy đơn, hủy hàng | Nhật ký |
| correlationId | ID theo dõi request xuyên suốt hệ thống | Bắt buộc trong log | - |

---

## Module hệ thống

| Module | Phạm vi | Ghi chú |
|--------|---------|---------|
| AUTH | Đăng ký, đăng nhập, OTP, token, phân quyền | - |
| USERS | Quản lý user, tầng, KYC, import danh sách | - |
| CATALOG | Sản phẩm, ngành hàng, lô kiểm duyệt, tồn kho | - |
| ORDERS | Đơn hàng, trạng thái, hủy đơn, giao hàng | - |
| PAYMENTS | COD, chuyển khoản, trả chậm, phiếu thu, đối soát | - |
| NOTIFICATIONS | ZNS template, gửi thông báo | - |
| ADMIN | Cấu hình hệ thống, audit log, phân quyền | - |
| REPORTING | Báo cáo doanh số, hoa hồng, công nợ, COD | - |
