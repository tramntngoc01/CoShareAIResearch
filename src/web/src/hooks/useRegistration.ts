/**
 * useRegistration hook - Registration flow state management
 * Manages the two-step registration: request OTP → verify OTP
 */

import { useState, useCallback } from 'react';
import { requestRegistrationOtp, verifyRegistrationOtp } from '../api/authApi';
import type { EndUserRegisterStartRequest, LoginResponse } from '../api/types';
import { getErrorMessage } from '../utils/errorMessages';

export type RegistrationStep = 'form' | 'otp' | 'success';

export interface RegistrationState {
  step: RegistrationStep;
  isLoading: boolean;
  error: string | null;
  correlationId: string | null;
  phone: string | null;
  loginResponse: LoginResponse | null;
}

export interface UseRegistrationReturn extends RegistrationState {
  submitRegistration: (data: EndUserRegisterStartRequest) => Promise<boolean>;
  submitOtp: (otpCode: string) => Promise<boolean>;
  resendOtp: () => Promise<boolean>;
  goBack: () => void;
  clearError: () => void;
}

export function useRegistration(): UseRegistrationReturn {
  const [state, setState] = useState<RegistrationState>({
    step: 'form',
    isLoading: false,
    error: null,
    correlationId: null,
    phone: null,
    loginResponse: null,
  });

  // Store registration data for resend
  const [registrationData, setRegistrationData] = useState<EndUserRegisterStartRequest | null>(null);

  const submitRegistration = useCallback(async (data: EndUserRegisterStartRequest): Promise<boolean> => {
    setState((prev) => ({ ...prev, isLoading: true, error: null }));
    setRegistrationData(data);

    const result = await requestRegistrationOtp(data);

    if (result.success) {
      setState((prev) => ({
        ...prev,
        isLoading: false,
        step: 'otp',
        phone: data.phone,
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

    const result = await verifyRegistrationOtp(state.phone, otpCode);

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
    if (!registrationData) {
      setState((prev) => ({ ...prev, error: 'Không thể gửi lại OTP. Vui lòng thử lại từ đầu.' }));
      return false;
    }

    setState((prev) => ({ ...prev, isLoading: true, error: null }));

    const result = await requestRegistrationOtp(registrationData);

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
  }, [registrationData]);

  const goBack = useCallback(() => {
    setState((prev) => ({
      ...prev,
      step: 'form',
      error: null,
    }));
  }, []);

  const clearError = useCallback(() => {
    setState((prev) => ({ ...prev, error: null }));
  }, []);

  return {
    ...state,
    submitRegistration,
    submitOtp,
    resendOtp,
    goBack,
    clearError,
  };
}
