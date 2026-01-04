
# Open Questions (Danh sách câu hỏi mở)

## Mục tiêu
- Ghi nhận mọi thông tin còn thiếu để requirements có thể đo lường và test được
- Giữ danh sách này gần về 0 trước khi vào Phase 2/3

---

## Câu hỏi mở

| ID | Câu hỏi | Tại sao quan trọng | Owner | Priority | Due Date | Status | Giả định (nếu có) |
|----|---------|-------------------|-------|----------|----------|--------|-------------------|
| Q1 | Target MAU (Monthly Active Users) là bao nhiêu? | Ảnh hưởng đến capacity planning, infra sizing | BA | P1 | | Open | |
| Q2 | Target Conversion Rate (tỷ lệ đặt hàng/lượt truy cập) là bao nhiêu? | Đo lường thành công sản phẩm | BA | P2 | | Open | |
| Q3 | Target Tỷ lệ nhận hàng thành công là bao nhiêu? | KPI vận hành | BA | P2 | | Open | |
| Q4 | Tỷ lệ quá hạn trả chậm chấp nhận được là bao nhiêu %? | Quản lý rủi ro tài chính | BA/Finance | P1 | | Open | |
| Q5 | ZNS đã đăng ký với Zalo chưa? Template OTP đã sẵn sàng? | Blocker cho đăng ký user | Tech Lead | P0 | | Open | Giả định: Đã đăng ký, cần xác nhận |
| Q6 | Chuyển khoản là manual (Admin xác nhận) hay tích hợp Payment Gateway? | Ảnh hưởng đến scope MVP và timeline | BA/Tech Lead | P0 | | Assumption | Giả định: Manual confirm trong MVP |
| Q7 | Khi SĐT không có trong danh sách nhân sự, xử lý thế nào? | Onboarding flow | BA | P1 | | Assumption | Giả định: Gán Tầng 3 mặc định, Admin review sau |
| Q8 | Giới hạn số lần gửi lại OTP là bao nhiêu? Thời gian chờ giữa các lần? | Chống abuse, chi phí ZNS | BA/Tech Lead | P1 | | Assumption | Giả định: 3 lần/10 phút, chờ 60s giữa các lần |
| Q9 | Có cần hỗ trợ offline mode cho Shipper không? | UX khi mất mạng tại điểm nhận | BA/Tech Lead | P2 | | Open | |
| Q10 | Page load time target là bao nhiêu giây? | NFR, UX | Tech Lead | P1 | | Assumption | Giả định: < 3 giây |
| Q11 | Uptime SLA target là bao nhiêu %? | NFR, infra design | Tech Lead/DevOps | P1 | | Assumption | Giả định: 99.5% |
| Q12 | Số lượng công ty pilot trong GĐ3-4 là bao nhiêu? | Test planning, capacity | BA | P2 | | Open | Giả định: 1-2 công ty |
| Q13 | Ngày nhận lương của từng công ty do ai cung cấp và cập nhật? | Cấu hình trả chậm | BA | P1 | | Open | |
| Q14 | Hạn mức trả chậm mặc định là bao nhiêu (số đơn, tổng tiền)? | Cấu hình hệ thống | BA/Finance | P1 | | Open | |
| Q15 | Có cần gửi ZNS nhắc nhận hàng không? Sau bao lâu? | Chi phí ZNS, UX | BA | P2 | | Assumption | Giả định: Có, sau 24h sẵn sàng nhận |
| Q16 | Có cần gửi ZNS nhắc nợ trả chậm không? Trước/sau ngày lương bao lâu? | Thu hồi công nợ | BA/Finance | P1 | | Assumption | Giả định: Có, trước 3 ngày + sau 1 ngày |
| Q17 | Token đăng nhập hết hạn sau bao lâu? Refresh token có thời hạn? | Security vs UX | Tech Lead | P1 | | Assumption | Giả định: Access 1h, Refresh 30 ngày |
| Q18 | Có giới hạn số thiết bị đăng nhập đồng thời không? | Security, chi phí | Tech Lead | P2 | | Assumption | Giả định: 1 thiết bị/user trong MVP |
| Q19 | Format CSV/XLSX import nhân sự có template chuẩn không? | Data quality | BA | P1 | | Open | Giả định: Cột Company, Employee Code, FullName, Phone, Tier, Ref Tier |
| Q20 | Fuzzy matching khi import nhân sự: ngưỡng similarity là bao nhiêu %? | Accuracy vs false positive | Tech Lead | P2 | | Assumption | Giả định: 85% |
| Q21 | Sản phẩm có thể thuộc nhiều ngành hàng không? | DB design, hoa hồng | BA/Tech Lead | P1 | | Assumption | Giả định: 1 sản phẩm = 1 ngành hàng |
| Q22 | Hoa hồng Tầng 1 và Tầng 2 là % cố định hay khác nhau theo ngành hàng? | Tính hoa hồng | BA | P1 | | Open | Giả định: Cấu hình riêng cho Tầng 1/Tầng 2 theo ngành hàng |
| Q23 | Khi thay đổi % hoa hồng, có gửi ZNS thông báo cho Tầng 1/2 không? | Communication | BA | P2 | | Assumption | Giả định: Có |
| Q24 | Thời gian lưu trữ audit log là bao lâu? | Compliance, storage | Tech Lead/DevOps | P2 | | Assumption | Giả định: 2 năm |
| Q25 | Có cần xuất báo cáo ra Excel/PDF không? | Feature scope | BA | P2 | | Assumption | Giả định: Có trong MVP |
| Q26 | Trợ lý AI sử dụng service nào? (OpenAI, Azure, self-hosted?) | Integration, cost | Tech Lead | P1 | | Open | |
| Q27 | Lượt bán sản phẩm cập nhật realtime hay batch (tối hôm trước)? | DB design, performance | Tech Lead | P2 | | Assumption | Giả định: Batch tối hôm trước theo tài liệu |
| Q28 | Số lượng tồn có tích hợp phần mềm kho không? API nào? | Integration scope | Tech Lead | P1 | | Assumption | Giả định: Chừa cổng, không tích hợp trong MVP |
| Q29 | Chính sách đổi trả là file upload hay nhập text? | UX, storage | BA | P2 | | Assumption | Giả định: Upload file/link Google Doc theo tài liệu |
| Q30 | Admin có thể chuyển User từ Tầng 2 này sang Tầng 2 khác không? | Feature scope | BA | P1 | | Assumption | Giả định: Có, Admin xác nhận hoặc User quét mã Tầng 2 mới |

