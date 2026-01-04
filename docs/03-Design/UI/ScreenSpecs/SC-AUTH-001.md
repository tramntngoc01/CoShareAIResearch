# SC-AUTH-001 — End User Registration

## Purpose
- Màn hình cho phép công nhân KCN (Tầng 3) đăng ký tài khoản bằng SĐT, Họ tên, chọn Công ty.
- Khởi phát luồng gửi OTP ZNS cho story US-AUTH-001.

## Fields
- Họ tên (text, bắt buộc).
- Công ty (dropdown, bắt buộc; dữ liệu từ danh sách Công ty cấu hình trước).
- Số điện thoại (text, bắt buộc).
- Checkbox chấp nhận điều khoản (boolean, bắt buộc để cho phép gửi OTP).

## Validations
- Họ tên: không rỗng, độ dài tối thiểu/tối đa sẽ được UX/Tech Lead chốt (Open Question).
- Công ty: phải chọn một giá trị hợp lệ trong danh sách.
- SĐT: không rỗng, đúng pattern SĐT Việt Nam (pattern cụ thể TBD).
- Checkbox điều khoản: phải được tick trước khi cho phép gửi OTP.

## States
- Idle: form trống hoặc sau khi load lần đầu.
- Loading: khi gửi yêu cầu OTP.
- Success: OTP đã gửi, điều hướng/hiển thị sang SC-AUTH-002.
- Error: lỗi validation tại client hoặc lỗi từ server (ZNS lỗi, cấu trúc request không hợp lệ...).

## Actions
- Nhập/chỉnh sửa các trường trên form.
- Nhấn nút "Gửi OTP" / "Đăng ký" để bắt đầu luồng OTP.
- (Nếu có trong thiết kế) điều hướng sang màn hình Đăng nhập thay vì đăng ký.

## Permissions
- Public (chưa cần đăng nhập).

## Error messages (conceptual)
- Lỗi validation field (ví dụ: "Vui lòng nhập số điện thoại hợp lệ").
- Lỗi gửi OTP (ví dụ: "Không thể gửi OTP, vui lòng thử lại sau").

## Open Questions
- Quy tắc cụ thể về độ dài/tập ký tự hợp lệ của Họ tên.
- Thông điệp chi tiết hiển thị khi SĐT không có trong danh sách nhân sự (USERS).
- Có cần hiển thị link tới điều khoản sử dụng/CS bảo mật ngay trên màn hình không.
