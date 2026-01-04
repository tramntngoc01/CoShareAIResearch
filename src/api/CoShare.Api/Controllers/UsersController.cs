using CoShare.Api.Contracts.Common;
using CoShare.Api.Contracts.Users;
using CoShare.Domain.Users;
using Microsoft.AspNetCore.Mvc;

namespace CoShare.Api.Controllers;

/// <summary>
/// USERS controller for user management operations.
/// Implements US-USERS-001 through US-USERS-006.
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUsersService usersService,
        ILogger<UsersController> logger)
    {
        _usersService = usersService;
        _logger = logger;
    }

    /// <summary>
    /// Import users from HR file - US-USERS-001
    /// POST /api/v1/users/import
    /// </summary>
    [HttpPost("import")]
    [ProducesResponseType(typeof(UsersImportJobCreated), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status413RequestEntityTooLarge)]
    public async Task<IActionResult> ImportFromFile(
        [FromForm] IFormFile file,
        [FromForm] string? source,
        [FromHeader(Name = "X-Correlation-Id")] string? correlationId,
        CancellationToken ct)
    {
        correlationId ??= Guid.NewGuid().ToString();
        Response.Headers["X-Correlation-Id"] = correlationId;

        // TODO: Get actual admin user ID from auth context
        long requestedBy = 1;

        try
        {
            // Basic validation
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "USERS_IMPORT_EMPTY_FILE",
                        Message = "File is required and cannot be empty",
                        CorrelationId = correlationId
                    }
                });
            }

            // Check file size (e.g., max 10MB)
            if (file.Length > 10 * 1024 * 1024)
            {
                return StatusCode(StatusCodes.Status413RequestEntityTooLarge, new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "USERS_IMPORT_TOO_LARGE",
                        Message = "File size exceeds maximum allowed (10MB)",
                        CorrelationId = correlationId
                    }
                });
            }

            var (importId, submittedAt) = await _usersService.StartImportAsync(
                file.FileName,
                source,
                requestedBy,
                correlationId,
                ct);

            // Start background processing (in real implementation, use a background job)
            _ = Task.Run(async () =>
            {
                try
                {
                    using var stream = file.OpenReadStream();
                    await _usersService.ProcessImportAsync(importId, stream, correlationId, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Background import processing failed. ImportId={ImportId}", importId);
                }
            });

            return Accepted(new UsersImportJobCreated
            {
                ImportId = importId,
                SubmittedAt = submittedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import failed. CorrelationId={CorrelationId}", correlationId);
            return StatusCode(500, new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "USERS_IMPORT_FAILED",
                    Message = "Import failed due to an internal error",
                    CorrelationId = correlationId
                }
            });
        }
    }

    /// <summary>
    /// Get import job result - US-USERS-001
    /// GET /api/v1/users/import/{importId}
    /// </summary>
    [HttpGet("import/{importId}")]
    [ProducesResponseType(typeof(UsersImportJobResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImportResult(
        Guid importId,
        [FromHeader(Name = "X-Correlation-Id")] string? correlationId,
        CancellationToken ct)
    {
        correlationId ??= Guid.NewGuid().ToString();
        Response.Headers["X-Correlation-Id"] = correlationId;

        try
        {
            var (id, status, totalRows, created, updated, failed) = await _usersService.GetImportResultAsync(importId, ct);

            return Ok(new UsersImportJobResult
            {
                ImportId = id,
                Status = status,
                TotalRows = totalRows,
                Created = created,
                Updated = updated,
                Failed = failed,
                ErrorSamples = new List<ErrorSample>() // Simplified
            });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "USERS_IMPORT_NOT_FOUND",
                    Message = "Import job not found",
                    CorrelationId = correlationId
                }
            });
        }
    }

    /// <summary>
    /// Search and list users - US-USERS-006
    /// GET /api/v1/users
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UserSummary>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchUsers(
        [FromQuery] long? companyId,
        [FromQuery] long? pickupPointId,
        [FromQuery] string? tier,
        [FromQuery] string? status,
        [FromQuery] string? employeeCode,
        [FromQuery] string? phone,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromHeader(Name = "X-Correlation-Id")] string? correlationId = null,
        CancellationToken ct = default)
    {
        correlationId ??= Guid.NewGuid().ToString();
        Response.Headers["X-Correlation-Id"] = correlationId;

        var (items, totalCount) = await _usersService.SearchUsersAsync(
            companyId,
            pickupPointId,
            tier,
            status,
            employeeCode,
            phone,
            page,
            pageSize,
            ct);

        var userSummaries = items.Select(u => new UserSummary
        {
            Id = u.Id,
            FullName = u.FullName,
            Phone = u.Phone,
            EmployeeCode = u.ExternalEmployeeCode,
            CompanyId = u.CompanyId,
            CompanyName = null, // TODO: Join with company table
            PickupPointId = u.PickupPointId,
            PickupPointName = null, // TODO: Join with pickup point table
            Tier = u.Tier,
            Status = u.Status
        }).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return Ok(new PagedResult<UserSummary>
        {
            Items = userSummaries,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalCount,
            TotalPages = totalPages
        });
    }

    /// <summary>
    /// Get user detail - US-USERS-003
    /// GET /api/v1/users/{id}
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        long id,
        [FromHeader(Name = "X-Correlation-Id")] string? correlationId,
        CancellationToken ct)
    {
        correlationId ??= Guid.NewGuid().ToString();
        Response.Headers["X-Correlation-Id"] = correlationId;

        var user = await _usersService.GetUserByIdAsync(id, ct);
        if (user == null)
        {
            return NotFound(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "USERS_USER_NOT_FOUND",
                    Message = "User not found",
                    CorrelationId = correlationId
                }
            });
        }

        return Ok(new UserDetail
        {
            Id = user.Id,
            FullName = user.FullName,
            Phone = user.Phone,
            EmployeeCode = user.ExternalEmployeeCode,
            CompanyId = user.CompanyId,
            CompanyName = null, // TODO: Join with company table
            PickupPointId = user.PickupPointId,
            PickupPointName = null, // TODO: Join with pickup point table
            Tier = user.Tier,
            Status = user.Status,
            CccdMasked = user.CccdHash != null ? "***masked***" : null,
            BirthDate = user.BirthDate,
            Email = user.Email,
            AddressDetail = user.AddressDetail,
            KycMetadata = null // TODO: Parse from JSON
        });
    }

    /// <summary>
    /// Update user KYC/profile (admin) - US-USERS-003
    /// PATCH /api/v1/users/{id}/kyc
    /// </summary>
    [HttpPatch("{id}/kyc")]
    [ProducesResponseType(typeof(UserDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateKyc(
        long id,
        [FromBody] UserKycUpdateRequest request,
        [FromHeader(Name = "X-Correlation-Id")] string? correlationId,
        CancellationToken ct)
    {
        correlationId ??= Guid.NewGuid().ToString();
        Response.Headers["X-Correlation-Id"] = correlationId;

        // TODO: Get actual admin user ID from auth context
        long updatedBy = 1;

        try
        {
            var user = await _usersService.UpdateKycAsync(
                id,
                request.FullName,
                request.Email,
                request.AddressDetail,
                request.BirthDate,
                updatedBy,
                correlationId,
                ct);

            return Ok(new UserDetail
            {
                Id = user.Id,
                FullName = user.FullName,
                Phone = user.Phone,
                EmployeeCode = user.ExternalEmployeeCode,
                CompanyId = user.CompanyId,
                PickupPointId = user.PickupPointId,
                Tier = user.Tier,
                Status = user.Status,
                Email = user.Email,
                AddressDetail = user.AddressDetail,
                BirthDate = user.BirthDate
            });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "USERS_USER_NOT_FOUND",
                    Message = "User not found",
                    CorrelationId = correlationId
                }
            });
        }
    }

    /// <summary>
    /// Change user status - US-USERS-005
    /// PATCH /api/v1/users/{id}/status
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(UserDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStatus(
        long id,
        [FromBody] UserStatusChangeRequest request,
        [FromHeader(Name = "X-Correlation-Id")] string? correlationId,
        CancellationToken ct)
    {
        correlationId ??= Guid.NewGuid().ToString();
        Response.Headers["X-Correlation-Id"] = correlationId;

        // TODO: Get actual admin user ID from auth context
        long changedBy = 1;

        try
        {
            var user = await _usersService.ChangeStatusAsync(
                id,
                request.NewStatus,
                request.Reason,
                changedBy,
                correlationId,
                ct);

            return Ok(new UserDetail
            {
                Id = user.Id,
                FullName = user.FullName,
                Phone = user.Phone,
                EmployeeCode = user.ExternalEmployeeCode,
                CompanyId = user.CompanyId,
                PickupPointId = user.PickupPointId,
                Tier = user.Tier,
                Status = user.Status,
                Email = user.Email,
                AddressDetail = user.AddressDetail,
                BirthDate = user.BirthDate
            });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "USERS_USER_NOT_FOUND",
                        Message = "User not found",
                        CorrelationId = correlationId
                    }
                });
            }

            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "USERS_STATUS_INVALID_TRANSITION",
                    Message = ex.Message,
                    CorrelationId = correlationId
                }
            });
        }
    }

    /// <summary>
    /// Get user ref tier - US-USERS-002
    /// GET /api/v1/users/{id}/ref-tier
    /// </summary>
    [HttpGet("{id}/ref-tier")]
    [ProducesResponseType(typeof(UserRefTierInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRefTier(
        long id,
        [FromHeader(Name = "X-Correlation-Id")] string? correlationId,
        CancellationToken ct)
    {
        correlationId ??= Guid.NewGuid().ToString();
        Response.Headers["X-Correlation-Id"] = correlationId;

        try
        {
            var (userId, tier, parentUserId, parentName, parentTier) = await _usersService.GetRefTierAsync(id, ct);

            return Ok(new UserRefTierInfo
            {
                UserId = userId,
                Tier = tier,
                ParentUserId = parentUserId,
                ParentUserName = parentName,
                ParentTier = parentTier
            });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "USERS_USER_NOT_FOUND",
                    Message = "User not found",
                    CorrelationId = correlationId
                }
            });
        }
    }

    /// <summary>
    /// Change user ref tier - US-USERS-002
    /// PUT /api/v1/users/{id}/ref-tier
    /// </summary>
    [HttpPut("{id}/ref-tier")]
    [ProducesResponseType(typeof(UserRefTierInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeRefTier(
        long id,
        [FromBody] UserRefTierChangeRequest request,
        [FromHeader(Name = "X-Correlation-Id")] string? correlationId,
        CancellationToken ct)
    {
        correlationId ??= Guid.NewGuid().ToString();
        Response.Headers["X-Correlation-Id"] = correlationId;

        // TODO: Get actual admin user ID from auth context
        long changedBy = 1;

        try
        {
            var (userId, tier, parentUserId, parentName, parentTier) = await _usersService.ChangeRefTierAsync(
                id,
                request.NewParentUserId,
                changedBy,
                request.Note,
                correlationId,
                ct);

            return Ok(new UserRefTierInfo
            {
                UserId = userId,
                Tier = tier,
                ParentUserId = parentUserId,
                ParentUserName = parentName,
                ParentTier = parentTier
            });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "USERS_USER_NOT_FOUND",
                        Message = ex.Message,
                        CorrelationId = correlationId
                    }
                });
            }

            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "USERS_REF_TIER_INVALID_PARENT",
                    Message = ex.Message,
                    CorrelationId = correlationId
                }
            });
        }
    }
}
