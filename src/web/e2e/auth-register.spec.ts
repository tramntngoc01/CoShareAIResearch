/**
 * US-AUTH-001 E2E Tests - End User Registration
 * 
 * Story: US-AUTH-001 — Đăng ký End User bằng SĐT + OTP ZNS
 * Screens: SC-AUTH-001, SC-AUTH-002
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
const VALID_REGISTRATION = {
  fullName: 'Trần Thị B',
  phone: '0912345678',
};

const INVALID_PHONE = '09123ABC';
const SHORT_PHONE = '0912345';
const VALID_OTP = '123456';
const INVALID_OTP = '000000';

test.describe('US-AUTH-001 - End User Registration via Phone + OTP', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/register');
    // Wait for the registration screen to be visible
    await expect(page.locator('[data-field="screen.auth.sc-auth-001"]')).toBeVisible();
  });

  // ============= HAPPY PATH =============

  test('US-AUTH-001 - Happy path: Register new user with valid phone and OTP', async ({ page }) => {
    // Mock the API responses for happy path
    await page.route('**/api/v1/admin/companies', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [
            { id: 501, companyCode: 'CTY001', companyName: 'Công ty Điện tử KCN X', status: 'ACTIVE' },
            { id: 502, companyCode: 'CTY002', companyName: 'Công ty ABC', status: 'ACTIVE' },
          ],
        }),
      });
    });

    await page.route('**/api/v1/auth/end-user/register/request-otp', async (route) => {
      await route.fulfill({
        status: 202,
        headers: { 'X-Correlation-Id': 'test-corr-001' },
      });
    });

    await page.route('**/api/v1/auth/end-user/register/verify-otp', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        headers: { 'X-Correlation-Id': 'test-corr-002' },
        body: JSON.stringify({
          accessToken: 'test-access-token',
          refreshToken: 'test-refresh-token',
          expiresIn: 3600,
          tokenType: 'Bearer',
          endUser: {
            userId: 10001,
            fullName: VALID_REGISTRATION.fullName,
            phone: VALID_REGISTRATION.phone,
            companyId: 501,
            companyName: 'Công ty Điện tử KCN X',
            tier: 'T3',
          },
        }),
      });
    });

    // Reload to get mocked companies
    await page.goto('/register');
    await expect(page.locator('[data-field="screen.auth.sc-auth-001"]')).toBeVisible();

    // Step 1: Fill registration form (SC-AUTH-001)
    await page.locator('[data-field="input.full-name"]').fill(VALID_REGISTRATION.fullName);
    
    // Wait for companies to load and select one
    await expect(page.locator('[data-field="input.company-id"]')).not.toHaveAttribute('disabled');
    await page.locator('[data-field="input.company-id"]').selectOption('501');
    
    await page.locator('[data-field="input.phone"]').fill(VALID_REGISTRATION.phone);
    await page.locator('[data-field="input.accept-terms"]').check();

    // Submit form
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
    await expect(page.locator('[data-field="state.success"]')).toContainText('Đăng ký thành công');
    await expect(page.locator('[data-field="state.success"]')).toContainText(VALID_REGISTRATION.fullName);
  });

  // ============= EDGE CASE: EC1 - Invalid phone format =============

  test('US-AUTH-001 - EC1: Invalid phone format shows validation error', async ({ page }) => {
    // Mock companies API
    await page.route('**/api/v1/admin/companies', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [
            { id: 501, companyCode: 'CTY001', companyName: 'Công ty Test', status: 'ACTIVE' },
          ],
        }),
      });
    });

    await page.goto('/register');
    await expect(page.locator('[data-field="screen.auth.sc-auth-001"]')).toBeVisible();

    // Fill form with invalid phone
    await page.locator('[data-field="input.full-name"]').fill(VALID_REGISTRATION.fullName);
    await expect(page.locator('[data-field="input.company-id"]')).not.toHaveAttribute('disabled');
    await page.locator('[data-field="input.company-id"]').selectOption('501');
    await page.locator('[data-field="input.phone"]').fill(INVALID_PHONE);
    await page.locator('[data-field="input.accept-terms"]').check();

    // Submit
    await page.locator('[data-field="action.request-otp"]').click();

    // Verify client-side validation error for phone
    await expect(page.locator('[data-field="error.phone"]')).toBeVisible();
    await expect(page.locator('[data-field="error.phone"]')).toContainText('không hợp lệ');

    // Should still be on SC-AUTH-001
    await expect(page.locator('[data-field="screen.auth.sc-auth-001"]')).toBeVisible();
  });

  // ============= EDGE CASE: EC2 - Rate limited OTP request =============

  test('US-AUTH-001 - EC2: Rate limited OTP request shows error message', async ({ page }) => {
    // Mock companies API
    await page.route('**/api/v1/admin/companies', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [
            { id: 501, companyCode: 'CTY001', companyName: 'Công ty Test', status: 'ACTIVE' },
          ],
        }),
      });
    });

    // Mock rate limit response
    await page.route('**/api/v1/auth/end-user/register/request-otp', async (route) => {
      await route.fulfill({
        status: 429,
        contentType: 'application/json',
        headers: { 'X-Correlation-Id': 'test-corr-rate-limit' },
        body: JSON.stringify({
          error: {
            code: 'AUTH_OTP_RATE_LIMITED',
            message: 'Too many OTP requests',
            correlationId: 'test-corr-rate-limit',
          },
        }),
      });
    });

    await page.goto('/register');
    await expect(page.locator('[data-field="screen.auth.sc-auth-001"]')).toBeVisible();

    // Fill valid form
    await page.locator('[data-field="input.full-name"]').fill(VALID_REGISTRATION.fullName);
    await expect(page.locator('[data-field="input.company-id"]')).not.toHaveAttribute('disabled');
    await page.locator('[data-field="input.company-id"]').selectOption('501');
    await page.locator('[data-field="input.phone"]').fill(VALID_REGISTRATION.phone);
    await page.locator('[data-field="input.accept-terms"]').check();

    // Submit
    await page.locator('[data-field="action.request-otp"]').click();

    // Verify error summary shows rate limit message
    await expect(page.locator('[data-field="error.summary"]')).toBeVisible();
    await expect(page.locator('[data-field="error.summary"]')).toContainText('quá nhiều lần');

    // Verify correlationId is shown
    await expect(page.locator('[data-field="error.summary"]')).toContainText('test-corr-rate-limit');
  });

  // ============= EDGE CASE: EC3 - OTP delivery failed =============

  test('US-AUTH-001 - EC3: ZNS delivery failure shows friendly error', async ({ page }) => {
    // Mock companies API
    await page.route('**/api/v1/admin/companies', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [
            { id: 501, companyCode: 'CTY001', companyName: 'Công ty Test', status: 'ACTIVE' },
          ],
        }),
      });
    });

    // Mock delivery failure response
    await page.route('**/api/v1/auth/end-user/register/request-otp', async (route) => {
      await route.fulfill({
        status: 500,
        contentType: 'application/json',
        headers: { 'X-Correlation-Id': 'test-corr-zns-fail' },
        body: JSON.stringify({
          error: {
            code: 'AUTH_OTP_DELIVERY_FAILED',
            message: 'ZNS provider error',
            correlationId: 'test-corr-zns-fail',
          },
        }),
      });
    });

    await page.goto('/register');
    await expect(page.locator('[data-field="screen.auth.sc-auth-001"]')).toBeVisible();

    // Fill valid form
    await page.locator('[data-field="input.full-name"]').fill(VALID_REGISTRATION.fullName);
    await expect(page.locator('[data-field="input.company-id"]')).not.toHaveAttribute('disabled');
    await page.locator('[data-field="input.company-id"]').selectOption('501');
    await page.locator('[data-field="input.phone"]').fill(VALID_REGISTRATION.phone);
    await page.locator('[data-field="input.accept-terms"]').check();

    // Submit
    await page.locator('[data-field="action.request-otp"]').click();

    // Verify error shows user-friendly message (not technical error)
    await expect(page.locator('[data-field="error.summary"]')).toBeVisible();
    await expect(page.locator('[data-field="error.summary"]')).toContainText('Không thể gửi mã OTP');
  });

  // ============= EDGE CASE: EC4 / Invalid OTP =============

  test('US-AUTH-001 - EC4: Wrong or expired OTP shows validation error', async ({ page }) => {
    // Mock companies API
    await page.route('**/api/v1/admin/companies', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [
            { id: 501, companyCode: 'CTY001', companyName: 'Công ty Test', status: 'ACTIVE' },
          ],
        }),
      });
    });

    // Mock successful OTP request
    await page.route('**/api/v1/auth/end-user/register/request-otp', async (route) => {
      await route.fulfill({
        status: 202,
        headers: { 'X-Correlation-Id': 'test-corr-003' },
      });
    });

    // Mock invalid OTP response
    await page.route('**/api/v1/auth/end-user/register/verify-otp', async (route) => {
      await route.fulfill({
        status: 401,
        contentType: 'application/json',
        headers: { 'X-Correlation-Id': 'test-corr-invalid-otp' },
        body: JSON.stringify({
          error: {
            code: 'AUTH_OTP_INVALID_OR_EXPIRED',
            message: 'OTP is invalid or expired',
            correlationId: 'test-corr-invalid-otp',
          },
        }),
      });
    });

    await page.goto('/register');
    await expect(page.locator('[data-field="screen.auth.sc-auth-001"]')).toBeVisible();

    // Fill and submit registration form
    await page.locator('[data-field="input.full-name"]').fill(VALID_REGISTRATION.fullName);
    await expect(page.locator('[data-field="input.company-id"]')).not.toHaveAttribute('disabled');
    await page.locator('[data-field="input.company-id"]').selectOption('501');
    await page.locator('[data-field="input.phone"]').fill(VALID_REGISTRATION.phone);
    await page.locator('[data-field="input.accept-terms"]').check();
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

  // ============= VALIDATION: Required fields =============

  test('US-AUTH-001 - Validation: Empty form shows all required field errors', async ({ page }) => {
    // Submit empty form
    await page.locator('[data-field="action.request-otp"]').click();

    // Verify all field errors are shown
    await expect(page.locator('[data-field="error.full-name"]')).toBeVisible();
    await expect(page.locator('[data-field="error.company-id"]')).toBeVisible();
    await expect(page.locator('[data-field="error.phone"]')).toBeVisible();
    await expect(page.locator('[data-field="error.accept-terms"]')).toBeVisible();

    // Should still be on registration screen
    await expect(page.locator('[data-field="screen.auth.sc-auth-001"]')).toBeVisible();
  });

  // ============= NAVIGATION: Back from OTP screen =============

  test('US-AUTH-001 - Navigation: Back button returns to registration form', async ({ page }) => {
    // Mock companies API
    await page.route('**/api/v1/admin/companies', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [
            { id: 501, companyCode: 'CTY001', companyName: 'Công ty Test', status: 'ACTIVE' },
          ],
        }),
      });
    });

    // Mock successful OTP request
    await page.route('**/api/v1/auth/end-user/register/request-otp', async (route) => {
      await route.fulfill({
        status: 202,
        headers: { 'X-Correlation-Id': 'test-corr-back' },
      });
    });

    await page.goto('/register');

    // Fill and submit form
    await page.locator('[data-field="input.full-name"]').fill(VALID_REGISTRATION.fullName);
    await expect(page.locator('[data-field="input.company-id"]')).not.toHaveAttribute('disabled');
    await page.locator('[data-field="input.company-id"]').selectOption('501');
    await page.locator('[data-field="input.phone"]').fill(VALID_REGISTRATION.phone);
    await page.locator('[data-field="input.accept-terms"]').check();
    await page.locator('[data-field="action.request-otp"]').click();

    // Wait for OTP screen
    await expect(page.locator('[data-field="screen.auth.sc-auth-002"]')).toBeVisible();

    // Click back button
    await page.locator('[data-field="action.back"]').click();

    // Should be back on registration form
    await expect(page.locator('[data-field="screen.auth.sc-auth-001"]')).toBeVisible();
  });
});
