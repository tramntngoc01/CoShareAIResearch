# SC-AUTH-003 — End User Login (first time)

## Purpose
- Màn hình cho phép End User nhập SĐT để nhận OTP khi đăng nhập lần đầu (US-AUTH-002).

## Fields
- Số điện thoại (text, bắt buộc).
- Nút "Gửi OTP" / "Đăng nhập".
- Link/chuyển hướng tới màn hình Đăng ký (SC-AUTH-001) nếu chưa có tài khoản.

## Validations
- SĐT: không rỗng, đúng pattern SĐT hợp lệ.

## States
- Idle: chờ người dùng nhập SĐT.
- Loading: đang gửi yêu cầu gửi OTP.
- Success: OTP đã được gửi, chuyển sang SC-AUTH-002.
- Error: SĐT không tồn tại trong hệ thống hoặc không gửi được OTP.

## Actions
- Nhập/chỉnh sửa SĐT.
- Nhấn "Gửi OTP" để bắt đầu luồng đăng nhập.
- Điều hướng qua lại giữa Đăng nhập và Đăng ký.

## Permissions
- Public (chưa đăng nhập).

## Error messages (conceptual)
- "Tài khoản chưa được đăng ký" khi SĐT chưa có trong hệ thống.
- "Không thể gửi OTP, vui lòng thử lại sau" nếu lỗi kỹ thuật.

## Open Questions
- Có tự động chuyển sang OTP nếu người dùng đã từng đăng nhập trên thiết bị này và còn token không, hay luôn yêu cầu SĐT trước.
- Ngôn ngữ và copywriting chính xác cho thông báo lỗi được UX/Marketing cung cấp.
