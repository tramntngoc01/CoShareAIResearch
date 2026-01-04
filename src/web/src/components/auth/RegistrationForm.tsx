/**
 * RegistrationForm - SC-AUTH-001 Implementation
 * 
 * Screen: SC-AUTH-001 — End User Registration
 * Story: US-AUTH-001
 * API: POST /api/v1/auth/end-user/register/request-otp
 * 
 * data-field selectors (per Frontend-Coding-Standards.md):
 * - screen.auth.sc-auth-001: Screen root container
 * - input.full-name: Full name input
 * - input.company-id: Company dropdown
 * - input.phone: Phone number input
 * - input.accept-terms: Terms checkbox
 * - action.request-otp: Submit button
 * - error.summary: Error message container
 * - error.full-name: Full name field error
 * - error.phone: Phone field error
 * - error.company-id: Company field error
 * - error.accept-terms: Terms field error
 * - state.loading: Loading indicator
 */

import { useState, useCallback, useEffect } from 'react';
import type { EndUserRegisterStartRequest, CompanySummary } from '../../api/types';
import { getCompanies } from '../../api/authApi';
import { FIELD_ERRORS } from '../../utils/errorMessages';

interface RegistrationFormProps {
  onSubmit: (data: EndUserRegisterStartRequest) => Promise<boolean>;
  isLoading: boolean;
  error: string | null;
  correlationId: string | null;
}

interface FormData {
  fullName: string;
  companyId: string;
  phone: string;
  acceptTerms: boolean;
}

interface FormErrors {
  fullName?: string;
  companyId?: string;
  phone?: string;
  acceptTerms?: string;
}

// Vietnam phone number pattern: 10 digits starting with 0
const PHONE_REGEX = /^0\d{9}$/;

