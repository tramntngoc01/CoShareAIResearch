/**
 * US-AUTH-002 E2E Tests - End User Login
 * 
 * Story: US-AUTH-002 — Đăng nhập End User lần đầu bằng OTP
 * Screens: LoginForm (custom), SC-AUTH-002 (OTP verification)
 * 
 * Test coverage:
 * - 1 happy path test
 * - 3+ edge case / negative tests (per StoryPack EC1-EC4)
 * 
 * Selector convention: data-field (NOT data-testid)
 * Rule: Do NOT use waitForTimeout. Use proper locators + expect.
 */

import { test, expect } from '@playwright/test';

// Test data (fake, per StoryPack sample data)
const VALID_LOGIN = {
  phone: '0912345678',
};

const INVALID_PHONE = '09123ABC';
const UNREGISTERED_PHONE = '0987654321';
const LOCKED_USER_PHONE = '0900000001';
const VALID_OTP = '112233';
const INVALID_OTP = '000000';

test.describe('US-AUTH-002 - End User Login via Phone + OTP', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    // Wait for the login screen to be visible
    await expect(page.locator('[data-field="screen.auth.login-form"]')).toBeVisible();
  });

  // ============= HAPPY PATH =============

  test('US-AUTH-002 - Happy path: Existing user login with valid phone and OTP', async ({ page }) => {
    // Mock the API responses for happy path
    await page.route('**/api/v1/auth/end-user/login/request-otp', async (route) => {
      await route.fulfill({
        status: 202,
        headers: { 'X-Correlation-Id': 'test-login-corr-001' },
      });
    });

    await page.route('**/api/v1/auth/end-user/login/verify-otp', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        headers: { 'X-Correlation-Id': 'test-login-corr-002' },
        body: JSON.stringify({
          accessToken: 'test-login-access-token',
          refreshToken: 'test-login-refresh-token',
          expiresIn: 3600,
          tokenType: 'Bearer',
          endUser: {
            userId: 10001,
            fullName: 'Nguyễn Văn A',
            phone: VALID_LOGIN.phone,
            companyId: 501,
            companyName: 'Công ty Điện tử KCN X',
            tier: 'T3',
          },
        }),
      });
    });

    // Step 1: Fill phone and submit
    await page.locator('[data-field="input.phone"]').fill(VALID_LOGIN.phone);
    await page.locator('[data-field="action.request-otp"]').click();

    // Step 2: Wait for OTP screen (SC-AUTH-002)
    await expect(page.locator('[data-field="screen.auth.sc-auth-002"]')).toBeVisible();

    // Verify phone is displayed (masked)
    await expect(page.locator('.phone-display')).toContainText('5678');

    // Enter OTP
    for (let i = 0; i < 6; i++) {
      await page.locator(`[data-field="input.otp-digit-${i}"]`).fill(VALID_OTP[i]);
    }

    // Submit OTP
    await page.locator('[data-field="action.verify-otp"]').click();

    // Step 3: Verify success state
    await expect(page.locator('[data-field="state.success"]')).toBeVisible();
    await expect(page.locator('[data-field="state.success"]')).toContainText('Đăng nhập thành công');
  });

  // ============= EDGE CASE: EC1 - Invalid phone format =============

  test('US-AUTH-002 - EC1: Invalid phone format shows validation error', async ({ page }) => {
    // Fill form with invalid phone
    await page.locator('[data-field="input.phone"]').fill(INVALID_PHONE);

    // Submit
    await page.locator('[data-field="action.request-otp"]').click();

    // Verify client-side validation error for phone
    await expect(page.locator('[data-field="error.phone"]')).toBeVisible();
    await expect(page.locator('[data-field="error.phone"]')).toContainText('không hợp lệ');

    // Should still be on login form
    await expect(page.locator('[data-field="screen.auth.login-form"]')).toBeVisible();
  });

  // ============= EDGE CASE: EC3 - Phone not registered (AUTH_USER_NOT_FOUND) =============

  test('US-AUTH-002 - EC3: Unregistered phone shows not found error', async ({ page }) => {
    // Mock user not found response
    await page.route('**/api/v1/auth/end-user/login/request-otp', async (route) => {
      await route.fulfill({
        status: 401,
        contentType: 'application/json',
        headers: { 'X-Correlation-Id': 'test-login-not-found' },
        body: JSON.stringify({
          error: {
            code: 'AUTH_USER_NOT_FOUND',
            message: 'User not found',
            correlationId: 'test-login-not-found',
          },
        }),
      });
    });

    // Fill valid phone format but unregistered
    await page.locator('[data-field="input.phone"]').fill(UNREGISTERED_PHONE);
    await page.locator('[data-field="action.request-otp"]').click();

    // Verify error summary shows not found message
    await expect(page.locator('[data-field="error.summary"]')).toBeVisible();
    await expect(page.locator('[data-field="error.summary"]')).toContainText('chưa được đăng ký');

    // Verify correlationId is shown
    await expect(page.locator('[data-field="error.summary"]')).toContainText('test-login-not-found');

    // Should still be on login form
    await expect(page.locator('[data-field="screen.auth.login-form"]')).toBeVisible();
  });

  // ============= EDGE CASE: EC3 - Account locked (AUTH_USER_LOCKED) =============

  test('US-AUTH-002 - EC3: Locked account shows locked error', async ({ page }) => {
    // Mock account locked response
    await page.route('**/api/v1/auth/end-user/login/request-otp', async (route) => {
      await route.fulfill({
        status: 401,
        contentType: 'application/json',
        headers: { 'X-Correlation-Id': 'test-login-locked' },
        body: JSON.stringify({
          error: {
            code: 'AUTH_USER_LOCKED',
            message: 'User account is locked',
            correlationId: 'test-login-locked',
          },
        }),
      });
    });

    // Fill phone for locked account
    await page.locator('[data-field="input.phone"]').fill(LOCKED_USER_PHONE);
    await page.locator('[data-field="action.request-otp"]').click();

    // Verify error summary shows locked message
    await expect(page.locator('[data-field="error.summary"]')).toBeVisible();
    await expect(page.locator('[data-field="error.summary"]')).toContainText('bị khóa');

    // Verify correlationId is shown
    await expect(page.locator('[data-field="error.summary"]')).toContainText('test-login-locked');

    // Should still be on login form
    await expect(page.locator('[data-field="screen.auth.login-form"]')).toBeVisible();
  });

  // ============= EDGE CASE: EC2 - Rate limited OTP request =============

  test('US-AUTH-002 - EC2: Rate limited OTP request shows error message', async ({ page }) => {
    // Mock rate limit response
    await page.route('**/api/v1/auth/end-user/login/request-otp', async (route) => {
      await route.fulfill({
        status: 429,
        contentType: 'application/json',
        headers: { 'X-Correlation-Id': 'test-login-rate-limit' },
        body: JSON.stringify({
          error: {
            code: 'AUTH_OTP_RATE_LIMITED',
            message: 'Too many OTP requests',
            correlationId: 'test-login-rate-limit',
          },
        }),
      });
    });

    // Fill valid phone
    await page.locator('[data-field="input.phone"]').fill(VALID_LOGIN.phone);
    await page.locator('[data-field="action.request-otp"]').click();

    // Verify error summary shows rate limit message
    await expect(page.locator('[data-field="error.summary"]')).toBeVisible();
    await expect(page.locator('[data-field="error.summary"]')).toContainText('quá nhiều lần');

    // Verify correlationId is shown
    await expect(page.locator('[data-field="error.summary"]')).toContainText('test-login-rate-limit');
  });

  // ============= EDGE CASE: Invalid OTP =============

  test('US-AUTH-002 - EC1: Wrong or expired OTP shows validation error', async ({ page }) => {
    // Mock successful OTP request
    await page.route('**/api/v1/auth/end-user/login/request-otp', async (route) => {
      await route.fulfill({
        status: 202,
        headers: { 'X-Correlation-Id': 'test-login-corr-003' },
      });
    });

    // Mock invalid OTP response
    await page.route('**/api/v1/auth/end-user/login/verify-otp', async (route) => {
      await route.fulfill({
        status: 401,
        contentType: 'application/json',
        headers: { 'X-Correlation-Id': 'test-login-invalid-otp' },
        body: JSON.stringify({
          error: {
            code: 'AUTH_OTP_INVALID_OR_EXPIRED',
            message: 'OTP is invalid or expired',
            correlationId: 'test-login-invalid-otp',
          },
        }),
      });
    });

    // Fill and submit phone
    await page.locator('[data-field="input.phone"]').fill(VALID_LOGIN.phone);
    await page.locator('[data-field="action.request-otp"]').click();

    // Wait for OTP screen
    await expect(page.locator('[data-field="screen.auth.sc-auth-002"]')).toBeVisible();

    // Enter wrong OTP
    for (let i = 0; i < 6; i++) {
      await page.locator(`[data-field="input.otp-digit-${i}"]`).fill(INVALID_OTP[i]);
    }

    // Submit OTP
    await page.locator('[data-field="action.verify-otp"]').click();

    // Verify error message
    await expect(page.locator('[data-field="error.summary"]')).toBeVisible();
    await expect(page.locator('[data-field="error.summary"]')).toContainText('không đúng hoặc đã hết hạn');

    // Should still be on OTP screen (not success)
    await expect(page.locator('[data-field="screen.auth.sc-auth-002"]')).toBeVisible();
    await expect(page.locator('[data-field="state.success"]')).not.toBeVisible();
  });

  // ============= VALIDATION: Empty phone field =============

  test('US-AUTH-002 - Validation: Empty phone shows required field error', async ({ page }) => {
    // Submit empty form
    await page.locator('[data-field="action.request-otp"]').click();

    // Verify phone field error is shown
    await expect(page.locator('[data-field="error.phone"]')).toBeVisible();
    await expect(page.locator('[data-field="error.phone"]')).toContainText('Vui lòng nhập');

    // Should still be on login form
    await expect(page.locator('[data-field="screen.auth.login-form"]')).toBeVisible();
  });

  // ============= NAVIGATION: Back from OTP screen =============

  test('US-AUTH-002 - Navigation: Back button returns to login form', async ({ page }) => {
    // Mock successful OTP request
    await page.route('**/api/v1/auth/end-user/login/request-otp', async (route) => {
      await route.fulfill({
        status: 202,
        headers: { 'X-Correlation-Id': 'test-login-back' },
      });
    });

    // Fill and submit phone
    await page.locator('[data-field="input.phone"]').fill(VALID_LOGIN.phone);
    await page.locator('[data-field="action.request-otp"]').click();

    // Wait for OTP screen
    await expect(page.locator('[data-field="screen.auth.sc-auth-002"]')).toBeVisible();

    // Click back button
    await page.locator('[data-field="action.back"]').click();

    // Should be back on login form
    await expect(page.locator('[data-field="screen.auth.login-form"]')).toBeVisible();
  });
});
