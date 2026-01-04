# Test Cases — AUTH: Authentication & Authorization (Complete Module Test Suite)

> **QA Lead Owner** | Last Updated: 2026-01-04  
> **Scope**: Complete test suite for AUTH module covering all P0/P1 stories  
> **Coverage Target**: All stories US-AUTH-001 through US-AUTH-005

---

## Section 1: Scope & Assumptions

### 1.1 Scope
This test suite covers the complete AUTH module:
- **US-AUTH-001**: End User Registration (SĐT + OTP ZNS)
- **US-AUTH-002**: End User Login (First time, OTP)
- **US-AUTH-003**: Auto-login via Session/Refresh Token
- **US-AUTH-004**: End User Logout & Token Revocation
- **US-AUTH-005**: Admin Portal Login

### 1.2 Assumptions
1. OTP delivery via ZNS is stubbed/mocked in test environment; actual ZNS integration is verified in NOTIFICATIONS module tests.
2. OTP TTL is configured at 120 seconds (per StoryPack).
3. Rate limit: max 5 OTP requests per phone per 5-minute window (per AUTH options).
4. Session Token TTL: 1 hour; Refresh Token TTL: 30 days.
5. OTP length: 6 digits.
6. Password hash algorithm for Admin: BCrypt (per Security-Baseline).
7. Test environment has separate test database with seeded fake data.

### 1.3 Out of Scope
- Actual ZNS/SMS delivery testing (covered in NOTIFICATIONS).
- Admin account creation (covered in ADMIN module).
- User tier calculation logic (covered in USERS module).

### 1.4 References
- SRS: `docs/02-Requirements/SRS/SRS-AUTH.md`
- Stories: `docs/02-Requirements/Stories/Stories-AUTH.md`
- OpenAPI: `docs/03-Design/OpenAPI/openapi.yaml`
- Screen Specs: `docs/03-Design/UI/ScreenSpecs/SC-AUTH-001..005.md`
- Error Conventions: `docs/03-Design/Error-Conventions.md`

---

## Section 2: Test Data Set (Fake)

### 2.1 End User Test Accounts
| ID | Phone | Full Name | Company | Tier | Status | Notes |
|----|-------|-----------|---------|------|--------|-------|
| EU-001 | 0912345678 | Trần Thị B | Công ty Điện tử KCN X | 3 | Active | Primary test user |
| EU-002 | 0987654321 | Nguyễn Văn C | Công ty May KCN A | 2 | Active | Secondary user |
| EU-003 | 0901234567 | Lê Thị D | Công ty Điện tử KCN X | 3 | Locked | Locked account test |
| EU-NEW | 0909876543 | New User Test | Công ty May KCN A | 3 | N/A | For registration tests |

### 2.2 Admin Test Accounts
| ID | Email | Phone | Password (fake) | Role(s) | Status |
|----|-------|-------|-----------------|---------|--------|
| AD-001 | superadmin@coshare.test | 0900000001 | P@ssw0rd!Super | Super Admin | Active |
| AD-002 | ops.admin@coshare.test | 0900000002 | P@ssw0rd!Ops | Ops | Active |
| AD-003 | qc.admin@coshare.test | 0900000003 | P@ssw0rd!QC | QC | Active |
| AD-004 | finance@coshare.test | 0900000004 | P@ssw0rd!Fin | Finance | Active |
| AD-005 | support@coshare.test | 0900000005 | P@ssw0rd!Sup | Support | Active |
| AD-006 | locked@coshare.test | 0900000006 | P@ssw0rd!Lock | Ops | Locked |
| AD-007 | multirole@coshare.test | 0900000007 | P@ssw0rd!Multi | Ops, Support | Active |

### 2.3 OTP Test Values
| Scenario | OTP Code | Phone | Status |
|----------|----------|-------|--------|
| Valid OTP | 482913 | 0909876543 | Pending |
| Expired OTP | 111111 | 0909876543 | Expired |
| Wrong OTP | 000000 | Any | Invalid |

### 2.4 Token Test Values
| Scenario | Token Value (Fake) | User | Status |
|----------|-------------------|------|--------|
| Valid Session | sess_valid_abc123 | EU-001 | Active |
| Expired Session | sess_expired_xyz789 | EU-001 | Expired |
| Valid Refresh | ref_valid_def456 | EU-001 | Active |
| Expired Refresh | ref_expired_uvw321 | EU-001 | Expired |
| Revoked Refresh | ref_revoked_rst654 | EU-001 | Revoked |

### 2.5 Companies
| ID | Name | Code |
|----|------|------|
| 501 | Công ty Điện tử KCN X | DTKX |
| 502 | Công ty May KCN A | MKCA |
| 503 | Công ty Thực phẩm KCN B | TPKB |

---

## Section 3: E2E Test Cases

### P0 — Critical Path Tests

---