export default function RegistrationForm({
  onSubmit,
  isLoading,
  error,
  correlationId,
}: RegistrationFormProps) {
  const [formData, setFormData] = useState<FormData>({
    fullName: '',
    companyId: '',
    phone: '',
    acceptTerms: false,
  });

  const [fieldErrors, setFieldErrors] = useState<FormErrors>({});
  const [companies, setCompanies] = useState<CompanySummary[]>([]);
  const [companiesLoading, setCompaniesLoading] = useState(true);

  // Load companies for dropdown
  useEffect(() => {
    async function loadCompanies() {
      setCompaniesLoading(true);
      const result = await getCompanies();
      if (result.success && result.data) {
        setCompanies(result.data.filter((c) => c.status === 'ACTIVE'));
      } else {
        // Fallback: allow manual entry or show error
        setCompanies([]);
      }
      setCompaniesLoading(false);
    }
    loadCompanies();
  }, []);

  const handleChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
      const { name, value, type } = e.target;
      const newValue = type === 'checkbox' ? (e.target as HTMLInputElement).checked : value;

      setFormData((prev) => ({ ...prev, [name]: newValue }));

      // Clear field error on change
      if (fieldErrors[name as keyof FormErrors]) {
        setFieldErrors((prev) => ({ ...prev, [name]: undefined }));
      }
    },
    [fieldErrors]
  );

  const validate = useCallback((): boolean => {
    const errors: FormErrors = {};

    // Full name validation
    if (!formData.fullName.trim()) {
      errors.fullName = FIELD_ERRORS.fullName.required;
    } else if (formData.fullName.trim().length < 2) {
      errors.fullName = FIELD_ERRORS.fullName.minLength;
    } else if (formData.fullName.length > 200) {
      errors.fullName = FIELD_ERRORS.fullName.maxLength;
    }

    // Company validation
    if (!formData.companyId) {
      errors.companyId = FIELD_ERRORS.companyId.required;
    }

    // Phone validation
    if (!formData.phone.trim()) {
      errors.phone = FIELD_ERRORS.phone.required;
    } else if (!PHONE_REGEX.test(formData.phone.trim())) {
      errors.phone = FIELD_ERRORS.phone.invalid;
    }

    // Terms validation
    if (!formData.acceptTerms) {
      errors.acceptTerms = FIELD_ERRORS.acceptTerms.required;
    }

    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  }, [formData]);

  const handleSubmit = useCallback(
    async (e: React.FormEvent) => {
      e.preventDefault();

      if (!validate()) {
        return;
      }

      const request: EndUserRegisterStartRequest = {
        fullName: formData.fullName.trim(),
        companyId: parseInt(formData.companyId, 10),
        phone: formData.phone.trim(),
        acceptTerms: formData.acceptTerms,
      };

      await onSubmit(request);
    },
    [formData, validate, onSubmit]
  );

  return (
    <div data-field="screen.auth.sc-auth-001">
      <h1 className="title">Đăng ký tài khoản</h1>
      <p className="subtitle">Nhập thông tin để nhận mã OTP qua Zalo</p>

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
        {/* Full Name */}
        <div className="form-group">
          <label htmlFor="fullName" className="form-label">
            Họ và tên <span style={{ color: 'var(--error)' }}>*</span>
          </label>
          <input
            type="text"
            id="fullName"
            name="fullName"
            data-field="input.full-name"
            className={`form-input ${fieldErrors.fullName ? 'error' : ''}`}
            placeholder="Nguyễn Văn A"
            value={formData.fullName}
            onChange={handleChange}
            maxLength={200}
            disabled={isLoading}
            autoComplete="name"
          />
          {fieldErrors.fullName && (
            <div className="error-text" data-field="error.full-name">
              {fieldErrors.fullName}
            </div>
          )}
        </div>

        {/* Company */}
        <div className="form-group">
          <label htmlFor="companyId" className="form-label">
            Công ty <span style={{ color: 'var(--error)' }}>*</span>
          </label>
          <select
            id="companyId"
            name="companyId"
            data-field="input.company-id"
            className={`form-select ${fieldErrors.companyId ? 'error' : ''}`}
            value={formData.companyId}
            onChange={handleChange}
            disabled={isLoading || companiesLoading}
          >
            <option value="">
              {companiesLoading ? 'Đang tải...' : 'Chọn công ty'}
            </option>
            {companies.map((company) => (
              <option key={company.id} value={company.id}>
                {company.companyName}
              </option>
            ))}
          </select>
          {fieldErrors.companyId && (
            <div className="error-text" data-field="error.company-id">
              {fieldErrors.companyId}
            </div>
          )}
        </div>

        {/* Phone */}
        <div className="form-group">
          <label htmlFor="phone" className="form-label">
            Số điện thoại <span style={{ color: 'var(--error)' }}>*</span>
          </label>
          <input
            type="tel"
            id="phone"
            name="phone"
            data-field="input.phone"
            className={`form-input ${fieldErrors.phone ? 'error' : ''}`}
            placeholder="0912345678"
            value={formData.phone}
            onChange={handleChange}
            maxLength={20}
            disabled={isLoading}
            autoComplete="tel"
          />
          {fieldErrors.phone && (
            <div className="error-text" data-field="error.phone">
              {fieldErrors.phone}
            </div>
          )}
        </div>

        {/* Accept Terms */}
        <div className="form-group">
          <div className="form-checkbox">
            <input
              type="checkbox"
              id="acceptTerms"
              name="acceptTerms"
              data-field="input.accept-terms"
              checked={formData.acceptTerms}
              onChange={handleChange}
              disabled={isLoading}
            />
            <label htmlFor="acceptTerms">
              Tôi đồng ý với <a href="/terms" target="_blank">Điều khoản sử dụng</a> và{' '}
              <a href="/privacy" target="_blank">Chính sách bảo mật</a>
            </label>
          </div>
          {fieldErrors.acceptTerms && (
            <div className="error-text" data-field="error.accept-terms">
              {fieldErrors.acceptTerms}
            </div>
          )}
        </div>

        {/* Submit Button */}
        <button
          type="submit"
          data-field="action.request-otp"
          className="btn btn-primary"
          disabled={isLoading}
        >
          {isLoading && <span className="loading-spinner" data-field="state.loading" />}
          {isLoading ? 'Đang gửi...' : 'Gửi mã OTP'}
        </button>
      </form>

      <a href="/login" className="back-link">
        Đã có tài khoản? Đăng nhập
      </a>
    </div>
  );
}
