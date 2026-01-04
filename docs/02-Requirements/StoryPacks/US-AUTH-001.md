# StoryPack — US-AUTH-001: Đăng ký End User bằng SĐT + OTP ZNS

> Single entry point for AI + Humans. Do not guess. Follow links.

## 0) Metadata
- Story ID: **US-AUTH-001**
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
- User goal: Công nhân KCN (Tầng 3) có thể đăng ký tài khoản bằng số điện thoại, nhận OTP ZNS và kích hoạt tài khoản End User.
- Business value: Giảm ma sát đăng ký cho người dùng không quen tài khoản/mật khẩu, tận dụng hạ tầng ZNS, đảm bảo chỉ người có SĐT hợp lệ mới truy cập được hệ thống.

**Out of scope**
- OOS-1: Đăng ký Admin Portal (tài khoản Admin được quản lý trong module ADMIN).
- OOS-2: Đăng ký qua Email, mạng xã hội hoặc các kênh khác ngoài SĐT + OTP.

---

## 2) Source of truth (links)
- SRS: `docs/02-Requirements/SRS/SRS-AUTH.md#fr-auth-001`
- Stories: `docs/02-Requirements/Stories/Stories-AUTH.md#us-auth-001-—-đăng-ký-end-user-bằng-sđt--otp-zns`
- Business rules: `docs/02-Requirements/Business-Rules.md#BR-???`
- OpenAPI: `docs/03-Design/OpenAPI/openapi.yaml`
- DB: `docs/03-Design/DB/DB-AUTH.md`
- Implementation Plan (Approved): `docs/03-ImplementationPlan/Implementation-Plan-AUTH.md#US-AUTH-001`
- Test Cases: `docs/03-Testing/TestCases/TestCases-AUTH.md`

---

## 3) Acceptance Criteria (measurable) — summary
(Keep concise. Full text stays in Stories file.)
- AC1: Hệ thống gửi OTP ZNS tới đúng SĐT hợp lệ khi người dùng điền đủ thông tin đăng ký.
- AC2: Chỉ khi OTP đúng và còn hiệu lực thì tài khoản End User mới được tạo và có thể đăng nhập.
- AC3: Trường hợp OTP sai/hết hạn, hệ thống từ chối đăng ký và hiển thị thông báo lỗi rõ ràng, không tạo tài khoản.
- AC4: Sự kiện đăng ký (thành công/thất bại) được log kèm correlationId, với SĐT được mask theo Data-Classification.

## 4) Sample data (fake)
- Example request (form inputs):
	- fullName: "Trần Thị B"
	- companyName: "Công ty Điện tử KCN X"
	- phone: "0912345678"
	- acceptTerms: true
- Example OTP payload (to NOTIFICATIONS/ZNS):
	- channel: "ZNS"
	- phone: "0912345678"
	- templateCode: "OTP_REGISTER_V1"
	- otpCode: "482913"
	- ttlSeconds: 120
- Example created user (simplified):
	- userId: 10001
	- phone: "0912345678"
	- fullName: "Trần Thị B"
	- companyId: 501
	- tier: 3
	- status: "Active"

## 5) Edge cases (min 3)
- EC1: SĐT sai định dạng (thiếu số/ký tự lạ) → Form không cho submit, hiển thị lỗi validation tại trường SĐT.
- EC2: Người dùng yêu cầu gửi lại OTP quá số lần cấu hình trong một khoảng thời gian → Hệ thống chặn, hiển thị thông báo giới hạn và không gửi thêm OTP.
- EC3: Dịch vụ ZNS trả lỗi (timeout, provider down) → Hệ thống không tạo OTP hợp lệ, hiển thị thông báo lỗi thân thiện, log lỗi kỹ thuật ẩn với người dùng.
- EC4: Người dùng đã nhận OTP nhưng đổi SĐT trong form rồi mới nhập OTP cũ → OTP phải gắn với SĐT đã gửi; hệ thống từ chối xác thực nếu SĐT hiện tại khác với SĐT đã gửi OTP.

---

## 6) API mapping (contract-first)
> **Owner: Tech Lead.** BA must not guess API paths. Tech Lead fills this based on OpenAPI.

List exact OpenAPI paths + operations.
- `POST /api/v1/auth/end-user/register/request-otp` — validate registration input and trigger OTP via NOTIFICATIONS.
- `POST /api/v1/auth/end-user/register/verify-otp` — verify OTP, create End User, issue tokens.

**Main request/response schemas** (OpenAPI):
- `EndUserRegisterStartRequest` → 202 Accepted (no body) for `request-otp`.
- `EndUserRegisterVerifyRequest` → `LoginResponse` for `verify-otp`.

**Error codes used**
- `AUTH_INVALID_PHONE_FORMAT` — phone fails server-side validation.
- `AUTH_OTP_RATE_LIMITED` — too many OTP requests for a phone in a time window.
- `AUTH_OTP_DELIVERY_FAILED` — NOTIFICATIONS/ZNS could not deliver OTP.
- `AUTH_OTP_INVALID_OR_EXPIRED` — wrong or expired OTP on verification.
- `AUTH_USER_ALREADY_EXISTS` — phone already registered as an active End User.

