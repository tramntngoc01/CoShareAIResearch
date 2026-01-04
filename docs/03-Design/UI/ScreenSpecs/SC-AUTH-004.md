# SC-AUTH-004 — End User Portal Shell / Header

## Purpose
- Khung giao diện (shell/header) hiển thị trạng thái đăng nhập của End User và chứa nút "Đăng xuất".
- Được sử dụng trong US-AUTH-003 (trạng thái sau auto-login) và US-AUTH-004 (logout).

## Fields
- Tên hiển thị người dùng (ví dụ: họ tên rút gọn, hoặc lời chào).
- Tầng/công ty (hiển thị hoặc không tùy thiết kế Figma).
- Nút "Đăng xuất".

## Validations
- Chỉ hiển thị shell khi người dùng đã đăng nhập hợp lệ (có token hợp lệ).

## States
- Logged-in: hiển thị tên người dùng, menu tài khoản, nút Đăng xuất.
- Logged-out: shell hiển thị tối giản (ví dụ chỉ logo, nút Đăng nhập/Đăng ký).

## Actions
- Click "Đăng xuất" để gọi luồng logout.
- (Tùy thiết kế) mở menu tài khoản, điều hướng sang màn hình hồ sơ.

## Permissions
- Logged-in state: yêu cầu token hợp lệ.
- Logged-out state: public.

## Error messages (conceptual)
- Không hiển thị lỗi trực tiếp; lỗi logout (nếu có) nên xử lý im lặng hoặc bằng thông báo toast nhẹ theo design.

## Open Questions
- Chi tiết thông tin hiển thị trong header (chỉ tên, hay thêm ảnh avatar, tầng, công ty...).
- Cách xử lý khi logout thất bại về mặt kỹ thuật (giữ trạng thái hay vẫn đưa về màn hình đăng nhập).