#### E2E-AUTH-001: Complete End User Registration Journey (Happy Path)
| Field | Value |
|-------|-------|
| **Test ID** | E2E-AUTH-001 |
| **Title** | Complete End User Registration with OTP Verification |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-001 |
| **Screen IDs** | SC-AUTH-001, SC-AUTH-002 |
| **Preconditions** | <ul><li>Phone 0909876543 is not registered</li><li>Test company "Công ty Điện tử KCN X" exists (ID: 501)</li><li>NOTIFICATIONS stub returns success</li></ul> |
| **Test Data** | <ul><li>fullName: "Người Dùng Mới Test"</li><li>companyId: 501</li><li>phone: "0909876543"</li><li>acceptTerms: true</li><li>Expected OTP: (from stub/log)</li></ul> |
| **Steps** | 1. Navigate to `/register` (SC-AUTH-001)<br/>2. Enter full name: "Người Dùng Mới Test"<br/>3. Select company: "Công ty Điện tử KCN X"<br/>4. Enter phone: "0909876543"<br/>5. Check "Accept terms" checkbox<br/>6. Click "Gửi mã OTP" button<br/>7. Verify redirect to OTP screen (SC-AUTH-002)<br/>8. Retrieve OTP from test stub/API log<br/>9. Enter OTP in verification screen<br/>10. Click "Xác nhận" button |
| **Expected Results** | <ul><li>Step 6: API returns 202 Accepted</li><li>Step 6: Success message "Đã gửi OTP" displayed</li><li>Step 7: Screen shows masked phone "****6543"</li><li>Step 10: API returns 200 with LoginResponse</li><li>Step 10: User receives accessToken and refreshToken</li><li>Step 10: User is redirected to home/dashboard</li><li>Step 10: User account created with tier=3</li></ul> |
| **Evidence** | <ul><li>Screenshot: Registration form filled</li><li>Screenshot: OTP screen with masked phone</li><li>Screenshot: Success state after verification</li><li>Log: X-Correlation-Id from both API calls</li><li>DB: New user record in users_user table</li></ul> |
| **Notes** | This is the primary happy path for registration. REG candidate. |

---

#### E2E-AUTH-002: End User First-Time Login with OTP (Happy Path)
| Field | Value |
|-------|-------|
| **Test ID** | E2E-AUTH-002 |
| **Title** | End User Login (First Time) via OTP |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-002 |
| **Screen IDs** | SC-AUTH-003, SC-AUTH-002 |
| **Preconditions** | <ul><li>User EU-001 (0912345678) is registered and active</li><li>User has no valid session token on device</li><li>NOTIFICATIONS stub returns success</li></ul> |
| **Test Data** | <ul><li>phone: "0912345678"</li><li>Expected OTP: (from stub/log)</li></ul> |
| **Steps** | 1. Navigate to `/login` (SC-AUTH-003)<br/>2. Enter phone: "0912345678"<br/>3. Click "Gửi OTP" button<br/>4. Verify redirect to OTP screen (SC-AUTH-002)<br/>5. Retrieve OTP from test stub<br/>6. Enter OTP in verification screen<br/>7. Click "Xác nhận" button |
| **Expected Results** | <ul><li>Step 3: API POST /login/request-otp returns 202</li><li>Step 4: OTP screen displays masked phone</li><li>Step 7: API POST /login/verify-otp returns 200 with LoginResponse</li><li>Step 7: Session & Refresh tokens issued</li><li>Step 7: User sees their tier-appropriate content</li></ul> |
| **Evidence** | <ul><li>Screenshot: Login form</li><li>Screenshot: OTP verification</li><li>Screenshot: Portal home after login</li><li>Log: Correlation IDs</li></ul> |
| **Notes** | REG candidate. |

---

#### E2E-AUTH-003: Auto-Login with Valid Session Token (Happy Path)
| Field | Value |
|-------|-------|
| **Test ID** | E2E-AUTH-003 |
| **Title** | Automatic Login via Valid Session Token |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-003 |
| **Screen IDs** | SC-AUTH-004 |
| **Preconditions** | <ul><li>User EU-001 has valid session token stored in browser</li><li>Session token not expired</li></ul> |
| **Test Data** | <ul><li>sessionToken: Valid token from previous login</li><li>phone: "0912345678"</li></ul> |
| **Steps** | 1. Open browser with existing session token<br/>2. Navigate to Portal End User home page<br/>3. Observe automatic authentication |
| **Expected Results** | <ul><li>No OTP prompt displayed</li><li>User immediately sees logged-in state (SC-AUTH-004)</li><li>Header shows user name/greeting</li><li>Protected API calls succeed with 200</li></ul> |
| **Evidence** | <ul><li>Screenshot: Portal showing logged-in header</li><li>Network: API call with Bearer token succeeds</li></ul> |
| **Notes** | Tests silent authentication flow. REG candidate. |

---

#### E2E-AUTH-004: Session Token Refresh Flow (Happy Path)
| Field | Value |
|-------|-------|
| **Test ID** | E2E-AUTH-004 |
| **Title** | Session Token Refresh when Expired |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-003 |
| **Screen IDs** | SC-AUTH-004 |
| **Preconditions** | <ul><li>User EU-001 has expired session token</li><li>Refresh token is still valid</li></ul> |
| **Test Data** | <ul><li>sessionToken: Expired</li><li>refreshToken: Valid</li></ul> |
| **Steps** | 1. Set up browser with expired session token and valid refresh token<br/>2. Navigate to protected page<br/>3. Observe token refresh |
| **Expected Results** | <ul><li>API POST /api/v1/auth/refresh called automatically</li><li>New session token issued</li><li>User remains logged in without OTP prompt</li><li>Protected content loads successfully</li></ul> |
| **Evidence** | <ul><li>Network: Refresh API call and response</li><li>Screenshot: Continuous logged-in experience</li></ul> |
| **Notes** | Critical for ZNS cost reduction. REG candidate. |

---

#### E2E-AUTH-005: End User Logout (Happy Path)
| Field | Value |
|-------|-------|
| **Test ID** | E2E-AUTH-005 |
| **Title** | End User Logout and Token Revocation |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-004 |
| **Screen IDs** | SC-AUTH-004 |
| **Preconditions** | <ul><li>User EU-001 is logged in with valid tokens</li></ul> |
| **Test Data** | <ul><li>Logged-in session for EU-001</li></ul> |
| **Steps** | 1. From logged-in state (SC-AUTH-004)<br/>2. Click "Đăng xuất" button<br/>3. Confirm logout if prompted<br/>4. Attempt to access protected page<br/>5. Attempt to use old token via API |
| **Expected Results** | <ul><li>Step 2: API POST /api/v1/auth/logout returns 200</li><li>Step 3: User redirected to login screen</li><li>Step 4: Protected pages redirect to login</li><li>Step 5: API returns 401 Unauthorized</li><li>Session token is marked revoked in DB</li></ul> |
| **Evidence** | <ul><li>Screenshot: Before logout (logged-in header)</li><li>Screenshot: After logout (login screen)</li><li>API: 401 response when using revoked token</li><li>Log: Logout event with correlationId</li></ul> |
| **Notes** | REG candidate. |

