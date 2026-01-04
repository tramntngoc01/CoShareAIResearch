# StoryPack — US-AUTH-004: Đăng xuất End User & thu hồi token trên thiết bị hiện tại

> Single entry point for AI + Humans. Do not guess. Follow links.

## 0) Metadata
- Story ID: **US-AUTH-004**
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
- User goal: End User có thể chủ động đăng xuất khỏi Portal trên thiết bị hiện tại và đảm bảo token không còn giá trị.
- Business value: Giảm rủi ro người khác dùng chung thiết bị (quán net, máy mượn), đảm bảo an toàn tài khoản người dùng.

**Out of scope**
- OOS-1: Đăng xuất khỏi tất cả thiết bị cùng lúc (global logout) — nếu cần sẽ có story riêng.
- OOS-2: Quản lý danh sách thiết bị đã đăng nhập (device management UI).

---

## 2) Source of truth (links)
- SRS: `docs/02-Requirements/SRS/SRS-AUTH.md#fr-auth-004`
- Stories: `docs/02-Requirements/Stories/Stories-AUTH.md#us-auth-004-—-đăng-xuất-end-user--thu-hồi-token-trên-thiết-bị-hiện-tại`
- Business rules: `docs/02-Requirements/Business-Rules.md#BR-???`
- OpenAPI: `docs/03-Design/OpenAPI/openapi.yaml`
- DB: `docs/03-Design/DB/DB-AUTH.md`
- Implementation Plan (Approved): `docs/03-ImplementationPlan/Implementation-Plan-AUTH.md#US-AUTH-004`
- Test Cases: `docs/03-Testing/TestCases/TestCases-AUTH.md`

---

## 3) Acceptance Criteria (measurable) — summary
- AC1: Khi người dùng nhấn "Đăng xuất", token hiện tại bị revoke và không thể dùng cho bất kỳ API bảo vệ nào.
- AC2: Sau khi đăng xuất, mọi tab của Portal trên trình duyệt hiện tại đều chuyển sang trạng thái chưa đăng nhập.
- AC3: Sự kiện logout được log kèm correlationId và userId/adminId.

## 4) Sample data (fake)
- Before logout:
	- sessionToken: "sess_active_123"
	- user: { userId: 10002, tier: 2 }
- Logout action (conceptual):
	- request: { action: "logout" }
- After logout:
	- sessionToken: revoked = true
	- subsequent API calls with old token → 401/unauthorized

## 5) Edge cases (min 3)
- EC1: Người dùng mở nhiều tab trên cùng trình duyệt → Logout tại một tab phải có hiệu lực trên tất cả tab.
- EC2: Request logout gặp lỗi mạng/tạm thời → FE vẫn xóa token client-side; server-side phải tối ưu cho trạng thái an toàn (có thể coi như đã logout nếu có nghi ngờ).
- EC3: Logout trong lúc có request nền đang xử lý → Token đã revoke không được chấp nhận cho các request hoàn thành sau thời điểm logout.

---

## 6) API mapping (contract-first)
> **Owner: Tech Lead.** BA must not guess API paths. Tech Lead fills this based on OpenAPI.

List exact OpenAPI paths + operations.
- `POST /api/v1/auth/logout` — revoke current access/refresh tokens for the authenticated End User on this device/browser.

**Main request/response schemas** (OpenAPI):
- No request body (relies on `Authorization: Bearer <accessToken>`).
- `204 No Content` on success.

**Error codes used**
- `AUTH_TOKEN_INVALID` — token malformed or signed with wrong key.
- `AUTH_TOKEN_EXPIRED` — already expired; logout treated as success but may still return a generic auth error.
- `AUTH_REFRESH_TOKEN_REVOKED` — refresh token already revoked (idempotent behavior).

**Idempotency**
- Required? **No** — logout is idempotent by design; repeated calls for the same session should be safe.
- Key header: N/A.
- Conflict behavior: N/A (no separate idempotency store).

**Pagination/filtering**
- N/A

---

## 7) Data & DB mapping
**Tables impacted**
- AUTH (R/W: trạng thái token, revoke timestamp, device info nếu có).

**Migration**
- Needed? Maybe (thêm cột revokeAt/revokedFlag nếu chưa có).
- Migration notes: đảm bảo index phục vụ truy vấn nhanh token hợp lệ.
- Rollback notes: có thể bỏ cột revokeAt/revokedFlag nếu cơ chế token thay đổi.

---

## 8) Authorization & Security notes
**Roles/permissions**
- Bất kỳ user đã đăng nhập qua AUTH đều có quyền logout chính mình.

**Security sanity**
- Logout phải xóa token khỏi mọi nơi phía client (cookie, local storage) theo thiết kế FE.
- Không để lộ thông tin nội bộ về cơ chế token trong thông báo lỗi.

---

## 9) Test plan (proof) — required IDs

### Unit tests
- UT-AUTH-004: Logic revoke token và không chấp nhận token đã revoke.

### Integration tests
- IT-AUTH-004: Logout flow giữa FE và AUTH, kiểm tra mọi request sau đó bị từ chối.

### E2E/Smoke (staging)
- E2E-AUTH-004: Người dùng đăng nhập, thực hiện thao tác, sau đó logout và xác nhận không truy cập được nữa.

### Security sanity
- SEC-AUTH-007: Negative tests với token đã revoke.

---

## 10) “Done” evidence checklist (copy into PR)
- [ ] AC covered (US-AUTH-004)
- [ ] OpenAPI implemented exactly (Tech Lead to confirm)
- [ ] AuthZ enforced + negative test exists
- [ ] Tests pass (UT-AUTH-004, IT-AUTH-004, E2E-AUTH-004, SEC-AUTH-007)
- [ ] No secrets / no PII logs / correlationId present
- [ ] Staging deploy + smoke evidence link
