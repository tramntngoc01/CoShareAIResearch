/**
 * LoginForm - Phone input form for login (SC-AUTH-001 adapted for login)
 * 
 * Story: US-AUTH-002 — Đăng nhập End User lần đầu bằng OTP
 * 
 * Data-field convention:
 * - Screen: screen.auth.login-form
 * - Input: input.phone
 * - Error: error.phone, error.summary
 * - Action: action.request-otp
 * - Loading: state.loading
 */

import { useState, FormEvent } from 'react';

interface LoginFormProps {
  onSubmit: (phone: string) => Promise<boolean>;
  isLoading: boolean;
  error: string | null;
  correlationId: string | null;
}

export default function LoginForm({ onSubmit, isLoading, error, correlationId }: LoginFormProps) {
  const [phone, setPhone] = useState('');
  const [phoneError, setPhoneError] = useState('');

  const validatePhone = (value: string): boolean => {
    if (!value.trim()) {
      setPhoneError('Vui lòng nhập số điện thoại');
      return false;
    }
    
    // Vietnamese phone format: 10 digits starting with 0
    const phoneRegex = /^0\d{9}$/;
    if (!phoneRegex.test(value)) {
      setPhoneError('Số điện thoại không hợp lệ (VD: 0912345678)');
      return false;
    }

    setPhoneError('');
    return true;
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();

    // Clear previous errors
    setPhoneError('');

    // Validate phone
    if (!validatePhone(phone)) {
      return;
    }

    // Submit to hook
    await onSubmit(phone);
  };

  return (
    <div data-field="screen.auth.login-form">
      <h1 className="title">Đăng nhập</h1>
      <p className="subtitle">Nhập số điện thoại để nhận mã OTP</p>

      {error && (
        <div className="error-summary" data-field="error.summary">
          {error}
          {correlationId && (
            <div style={{ fontSize: '0.7rem', marginTop: '0.5rem', opacity: 0.7 }}>
              ID: {correlationId}
            </div>
          )}
        </div>
      )}

      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="phone" className="form-label">
            Số điện thoại <span style={{ color: 'var(--error)' }}>*</span>
          </label>
          <input
            id="phone"
            type="tel"
            className={`form-input ${phoneError ? 'error' : ''}`}
            data-field="input.phone"
            value={phone}
            onChange={(e) => setPhone(e.target.value)}
            placeholder="VD: 0912345678"
            disabled={isLoading}
            maxLength={20}
          />
          {phoneError && (
            <div className="error-text" data-field="error.phone">
              {phoneError}
            </div>
          )}
        </div>

        <button
          type="submit"
          className="btn btn-primary"
          data-field="action.request-otp"
          disabled={isLoading}
        >
          {isLoading && (
            <span className="loading-spinner" data-field="state.loading"></span>
          )}
          {isLoading ? 'Đang gửi...' : 'Nhận mã OTP'}
        </button>
      </form>

      <div style={{ textAlign: 'center', marginTop: '1.5rem', fontSize: '0.875rem' }}>
        <span style={{ color: 'var(--text-muted)' }}>Chưa có tài khoản? </span>
        <a href="/register" style={{ color: 'var(--primary)', textDecoration: 'none' }}>
          Đăng ký ngay
        </a>
      </div>
    </div>
  );
}
