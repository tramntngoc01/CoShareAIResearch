using CoShare.Domain.Users;
using CoShare.Infrastructure.Services.Users;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CoShare.Domain.UnitTests.Users;

/// <summary>
/// Unit tests for USERS module service - covering all stories US-USERS-001 through US-USERS-006.
/// Test IDs: UT-USERS-001 through UT-USERS-006
/// </summary>
public class UsersServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IUserImportRepository> _importRepoMock;
    private readonly Mock<ILogger<UsersService>> _loggerMock;
    private readonly Mock<TimeProvider> _timeProviderMock;
    private readonly UsersService _usersService;
    private readonly DateTimeOffset _fixedNow;

    public UsersServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _importRepoMock = new Mock<IUserImportRepository>();
        _loggerMock = new Mock<ILogger<UsersService>>();
        _timeProviderMock = new Mock<TimeProvider>();

        _fixedNow = new DateTimeOffset(2026, 1, 4, 10, 0, 0, TimeSpan.Zero);
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(_fixedNow);

        _usersService = new UsersService(
            _userRepoMock.Object,
            _importRepoMock.Object,
            _loggerMock.Object,
            _timeProviderMock.Object);
    }

    #region US-USERS-001: Import Users Tests

    [Fact]
    public async Task StartImportAsync_WithValidInput_CreatesJobAndReturnsUuid()
    {
        // Arrange
        var fileName = "employees.csv";
        var source = "HR_System";
        long requestedBy = 1001;
        var correlationId = "test-correlation-001";

        _importRepoMock.Setup(x => x.CreateJobAsync(It.IsAny<UserImportJob>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserImportJob job, CancellationToken ct) =>
            {
                job.Id = 1;
                return job;
            });

        // Act
        var (importId, submittedAt) = await _usersService.StartImportAsync(
            fileName, source, requestedBy, correlationId);

        // Assert
        Assert.NotEqual(Guid.Empty, importId);
        Assert.Equal(_fixedNow.DateTime, submittedAt);

        _importRepoMock.Verify(x => x.CreateJobAsync(
            It.Is<UserImportJob>(j =>
                j.FileName == fileName &&
                j.Source == source &&
                j.Status == "Pending" &&
                j.RequestedBy == requestedBy),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetImportResultAsync_WithExistingJob_ReturnsJobDetails()
    {
        // Arrange
        var importUuid = Guid.NewGuid();
        var job = new UserImportJob
        {
            Id = 1,
            ImportUuid = importUuid,
            Status = "Completed",
            TotalRows = 100,
            CreatedRows = 90,
            UpdatedRows = 5,
            FailedRows = 5
        };

        _importRepoMock.Setup(x => x.GetJobByUuidAsync(importUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        // Act
        var (id, status, totalRows, created, updated, failed) = 
            await _usersService.GetImportResultAsync(importUuid);

        // Assert
        Assert.Equal(importUuid, id);
        Assert.Equal("Completed", status);
        Assert.Equal(100, totalRows);
        Assert.Equal(90, created);
        Assert.Equal(5, updated);
        Assert.Equal(5, failed);
    }

    [Fact]
    public async Task GetImportResultAsync_WithNonExistentJob_ThrowsException()
    {
        // Arrange
        var importUuid = Guid.NewGuid();
        _importRepoMock.Setup(x => x.GetJobByUuidAsync(importUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserImportJob?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _usersService.GetImportResultAsync(importUuid));
    }

    #endregion

    #region US-USERS-002: Ref Tier Tests

    [Fact]
    public async Task ChangeRefTierAsync_WithValidT3ToT2Parent_UpdatesSuccessfully()
    {
        // Arrange
        var userId = 10001L;
        var parentUserId = 10002L;
        var changedBy = 1001L;
        var note = "Moved to new T2 supervisor";
        var correlationId = "test-correlation-002";

        var user = new User
        {
            Id = userId,
            FullName = "Nguyen Van A",
            Tier = "T3",
            Status = "Active",
            ExternalEmployeeCode = "EMP001",
            Phone = "0900123456",
            CompanyId = 501,
            PickupPointId = 601,
            IsDeleted = false,
            CreatedAt = _fixedNow
        };

        var parentUser = new User
        {
            Id = parentUserId,
            FullName = "Tran Thi B",
            Tier = "T2",
            Status = "Active",
            ExternalEmployeeCode = "EMP002",
            Phone = "0900123457",
            CompanyId = 501,
            PickupPointId = 601,
            IsDeleted = false,
            CreatedAt = _fixedNow
        };

        _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepoMock.Setup(x => x.GetByIdAsync(parentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentUser);
        _userRepoMock.Setup(x => x.UpdateRefTierAsync(userId, parentUserId, changedBy, note, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var (resultUserId, tier, resultParentUserId, parentName, parentTier) = 
            await _usersService.ChangeRefTierAsync(userId, parentUserId, changedBy, note, correlationId);

        // Assert
        Assert.Equal(userId, resultUserId);
        Assert.Equal("T3", tier);
        Assert.Equal(parentUserId, resultParentUserId);
        Assert.Equal("Tran Thi B", parentName);
        Assert.Equal("T2", parentTier);

        _userRepoMock.Verify(x => x.UpdateRefTierAsync(
            userId, parentUserId, changedBy, note, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeRefTierAsync_WithInvalidParentTier_ThrowsException()
    {
        // Arrange - T3 user trying to link to T3 parent (should be T2)
        var userId = 10001L;
        var parentUserId = 10002L;
        var changedBy = 1001L;
        var correlationId = "test-correlation-003";

        var user = new User
        {
            Id = userId,
            Tier = "T3",
            Status = "Active",
            ExternalEmployeeCode = "EMP001",
            Phone = "0900123456",
            CompanyId = 501,
            PickupPointId = 601,
            FullName = "User A",
            IsDeleted = false,
            CreatedAt = _fixedNow
        };

        var parentUser = new User
        {
            Id = parentUserId,
            Tier = "T3", // Invalid - should be T2
            Status = "Active",
            ExternalEmployeeCode = "EMP002",
            Phone = "0900123457",
            CompanyId = 501,
            PickupPointId = 601,
            FullName = "Parent A",
            IsDeleted = false,
            CreatedAt = _fixedNow
        };

        _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepoMock.Setup(x => x.GetByIdAsync(parentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentUser);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _usersService.ChangeRefTierAsync(
                userId, parentUserId, changedBy, null, correlationId));

        Assert.Contains("Invalid parent tier", ex.Message);
        Assert.Contains("T3", ex.Message);
        Assert.Contains("T2", ex.Message);
    }

    [Fact]
    public async Task ChangeRefTierAsync_WithNonExistentUser_ThrowsException()
    {
        // Arrange
        var userId = 99999L;
        var parentUserId = 10002L;
        var changedBy = 1001L;

        _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _usersService.ChangeRefTierAsync(
                userId, parentUserId, changedBy, null, "corr-id"));
    }

    [Fact]
    public async Task ChangeRefTierAsync_WithNonExistentParent_ThrowsException()
    {
        // Arrange
        var userId = 10001L;
        var parentUserId = 99999L;
        var changedBy = 1001L;

        var user = new User
        {
            Id = userId,
            Tier = "T3",
            Status = "Active",
            ExternalEmployeeCode = "EMP001",
            Phone = "0900123456",
            CompanyId = 501,
            PickupPointId = 601,
            FullName = "User A",
            IsDeleted = false,
            CreatedAt = _fixedNow
        };

        _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepoMock.Setup(x => x.GetByIdAsync(parentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _usersService.ChangeRefTierAsync(
                userId, parentUserId, changedBy, null, "corr-id"));
    }

    #endregion

    #region US-USERS-005: Status Management Tests

    [Fact]
    public async Task ChangeStatusAsync_WithValidStatus_UpdatesSuccessfully()
    {
        // Arrange
        var userId = 10001L;
        var changedBy = 1001L;
        var correlationId = "test-correlation-005";

        var user = new User
        {
            Id = userId,
            FullName = "User A",
            Status = "Active",
            Tier = "T3",
            ExternalEmployeeCode = "EMP001",
            Phone = "0900123456",
            CompanyId = 501,
            PickupPointId = 601,
            IsDeleted = false,
            CreatedAt = _fixedNow
        };

        _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepoMock.Setup(x => x.UpdateStatusAsync(
                userId, "Locked", changedBy, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _usersService.ChangeStatusAsync(
            userId, "Locked", "User violated policy", changedBy, correlationId);

        // Assert
        Assert.Equal("Locked", result.Status);

        _userRepoMock.Verify(x => x.UpdateStatusAsync(
            userId, "Locked", changedBy, "User violated policy", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeStatusAsync_WithSameStatus_ReturnsWithoutUpdate()
    {
        // Arrange
        var userId = 10001L;
        var changedBy = 1001L;

        var user = new User
        {
            Id = userId,
            FullName = "User A",
            Status = "Active",
            Tier = "T3",
            ExternalEmployeeCode = "EMP001",
            Phone = "0900123456",
            CompanyId = 501,
            PickupPointId = 601,
            IsDeleted = false,
            CreatedAt = _fixedNow
        };

        _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _usersService.ChangeStatusAsync(
            userId, "Active", "No change needed", changedBy, "corr-id");

        // Assert
        Assert.Equal("Active", result.Status);
        
        // Should not call UpdateStatusAsync since status is unchanged
        _userRepoMock.Verify(x => x.UpdateStatusAsync(
            It.IsAny<long>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task ChangeStatusAsync_WithInvalidStatus_ThrowsException()
    {
        // Arrange
        var userId = 10001L;
        var changedBy = 1001L;

        var user = new User
        {
            Id = userId,
            FullName = "User A",
            Status = "Active",
            Tier = "T3",
            ExternalEmployeeCode = "EMP001",
            Phone = "0900123456",
            CompanyId = 501,
            PickupPointId = 601,
            IsDeleted = false,
            CreatedAt = _fixedNow
        };

        _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _usersService.ChangeStatusAsync(
                userId, "InvalidStatus", "Reason", changedBy, "corr-id"));

        Assert.Contains("Invalid status", ex.Message);
    }

    [Fact]
    public async Task ChangeStatusAsync_WithNonExistentUser_ThrowsException()
    {
        // Arrange
        var userId = 99999L;
        _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _usersService.ChangeStatusAsync(
                userId, "Locked", "Reason", 1001, "corr-id"));
    }

    #endregion

    #region US-USERS-006: Search Users Tests

    [Fact]
    public async Task SearchUsersAsync_WithValidFilters_ReturnsFilteredResults()
    {
        // Arrange
        var companyId = 501L;
        var tier = "T3";
        var status = "Active";
        var page = 1;
        var pageSize = 20;

        var users = new List<User>
        {
            new User
            {
                Id = 10001,
                FullName = "User 1",
                Tier = "T3",
                Status = "Active",
                ExternalEmployeeCode = "EMP001",
                Phone = "0900123456",
                CompanyId = 501,
                PickupPointId = 601,
                IsDeleted = false,
                CreatedAt = _fixedNow
            },
            new User
            {
                Id = 10002,
                FullName = "User 2",
                Tier = "T3",
                Status = "Active",
                ExternalEmployeeCode = "EMP002",
                Phone = "0900123457",
                CompanyId = 501,
                PickupPointId = 601,
                IsDeleted = false,
                CreatedAt = _fixedNow
            }
        };

        _userRepoMock.Setup(x => x.SearchAsync(
                companyId, null, tier, status, null, null, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((users, 2));

        // Act
        var (items, totalCount) = await _usersService.SearchUsersAsync(
            companyId, null, tier, status, null, null, page, pageSize);

        // Assert
        Assert.Equal(2, items.Count);
        Assert.Equal(2, totalCount);
        Assert.All(items, u => Assert.Equal("T3", u.Tier));
        Assert.All(items, u => Assert.Equal("Active", u.Status));
    }

    [Fact]
    public async Task SearchUsersAsync_WithInvalidPage_NormalizesToPage1()
    {
        // Arrange
        var page = -1; // Invalid
        var pageSize = 20;

        _userRepoMock.Setup(x => x.SearchAsync(
                null, null, null, null, null, null, 1, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<User>(), 0));

        // Act
        var (items, totalCount) = await _usersService.SearchUsersAsync(
            null, null, null, null, null, null, page, pageSize);

        // Assert - Should normalize to page 1
        _userRepoMock.Verify(x => x.SearchAsync(
            null, null, null, null, null, null, 1, pageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchUsersAsync_WithInvalidPageSize_NormalizesToDefault()
    {
        // Arrange
        var page = 1;
        var pageSize = -5; // Invalid

        _userRepoMock.Setup(x => x.SearchAsync(
                null, null, null, null, null, null, page, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<User>(), 0));

        // Act
        var (items, totalCount) = await _usersService.SearchUsersAsync(
            null, null, null, null, null, null, page, pageSize);

        // Assert - Should normalize to 20
        _userRepoMock.Verify(x => x.SearchAsync(
            null, null, null, null, null, null, page, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchUsersAsync_WithExcessivePageSize_CapsAt100()
    {
        // Arrange
        var page = 1;
        var pageSize = 500; // Exceeds max

        _userRepoMock.Setup(x => x.SearchAsync(
                null, null, null, null, null, null, page, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<User>(), 0));

        // Act
        var (items, totalCount) = await _usersService.SearchUsersAsync(
            null, null, null, null, null, null, page, pageSize);

        // Assert - Should cap at 100
        _userRepoMock.Verify(x => x.SearchAsync(
            null, null, null, null, null, null, page, 100, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
