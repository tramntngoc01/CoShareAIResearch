# StoryPack — US-AUTH-003: Đăng nhập tự động bằng Session Token / Refresh Token

> Single entry point for AI + Humans. Do not guess. Follow links.

## 0) Metadata
- Story ID: **US-AUTH-003**
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
- User goal: End User đã đăng nhập trước đó có thể được tự động đăng nhập lại trên cùng thiết bị, dựa vào Session Token/Refresh Token hợp lệ.
- Business value: Giảm số lần yêu cầu OTP, giảm chi phí ZNS và tăng trải nghiệm mượt mà cho người dùng thường xuyên.

**Out of scope**
- OOS-1: Luồng reset/thu hồi token trên tất cả thiết bị (có thể có story riêng nếu cần).
- OOS-2: Quản lý thiết bị tin cậy chi tiết (trusted devices list) ngoài yêu cầu cơ bản của MVP.

---

## 2) Source of truth (links)
- SRS: `docs/02-Requirements/SRS/SRS-AUTH.md#fr-auth-003`
- Stories: `docs/02-Requirements/Stories/Stories-AUTH.md#us-auth-003-—-đăng-nhập-tự-động-bằng-session-token--refresh-token`
- Business rules: `docs/02-Requirements/Business-Rules.md#BR-???`
- OpenAPI: `docs/03-Design/OpenAPI/openapi.yaml`
- DB: `docs/03-Design/DB/DB-AUTH.md`
- Implementation Plan (Approved): `docs/03-ImplementationPlan/Implementation-Plan-AUTH.md#US-AUTH-003`
- Test Cases: `docs/03-Testing/TestCases/TestCases-AUTH.md`

---

## 3) Acceptance Criteria (measurable) — summary
- AC1: Khi Session Token còn hiệu lực, người dùng được tự động đăng nhập mà không cần OTP.
- AC2: Khi Session Token hết hạn nhưng Refresh Token còn hiệu lực, hệ thống cấp Session Token mới và cho phép truy cập không cần OTP.
- AC3: Khi cả Session Token và Refresh Token hết hạn/không hợp lệ, hệ thống yêu cầu đăng nhập lại bằng SĐT + OTP.

## 4) Sample data (fake)
- Before request:
	- sessionToken: "sess_abc123_fake" (expired)
	- refreshToken: "ref_def456_fake" (valid)
- Auto-login request:
	- headers: { Authorization: "Bearer sess_abc123_fake" }
- Refresh flow (conceptual):
	- send: { refreshToken: "ref_def456_fake" }
	- receive: { newSessionToken: "sess_new789_fake", user: { userId: 10001, tier: 3 } }

## 5) Edge cases (min 3)
- EC1: Refresh Token đã bị revoke (user reset bảo mật) nhưng client vẫn gửi → Hệ thống từ chối, yêu cầu đăng nhập lại, log sự kiện bảo mật.
- EC2: Session Token bị chỉnh sửa/corrupted → Hệ thống phát hiện token không hợp lệ, không crash, trả về lỗi chuẩn 401/unauthorized.
- EC3: Đồng hồ client lệch nhiều so với server → Thời gian hết hạn phải dựa trên server, không dựa vào giờ client.
- EC4: Người dùng có nhiều thiết bị, 1 thiết bị đăng xuất toàn bộ (scope story khác) → Token trên thiết bị khác phải bị revoke nếu chính sách yêu cầu.

---

## 6) API mapping (contract-first)
> **Owner: Tech Lead.** BA must not guess API paths. Tech Lead fills this based on OpenAPI.

List exact OpenAPI paths + operations.
- `POST /api/v1/auth/refresh` — exchange a valid refresh token for new access (and possibly refresh) tokens.
- (Implicit) All protected business APIs use `Authorization: Bearer <accessToken>` and rely on auto-login semantics.

**Main request/response schemas** (OpenAPI):
- `RefreshRequest` → `LoginResponse`.

**Error codes used**
- `AUTH_TOKEN_INVALID` — access token is malformed or fails validation.
- `AUTH_TOKEN_EXPIRED` — access token is expired and cannot be used directly.
- `AUTH_REFRESH_TOKEN_REVOKED` — refresh token has been revoked (e.g., logout-all, security reset).
- `AUTH_REFRESH_TOKEN_EXPIRED` — refresh token is past `expires_at`.

**Idempotency**
- Required? **No** — refresh is logically idempotent per refresh token; repeated use after rotation should be rejected with `AUTH_REFRESH_TOKEN_REVOKED/EXPIRED`.
- Key header: N/A.
- Conflict behavior: Not applicable (no explicit idempotency store for this flow).

**Pagination/filtering**
- N/A

---

## 7) Data & DB mapping
**Tables impacted**
- AUTH (R/W: lưu Session Token/Refresh Token, trạng thái revoke, TTL).
- USERS (R: kiểm tra userId, trạng thái Active/Locked).

**Migration**
- Needed? Maybe (nếu chưa có bảng token store riêng).
- Migration notes: cần bảng lưu refresh tokens, index theo userId + deviceId (nếu có khái niệm thiết bị).
- Rollback notes: Xoá bảng/column liên quan nếu bỏ cơ chế auto-login.

---

## 8) Authorization & Security notes
**Roles/permissions**
- Chỉ End User đã đăng ký (tier 1/2/3) mới có token hợp lệ, các role khác không dùng luồng này.

**Security sanity**
- Token phải có TTL hợp lý, không vô hạn.
- Cần cơ chế revoke token khi nghi ngờ rò rỉ hoặc khi user reset bảo mật.
- Không được chấp nhận token từ nguồn không tin cậy (CORS, origin policy theo thiết kế FE/BE).

---

## 9) Test plan (proof) — required IDs

### Unit tests
- UT-AUTH-003: Kiểm tra logic TTL và refresh token (hết hạn, revoke, cấp token mới).

### Integration tests
- IT-AUTH-003: Kiểm tra auto-login giữa FE và AUTH với token store.

### E2E/Smoke (staging)
- E2E-AUTH-003: Người dùng đăng nhập lần đầu, đóng ứng dụng, mở lại trong TTL và được tự động đăng nhập.

### Security sanity
- SEC-AUTH-005: Negative tests với token giả mạo/corrupted.
- SEC-AUTH-006: Kiểm tra revoke token hoạt động đúng.

---

## 10) “Done” evidence checklist (copy into PR)
- [ ] AC covered (US-AUTH-003)
- [ ] OpenAPI implemented exactly (Tech Lead to confirm)
- [ ] AuthZ enforced + negative test exists
- [ ] Tests pass (UT-AUTH-003, IT-AUTH-003, E2E-AUTH-003, SEC-AUTH-005..006)
- [ ] No secrets / no PII logs / correlationId present
- [ ] Staging deploy + smoke evidence link
