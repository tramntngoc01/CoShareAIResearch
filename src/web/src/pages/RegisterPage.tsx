/**
 * RegisterPage - US-AUTH-001 Page Container
 * 
 * Orchestrates the registration flow:
 * 1. SC-AUTH-001: Registration form
 * 2. SC-AUTH-002: OTP verification
 * 3. Success: Registration complete
 * 
 * Story: US-AUTH-001 — Đăng ký End User bằng SĐT + OTP ZNS
 */

import { useRegistration } from '../hooks/useRegistration';
import RegistrationForm from '../components/auth/RegistrationForm';
import OtpVerification from '../components/auth/OtpVerification';
import SuccessScreen from '../components/auth/SuccessScreen';

export default function RegisterPage() {
  const registration = useRegistration();

  return (
    <div className="container">
      <div className="card">
        {registration.step === 'form' && (
          <RegistrationForm
            onSubmit={registration.submitRegistration}
            isLoading={registration.isLoading}
            error={registration.error}
            correlationId={registration.correlationId}
          />
        )}

        {registration.step === 'otp' && registration.phone && (
          <OtpVerification
            phone={registration.phone}
            onVerify={registration.submitOtp}
            onResend={registration.resendOtp}
            onBack={registration.goBack}
            isLoading={registration.isLoading}
            error={registration.error}
            correlationId={registration.correlationId}
          />
        )}

        {registration.step === 'success' && registration.loginResponse && (
          <SuccessScreen loginResponse={registration.loginResponse} />
        )}
      </div>
    </div>
  );
}
