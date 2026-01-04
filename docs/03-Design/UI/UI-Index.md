# UI Index

## Figma prototype
https://www.figma.com/proto/EKlqNoubSKqb7bH3tGzhmf/CoShare?node-id=43-17&t=61rG8wXkRSIx8B6q-0&scaling=scale-down-width&content-scaling=fixed&page-id=0%3A1&starting-point-node-id=43%3A17&show-proto-sidebar=1

## AUTH module screens

- SC-AUTH-001 — End User Registration form (đăng ký bằng SĐT, Họ tên, Công ty).  
  - Used by: US-AUTH-001
- SC-AUTH-002 — OTP verification dialog/section (nhập mã OTP cho đăng ký/đăng nhập).  
  - Used by: US-AUTH-001, US-AUTH-002
- SC-AUTH-003 — End User Login screen (nhập SĐT để nhận OTP, lần đầu).  
  - Used by: US-AUTH-002
- SC-AUTH-004 — End User Portal shell/header (trạng thái đăng nhập, nút Đăng xuất).  
  - Used by: US-AUTH-003, US-AUTH-004
- SC-AUTH-005 — Admin Login screen (Portal Admin).  
  - Used by: US-AUTH-005

> Ghi chú: Mọi hành vi UI chi tiết phải bám theo prototype Figma và không mở rộng yêu cầu ngoài các stories/SRS hiện tại. Những điểm chưa rõ được ghi ở phần Open Questions trong từng ScreenSpec.
