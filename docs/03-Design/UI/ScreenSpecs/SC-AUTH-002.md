# SC-AUTH-002 — OTP Verification (End User)

## Purpose
- Màn hình/popup cho phép người dùng nhập mã OTP ZNS để hoàn tất đăng ký hoặc đăng nhập lần đầu.
- Được gọi từ US-AUTH-001 và US-AUTH-002.

## Fields
- Số điện thoại (hiển thị read-only để người dùng xác nhận).
- Trường nhập OTP (text hoặc 6 ô số, bắt buộc).
- Nút "Xác nhận".
- Nút/Link "Gửi lại OTP" (nếu được phép).

## Validations
- OTP: không rỗng, đúng độ dài và format (ví dụ 6 chữ số; chi tiết TBD).
- Số lần nhập sai và số lần gửi lại OTP không vượt quá giới hạn cấu hình.

## States
- Idle: chờ người dùng nhập OTP.
- Verifying: gửi OTP lên server để kiểm tra.
- Success: OTP hợp lệ, tiếp tục sang Portal End User hoặc thông báo đăng ký thành công.
- Error: OTP sai/hết hạn, hiển thị thông báo và cho phép nhập lại/gửi lại (nếu còn trong giới hạn).

## Actions
- Nhập OTP.
- Nhấn "Xác nhận" để kiểm tra OTP.
- Nhấn "Gửi lại OTP" nếu không nhận được mã (trong giới hạn cho phép).

## Permissions
- Public (gắn với session đăng ký/đăng nhập hiện tại, chưa cần token bền vững).

## Error messages (conceptual)
- "Mã OTP không đúng, vui lòng kiểm tra lại".
- "Mã OTP đã hết hạn, vui lòng yêu cầu mã mới".
- "Bạn đã yêu cầu/gõ sai OTP quá số lần cho phép, vui lòng thử lại sau".

## Open Questions
- Số ký tự OTP và thời gian hiệu lực chính xác.
- Số lần tối đa được phép gửi lại/nhập sai OTP.
- Cách hiển thị mãn tính năng bảo mật (ẩn/bỏ qua phần nào trên UI theo yêu cầu Security).
