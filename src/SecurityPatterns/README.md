# SecurityPatterns

Demonstrates advanced security patterns for ASP.NET Core APIs.

## Key Concepts
1.  **OIDC Authentication**: Configured with `Microsoft.AspNetCore.Authentication.JwtBearer` to validate tokens from Keycloak.
2.  **Policy-based Authorization**:
    - `MinimumAgeRequirement`: A custom requirement that checks a `custom_age` claim.
    - Usage: `.RequireAuthorization("Over18")`.
3.  **Security Headers**:
    - `SecurityHeadersMiddleware` adds `Content-Security-Policy`, `Strict-Transport-Security`, `X-Content-Type-Options`, `X-Frame-Options`.
4.  **IP Safelisting**:
    - `IpSafelistMiddleware` blocks requests from non-safelisted IPs (configured in `appsettings.json` or code).

## Infrastructure
Keycloak is required for full token validation.
```bash
docker compose -f docker/docker-compose.yml up -d
```
Keycloak Console: http://localhost:8080 (admin/admin).

## How to Test
1.  **Public Endpoint**: `GET /public` -> 200 OK.
2.  **Secure Endpoint**: `GET /secure` -> 401 Unauthorized (without token).
3.  **Security Headers**: Inspect response headers of any request.
    - Expected: `Strict-Transport-Security: max-age=31536000; includeSubDomains` etc.
4.  **Full Flow (Manual)**:
    - Create Realm `advanced-playground` in Keycloak.
    - Create Client `account` (or `api`).
    - Get Token via Postman/Curl.
    - Call `/secure` with `Authorization: Bearer <TOKEN>`.