---

#### E2E-AUTH-006: Admin Portal Login (Happy Path)
| Field | Value |
|-------|-------|
| **Test ID** | E2E-AUTH-006 |
| **Title** | Admin Login with Email and Password |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-005 |
| **Screen IDs** | SC-AUTH-005 |
| **Preconditions** | <ul><li>Admin AD-002 (ops.admin@coshare.test) exists and is active</li></ul> |
| **Test Data** | <ul><li>email: "ops.admin@coshare.test"</li><li>password: "P@ssw0rd!Ops"</li></ul> |
| **Steps** | 1. Navigate to Admin Portal login (SC-AUTH-005)<br/>2. Enter email: "ops.admin@coshare.test"<br/>3. Enter password: "P@ssw0rd!Ops"<br/>4. Click "Đăng nhập" button |
| **Expected Results** | <ul><li>Step 4: API POST /api/v1/auth/login returns 200</li><li>Step 4: Admin session token issued</li><li>Step 4: Redirected to Admin Dashboard</li><li>Step 4: Dashboard shows Ops-specific features</li><li>Login event logged with adminUserId, correlationId</li></ul> |
| **Evidence** | <ul><li>Screenshot: Admin login form</li><li>Screenshot: Admin dashboard with Ops menu</li><li>Log: Successful login event</li></ul> |
| **Notes** | REG candidate. |

---

### P0 — Negative/Edge Case Tests

---

#### E2E-AUTH-007: Registration with Invalid Phone Format
| Field | Value |
|-------|-------|
| **Test ID** | E2E-AUTH-007 |
| **Title** | Registration Rejected for Invalid Phone Format |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-001 (EC1) |
| **Screen IDs** | SC-AUTH-001 |
| **Preconditions** | None |
| **Test Data** | <ul><li>fullName: "Test User"</li><li>companyId: 501</li><li>phone: "09123ABC" (invalid)</li><li>acceptTerms: true</li></ul> |
| **Steps** | 1. Navigate to registration form<br/>2. Fill all fields with invalid phone<br/>3. Click "Gửi mã OTP" |
| **Expected Results** | <ul><li>Client-side validation shows error on phone field</li><li>If bypassed, API returns 400 Bad Request</li><li>Error code: AUTH_INVALID_PHONE_FORMAT</li><li>No OTP is sent</li></ul> |
| **Evidence** | <ul><li>Screenshot: Validation error on phone field</li><li>API response showing error code</li></ul> |
| **Notes** | Tests both client and server validation. |

---

#### E2E-AUTH-008: Registration with Wrong/Expired OTP
| Field | Value |
|-------|-------|
| **Test ID** | E2E-AUTH-008 |
| **Title** | Registration Fails with Wrong or Expired OTP |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-001 (AC3) |
| **Screen IDs** | SC-AUTH-001, SC-AUTH-002 |
| **Preconditions** | <ul><li>Phone 0909111222 not registered</li><li>OTP has been requested</li></ul> |
| **Test Data** | <ul><li>phone: "0909111222"</li><li>wrongOtp: "000000"</li></ul> |
| **Steps** | 1. Complete registration form and request OTP<br/>2. Enter wrong OTP "000000"<br/>3. Click "Xác nhận" |
| **Expected Results** | <ul><li>API returns 401 Unauthorized</li><li>Error code: AUTH_OTP_INVALID_OR_EXPIRED</li><li>Error message: "Mã OTP không đúng hoặc đã hết hạn"</li><li>No user account created</li><li>User can retry or request new OTP</li></ul> |
| **Evidence** | <ul><li>Screenshot: Error message on OTP screen</li><li>API response with error code</li><li>DB: No new user record</li></ul> |
| **Notes** | Error message must not reveal which part is wrong. |

---

#### E2E-AUTH-009: Login with Unregistered Phone
| Field | Value |
|-------|-------|
| **Test ID** | E2E-AUTH-009 |
| **Title** | Login Rejected for Unregistered Phone |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-002 (AC3) |
| **Screen IDs** | SC-AUTH-003 |
| **Preconditions** | <ul><li>Phone 0999999999 is not registered</li></ul> |
| **Test Data** | <ul><li>phone: "0999999999"</li></ul> |
| **Steps** | 1. Navigate to login screen<br/>2. Enter unregistered phone<br/>3. Click "Gửi OTP" |
| **Expected Results** | <ul><li>No OTP is sent</li><li>Error message displayed (generic: "Tài khoản chưa được đăng ký" or similar)</li><li>User prompted to register instead</li></ul> |
| **Evidence** | <ul><li>Screenshot: Error message on login screen</li></ul> |
| **Notes** | Message should not confirm/deny account existence for security. |

---

#### E2E-AUTH-010: Login with Locked Account
| Field | Value |
|-------|-------|
| **Test ID** | E2E-AUTH-010 |
| **Title** | Login Rejected for Locked User Account |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-002 (EC3) |
| **Screen IDs** | SC-AUTH-003 |
| **Preconditions** | <ul><li>User EU-003 (0901234567) has status=Locked</li></ul> |
| **Test Data** | <ul><li>phone: "0901234567"</li></ul> |
| **Steps** | 1. Navigate to login screen<br/>2. Enter locked user's phone<br/>3. Click "Gửi OTP" |
| **Expected Results** | <ul><li>Generic error message (no detail about lock reason)</li><li>No OTP is sent</li><li>Event logged with correlationId</li></ul> |
| **Evidence** | <ul><li>Screenshot: Generic error message</li><li>Log: Login attempt for locked account</li></ul> |
| **Notes** | Security: Do not reveal lock status explicitly. |

