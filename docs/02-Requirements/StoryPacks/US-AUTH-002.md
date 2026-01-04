# StoryPack — US-AUTH-002: Đăng nhập End User lần đầu bằng OTP

> Single entry point for AI + Humans. Do not guess. Follow links.

## 0) Metadata
- Story ID: **US-AUTH-002**
- Module: **AUTH**
- Priority: P0
- Status: Draft
- Owner (BA):
- Reviewers: Tech Lead / QA
- API mapping owner: Tech Lead
- Target sprint/release:
- Last updated:

---

## 1) What to build (1–2 paragraphs)
- User goal: End User đã có tài khoản có thể đăng nhập lần đầu bằng SĐT + OTP ZNS, không cần mật khẩu.
- Business value: Đơn giản hóa trải nghiệm đăng nhập cho người dùng mới, giảm chi phí hỗ trợ quên mật khẩu, vẫn đảm bảo xác thực qua SĐT và OTP.

**Out of scope**
- OOS-1: Đăng nhập sử dụng mật khẩu truyền thống (password-based login) cho End User.
- OOS-2: Cơ chế đăng nhập Admin Portal (được xử lý riêng trong US-AUTH-005).

---

## 2) Source of truth (links)
- SRS: `docs/02-Requirements/SRS/SRS-AUTH.md#fr-auth-002`
- Stories: `docs/02-Requirements/Stories/Stories-AUTH.md#us-auth-002-—-đăng-nhập-end-user-lần-đầu-bằng-otp`
- Business rules: `docs/02-Requirements/Business-Rules.md#BR-???`
- OpenAPI: `docs/03-Design/OpenAPI/openapi.yaml`
- DB: `docs/03-Design/DB/DB-AUTH.md`
- Implementation Plan (Approved): `docs/03-ImplementationPlan/Implementation-Plan-AUTH.md#US-AUTH-002`
- Test Cases: `docs/03-Testing/TestCases/TestCases-AUTH.md`

---

## 3) Acceptance Criteria (measurable) — summary
- AC1: Khi người dùng nhập SĐT đã đăng ký, hệ thống gửi OTP ZNS và hiển thị bước nhập OTP.
- AC2: Khi OTP đúng và còn hiệu lực, hệ thống cấp Session Token + Refresh Token và cho phép truy cập Portal End User.
- AC3: Nếu SĐT chưa có tài khoản, hệ thống không gửi OTP và hiển thị thông báo phù hợp.
- AC4: Sau đăng nhập thành công, người dùng thấy giao diện phù hợp với tier (Tầng 1/2/3) theo dữ liệu từ USERS.

## 4) Sample data (fake)
- Example login request:
	- phone: "0912345678"
- OTP verification input:
	- otpCode: "112233"
- Example login response (conceptual):
	- sessionToken: "sess_abc123_fake"
	- refreshToken: "ref_def456_fake"
	- user: { userId: 10001, tier: 3, companyId: 501 }

## 5) Edge cases (min 3)
- EC1: Sai OTP quá số lần cho phép → Hệ thống khoá luồng OTP tạm thời cho SĐT đó, log chi tiết sự kiện.
- EC2: OTP hết hạn đúng lúc người dùng nhấn xác nhận → Hệ thống từ chối, hiển thị thông báo OTP hết hạn, cho phép gửi lại nếu chưa vượt giới hạn.
- EC3: Tài khoản tương ứng với SĐT đã bị khoá (Locked) trong USERS → Hệ thống không đăng nhập, hiển thị thông báo chung (không lộ lý do), log trạng thái khoá.
- EC4: Người dùng nhập SĐT không thuộc hệ thống → Không gửi OTP, hiển thị thông báo "Tài khoản chưa được đăng ký" hoặc thông điệp tương đương.

---

## 6) API mapping (contract-first)
> **Owner: Tech Lead.** BA must not guess API paths. Tech Lead fills this based on OpenAPI.

List exact OpenAPI paths + operations.
- `POST /api/v1/auth/end-user/login/request-otp` — validate existing phone, trigger OTP for first-time login.
- `POST /api/v1/auth/end-user/login/verify-otp` — verify OTP, issue Session/Refresh tokens.

**Main request/response schemas** (OpenAPI):
- `EndUserLoginStartRequest` → 202 Accepted (no body) for `request-otp`.
- `EndUserLoginVerifyRequest` → `LoginResponse` for `verify-otp`.

**Error codes used**
- `AUTH_INVALID_PHONE_FORMAT` — phone fails server-side validation.
- `AUTH_USER_NOT_FOUND` — phone not associated with any existing End User.
- `AUTH_USER_LOCKED` — End User exists but is Locked/Inactive in USERS.
- `AUTH_OTP_RATE_LIMITED` — too many OTP requests for this phone.
- `AUTH_OTP_INVALID_OR_EXPIRED` — wrong or expired OTP on verification.

**Idempotency**
- Required? **Yes**, for both `request-otp` and `verify-otp`.
- Key header: `Idempotency-Key`.
- Conflict behavior: 409 with `AUTH_IDEMPOTENCY_CONFLICT` if the same key is reused with different payload.

**Pagination/filtering**
- N/A (login is not paginated)

---

## 7) Data & DB mapping
**Tables impacted**
- AUTH (session/refresh token store; chi tiết trong DB-AUTH).
- USERS (R: xác nhận tồn tại user, trạng thái, tier, company).

**Migration**
- Needed? Maybe (nếu chưa có bảng/token store cho Session/Refresh Tokens).
- Migration notes: BA đánh dấu nhu cầu; chi tiết cột và index do Tech Lead + DB Designer xác định.
- Rollback notes: Xoá bảng/token store mới hoặc cột liên quan nếu rollback.

---

## 8) Authorization & Security notes
**Roles/permissions**
- Public (chưa đăng nhập) được phép gọi luồng đăng nhập này.
- Các API nội bộ xác thực End User phải từ chối truy cập nếu không có token hợp lệ.

**Security sanity**
- Token phải là JWT/ngẫu nhiên đủ entropy, không chứa PII trực tiếp.
- Giới hạn số lần thử OTP, khoá tạm thời nếu nghi ngờ brute force.
- Không log token đầy đủ; chỉ log truncated hoặc ID tham chiếu.

---

## 9) Test plan (proof) — required IDs

### Unit tests
- UT-AUTH-002: Logic kiểm tra OTP, TTL, giới hạn sai, sinh token.

### Integration tests
- IT-AUTH-002: Luồng login AUTH + NOTIFICATIONS (OTP) + USERS.

### E2E/Smoke (staging)
- E2E-AUTH-002: Người dùng đã đăng ký đăng nhập lần đầu thành công, truy cập Portal.

### Security sanity
- SEC-AUTH-003: Negative login tests với OTP sai, OTP hết hạn, user bị khoá.
- SEC-AUTH-004: Kiểm tra token không lộ PII, không log token full.

---

## 10) “Done” evidence checklist (copy into PR)
- [ ] AC covered (US-AUTH-002)
- [ ] OpenAPI implemented exactly (Tech Lead to confirm)
- [ ] AuthZ enforced + negative test exists
- [ ] Tests pass (UT-AUTH-002, IT-AUTH-002, E2E-AUTH-002, SEC-AUTH-003..004)
- [ ] No secrets / no PII logs / correlationId present
- [ ] Staging deploy + smoke evidence link
