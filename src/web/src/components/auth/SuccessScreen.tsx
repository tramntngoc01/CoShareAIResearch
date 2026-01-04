/**
 * SuccessScreen - Registration Success State
 * 
 * Displayed after successful OTP verification.
 * 
 * data-field selectors:
 * - state.success: Success state container
 * - action.continue: Continue/proceed button
 */

import type { LoginResponse } from '../../api/types';

interface SuccessScreenProps {
  loginResponse: LoginResponse;
}

export default function SuccessScreen({ loginResponse }: SuccessScreenProps) {
  const user = loginResponse.endUser;

  const handleContinue = () => {
    // Store tokens and redirect to main app
    localStorage.setItem('accessToken', loginResponse.accessToken);
    localStorage.setItem('refreshToken', loginResponse.refreshToken);
    
    // Redirect to home/dashboard
    window.location.href = '/';
  };

  return (
    <div data-field="state.success">
      <div className="success-message">
        <svg
          width="48"
          height="48"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          style={{ margin: '0 auto 1rem', display: 'block' }}
        >
          <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" />
          <polyline points="22,4 12,14.01 9,11.01" />
        </svg>
        <h2 style={{ fontSize: '1.25rem', marginBottom: '0.5rem' }}>
          Đăng ký thành công!
        </h2>
        <p>Chào mừng bạn đến với CoShare</p>
      </div>

      {user && (
        <div style={{ textAlign: 'center', marginBottom: '1.5rem' }}>
          <p style={{ fontSize: '1.125rem', fontWeight: 500 }}>
            {user.fullName}
          </p>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>
            {user.companyName}
          </p>
        </div>
      )}

      <button
        type="button"
        data-field="action.continue"
        className="btn btn-primary"
        onClick={handleContinue}
      >
        Tiếp tục
      </button>
    </div>
  );
}
