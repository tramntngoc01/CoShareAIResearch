-- =====================================================
-- CoShare AUTH Schema for Supabase
-- Run this in Supabase SQL Editor
-- =====================================================

-- Create EF Migrations History table
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Create auth_otp_request table
CREATE TABLE IF NOT EXISTS auth_otp_request (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    phone VARCHAR(20) NOT NULL,
    purpose VARCHAR(50) NOT NULL,
    otp_code_hash VARCHAR(255) NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    status VARCHAR(30) NOT NULL,
    attempt_count INTEGER NOT NULL DEFAULT 0,
    last_attempt_at TIMESTAMPTZ,
    notification_template_code VARCHAR(100),
    notification_message_id VARCHAR(100),
    correlation_id VARCHAR(100),
    full_name VARCHAR(200),
    company_id BIGINT,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by VARCHAR(500),
    updated_at TIMESTAMPTZ,
    updated_by VARCHAR(500)
);

-- Create auth_refresh_token table
CREATE TABLE IF NOT EXISTS auth_refresh_token (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id BIGINT,
    admin_user_id BIGINT,
    token_hash VARCHAR(255) NOT NULL,
    device_id VARCHAR(128),
    user_agent VARCHAR(512),
    platform VARCHAR(128),
    expires_at TIMESTAMPTZ NOT NULL,
    revoked_at TIMESTAMPTZ,
    revoked_reason VARCHAR(255),
    correlation_id VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by VARCHAR(500),
    updated_at TIMESTAMPTZ,
    updated_by VARCHAR(500)
);

-- Create indexes for auth_otp_request
CREATE INDEX IF NOT EXISTS ix_auth_otp_request__phone_purpose_status 
    ON auth_otp_request (phone, purpose, status);
CREATE INDEX IF NOT EXISTS ix_auth_otp_request__expires_at 
    ON auth_otp_request (expires_at);

-- Create indexes for auth_refresh_token
CREATE UNIQUE INDEX IF NOT EXISTS ux_auth_refresh_token__token_hash 
    ON auth_refresh_token (token_hash);
CREATE INDEX IF NOT EXISTS ix_auth_refresh_token__user_id 
    ON auth_refresh_token (user_id);
CREATE INDEX IF NOT EXISTS ix_auth_refresh_token__admin_user_id 
    ON auth_refresh_token (admin_user_id);
CREATE INDEX IF NOT EXISTS ix_auth_refresh_token__expires_at 
    ON auth_refresh_token (expires_at);

-- Record migration as applied
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260104000001_InitialAuthSchema', '9.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;

-- Verify tables created
SELECT table_name FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN ('auth_otp_request', 'auth_refresh_token', '__EFMigrationsHistory');