---

## Giả định đã ghi nhận (cần xác nhận)

| ID | Giả định | Liên quan đến Q# | Xác nhận bởi | Ngày xác nhận |
|----|----------|------------------|--------------|---------------|
| A1 | Chuyển khoản là manual confirm trong MVP | Q6 | | |
| A2 | SĐT không có trong danh sách → gán Tầng 3 mặc định | Q7 | | |
| A3 | OTP: 3 lần/10 phút, chờ 60s giữa các lần | Q8 | | |
| A4 | Page load < 3s | Q10 | | |
| A5 | Uptime 99.5% | Q11 | | |
| A6 | 1-2 công ty pilot | Q12 | | |
| A7 | ZNS nhắc nhận hàng sau 24h | Q15 | | |
| A8 | ZNS nhắc nợ: trước 3 ngày + sau 1 ngày | Q16 | | |
| A9 | Access token 1h, Refresh token 30 ngày | Q17 | | |
| A10 | 1 thiết bị/user | Q18 | | |
| A11 | Fuzzy matching 85% | Q20 | | |
| A12 | 1 sản phẩm = 1 ngành hàng | Q21 | | |
| A13 | Gửi ZNS khi thay đổi hoa hồng | Q23 | | |
| A14 | Audit log lưu 2 năm | Q24 | | |
| A15 | Xuất báo cáo Excel/PDF trong MVP | Q25 | | |
| A16 | Lượt bán batch tối hôm trước | Q27 | | |
| A17 | Tồn kho: chừa cổng, không tích hợp MVP | Q28 | | |
| A18 | Chính sách đổi trả: upload file/link | Q29 | | |
| A19 | Admin có thể chuyển User sang Tầng 2 khác | Q30 | | |

---

## Quy trình xử lý

1. **Khi có câu hỏi mới:** Thêm vào bảng với Status = Open
2. **Khi có câu trả lời:** Cập nhật Status = Answered, ghi câu trả lời vào tài liệu liên quan
3. **Khi không có câu trả lời nhưng cần tiến hành:** Ghi Assumption, đánh dấu Status = Assumption
4. **Trước mỗi Phase gate:** Review tất cả Open questions, escalate P0/P1 chưa resolved
