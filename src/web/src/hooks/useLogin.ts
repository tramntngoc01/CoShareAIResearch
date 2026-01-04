/**
 * useLogin hook - Login flow state management
 * Manages the two-step login: request OTP → verify OTP
 * 
 * Story: US-AUTH-002 — Đăng nhập End User lần đầu bằng OTP
 */

import { useState, useCallback } from 'react';
import { requestLoginOtp, verifyLoginOtp } from '../api/authApi';
import type { LoginResponse } from '../api/types';
import { getErrorMessage } from '../utils/errorMessages';

export type LoginStep = 'phone-input' | 'otp' | 'success';

export interface LoginState {
  step: LoginStep;
  isLoading: boolean;
  error: string | null;
  correlationId: string | null;
  phone: string | null;
  loginResponse: LoginResponse | null;
}

export interface UseLoginReturn extends LoginState {
  submitPhone: (phone: string) => Promise<boolean>;
  submitOtp: (otpCode: string) => Promise<boolean>;
  resendOtp: () => Promise<boolean>;
  goBack: () => void;
  clearError: () => void;
}

export function useLogin(): UseLoginReturn {
  const [state, setState] = useState<LoginState>({
    step: 'phone-input',
    isLoading: false,
    error: null,
    correlationId: null,
    phone: null,
    loginResponse: null,
  });

  const submitPhone = useCallback(async (phone: string): Promise<boolean> => {
    setState((prev) => ({ ...prev, isLoading: true, error: null }));

    const result = await requestLoginOtp({ phone });

    if (result.success) {
      setState((prev) => ({
        ...prev,
        isLoading: false,
        step: 'otp',
        phone,
        correlationId: result.correlationId || null,
      }));
      return true;
    }

    setState((prev) => ({
      ...prev,
      isLoading: false,
      error: getErrorMessage(result.error?.code || 'UNKNOWN_ERROR'),
      correlationId: result.correlationId || null,
    }));
    return false;
  }, []);

  const submitOtp = useCallback(async (otpCode: string): Promise<boolean> => {
    if (!state.phone) {
      setState((prev) => ({ ...prev, error: 'Không tìm thấy số điện thoại. Vui lòng thử lại.' }));
      return false;
    }

    setState((prev) => ({ ...prev, isLoading: true, error: null }));

    const result = await verifyLoginOtp(state.phone, otpCode);

    if (result.success && result.data) {
      setState((prev) => ({
        ...prev,
        isLoading: false,
        step: 'success',
        loginResponse: result.data!,
        correlationId: result.correlationId || null,
      }));
      return true;
    }

    setState((prev) => ({
      ...prev,
      isLoading: false,
      error: getErrorMessage(result.error?.code || 'UNKNOWN_ERROR'),
      correlationId: result.correlationId || null,
    }));
    return false;
  }, [state.phone]);

  const resendOtp = useCallback(async (): Promise<boolean> => {
    if (!state.phone) {
      setState((prev) => ({ ...prev, error: 'Không thể gửi lại OTP. Vui lòng thử lại từ đầu.' }));
      return false;
    }

    setState((prev) => ({ ...prev, isLoading: true, error: null }));

    const result = await requestLoginOtp({ phone: state.phone });

    if (result.success) {
      setState((prev) => ({
        ...prev,
        isLoading: false,
        correlationId: result.correlationId || null,
      }));
      return true;
    }

    setState((prev) => ({
      ...prev,
      isLoading: false,
      error: getErrorMessage(result.error?.code || 'UNKNOWN_ERROR'),
      correlationId: result.correlationId || null,
    }));
    return false;
  }, [state.phone]);

  const goBack = useCallback(() => {
    setState((prev) => ({
      ...prev,
      step: 'phone-input',
      error: null,
    }));
  }, []);

  const clearError = useCallback(() => {
    setState((prev) => ({ ...prev, error: null }));
  }, []);

  return {
    ...state,
    submitPhone,
    submitOtp,
    resendOtp,
    goBack,
    clearError,
  };
}
