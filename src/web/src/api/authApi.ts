/**
 * AUTH API client - Contract-first implementation
 * Source: docs/03-Design/OpenAPI/openapi.yaml
 * 
 * Endpoints:
 * - POST /api/v1/auth/end-user/register/request-otp
 * - POST /api/v1/auth/end-user/register/verify-otp
 * - POST /api/v1/auth/end-user/login/request-otp
 * - POST /api/v1/auth/end-user/login/verify-otp
 */

import type {
  EndUserRegisterStartRequest,
  EndUserRegisterVerifyRequest,
  EndUserLoginStartRequest,
  EndUserLoginVerifyRequest,
  LoginResponse,
  ErrorResponse,
  CompanySummary,
} from './types';

const API_BASE = '/api/v1';

/**
 * Generate a unique idempotency key
 */
function generateIdempotencyKey(): string {
  return crypto.randomUUID();
}

/**
 * Get device info for token binding
 */
function getDeviceInfo() {
  return {
    deviceId: localStorage.getItem('deviceId') || generateAndStoreDeviceId(),
    userAgent: navigator.userAgent.substring(0, 512),
    platform: navigator.platform.substring(0, 128),
  };
}

function generateAndStoreDeviceId(): string {
  const id = crypto.randomUUID();
  localStorage.setItem('deviceId', id);
  return id;
}

/**
 * Parse error response and extract error details
 */
async function parseErrorResponse(response: Response): Promise<ErrorResponse> {
  try {
    const data = await response.json();
    return data as ErrorResponse;
  } catch {
    return {
      error: {
        code: 'UNKNOWN_ERROR',
        message: 'Đã xảy ra lỗi không xác định',
        correlationId: response.headers.get('X-Correlation-Id') || undefined,
      },
    };
  }
}

export interface ApiResult<T> {
  success: boolean;
  data?: T;
  error?: ErrorResponse['error'];
  correlationId?: string;
}

/**
 * POST /api/v1/auth/end-user/register/request-otp
 * 
 * Starts registration for an end user by validating input and sending an OTP via NOTIFICATIONS.
 * Returns 202 Accepted on success.
 */
export async function requestRegistrationOtp(
  request: EndUserRegisterStartRequest,
  idempotencyKey?: string
): Promise<ApiResult<void>> {
  const key = idempotencyKey || generateIdempotencyKey();

  const response = await fetch(`${API_BASE}/auth/end-user/register/request-otp`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Idempotency-Key': key,
    },
    body: JSON.stringify(request),
  });

  const correlationId = response.headers.get('X-Correlation-Id') || undefined;

  if (response.status === 202) {
    return { success: true, correlationId };
  }

  const errorResponse = await parseErrorResponse(response);
  return {
    success: false,
    error: errorResponse.error,
    correlationId: errorResponse.error.correlationId || correlationId,
  };
}

/**
 * POST /api/v1/auth/end-user/register/verify-otp
 * 
 * Verifies the OTP for the given phone number and, on success, creates the end-user account and issues tokens.
 * Returns 200 with LoginResponse on success.
 */
export async function verifyRegistrationOtp(
  phone: string,
  otpCode: string,
  idempotencyKey?: string
): Promise<ApiResult<LoginResponse>> {
  const key = idempotencyKey || generateIdempotencyKey();

  const request: EndUserRegisterVerifyRequest = {
    phone,
    otpCode,
    deviceInfo: getDeviceInfo(),
  };

  const response = await fetch(`${API_BASE}/auth/end-user/register/verify-otp`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Idempotency-Key': key,
    },
    body: JSON.stringify(request),
  });

  const correlationId = response.headers.get('X-Correlation-Id') || undefined;

  if (response.ok) {
    const data = (await response.json()) as LoginResponse;
    return { success: true, data, correlationId };
  }

  const errorResponse = await parseErrorResponse(response);
  return {
    success: false,
    error: errorResponse.error,
    correlationId: errorResponse.error.correlationId || correlationId,
  };
}

/**
 * GET /api/v1/admin/companies (simplified for dropdown)
 * 
 * For registration form company dropdown.
 * Note: Actual endpoint may vary based on admin/public API design.
 */
export async function getCompanies(): Promise<ApiResult<CompanySummary[]>> {
  try {
    const response = await fetch(`${API_BASE}/admin/companies`);
    const correlationId = response.headers.get('X-Correlation-Id') || undefined;

    if (response.ok) {
      const data = await response.json();
      // Handle paged response format
      const items = data.items || data;
      return { success: true, data: items, correlationId };
    }

    const errorResponse = await parseErrorResponse(response);
    return {
      success: false,
      error: errorResponse.error,
      correlationId,
    };
  } catch {
    return {
      success: false,
      error: {
        code: 'NETWORK_ERROR',
        message: 'Không thể kết nối đến máy chủ',
      },
    };
  }
}

/**
 * POST /api/v1/auth/end-user/login/request-otp
 * 
 * For an existing end user, validate the phone number and trigger an OTP via NOTIFICATIONS for first-time login.
 * Returns 202 Accepted on success.
 */
export async function requestLoginOtp(
  request: EndUserLoginStartRequest,
  idempotencyKey?: string
): Promise<ApiResult<void>> {
  const key = idempotencyKey || generateIdempotencyKey();

  const response = await fetch(`${API_BASE}/auth/end-user/login/request-otp`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Idempotency-Key': key,
    },
    body: JSON.stringify(request),
  });

  const correlationId = response.headers.get('X-Correlation-Id') || undefined;

  if (response.status === 202) {
    return { success: true, correlationId };
  }

  const errorResponse = await parseErrorResponse(response);
  return {
    success: false,
    error: errorResponse.error,
    correlationId: errorResponse.error.correlationId || correlationId,
  };
}

/**
 * POST /api/v1/auth/end-user/login/verify-otp
 * 
 * Verifies the OTP for an existing end user and issues tokens for the device/browser.
 * Returns 200 with LoginResponse on success.
 */
export async function verifyLoginOtp(
  phone: string,
  otpCode: string,
  idempotencyKey?: string
): Promise<ApiResult<LoginResponse>> {
  const key = idempotencyKey || generateIdempotencyKey();

  const request: EndUserLoginVerifyRequest = {
    phone,
    otpCode,
    deviceInfo: getDeviceInfo(),
  };

  const response = await fetch(`${API_BASE}/auth/end-user/login/verify-otp`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Idempotency-Key': key,
    },
    body: JSON.stringify(request),
  });

  const correlationId = response.headers.get('X-Correlation-Id') || undefined;

  if (response.ok) {
    const data = (await response.json()) as LoginResponse;
    return { success: true, data, correlationId };
  }

  const errorResponse = await parseErrorResponse(response);
  return {
    success: false,
    error: errorResponse.error,
    correlationId: errorResponse.error.correlationId || correlationId,
  };
}
