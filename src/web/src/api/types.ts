/**
 * TypeScript types derived from OpenAPI spec.
 * Source: docs/03-Design/OpenAPI/openapi.yaml
 * 
 * Contract-first: Do NOT modify without updating OpenAPI first.
 */

// ============= REQUEST TYPES =============

/**
 * POST /api/v1/auth/end-user/register/request-otp
 * Schema: EndUserRegisterStartRequest
 */
export interface EndUserRegisterStartRequest {
  fullName: string;       // maxLength: 200
  companyId: number;      // int64
  phone: string;          // maxLength: 20
  acceptTerms: boolean;
}

/**
 * POST /api/v1/auth/end-user/register/verify-otp
 * Schema: EndUserRegisterVerifyRequest
 */
export interface EndUserRegisterVerifyRequest {
  phone: string;          // maxLength: 20
  otpCode: string;        // maxLength: 10
  deviceInfo?: DeviceInfo;
}

/**
 * POST /api/v1/auth/end-user/login/request-otp
 * Schema: EndUserLoginStartRequest
 */
export interface EndUserLoginStartRequest {
  phone: string;          // maxLength: 20
}

/**
 * POST /api/v1/auth/end-user/login/verify-otp
 * Schema: EndUserLoginVerifyRequest
 */
export interface EndUserLoginVerifyRequest {
  phone: string;          // maxLength: 20
  otpCode: string;        // maxLength: 10
  deviceInfo?: DeviceInfo;
}

/**
 * Schema: DeviceInfo
 */
export interface DeviceInfo {
  deviceId?: string;      // maxLength: 128
  userAgent?: string;     // maxLength: 512
  platform?: string;      // maxLength: 128
}

// ============= RESPONSE TYPES =============

/**
 * Response for verify-otp: LoginResponse
 */
export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
  endUser?: EndUserProfileSummary;
  admin?: AdminProfileSummary;
}

export interface EndUserProfileSummary {
  userId: number;
  fullName: string;
  phone: string;
  companyId: number;
  companyName: string;
  tier: string;
}

export interface AdminProfileSummary {
  adminUserId: number;
  loginId: string;
}

/**
 * Error response envelope
 */
export interface ErrorResponse {
  error: {
    code: string;
    message: string;
    correlationId?: string;
  };
}

// ============= COMPANY TYPES (for dropdown) =============

export interface CompanySummary {
  id: number;
  companyCode: string;
  companyName: string;
  zone?: string;
  status: string;
}

// ============= AUTH ERROR CODES (from StoryPack) =============

export const AUTH_ERROR_CODES = {
  AUTH_INVALID_PHONE_FORMAT: 'AUTH_INVALID_PHONE_FORMAT',
  AUTH_OTP_RATE_LIMITED: 'AUTH_OTP_RATE_LIMITED',
  AUTH_OTP_DELIVERY_FAILED: 'AUTH_OTP_DELIVERY_FAILED',
  AUTH_OTP_INVALID_OR_EXPIRED: 'AUTH_OTP_INVALID_OR_EXPIRED',
  AUTH_USER_ALREADY_EXISTS: 'AUTH_USER_ALREADY_EXISTS',
  AUTH_USER_NOT_FOUND: 'AUTH_USER_NOT_FOUND',
  AUTH_USER_LOCKED: 'AUTH_USER_LOCKED',
  AUTH_IDEMPOTENCY_CONFLICT: 'AUTH_IDEMPOTENCY_CONFLICT',
} as const;

export type AuthErrorCode = keyof typeof AUTH_ERROR_CODES;