---

#### E2E-AUTH-011: Both Tokens Expired Forces Re-Login
| Field | Value |
|-------|-------|
| **Test ID** | E2E-AUTH-011 |
| **Title** | Expired Session and Refresh Tokens Require OTP Login |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-003 (AC3) |
| **Screen IDs** | SC-AUTH-003, SC-AUTH-004 |
| **Preconditions** | <ul><li>Both session and refresh tokens are expired</li></ul> |
| **Test Data** | <ul><li>sessionToken: Expired</li><li>refreshToken: Expired</li></ul> |
| **Steps** | 1. Set up browser with both tokens expired<br/>2. Navigate to protected page<br/>3. Observe redirect |
| **Expected Results** | <ul><li>User is redirected to login screen (SC-AUTH-003)</li><li>OTP is required for new login</li><li>No automatic access granted</li></ul> |
| **Evidence** | <ul><li>Screenshot: Redirect to login screen</li><li>Network: 401 on protected API call</li></ul> |
| **Notes** | REG candidate. |

---

#### E2E-AUTH-012: Admin Login with Wrong Credentials
| Field | Value |
|-------|-------|
| **Test ID** | E2E-AUTH-012 |
| **Title** | Admin Login Fails with Wrong Password |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-005 (AC4) |
| **Screen IDs** | SC-AUTH-005 |
| **Preconditions** | <ul><li>Admin AD-002 exists</li></ul> |
| **Test Data** | <ul><li>email: "ops.admin@coshare.test"</li><li>password: "WrongPassword123"</li></ul> |
| **Steps** | 1. Navigate to Admin login<br/>2. Enter correct email, wrong password<br/>3. Click "Đăng nhập" |
| **Expected Results** | <ul><li>API returns 401 Unauthorized</li><li>Generic error: "Thông tin đăng nhập không hợp lệ"</li><li>Does NOT reveal which field is wrong</li><li>Login failure logged</li></ul> |
| **Evidence** | <ul><li>Screenshot: Generic error message</li><li>Log: Failed login attempt</li></ul> |
| **Notes** | Security critical - no information leakage. |

---