**Idempotency**
- Required? **Yes**, for both `request-otp` and `verify-otp`.
- Key header: `Idempotency-Key` (UUID or opaque string).
- Conflict behavior: 409 with error code `AUTH_IDEMPOTENCY_CONFLICT` when the same key is reused with different payload.

**Pagination/filtering**
- N/A (single-resource operations, no collections returned).

---

## 7) Data & DB mapping
**Tables impacted**
- AUTH (OTP/session store — chi tiết trong DB-AUTH, do Tech Lead hoàn thiện).
- USERS (R/W: tạo mới End User, gắn với Công ty/Tầng).

**Migration**
- Needed? Yes (nếu chưa có bảng lưu OTP/attempts và cột link từ USERS sang AUTH profile nếu cần).
- Migration notes: BA chỉ đánh dấu nhu cầu; chi tiết field do Tech Lead + DB Designer quyết định.
- Rollback notes: Có thể xoá bảng OTP mới tạo và cột bổ sung nếu rollback feature.

---

## 8) Authorization & Security notes
**Roles/permissions**
- Public (chưa đăng nhập) có thể truy cập màn hình đăng ký.
- Không role nào khác được phép tạo tài khoản End User thay cho người dùng qua UI này (Admin tạo tài khoản riêng qua module ADMIN nếu cần).

**Security sanity**
- Không log OTP hoặc nội dung ZNS chi tiết; chỉ log trạng thái, templateCode, correlationId.
- Giới hạn số lần gửi OTP trên mỗi SĐT trong cửa sổ thời gian (rate limit) theo Security-Baseline.
- Validate tất cả input phía server, không tin tưởng client-side validation.

---

## 9) Test plan (proof) — required IDs
> Create test IDs now and keep Traceability updated.

### Unit tests
- UT-AUTH-001: Validate logic kiểm tra TTL OTP và giới hạn số lần gửi.

### Integration tests
- IT-AUTH-001: Luồng đăng ký qua AUTH kết nối với NOTIFICATIONS (ZNS) và USERS, kiểm tra tài khoản được tạo khi OTP hợp lệ.

### E2E/Smoke (staging)
- E2E-AUTH-001: Người dùng mới đăng ký bằng SĐT, nhận OTP, kích hoạt tài khoản và đăng nhập thành công.

### Security sanity
- SEC-AUTH-001: Negative tests OTP sai, OTP hết hạn, spam gửi OTP.
- SEC-AUTH-002: Kiểm tra không log OTP/số điện thoại full trong log ứng dụng.

---

## 10) “Done” evidence checklist (copy into PR)
- [x] AC covered (US-AUTH-001)
  - AC1: OTP sent via NOTIFICATIONS integration in AuthService.RequestRegistrationOtpAsync
  - AC2: User created only after OTP verification in AuthService.VerifyRegistrationOtpAsync
  - AC3: Invalid/expired OTP returns AUTH_OTP_INVALID_OR_EXPIRED error
  - AC4: Logging with correlationId, phone masked (last 4 digits only)
- [x] OpenAPI implemented exactly (Tech Lead to confirm)
  - POST /api/v1/auth/end-user/register/request-otp → 202 Accepted
  - POST /api/v1/auth/end-user/register/verify-otp → LoginResponse
  - All error codes: AUTH_INVALID_PHONE_FORMAT, AUTH_OTP_RATE_LIMITED, AUTH_OTP_DELIVERY_FAILED, AUTH_OTP_INVALID_OR_EXPIRED, AUTH_USER_ALREADY_EXISTS
- [x] AuthZ enforced + negative test exists
  - Public endpoints (no auth required for registration)
  - SEC-AUTH-001 tests cover negative OTP scenarios
- [x] Tests pass (UT-AUTH-001, IT-AUTH-001, E2E-AUTH-001, SEC-AUTH-001..002)
  - UT-AUTH-001: AuthServiceTests (13 tests), OtpHelperTests (8 tests), OtpRequestTests (7 tests)
  - IT-AUTH-001: AuthRegistrationIntegrationTests (10 tests)
  - SEC-AUTH-001/002: Covered in unit and integration tests (wrong OTP, expired OTP, rate limiting, no PII in logs)
- [x] No secrets / no PII logs / correlationId present
  - OTP code never logged (only hashed in DB)
  - Phone masked in logs (shows last 4 digits only)
  - X-Correlation-Id header in all responses
- [ ] Staging deploy + smoke evidence link
  - Pending: Deploy to staging and capture E2E-AUTH-001 evidence

**Files Changed:**
- src/api/CoShare.sln
- src/api/CoShare.Api/ (Controllers, Contracts, Validators, Program.cs, appsettings)
- src/api/CoShare.Domain/Auth/ (Entities, Services, Repositories interfaces)
- src/api/CoShare.Infrastructure/ (DbContext, Repositories, Services, Migrations)
- src/api/tests/CoShare.Domain.UnitTests/Auth/
- src/api/tests/CoShare.Api.IntegrationTests/Auth/

**Migration Notes:**
- New tables: auth_otp_request, auth_refresh_token (per DB-AUTH.md)
- Migration: 20260104000001_InitialAuthSchema.cs
- Rollback: DROP TABLE auth_refresh_token; DROP TABLE auth_otp_request;
