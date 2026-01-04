/**
 * OtpVerification - SC-AUTH-002 Implementation
 * 
 * Screen: SC-AUTH-002 — OTP Verification (End User)
 * Story: US-AUTH-001
 * API: POST /api/v1/auth/end-user/register/verify-otp
 * 
 * data-field selectors (per Frontend-Coding-Standards.md):
 * - screen.auth.sc-auth-002: Screen root container
 * - input.otp: OTP input container
 * - input.otp-digit-{0-5}: Individual OTP digit inputs
 * - action.verify-otp: Verify button
 * - action.resend-otp: Resend OTP button
 * - action.back: Back button
 * - error.summary: Error message container
 * - error.otp: OTP field error
 * - state.loading: Loading indicator
 * - state.success: Success state
 */

import { useState, useCallback, useRef, useEffect } from 'react';
import { FIELD_ERRORS } from '../../utils/errorMessages';

interface OtpVerificationProps {
  phone: string;
  onVerify: (otpCode: string) => Promise<boolean>;
  onResend: () => Promise<boolean>;
  onBack: () => void;
  isLoading: boolean;
  error: string | null;
  correlationId: string | null;
}

const OTP_LENGTH = 6;
const RESEND_COOLDOWN_SECONDS = 60;

export default function OtpVerification({
  phone,
  onVerify,
  onResend,
  onBack,
  isLoading,
  error,
  correlationId,
}: OtpVerificationProps) {
  const [otpDigits, setOtpDigits] = useState<string[]>(Array(OTP_LENGTH).fill(''));
  const [fieldError, setFieldError] = useState<string | null>(null);
  const [resendCooldown, setResendCooldown] = useState(RESEND_COOLDOWN_SECONDS);
  const [resendEnabled, setResendEnabled] = useState(false);
  const inputRefs = useRef<(HTMLInputElement | null)[]>([]);

  // Cooldown timer for resend
  useEffect(() => {
    if (resendCooldown > 0) {
      const timer = setTimeout(() => {
        setResendCooldown((prev) => prev - 1);
      }, 1000);
      return () => clearTimeout(timer);
    } else {
      setResendEnabled(true);
    }
  }, [resendCooldown]);

  // Focus first input on mount
  useEffect(() => {
    inputRefs.current[0]?.focus();
  }, []);

  const handleDigitChange = useCallback(
    (index: number, value: string) => {
      // Allow only digits
      const digit = value.replace(/\D/g, '').slice(-1);

      const newDigits = [...otpDigits];
      newDigits[index] = digit;
      setOtpDigits(newDigits);
      setFieldError(null);

      // Auto-focus next input
      if (digit && index < OTP_LENGTH - 1) {
        inputRefs.current[index + 1]?.focus();
      }
    },
    [otpDigits]
  );

  const handleKeyDown = useCallback(
    (index: number, e: React.KeyboardEvent<HTMLInputElement>) => {
      if (e.key === 'Backspace' && !otpDigits[index] && index > 0) {
        // Move to previous input on backspace if current is empty
        inputRefs.current[index - 1]?.focus();
      }
    },
    [otpDigits]
  );

  const handlePaste = useCallback((e: React.ClipboardEvent) => {
    e.preventDefault();
    const pastedData = e.clipboardData.getData('text').replace(/\D/g, '').slice(0, OTP_LENGTH);
    
    if (pastedData.length > 0) {
      const newDigits = Array(OTP_LENGTH).fill('');
      for (let i = 0; i < pastedData.length; i++) {
        newDigits[i] = pastedData[i];
      }
      setOtpDigits(newDigits);
      setFieldError(null);

      // Focus last filled input or submit if complete
      const lastIndex = Math.min(pastedData.length - 1, OTP_LENGTH - 1);
      inputRefs.current[lastIndex]?.focus();
    }
  }, []);

  const getOtpCode = useCallback(() => {
    return otpDigits.join('');
  }, [otpDigits]);

  const validate = useCallback((): boolean => {
    const code = getOtpCode();
    
    if (code.length === 0) {
      setFieldError(FIELD_ERRORS.otpCode.required);
      return false;
    }
    
    if (code.length !== OTP_LENGTH) {
      setFieldError(FIELD_ERRORS.otpCode.invalid);
      return false;
    }

    return true;
  }, [getOtpCode]);

  const handleSubmit = useCallback(
    async (e: React.FormEvent) => {
      e.preventDefault();

      if (!validate()) {
        return;
      }

      await onVerify(getOtpCode());
    },
    [validate, onVerify, getOtpCode]
  );

  const handleResend = useCallback(async () => {
    if (!resendEnabled || isLoading) return;

    const success = await onResend();
    if (success) {
      setResendCooldown(RESEND_COOLDOWN_SECONDS);
      setResendEnabled(false);
      setOtpDigits(Array(OTP_LENGTH).fill(''));
      inputRefs.current[0]?.focus();
    }
  }, [resendEnabled, isLoading, onResend]);

  // Mask phone number for display (show last 4 digits)
  const maskedPhone = phone ? `****${phone.slice(-4)}` : '';

  return (
    <div data-field="screen.auth.sc-auth-002">
      <h1 className="title">Xác thực OTP</h1>
      <p className="subtitle">Nhập mã OTP đã gửi đến số điện thoại của bạn</p>

      <div className="phone-display">{maskedPhone}</div>

      {error && (
        <div className="error-summary" data-field="error.summary">
          {error}
          {correlationId && (
            <div style={{ fontSize: '0.75rem', marginTop: '0.5rem', opacity: 0.7 }}>
              Mã lỗi: {correlationId}
            </div>
          )}
        </div>
      )}

      <form onSubmit={handleSubmit} noValidate>
        {/* OTP Input */}
        <div className="form-group">
          <div className="otp-inputs" data-field="input.otp">
            {otpDigits.map((digit, index) => (
              <input
                key={index}
                ref={(el) => { inputRefs.current[index] = el; }}
                type="text"
                inputMode="numeric"
                data-field={`input.otp-digit-${index}`}
                className={`otp-input ${fieldError ? 'error' : ''}`}
                value={digit}
                onChange={(e) => handleDigitChange(index, e.target.value)}
                onKeyDown={(e) => handleKeyDown(index, e)}
                onPaste={handlePaste}
                maxLength={1}
                disabled={isLoading}
                autoComplete={index === 0 ? 'one-time-code' : 'off'}
              />
            ))}
          </div>
          {fieldError && (
            <div className="error-text" data-field="error.otp" style={{ textAlign: 'center' }}>
              {fieldError}
            </div>
          )}
        </div>

        {/* Verify Button */}
        <button
          type="submit"
          data-field="action.verify-otp"
          className="btn btn-primary"
          disabled={isLoading}
        >
          {isLoading && <span className="loading-spinner" data-field="state.loading" />}
          {isLoading ? 'Đang xác thực...' : 'Xác nhận'}
        </button>
      </form>

      {/* Resend OTP */}
      <div className="resend-section">
        {resendEnabled ? (
          <button
            type="button"
            data-field="action.resend-otp"
            className="btn btn-link"
            onClick={handleResend}
            disabled={isLoading}
          >
            Gửi lại mã OTP
          </button>
        ) : (
          <span className="countdown">
            Gửi lại sau {resendCooldown}s
          </span>
        )}
      </div>

      {/* Back Button */}
      <button
        type="button"
        data-field="action.back"
        className="back-link"
        onClick={onBack}
        disabled={isLoading}
        style={{ background: 'none', border: 'none', cursor: 'pointer', width: '100%' }}
      >
        ← Quay lại
      </button>
    </div>
  );
}
