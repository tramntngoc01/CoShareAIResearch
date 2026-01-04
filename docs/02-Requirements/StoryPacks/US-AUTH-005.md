# StoryPack — US-AUTH-005: Đăng nhập Admin Portal

> Single entry point for AI + Humans. Do not guess. Follow links.

## 0) Metadata
- Story ID: **US-AUTH-005**
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
- User goal: Admin (Super Admin/Ops/QC/Finance/Support) có thể đăng nhập vào Admin Portal bằng SĐT/Email và mật khẩu.
- Business value: Đảm bảo chỉ người có tài khoản Admin hợp lệ, được gán role đúng, mới truy cập được các chức năng vận hành nhạy cảm.

**Out of scope**
- OOS-1: Quản lý chi tiết account Admin (tạo/sửa/xoá) — thuộc module ADMIN.
- OOS-2: Đăng nhập End User (đã được bao phủ bởi các story khác trong AUTH).

---

## 2) Source of truth (links)
- SRS: `docs/02-Requirements/SRS/SRS-AUTH.md#fr-auth-005`
- Stories: `docs/02-Requirements/Stories/Stories-AUTH.md#us-auth-005-—-đăng-nhập-admin-portal`
- Business rules: `docs/02-Requirements/Business-Rules.md#BR-???`
- OpenAPI: `docs/03-Design/OpenAPI/openapi.yaml`
- DB: `docs/03-Design/DB/DB-AUTH.md`
- Implementation Plan (Approved): `docs/03-ImplementationPlan/Implementation-Plan-AUTH.md#US-AUTH-005`
- Test Cases: `docs/03-Testing/TestCases/TestCases-AUTH.md`

---

## 3) Acceptance Criteria (measurable) — summary
- AC1: Admin với thông tin đăng nhập hợp lệ có thể đăng nhập và thấy Dashboard phù hợp với role của mình.
- AC2: End User (không phải Admin) không thể đăng nhập Admin Portal dù nhập đúng thông tin.
- AC3: Mỗi lần đăng nhập (thành công/thất bại) được log với thời gian, user/admin id và correlationId, không lộ mật khẩu.
- AC4: Thông báo lỗi khi đăng nhập thất bại không tiết lộ chi tiết về tài khoản (ví dụ không phân biệt sai email hay sai mật khẩu).

## 4) Sample data (fake)
- Login request:
	- email: "ops.admin@example.com"
	- password: "P@ssw0rd!" (ví dụ — hệ thống lưu dạng hash)
- Successful response (conceptual):
	- adminUserId: 9001
	- roles: ["Ops"]
	- adminSessionToken: "admin_sess_abc123"

## 5) Edge cases (min 3)
- EC1: Admin nhập sai mật khẩu nhiều lần vượt quá ngưỡng → Tài khoản bị khoá tạm thời theo rule, log sự kiện bảo mật.
- EC2: Tài khoản có nhiều role (Ops + Support) → Sau đăng nhập, giao diện hiển thị đầy đủ chức năng tương ứng, không cho phép chức năng vượt quyền.
- EC3: End User cố đăng nhập vào Admin Portal bằng thông tin End User → Bị từ chối, thông báo chung, log sự kiện.
- EC4: Tài khoản Admin đã ngưng hoạt động (disabled) → Không đăng nhập được, hiển thị thông báo chung, không lộ chi tiết.

---

## 6) API mapping (contract-first)
> **Owner: Tech Lead.** BA must not guess API paths. Tech Lead fills this based on OpenAPI.

List exact OpenAPI paths + operations.
- `POST /api/v1/auth/login` — Admin Portal login with username (email/phone) + password.

**Main request/response schemas** (OpenAPI):
- `LoginRequest` → `LoginResponse` (with `admin` profile + `roles`).

**Error codes used**
- `AUTH_INVALID_CREDENTIALS` — username/password pair invalid.
- `AUTH_ADMIN_LOCKED` — admin account locked due to policy (too many failures, disabled, etc.).
- `AUTH_ADMIN_NOT_AUTHORIZED` — principal exists but has no Admin role configured in ADMIN.

**Idempotency**
- Required? **No** — login is not idempotent in the same sense as writes; each request evaluates credentials independently.
- Key header: N/A.
- Conflict behavior: N/A.

**Pagination/filtering**
- N/A

---

## 7) Data & DB mapping
**Tables impacted**
- AUTH (R/W: thông tin xác thực Admin, token đăng nhập Admin).
- ADMIN (R: thông tin role, phân quyền Admin).

**Migration**
- Needed? Yes (nếu chưa có bảng/column cho admin credentials hoặc link tới role trong ADMIN).
- Migration notes: mật khẩu phải được lưu dạng hash (BCrypt/Argon2), không plaintext.
- Rollback notes: Xoá các column/bảng mới nếu quyết định đổi cơ chế đăng nhập.

---

## 8) Authorization & Security notes
**Roles/permissions**
- Chỉ user được gán role Admin (Super Admin/Ops/QC/Finance/Support) mới sử dụng được luồng này.

**Security sanity**
- Bảo vệ endpoint đăng nhập Admin khỏi brute force (rate limit, captcha nếu cần trong tương lai).
- Không log mật khẩu, chỉ log username/email, trạng thái, correlationId.
- Tuân thủ deny-by-default: sau đăng nhập, chỉ cấp quyền dựa trên role đã gán từ ADMIN.

---

## 9) Test plan (proof) — required IDs

### Unit tests
- UT-AUTH-005: Logic xác thực Admin, khoá tài khoản sau nhiều lần sai.

### Integration tests
- IT-AUTH-005: Login Admin giữa AUTH và ADMIN (role mapping).

### E2E/Smoke (staging)
- E2E-AUTH-005: Admin hợp lệ đăng nhập, thấy dashboard phù hợp; End User thử đăng nhập bị từ chối.

### Security sanity
- SEC-AUTH-008: Negative tests brute force login.
- SEC-AUTH-009: Đảm bảo không log mật khẩu, thông điệp lỗi không lộ thông tin nhạy cảm.

---

## 10) “Done” evidence checklist (copy into PR)
- [ ] AC covered (US-AUTH-005)
- [ ] OpenAPI implemented exactly (Tech Lead to confirm)
- [ ] AuthZ enforced + negative test exists
- [ ] Tests pass (UT-AUTH-005, IT-AUTH-005, E2E-AUTH-005, SEC-AUTH-008..009)
- [ ] No secrets / no PII logs / correlationId present
- [ ] Staging deploy + smoke evidence link