#### E2E-AUTH-013: End User Cannot Access Admin Portal
| Field | Value |
|-------|-------|
| **Test ID** | E2E-AUTH-013 |
| **Title** | End User Denied Access to Admin Portal |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-005 (AC2) |
| **Screen IDs** | SC-AUTH-005 |
| **Preconditions** | <ul><li>End User EU-001 exists with valid credentials</li><li>EU-001 does not have admin role</li></ul> |
| **Test Data** | <ul><li>phone: "0912345678"</li><li>password: (any value - End Users don't have passwords)</li></ul> |
| **Steps** | 1. Navigate to Admin Portal login<br/>2. Attempt to login with End User phone<br/>3. Observe result |
| **Expected Results** | <ul><li>Login rejected</li><li>Generic error message</li><li>No admin session created</li></ul> |
| **Evidence** | <ul><li>Screenshot: Rejection message</li></ul> |
| **Notes** | Validates role-based access separation. |

---

### P1 — Additional Coverage Tests

---

#### E2E-AUTH-014: Multi-Tab Logout Propagation
| Field | Value |
|-------|-------|
| **Test ID** | E2E-AUTH-014 |
| **Title** | Logout in One Tab Affects All Tabs |
| **Priority** | P1 |
| **Story IDs** | US-AUTH-004 (EC1) |
| **Screen IDs** | SC-AUTH-004 |
| **Preconditions** | <ul><li>User logged in with multiple browser tabs open</li></ul> |
| **Test Data** | <ul><li>Logged-in session for EU-001</li></ul> |
| **Steps** | 1. Open Portal in Tab A (logged in)<br/>2. Open Portal in Tab B (logged in)<br/>3. Click "Đăng xuất" in Tab A<br/>4. Refresh Tab B or perform action |
| **Expected Results** | <ul><li>Tab A: Redirected to login</li><li>Tab B: Also redirected to login or shows logout state</li><li>Token is globally revoked</li></ul> |
| **Evidence** | <ul><li>Screenshot: Both tabs showing logged-out state</li></ul> |
| **Notes** | Browser storage mechanism dependent. |

---

#### E2E-AUTH-015: Admin with Multiple Roles Sees Combined Features
| Field | Value |
|-------|-------|
| **Test ID** | E2E-AUTH-015 |
| **Title** | Admin with Multiple Roles Has Combined Access |
| **Priority** | P1 |
| **Story IDs** | US-AUTH-005 (EC2) |
| **Screen IDs** | SC-AUTH-005 |
| **Preconditions** | <ul><li>Admin AD-007 has roles: Ops + Support</li></ul> |
| **Test Data** | <ul><li>email: "multirole@coshare.test"</li><li>password: "P@ssw0rd!Multi"</li></ul> |
| **Steps** | 1. Login as AD-007<br/>2. Navigate Admin Dashboard<br/>3. Check available menu items/features |
| **Expected Results** | <ul><li>Dashboard shows features for both Ops and Support roles</li><li>No role conflict or missing features</li></ul> |
| **Evidence** | <ul><li>Screenshot: Admin dashboard with both role features visible</li></ul> |
| **Notes** | Tests multi-role handling. |

---

## Section 4: Integration/API Test Cases

---

#### IT-AUTH-001: Registration OTP Request API
| Field | Value |
|-------|-------|
| **Test ID** | IT-AUTH-001 |
| **Title** | POST /auth/end-user/register/request-otp Success |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-001 |
| **Screen IDs** | N/A (API only) |
| **Preconditions** | <ul><li>Phone not registered</li><li>NOTIFICATIONS service available</li></ul> |
| **Test Data** | ```json
{
  "phone": "0909876543",
  "fullName": "API Test User",
  "companyId": 501,
  "acceptTerms": true
}
``` |
| **Steps** | 1. Send POST to `/api/v1/auth/end-user/register/request-otp`<br/>2. Include valid Idempotency-Key header |
| **Expected Results** | <ul><li>Response: 202 Accepted</li><li>Empty body</li><li>X-Correlation-Id header present</li><li>OTP record created in auth_otp_request table</li></ul> |
| **Evidence** | <ul><li>API response headers</li><li>DB: auth_otp_request record</li></ul> |
| **Notes** | Base API functionality test. |

---

#### IT-AUTH-002: Registration OTP Verification API
| Field | Value |
|-------|-------|
| **Test ID** | IT-AUTH-002 |
| **Title** | POST /auth/end-user/register/verify-otp Success |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-001 |
| **Screen IDs** | N/A (API only) |
| **Preconditions** | <ul><li>Valid pending OTP exists for phone</li></ul> |
| **Test Data** | ```json
{
  "phone": "0909876543",
  "otpCode": "482913",
  "deviceInfo": {
    "deviceId": "test-device-001",
    "platform": "web",
    "userAgent": "TestAgent/1.0"
  }
}
``` |
| **Steps** | 1. Request OTP first (IT-AUTH-001)<br/>2. Retrieve OTP from DB/stub<br/>3. Send POST to `/api/v1/auth/end-user/register/verify-otp` |
| **Expected Results** | <ul><li>Response: 200 OK</li><li>Body contains: accessToken, refreshToken, expiresIn, user object</li><li>OTP status updated to VERIFIED</li><li>User created in users_user table</li><li>Refresh token stored in auth_refresh_token</li></ul> |
| **Evidence** | <ul><li>API response body (tokens)</li><li>DB: User and token records</li></ul> |
| **Notes** | Critical integration point. |

---

#### IT-AUTH-003: Login OTP Request for Existing User
| Field | Value |
|-------|-------|
| **Test ID** | IT-AUTH-003 |
| **Title** | POST /auth/end-user/login/request-otp Success |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-002 |
| **Screen IDs** | N/A (API only) |
| **Preconditions** | <ul><li>User EU-001 (0912345678) is registered and active</li></ul> |
| **Test Data** | ```json
{
  "phone": "0912345678"
}
``` |
| **Steps** | 1. Send POST to `/api/v1/auth/end-user/login/request-otp` |
| **Expected Results** | <ul><li>Response: 202 Accepted</li><li>OTP record created with purpose=LOGIN</li></ul> |
| **Evidence** | <ul><li>API response</li><li>DB: New OTP request record</li></ul> |
| **Notes** | |

---

#### IT-AUTH-004: Login OTP Verification Issues Tokens
| Field | Value |
|-------|-------|
| **Test ID** | IT-AUTH-004 |
| **Title** | POST /auth/end-user/login/verify-otp Issues Tokens |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-002 |
| **Screen IDs** | N/A (API only) |
| **Preconditions** | <ul><li>Valid login OTP exists</li></ul> |
| **Test Data** | ```json
{
  "phone": "0912345678",
  "otpCode": "654321",
  "deviceInfo": {
    "deviceId": "test-device-002",
    "platform": "web"
  }
}
``` |
| **Steps** | 1. Request login OTP<br/>2. Retrieve OTP<br/>3. Send POST to `/api/v1/auth/end-user/login/verify-otp` |
| **Expected Results** | <ul><li>Response: 200 OK with LoginResponse</li><li>Tokens bound to device</li><li>OTP marked as used</li></ul> |
| **Evidence** | <ul><li>API response with tokens</li><li>DB: Token record with deviceId</li></ul> |
| **Notes** | |

---

#### IT-AUTH-005: Token Refresh API
| Field | Value |
|-------|-------|
| **Test ID** | IT-AUTH-005 |
| **Title** | POST /auth/refresh Issues New Session Token |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-003 |
| **Screen IDs** | N/A (API only) |
| **Preconditions** | <ul><li>Valid refresh token exists</li></ul> |
| **Test Data** | ```json
{
  "refreshToken": "ref_valid_def456"
}
``` |
| **Steps** | 1. Send POST to `/api/v1/auth/refresh` with valid refresh token |
| **Expected Results** | <ul><li>Response: 200 OK</li><li>New accessToken issued</li><li>New refreshToken issued (rotation)</li><li>Old refresh token may be invalidated (per policy)</li></ul> |
| **Evidence** | <ul><li>API response with new tokens</li></ul> |
| **Notes** | Token rotation policy may vary. |

---

#### IT-AUTH-006: Logout API Revokes Tokens
| Field | Value |
|-------|-------|
| **Test ID** | IT-AUTH-006 |
| **Title** | POST /auth/logout Revokes Session |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-004 |
| **Screen IDs** | N/A (API only) |
| **Preconditions** | <ul><li>User has valid session</li></ul> |
| **Test Data** | <ul><li>Authorization: Bearer {valid_access_token}</li></ul> |
| **Steps** | 1. Send POST to `/api/v1/auth/logout` with Bearer token<br/>2. Attempt to use same token for protected API |
| **Expected Results** | <ul><li>Step 1: 200 OK</li><li>Step 2: 401 Unauthorized</li><li>Token marked revoked in DB</li></ul> |
| **Evidence** | <ul><li>API responses</li><li>DB: revoked_at timestamp set</li></ul> |
| **Notes** | |

---

#### IT-AUTH-007: Admin Login API
| Field | Value |
|-------|-------|
| **Test ID** | IT-AUTH-007 |
| **Title** | POST /auth/login Admin Authentication |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-005 |
| **Screen IDs** | N/A (API only) |
| **Preconditions** | <ul><li>Admin AD-001 exists</li></ul> |
| **Test Data** | ```json
{
  "email": "superadmin@coshare.test",
  "password": "P@ssw0rd!Super"
}
``` |
| **Steps** | 1. Send POST to `/api/v1/auth/login` |
| **Expected Results** | <ul><li>Response: 200 OK</li><li>Body contains admin tokens and roles array</li><li>Login event logged</li></ul> |
| **Evidence** | <ul><li>API response</li><li>Log: Login event</li></ul> |
| **Notes** | |

---

#### IT-AUTH-008: OTP Rate Limiting API Behavior
| Field | Value |
|-------|-------|
| **Test ID** | IT-AUTH-008 |
| **Title** | OTP Requests Rate Limited at API Level |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-001 (EC2), US-AUTH-002 |
| **Screen IDs** | N/A (API only) |
| **Preconditions** | <ul><li>Rate limit: 5 requests per 5 minutes</li></ul> |
| **Test Data** | <ul><li>phone: "0909888888"</li></ul> |
| **Steps** | 1. Send 5 valid OTP requests for same phone<br/>2. Send 6th request immediately |
| **Expected Results** | <ul><li>Requests 1-5: 202 Accepted</li><li>Request 6: 429 Too Many Requests</li><li>Error code: AUTH_OTP_RATE_LIMITED</li></ul> |
| **Evidence** | <ul><li>API responses with timestamps</li><li>All correlation IDs</li></ul> |
| **Notes** | REG candidate. |

---

#### IT-AUTH-009: Idempotency Key Conflict
| Field | Value |
|-------|-------|
| **Test ID** | IT-AUTH-009 |
| **Title** | Same Idempotency Key with Different Payload Returns 409 |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-001 |
| **Screen IDs** | N/A (API only) |
| **Preconditions** | None |
| **Test Data** | <ul><li>Idempotency-Key: "idem-key-12345"</li><li>Payload A: phone="0909111111"</li><li>Payload B: phone="0909222222"</li></ul> |
| **Steps** | 1. Send request with Payload A and Idempotency-Key<br/>2. Send request with Payload B and same Idempotency-Key |
| **Expected Results** | <ul><li>Request 1: 202 Accepted</li><li>Request 2: 409 Conflict</li><li>Error code: AUTH_IDEMPOTENCY_CONFLICT</li></ul> |
| **Evidence** | <ul><li>Both API responses</li></ul> |
| **Notes** | Critical for duplicate request handling. |

---

#### IT-AUTH-010: Invalid Token Returns 401
| Field | Value |
|-------|-------|
| **Test ID** | IT-AUTH-010 |
| **Title** | Invalid/Corrupted Token Rejected with 401 |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-003 (EC2) |
| **Screen IDs** | N/A (API only) |
| **Preconditions** | None |
| **Test Data** | <ul><li>Authorization: Bearer invalid_corrupted_token_xyz</li></ul> |
| **Steps** | 1. Send GET to any protected endpoint with invalid token |
| **Expected Results** | <ul><li>Response: 401 Unauthorized</li><li>System does not crash</li><li>Generic error message</li></ul> |
| **Evidence** | <ul><li>API response</li></ul> |
| **Notes** | Ensures graceful handling of malformed tokens. |

---

## Section 5: Security Sanity Test Cases

---

#### SEC-AUTH-001: OTP Brute Force Prevention
| Field | Value |
|-------|-------|
| **Test ID** | SEC-AUTH-001 |
| **Title** | OTP Verification Blocked After Max Attempts |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-001, US-AUTH-002 |
| **Screen IDs** | SC-AUTH-002 |
| **Preconditions** | <ul><li>Max OTP attempts configured (e.g., 5)</li><li>Valid OTP exists for phone</li></ul> |
| **Test Data** | <ul><li>phone: "0909777777"</li><li>wrongOtp: "000000" (repeat)</li></ul> |
| **Steps** | 1. Request OTP for phone<br/>2. Submit wrong OTP 5 times<br/>3. Submit correct OTP on 6th attempt |
| **Expected Results** | <ul><li>Attempts 1-5: 401 with AUTH_OTP_INVALID_OR_EXPIRED</li><li>Attempt 6: 401 with AUTH_OTP_MAX_ATTEMPTS_EXCEEDED (or similar)</li><li>OTP is invalidated after max attempts</li><li>Correct OTP no longer works</li></ul> |
| **Evidence** | <ul><li>All API responses with timestamps</li><li>DB: OTP status=FAILED or attempt_count</li></ul> |
| **Notes** | Prevents OTP guessing attacks. REG candidate. |

---

#### SEC-AUTH-002: OTP Rate Limiting (Spam Prevention)
| Field | Value |
|-------|-------|
| **Test ID** | SEC-AUTH-002 |
| **Title** | OTP Request Rate Limiting Prevents Spam |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-001 (EC2) |
| **Screen IDs** | N/A |
| **Preconditions** | <ul><li>Rate limit: 5 requests per 5 minutes</li></ul> |
| **Test Data** | <ul><li>phone: "0909666666"</li></ul> |
| **Steps** | 1. Send 10 OTP requests in rapid succession |
| **Expected Results** | <ul><li>First 5: 202 Accepted</li><li>Remaining: 429 Too Many Requests</li><li>No more than 5 OTPs actually sent</li></ul> |
| **Evidence** | <ul><li>API response codes and timestamps</li><li>DB: Only 5 OTP records created</li></ul> |
| **Notes** | Prevents ZNS cost abuse. REG candidate. |

---

#### SEC-AUTH-003: SQL Injection in Phone Field
| Field | Value |
|-------|-------|
| **Test ID** | SEC-AUTH-003 |
| **Title** | SQL Injection Attempt in Phone Field Rejected |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-001, US-AUTH-002 |
| **Screen IDs** | SC-AUTH-001, SC-AUTH-003 |
| **Preconditions** | None |
| **Test Data** | <ul><li>phone: "0912345678'; DROP TABLE users;--"</li></ul> |
| **Steps** | 1. Submit registration with SQL injection in phone field |
| **Expected Results** | <ul><li>Response: 400 Bad Request</li><li>Error code: AUTH_INVALID_PHONE_FORMAT</li><li>No SQL executed</li><li>Database tables intact</li></ul> |
| **Evidence** | <ul><li>API response</li><li>DB: Tables unaffected</li></ul> |
| **Notes** | Input sanitization validation. |

---

#### SEC-AUTH-004: XSS in FullName Field
| Field | Value |
|-------|-------|
| **Test ID** | SEC-AUTH-004 |
| **Title** | XSS Attempt in FullName Field Sanitized |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-001 |
| **Screen IDs** | SC-AUTH-001 |
| **Preconditions** | None |
| **Test Data** | <ul><li>fullName: "&lt;script&gt;alert('XSS')&lt;/script&gt;"</li><li>phone: "0909555555"</li></ul> |
| **Steps** | 1. Submit registration with XSS payload in fullName<br/>2. Complete registration if allowed<br/>3. View user profile or any display of name |
| **Expected Results** | <ul><li>Either: 400 Bad Request with validation error</li><li>Or: Name stored but HTML-escaped on output</li><li>No script execution on any page</li></ul> |
| **Evidence** | <ul><li>API response</li><li>Screenshot: Profile display (if created)</li></ul> |
| **Notes** | Output encoding must be verified. |

---

#### SEC-AUTH-005: Token Expiration Enforced
| Field | Value |
|-------|-------|
| **Test ID** | SEC-AUTH-005 |
| **Title** | Expired Token Cannot Access Protected Resources |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-003 |
| **Screen IDs** | N/A |
| **Preconditions** | <ul><li>Access token with known expiry</li></ul> |
| **Test Data** | <ul><li>Expired access token (wait past TTL or use test token)</li></ul> |
| **Steps** | 1. Obtain valid access token<br/>2. Wait until token expires (or use pre-expired test token)<br/>3. Attempt to access protected API |
| **Expected Results** | <ul><li>Response: 401 Unauthorized</li><li>Token expiration is server-enforced</li></ul> |
| **Evidence** | <ul><li>API response</li><li>Token expiry timestamp</li></ul> |
| **Notes** | Server must not trust client-reported expiry. |

---

#### SEC-AUTH-006: Revoked Refresh Token Cannot Be Used
| Field | Value |
|-------|-------|
| **Test ID** | SEC-AUTH-006 |
| **Title** | Revoked Refresh Token Rejected |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-003 (EC1), US-AUTH-004 |
| **Screen IDs** | N/A |
| **Preconditions** | <ul><li>User has logged out (token revoked)</li></ul> |
| **Test Data** | <ul><li>Revoked refresh token</li></ul> |
| **Steps** | 1. Logout (revokes tokens)<br/>2. Attempt to use revoked refresh token via /auth/refresh |
| **Expected Results** | <ul><li>Response: 401 Unauthorized</li><li>No new tokens issued</li></ul> |
| **Evidence** | <ul><li>API response</li></ul> |
| **Notes** | Prevents token reuse after logout. REG candidate. |

---

#### SEC-AUTH-007: No PII/OTP in Logs
| Field | Value |
|-------|-------|
| **Test ID** | SEC-AUTH-007 |
| **Title** | OTP Values and Full Phone Not Logged |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-001 (AC4), BR-AUTH-004 |
| **Screen IDs** | N/A |
| **Preconditions** | <ul><li>Access to application logs</li></ul> |
| **Test Data** | <ul><li>Execute any OTP flow</li></ul> |
| **Steps** | 1. Perform OTP request and verification<br/>2. Search logs for OTP value<br/>3. Search logs for full phone number |
| **Expected Results** | <ul><li>OTP code (e.g., "482913") NOT found in logs</li><li>Phone is masked (e.g., "***6543" or similar)</li><li>CorrelationId IS present</li></ul> |
| **Evidence** | <ul><li>Log snippets (redacted)</li></ul> |
| **Notes** | Data-Classification compliance. REG candidate. |

---

#### SEC-AUTH-008: Admin Password Not Exposed in Error Messages
| Field | Value |
|-------|-------|
| **Test ID** | SEC-AUTH-008 |
| **Title** | Admin Login Errors Do Not Reveal Password Status |
| **Priority** | P0 |
| **Story IDs** | US-AUTH-005 |
| **Screen IDs** | SC-AUTH-005 |
| **Preconditions** | None |
| **Test Data** | <ul><li>Correct email, wrong password</li><li>Wrong email, any password</li></ul> |
| **Steps** | 1. Attempt login with correct email, wrong password<br/>2. Note error message<br/>3. Attempt login with non-existent email<br/>4. Note error message |
| **Expected Results** | <ul><li>Both scenarios return same generic error message</li><li>Cannot distinguish "user exists" from "wrong password"</li></ul> |
| **Evidence** | <ul><li>Screenshots of both error messages</li></ul> |
| **Notes** | Prevents user enumeration attacks. |

---

#### SEC-AUTH-009: Device Binding Enforcement
| Field | Value |
|-------|-------|
| **Test ID** | SEC-AUTH-009 |
| **Title** | Token Cannot Be Used From Different Device |
| **Priority** | P1 |
| **Story IDs** | US-AUTH-003, BR-AUTH-002 |
| **Screen IDs** | N/A |
| **Preconditions** | <ul><li>Token issued for device A</li></ul> |
| **Test Data** | <ul><li>Token from Device A</li><li>Request from Device B (different user-agent/fingerprint)</li></ul> |
| **Steps** | 1. Obtain token on Device A<br/>2. Send request using that token from Device B |
| **Expected Results** | <ul><li>Depends on policy: Either 401 Unauthorized, or OTP required</li><li>Policy documented</li></ul> |
| **Evidence** | <ul><li>API response</li></ul> |
| **Notes** | **Open Question**: Exact device binding policy needs Tech Lead clarification. |

---

#### SEC-AUTH-010: Admin Account Lockout After Failed Attempts
| Field | Value |
|-------|-------|
| **Test ID** | SEC-AUTH-010 |
| **Title** | Admin Account Locked After X Failed Login Attempts |
| **Priority** | P1 |
| **Story IDs** | US-AUTH-005 (EC1) |
| **Screen IDs** | SC-AUTH-005 |
| **Preconditions** | <ul><li>Lockout policy configured (e.g., 5 attempts)</li></ul> |
| **Test Data** | <ul><li>email: "ops.admin@coshare.test"</li><li>password: "WrongPassword" (repeat)</li></ul> |
| **Steps** | 1. Attempt login with wrong password 5+ times<br/>2. Attempt login with correct password |
| **Expected Results** | <ul><li>After threshold: Account locked</li><li>Correct password rejected with lockout message</li><li>Lockout logged</li></ul> |
| **Evidence** | <ul><li>API responses</li><li>DB: Account status or lockout timestamp</li></ul> |
| **Notes** | **Open Question**: Lockout threshold and duration not specified in SRS. Blocked pending requirement clarification. |

---

## Section 6: Regression Suite

The following test IDs are marked for inclusion in the regression test suite. These tests must pass before any release.

### Critical Path (P0)
| Test ID | Description | Story |
|---------|-------------|-------|
| E2E-AUTH-001 | Complete Registration Journey | US-AUTH-001 |
| E2E-AUTH-002 | First-Time Login with OTP | US-AUTH-002 |
| E2E-AUTH-003 | Auto-Login with Session Token | US-AUTH-003 |
| E2E-AUTH-004 | Token Refresh Flow | US-AUTH-003 |
| E2E-AUTH-005 | End User Logout | US-AUTH-004 |
| E2E-AUTH-006 | Admin Portal Login | US-AUTH-005 |
| E2E-AUTH-011 | Expired Tokens Force Re-Login | US-AUTH-003 |

### Security Critical (P0)
| Test ID | Description | Story |
|---------|-------------|-------|
| SEC-AUTH-001 | OTP Brute Force Prevention | US-AUTH-001/002 |
| SEC-AUTH-002 | OTP Rate Limiting | US-AUTH-001 |
| SEC-AUTH-006 | Revoked Token Rejected | US-AUTH-004 |
| SEC-AUTH-007 | No PII in Logs | All |

### API Stability (P0)
| Test ID | Description | Story |
|---------|-------------|-------|
| IT-AUTH-001 | Registration OTP Request | US-AUTH-001 |
| IT-AUTH-002 | Registration OTP Verify | US-AUTH-001 |
| IT-AUTH-005 | Token Refresh | US-AUTH-003 |
| IT-AUTH-006 | Logout | US-AUTH-004 |
| IT-AUTH-008 | Rate Limiting | US-AUTH-001 |

### Regression Suite Command (suggested)
```bash
# E2E Regression
npx playwright test --grep "@reg" --project=chromium

# API Regression  
dotnet test --filter "Category=Regression"
```

---

## Section 7: Open Questions / Blockers

| ID | Question | Impact | Status |
|----|----------|--------|--------|
| OQ-AUTH-001 | What is the exact admin account lockout threshold and duration? | Blocks SEC-AUTH-010 | Pending requirement clarification |
| OQ-AUTH-002 | What is the exact device binding policy when user switches devices? | Blocks SEC-AUTH-009 | Pending Tech Lead decision |
| OQ-AUTH-003 | Is there a "forgot password" flow for Admin Portal? | May need additional test cases | Pending requirement |
| OQ-AUTH-004 | What is the exact phone number regex pattern? | Affects IT-AUTH test data | Pending Tech Lead |
| OQ-AUTH-005 | Should logout revoke all devices or only current device? | Affects E2E-AUTH-014 expected behavior | Pending requirement |

### Blocked Test Cases
| Test ID | Blocked By | Resolution |
|---------|------------|------------|
| SEC-AUTH-010 | OQ-AUTH-001 | Cannot verify lockout behavior until threshold is defined |
| SEC-AUTH-009 | OQ-AUTH-002 | Cannot verify device binding until policy is confirmed |

---

## Appendix A: Test ID Convention

| Prefix | Type | Description |
|--------|------|-------------|
| E2E-AUTH-### | End-to-End | Full user journey tests via UI |
| IT-AUTH-### | Integration | API-level tests with DB |
| SEC-AUTH-### | Security | Security sanity and penetration tests |
| REG-AUTH-### | Regression | Subset of above marked for regression |

---

## Appendix B: Evidence Checklist Template

For each test execution, capture:
- [ ] Test ID and execution timestamp
- [ ] Test environment (staging/dev)
- [ ] X-Correlation-Id from API responses
- [ ] Screenshots for UI steps
- [ ] API response bodies (sensitive data redacted)
- [ ] DB state before/after (for state-changing tests)
- [ ] Log snippets (for security tests)
- [ ] Pass/Fail status with notes

---

**Document Version**: 1.0  
**Created**: 2026-01-04  
**QA Lead**: CoShare QA Team
