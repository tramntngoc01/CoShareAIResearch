/**
 * LoginPage - US-AUTH-002 Page Container
 * 
 * Orchestrates the login flow:
 * 1. Phone input: Request OTP
 * 2. SC-AUTH-002: OTP verification (reused from registration)
 * 3. Success: Login complete
 * 
 * Story: US-AUTH-002 — Đăng nhập End User lần đầu bằng OTP
 */

import { useLogin } from '../hooks/useLogin';
import LoginForm from '../components/auth/LoginForm';
import OtpVerification from '../components/auth/OtpVerification';
import SuccessScreen from '../components/auth/SuccessScreen';

export default function LoginPage() {
  const login = useLogin();

  return (
    <div className="container">
      <div className="card">
        {login.step === 'phone-input' && (
          <LoginForm
            onSubmit={login.submitPhone}
            isLoading={login.isLoading}
            error={login.error}
            correlationId={login.correlationId}
          />
        )}

        {login.step === 'otp' && login.phone && (
          <OtpVerification
            phone={login.phone}
            onVerify={login.submitOtp}
            onResend={login.resendOtp}
            onBack={login.goBack}
            isLoading={login.isLoading}
            error={login.error}
            correlationId={login.correlationId}
          />
        )}

        {login.step === 'success' && login.loginResponse && (
          <SuccessScreen loginResponse={login.loginResponse} />
        )}
      </div>
    </div>
  );
}
