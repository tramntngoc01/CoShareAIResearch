/**
 * Error code to user-friendly message mapping
 * Per Frontend-Coding-Standards.md: Map error.code to user-friendly messages.
 * 
 * Source: docs/02-Requirements/StoryPacks/US-AUTH-001.md (Error codes used section)
 */

import { AUTH_ERROR_CODES } from '../api/types';

const ERROR_MESSAGES: Record<string, string> = {
  // AUTH module error codes (from StoryPack)
  [AUTH_ERROR_CODES.AUTH_INVALID_PHONE_FORMAT]:
    'Số điện thoại không đúng định dạng. Vui lòng kiểm tra lại.',
  [AUTH_ERROR_CODES.AUTH_OTP_RATE_LIMITED]:
    'Bạn đã yêu cầu OTP quá nhiều lần. Vui lòng thử lại sau ít phút.',
  [AUTH_ERROR_CODES.AUTH_OTP_DELIVERY_FAILED]:
    'Không thể gửi mã OTP. Vui lòng thử lại sau.',
  [AUTH_ERROR_CODES.AUTH_OTP_INVALID_OR_EXPIRED]:
    'Mã OTP không đúng hoặc đã hết hạn. Vui lòng kiểm tra lại hoặc yêu cầu mã mới.',
  [AUTH_ERROR_CODES.AUTH_USER_ALREADY_EXISTS]:
    'Số điện thoại này đã được đăng ký. Vui lòng đăng nhập hoặc sử dụng số khác.',
  [AUTH_ERROR_CODES.AUTH_IDEMPOTENCY_CONFLICT]:
    'Yêu cầu trùng lặp. Vui lòng thử lại.',

  // Generic errors
  NETWORK_ERROR: 'Không thể kết nối đến máy chủ. Vui lòng kiểm tra kết nối mạng.',
  UNKNOWN_ERROR: 'Đã xảy ra lỗi không xác định. Vui lòng thử lại sau.',
  VALIDATION_ERROR: 'Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.',
};

/**
 * Get user-friendly error message from error code
 */
export function getErrorMessage(code: string, fallback?: string): string {
  return ERROR_MESSAGES[code] || fallback || ERROR_MESSAGES.UNKNOWN_ERROR;
}

/**
 * Field-level validation error messages
 */
export const FIELD_ERRORS = {
  fullName: {
    required: 'Vui lòng nhập họ tên',
    minLength: 'Họ tên phải có ít nhất 2 ký tự',
    maxLength: 'Họ tên không được quá 200 ký tự',
  },
  phone: {
    required: 'Vui lòng nhập số điện thoại',
    invalid: 'Số điện thoại không hợp lệ (VD: 0912345678)',
  },
  companyId: {
    required: 'Vui lòng chọn công ty',
  },
  acceptTerms: {
    required: 'Vui lòng đồng ý với điều khoản sử dụng',
  },
  otpCode: {
    required: 'Vui lòng nhập mã OTP',
    invalid: 'Mã OTP phải gồm 6 chữ số',
  },
} as const;
