# SC-AUTH-005 — Admin Login

## Purpose
- Màn hình đăng nhập dành cho Admin Portal (Super Admin, Ops, QC, Finance, Support) cho US-AUTH-005.

## Fields
- SĐT hoặc Email (text, bắt buộc; tuỳ thiết kế cho phép một trong hai hoặc tách thành 2 trường).
- Mật khẩu (password, bắt buộc).
- Nút "Đăng nhập".

## Validations
- SĐT/Email không rỗng, đúng format cơ bản.
- Mật khẩu không rỗng.

## States
- Idle: chờ nhập thông tin.
- Authenticating: gửi request xác thực.
- Success: chuyển hướng sang Admin Dashboard.
- Error: thông tin đăng nhập sai hoặc tài khoản không có quyền Admin.

## Actions
- Nhập/chỉnh sửa SĐT/Email và mật khẩu.
- Nhấn "Đăng nhập" để gửi yêu cầu xác thực.

## Permissions
- Public (trước đăng nhập Admin).

## Error messages (conceptual)
- Thông báo lỗi chung cho mọi trường hợp sai thông tin (không tiết lộ trường nào sai), ví dụ: "Thông tin đăng nhập không hợp lệ".
- Thông báo khi tài khoản bị khoá (theo policy), ví dụ: "Tài khoản của bạn tạm thời bị khoá, vui lòng liên hệ quản trị".

## Open Questions
- Có yêu cầu thêm OTP/ZNS hoặc 2FA khác cho Admin không.
- Số lần nhập sai tối đa trước khi khoá tài khoản và thời gian khoá.
- Có cần chức năng "Quên mật khẩu" trên cùng màn hình không, và luồng chi tiết.
