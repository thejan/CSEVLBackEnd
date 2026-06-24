/*
    ATME CSE Virtual Lab
    Complete SQL Server database creation script

    WARNING:
    This script drops and recreates the atmecsevlab database.
*/

USE [master]
GO

IF DB_ID(N'atmecsevlab') IS NOT NULL
BEGIN
    ALTER DATABASE [atmecsevlab]
        SET SINGLE_USER
        WITH ROLLBACK IMMEDIATE;

    DROP DATABASE [atmecsevlab];
END
GO

CREATE DATABASE [atmecsevlab]
GO

USE [atmecsevlab]
GO

/* =========================================================
   TABLES
   ========================================================= */

CREATE TABLE [Roles]
(
    [RoleId] TINYINT IDENTITY(1,1)
        CONSTRAINT [PK_Roles] PRIMARY KEY,

    [RoleName] VARCHAR(30)
        CONSTRAINT [UQ_Roles_RoleName] UNIQUE
        NOT NULL
)
GO

CREATE TABLE [Registration]
(
    [UserId] INT IDENTITY(1,1)
        CONSTRAINT [PK_Registration] PRIMARY KEY,

    [College] VARCHAR(250) NOT NULL,

    [Department] VARCHAR(200) NOT NULL,

    [StudentName] VARCHAR(150) NOT NULL,

    [USN] VARCHAR(30)
        CONSTRAINT [UQ_Registration_USN] UNIQUE
        NOT NULL,

    [Semester] TINYINT
        CONSTRAINT [CK_Registration_Semester]
        CHECK ([Semester] BETWEEN 1 AND 8)
        NOT NULL,

    [EmailId] VARCHAR(256)
        CONSTRAINT [UQ_Registration_EmailId] UNIQUE
        NOT NULL,

    [PasswordHash] VARCHAR(500) NOT NULL,

    [RoleId] TINYINT
        CONSTRAINT [FK_Registration_Roles]
        REFERENCES [Roles]([RoleId])
        NOT NULL,

    [RegistrationStatus] VARCHAR(20)
        CONSTRAINT [DF_Registration_Status]
        DEFAULT 'Pending'
        CONSTRAINT [CK_Registration_Status]
        CHECK
        (
            [RegistrationStatus] IN
            ('Pending', 'Approved', 'Rejected')
        )
        NOT NULL,

    [IsActive] BIT
        CONSTRAINT [DF_Registration_IsActive]
        DEFAULT 0
        NOT NULL,

    [CreatedAt] DATETIME2(0)
        CONSTRAINT [DF_Registration_CreatedAt]
        DEFAULT SYSUTCDATETIME()
        NOT NULL,

    [UpdatedAt] DATETIME2(0) NULL,

    [ApprovedAt] DATETIME2(0) NULL,

    [ApprovedBy] INT NULL
        CONSTRAINT [FK_Registration_ApprovedBy]
        REFERENCES [Registration]([UserId])
)
GO

CREATE TABLE [UserRequests]
(
    [RequestId] BIGINT IDENTITY(1,1)
        CONSTRAINT [PK_UserRequests] PRIMARY KEY,

    [UserId] INT
        CONSTRAINT [FK_UserRequests_Registration]
        REFERENCES [Registration]([UserId])
        NOT NULL,

    [EmailId] VARCHAR(256) NOT NULL,

    [RequestType] VARCHAR(30)
        CONSTRAINT [CK_UserRequests_Type]
        CHECK
        (
            [RequestType] IN
            ('Query', 'Feedback', 'Defects/Bugs')
        )
        NOT NULL,

    [Description] NVARCHAR(2000) NOT NULL,

    [SubmittedAt] DATETIME2(0)
        CONSTRAINT [DF_UserRequests_SubmittedAt]
        DEFAULT SYSUTCDATETIME()
        NOT NULL,

    [Status] VARCHAR(10)
        CONSTRAINT [DF_UserRequests_Status]
        DEFAULT 'Open'
        CONSTRAINT [CK_UserRequests_Status]
        CHECK ([Status] IN ('Open', 'Closed'))
        NOT NULL,

    [ClosedAt] DATETIME2(0) NULL,

    [Remarks] NVARCHAR(2000) NULL,

    [UpdatedAt] DATETIME2(0) NULL
)
GO

CREATE TABLE [Labs]
(
    [LabId] INT IDENTITY(1,1)
        CONSTRAINT [PK_Labs] PRIMARY KEY,

    [LabName] VARCHAR(150) NOT NULL,

    [CourseCode] VARCHAR(30)
        CONSTRAINT [UQ_Labs_CourseCode] UNIQUE
        NOT NULL,

    [Semester] TINYINT
        CONSTRAINT [CK_Labs_Semester]
        CHECK ([Semester] BETWEEN 1 AND 8)
        NOT NULL,

    [Scheme] SMALLINT NOT NULL,

    [IsAvailable] BIT
        CONSTRAINT [DF_Labs_IsAvailable]
        DEFAULT 0
        NOT NULL,

    [IsActive] BIT
        CONSTRAINT [DF_Labs_IsActive]
        DEFAULT 1
        NOT NULL,

    [CreatedAt] DATETIME2(0)
        CONSTRAINT [DF_Labs_CreatedAt]
        DEFAULT SYSUTCDATETIME()
        NOT NULL
)
GO

CREATE TABLE [Experiments]
(
    [ExperimentId] INT IDENTITY(1,1)
        CONSTRAINT [PK_Experiments] PRIMARY KEY,

    [LabId] INT
        CONSTRAINT [FK_Experiments_Labs]
        REFERENCES [Labs]([LabId])
        NOT NULL,

    [ExperimentNumber] TINYINT
        CONSTRAINT [CK_Experiments_Number]
        CHECK ([ExperimentNumber] > 0)
        NOT NULL,

    [ExperimentTitle] VARCHAR(500) NOT NULL,

    [IsActive] BIT
        CONSTRAINT [DF_Experiments_IsActive]
        DEFAULT 1
        NOT NULL,

    [CreatedAt] DATETIME2(0)
        CONSTRAINT [DF_Experiments_CreatedAt]
        DEFAULT SYSUTCDATETIME()
        NOT NULL,

    CONSTRAINT [UQ_Experiments_Lab_Number]
        UNIQUE ([LabId], [ExperimentNumber]),

    CONSTRAINT [UQ_Experiments_Id_Lab]
        UNIQUE ([ExperimentId], [LabId])
)
GO

CREATE TABLE [LabEnrollment]
(
    [EnrollmentId] BIGINT IDENTITY(1,1)
        CONSTRAINT [PK_LabEnrollment] PRIMARY KEY,

    [UserId] INT
        CONSTRAINT [FK_LabEnrollment_Registration]
        REFERENCES [Registration]([UserId])
        NOT NULL,

    [LabId] INT
        CONSTRAINT [FK_LabEnrollment_Labs]
        REFERENCES [Labs]([LabId])
        NOT NULL,

    [EnrollmentStatus] VARCHAR(20)
        CONSTRAINT [DF_LabEnrollment_Status]
        DEFAULT 'Enrolled'
        CONSTRAINT [CK_LabEnrollment_Status]
        CHECK
        (
            [EnrollmentStatus] IN
            ('Enrolled', 'Completed', 'Cancelled')
        )
        NOT NULL,

    [EnrolledAt] DATETIME2(0)
        CONSTRAINT [DF_LabEnrollment_EnrolledAt]
        DEFAULT SYSUTCDATETIME()
        NOT NULL,

    [CompletedAt] DATETIME2(0) NULL,

    CONSTRAINT [UQ_LabEnrollment_User_Lab]
        UNIQUE ([UserId], [LabId])
)
GO

CREATE TABLE [LabStatus]
(
    [LabStatusId] BIGINT IDENTITY(1,1)
        CONSTRAINT [PK_LabStatus] PRIMARY KEY,

    [LabId] INT NOT NULL,

    [UserId] INT
        CONSTRAINT [FK_LabStatus_Registration]
        REFERENCES [Registration]([UserId])
        NOT NULL,

    [ExperimentId] INT NOT NULL,

    [Theory] BIT
        CONSTRAINT [DF_LabStatus_Theory]
        DEFAULT 0
        NOT NULL,

    [Execution] BIT
        CONSTRAINT [DF_LabStatus_Execution]
        DEFAULT 0
        NOT NULL,

    [Quiz] BIT
        CONSTRAINT [DF_LabStatus_Quiz]
        DEFAULT 0
        NOT NULL,

    [Assignment1] BIT
        CONSTRAINT [DF_LabStatus_Assignment1]
        DEFAULT 0
        NOT NULL,

    [Assignment2] BIT
        CONSTRAINT [DF_LabStatus_Assignment2]
        DEFAULT 0
        NOT NULL,

    [Assignment3] BIT
        CONSTRAINT [DF_LabStatus_Assignment3]
        DEFAULT 0
        NOT NULL,

    [Assignment4] BIT
        CONSTRAINT [DF_LabStatus_Assignment4]
        DEFAULT 0
        NOT NULL,

    [Assignment5] BIT
        CONSTRAINT [DF_LabStatus_Assignment5]
        DEFAULT 0
        NOT NULL,

    [CompletionStatus] AS
    (
        CASE
            WHEN [Theory] = 1
             AND [Execution] = 1
             AND [Quiz] = 1
             AND [Assignment1] = 1
             AND [Assignment2] = 1
             AND [Assignment3] = 1
             AND [Assignment4] = 1
             AND [Assignment5] = 1
            THEN 'Completed'
            ELSE 'Not Completed'
        END
    ) PERSISTED,

    [CreatedAt] DATETIME2(0)
        CONSTRAINT [DF_LabStatus_CreatedAt]
        DEFAULT SYSUTCDATETIME()
        NOT NULL,

    [UpdatedAt] DATETIME2(0) NULL,

    CONSTRAINT [FK_LabStatus_Experiment_Lab]
        FOREIGN KEY ([ExperimentId], [LabId])
        REFERENCES [Experiments]
        ([ExperimentId], [LabId]),

    CONSTRAINT [UQ_LabStatus_User_Lab_Experiment]
        UNIQUE ([UserId], [LabId], [ExperimentId])
)
GO

CREATE TABLE [UserActivitySessions]
(
    [ActivitySessionId] BIGINT IDENTITY(1,1)
        CONSTRAINT [PK_UserActivitySessions]
        PRIMARY KEY,

    [UserId] INT
        CONSTRAINT [FK_UserActivitySessions_Registration]
        REFERENCES [Registration]([UserId])
        NOT NULL,

    [LoginTime] DATETIME2(0)
        CONSTRAINT [DF_UserActivity_LoginTime]
        DEFAULT SYSUTCDATETIME()
        NOT NULL,

    [LogoutTime] DATETIME2(0) NULL,

    [LastActivityTime] DATETIME2(0)
        CONSTRAINT [DF_UserActivity_LastActivity]
        DEFAULT SYSUTCDATETIME()
        NOT NULL,

    [LastHeartbeatTime] DATETIME2(0)
        CONSTRAINT [DF_UserActivity_LastHeartbeat]
        DEFAULT SYSUTCDATETIME()
        NOT NULL,

    [ActiveSeconds] INT
        CONSTRAINT [DF_UserActivity_ActiveSeconds]
        DEFAULT 0
        CONSTRAINT [CK_UserActivity_ActiveSeconds]
        CHECK ([ActiveSeconds] >= 0)
        NOT NULL,

    [IsSessionOpen] BIT
        CONSTRAINT [DF_UserActivity_IsSessionOpen]
        DEFAULT 1
        NOT NULL
)
GO

/* =========================================================
   INDEXES
   ========================================================= */

CREATE INDEX [IX_Registration_College]
    ON [Registration]([College])
GO

CREATE INDEX [IX_Registration_Department]
    ON [Registration]([Department])
GO

CREATE INDEX [IX_Registration_Status]
    ON [Registration]([RegistrationStatus])
GO

CREATE INDEX [IX_UserRequests_Status_SubmittedAt]
    ON [UserRequests]([Status], [SubmittedAt])
GO

CREATE INDEX [IX_UserRequests_User_SubmittedAt]
    ON [UserRequests]([UserId], [SubmittedAt])
GO

CREATE INDEX [IX_LabStatus_User_Lab]
    ON [LabStatus]([UserId], [LabId])
GO

CREATE INDEX [IX_Activity_User_Login]
    ON [UserActivitySessions]([UserId], [LoginTime])
GO

/* =========================================================
   TABLE TYPE FOR BULK APPROVAL / REJECTION
   ========================================================= */

CREATE TYPE [dbo].[UserIdList] AS TABLE
(
    [UserId] INT PRIMARY KEY
)
GO

/* =========================================================
   FUNCTIONS
   ========================================================= */

CREATE FUNCTION [dbo].[ufn_CheckEmailAvailability]
(
    @EmailId VARCHAR(256)
)
RETURNS BIT
AS
BEGIN
    DECLARE @IsAvailable BIT = 0;

    IF NOT EXISTS
    (
        SELECT 1
        FROM [Registration]
        WHERE LOWER([EmailId]) = LOWER(LTRIM(RTRIM(@EmailId)))
    )
    BEGIN
        SET @IsAvailable = 1;
    END

    RETURN @IsAvailable;
END
GO

CREATE FUNCTION [dbo].[ufn_GetActiveDurationText]
(
    @ActiveSeconds INT
)
RETURNS VARCHAR(60)
AS
BEGIN
    DECLARE @Hours INT =
        ISNULL(@ActiveSeconds, 0) / 3600;

    DECLARE @Minutes INT =
        (ISNULL(@ActiveSeconds, 0) % 3600) / 60;

    RETURN
        CAST(@Hours AS VARCHAR(10)) +
        ' hrs ' +
        CAST(@Minutes AS VARCHAR(10)) +
        ' minutes';
END
GO

/* =========================================================
   REGISTRATION PROCEDURES
   PasswordHash must be created and verified by ASP.NET Core.
   ConfirmPassword must never be stored.
   ========================================================= */

CREATE PROCEDURE [dbo].[usp_RegisterUser]
(
    @College VARCHAR(250),
    @Department VARCHAR(200),
    @StudentName VARCHAR(150),
    @USN VARCHAR(30),
    @Semester TINYINT,
    @EmailId VARCHAR(256),
    @PasswordHash VARCHAR(500),
    @UserId INT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @UserId = 0;

    SET @College = LTRIM(RTRIM(@College));
    SET @Department = LTRIM(RTRIM(@Department));
    SET @StudentName = LTRIM(RTRIM(@StudentName));
    SET @USN = UPPER(LTRIM(RTRIM(@USN)));
    SET @EmailId = LOWER(LTRIM(RTRIM(@EmailId)));

    IF NULLIF(@College, '') IS NULL
        THROW 50001, 'College is required.', 1;

    IF NULLIF(@Department, '') IS NULL
        THROW 50002, 'Department is required.', 1;

    IF NULLIF(@StudentName, '') IS NULL
        THROW 50003, 'Student name is required.', 1;

    IF NULLIF(@USN, '') IS NULL
        THROW 50004, 'USN is required.', 1;

    IF @Semester NOT BETWEEN 1 AND 8
        THROW 50005, 'Semester must be between 1 and 8.', 1;

    IF NULLIF(@EmailId, '') IS NULL
        THROW 50006, 'Email ID is required.', 1;

    IF NULLIF(@PasswordHash, '') IS NULL
        THROW 50007, 'Password hash is required.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM [Registration]
        WHERE [EmailId] = @EmailId
    )
        THROW 50008, 'Email ID is already registered.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM [Registration]
        WHERE [USN] = @USN
    )
        THROW 50009, 'USN is already registered.', 1;

    DECLARE @StudentRoleId TINYINT;

    SELECT @StudentRoleId = [RoleId]
    FROM [Roles]
    WHERE [RoleName] = 'Student';

    IF @StudentRoleId IS NULL
        THROW 50010, 'Student role is not configured.', 1;

    INSERT INTO [Registration]
    (
        [College],
        [Department],
        [StudentName],
        [USN],
        [Semester],
        [EmailId],
        [PasswordHash],
        [RoleId]
    )
    VALUES
    (
        @College,
        @Department,
        @StudentName,
        @USN,
        @Semester,
        @EmailId,
        @PasswordHash,
        @StudentRoleId
    );

    SET @UserId = SCOPE_IDENTITY();
END
GO

CREATE PROCEDURE [dbo].[usp_UpdateUserProfile]
(
    @UserId INT,
    @StudentName VARCHAR(150),
    @USN VARCHAR(30),
    @Semester TINYINT
)
AS
BEGIN
    SET NOCOUNT ON;

    SET @StudentName = LTRIM(RTRIM(@StudentName));
    SET @USN = UPPER(LTRIM(RTRIM(@USN)));

    IF NOT EXISTS
    (
        SELECT 1
        FROM [Registration]
        WHERE [UserId] = @UserId
    )
        THROW 50011, 'User does not exist.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM [Registration]
        WHERE [USN] = @USN
          AND [UserId] <> @UserId
    )
        THROW 50012, 'USN is already registered.', 1;

    IF @Semester NOT BETWEEN 1 AND 8
        THROW 50013, 'Semester must be between 1 and 8.', 1;

    UPDATE [Registration]
    SET
        [StudentName] = @StudentName,
        [USN] = @USN,
        [Semester] = @Semester,
        [UpdatedAt] = SYSUTCDATETIME()
    WHERE [UserId] = @UserId;
END
GO

CREATE PROCEDURE [dbo].[usp_ChangePassword]
(
    @UserId INT,
    @PasswordHash VARCHAR(500)
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NULLIF(@PasswordHash, '') IS NULL
        THROW 50014, 'Password hash is required.', 1;

    UPDATE [Registration]
    SET
        [PasswordHash] = @PasswordHash,
        [UpdatedAt] = SYSUTCDATETIME()
    WHERE [UserId] = @UserId;

    IF @@ROWCOUNT = 0
        THROW 50015, 'User does not exist.', 1;
END
GO

CREATE PROCEDURE [dbo].[usp_UpdateRegistrationStatus]
(
    @UserId INT,
    @Status VARCHAR(20),
    @AdminUserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Status NOT IN ('Approved', 'Rejected')
        THROW 50016, 'Status must be Approved or Rejected.', 1;

    UPDATE [Registration]
    SET
        [RegistrationStatus] = @Status,
        [IsActive] =
            CASE WHEN @Status = 'Approved' THEN 1 ELSE 0 END,
        [ApprovedAt] = SYSUTCDATETIME(),
        [ApprovedBy] = @AdminUserId,
        [UpdatedAt] = SYSUTCDATETIME()
    WHERE [UserId] = @UserId
      AND [RegistrationStatus] = 'Pending';

    IF @@ROWCOUNT = 0
        THROW 50017, 'Pending registration was not found.', 1;
END
GO

CREATE PROCEDURE [dbo].[usp_BulkUpdateRegistrationStatus]
(
    @UserIds [dbo].[UserIdList] READONLY,
    @Status VARCHAR(20),
    @AdminUserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Status NOT IN ('Approved', 'Rejected')
        THROW 50018, 'Status must be Approved or Rejected.', 1;

    UPDATE R
    SET
        R.[RegistrationStatus] = @Status,
        R.[IsActive] =
            CASE WHEN @Status = 'Approved' THEN 1 ELSE 0 END,
        R.[ApprovedAt] = SYSUTCDATETIME(),
        R.[ApprovedBy] = @AdminUserId,
        R.[UpdatedAt] = SYSUTCDATETIME()
    FROM [Registration] R
    INNER JOIN @UserIds U
        ON U.[UserId] = R.[UserId]
    WHERE R.[RegistrationStatus] = 'Pending';

    SELECT @@ROWCOUNT AS [UpdatedCount];
END
GO

/* =========================================================
   LAB ENROLLMENT AND PROGRESS PROCEDURES
   ========================================================= */

CREATE PROCEDURE [dbo].[usp_EnrollUserInLab]
(
    @UserId INT,
    @LabId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS
        (
            SELECT 1
            FROM [Registration]
            WHERE [UserId] = @UserId
              AND [RegistrationStatus] = 'Approved'
              AND [IsActive] = 1
        )
            THROW 50019, 'User must be approved and active.', 1;

        IF NOT EXISTS
        (
            SELECT 1
            FROM [Labs]
            WHERE [LabId] = @LabId
              AND [IsActive] = 1
              AND [IsAvailable] = 1
        )
            THROW 50020, 'Lab is not available for enrollment.', 1;

        IF EXISTS
        (
            SELECT 1
            FROM [LabEnrollment]
            WHERE [UserId] = @UserId
              AND [LabId] = @LabId
        )
            THROW 50021, 'User is already enrolled in this lab.', 1;

        INSERT INTO [LabEnrollment]
        (
            [UserId],
            [LabId]
        )
        VALUES
        (
            @UserId,
            @LabId
        );

        INSERT INTO [LabStatus]
        (
            [LabId],
            [UserId],
            [ExperimentId]
        )
        SELECT
            E.[LabId],
            @UserId,
            E.[ExperimentId]
        FROM [Experiments] E
        WHERE E.[LabId] = @LabId
          AND E.[IsActive] = 1;

        DECLARE @RowsCreated INT = @@ROWCOUNT;

        COMMIT TRANSACTION;

        SELECT
            CAST(1 AS BIT) AS [Success],
            @RowsCreated AS [ExperimentRowsCreated],
            'Lab enrollment completed successfully.' AS [Message];
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END
GO

CREATE PROCEDURE [dbo].[usp_UpdateExperimentProgress]
(
    @UserId INT,
    @LabId INT,
    @ExperimentId INT,
    @ProgressItem VARCHAR(30),
    @IsCompleted BIT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @ProgressItem NOT IN
    (
        'Theory',
        'Execution',
        'Quiz',
        'Assignment1',
        'Assignment2',
        'Assignment3',
        'Assignment4',
        'Assignment5'
    )
        THROW 50022, 'Invalid progress item.', 1;

    DECLARE @AffectedRows INT = 0;

    DECLARE @Sql NVARCHAR(MAX) =
        N'UPDATE [dbo].[LabStatus]
          SET ' + QUOTENAME(@ProgressItem) + N' = @Completed,
              [UpdatedAt] = SYSUTCDATETIME()
          WHERE [UserId] = @User
            AND [LabId] = @Lab
            AND [ExperimentId] = @Experiment;

          SET @Affected = @@ROWCOUNT;';

    EXEC sys.sp_executesql
        @Sql,
        N'@Completed BIT,
          @User INT,
          @Lab INT,
          @Experiment INT,
          @Affected INT OUTPUT',
        @Completed = @IsCompleted,
        @User = @UserId,
        @Lab = @LabId,
        @Experiment = @ExperimentId,
        @Affected = @AffectedRows OUTPUT;

    IF @AffectedRows = 0
        THROW 50023, 'Experiment progress row was not found.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM [LabStatus]
        WHERE [UserId] = @UserId
          AND [LabId] = @LabId
          AND [CompletionStatus] <> 'Completed'
    )
    BEGIN
        UPDATE [LabEnrollment]
        SET
            [EnrollmentStatus] = 'Completed',
            [CompletedAt] = SYSUTCDATETIME()
        WHERE [UserId] = @UserId
          AND [LabId] = @LabId;
    END
END
GO

/* =========================================================
   ACTIVE-TIME PROCEDURES
   The frontend sends a heartbeat only while the browser is
   visible and the user is not idle.
   ========================================================= */

CREATE PROCEDURE [dbo].[usp_StartActivitySession]
(
    @UserId INT,
    @ActivitySessionId BIGINT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET @ActivitySessionId = 0;

    IF NOT EXISTS
    (
        SELECT 1
        FROM [Registration]
        WHERE [UserId] = @UserId
          AND [RegistrationStatus] = 'Approved'
          AND [IsActive] = 1
    )
        THROW 50024, 'User is not approved or active.', 1;

    INSERT INTO [UserActivitySessions]
    (
        [UserId]
    )
    VALUES
    (
        @UserId
    );

    SET @ActivitySessionId = SCOPE_IDENTITY();
END
GO

CREATE PROCEDURE [dbo].[usp_RecordActivityHeartbeat]
(
    @ActivitySessionId BIGINT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Now DATETIME2(0) =
        SYSUTCDATETIME();

    DECLARE @PreviousHeartbeat DATETIME2(0);

    SELECT
        @PreviousHeartbeat = [LastHeartbeatTime]
    FROM [UserActivitySessions]
    WHERE [ActivitySessionId] = @ActivitySessionId
      AND [IsSessionOpen] = 1;

    IF @PreviousHeartbeat IS NULL
        THROW 50025, 'Open activity session was not found.', 1;

    DECLARE @ElapsedSeconds INT =
        DATEDIFF(SECOND, @PreviousHeartbeat, @Now);

    /*
       Count at most 35 seconds for one heartbeat.
       This prevents a long idle gap from being added as active time.
       Heartbeats arriving faster than 20 seconds are ignored.
    */
    IF @ElapsedSeconds BETWEEN 20 AND 35
    BEGIN
        UPDATE [UserActivitySessions]
        SET
            [ActiveSeconds] =
                [ActiveSeconds] + @ElapsedSeconds,
            [LastActivityTime] = @Now,
            [LastHeartbeatTime] = @Now
        WHERE [ActivitySessionId] = @ActivitySessionId
          AND [IsSessionOpen] = 1;
    END
    ELSE IF @ElapsedSeconds > 35
    BEGIN
        UPDATE [UserActivitySessions]
        SET
            [LastHeartbeatTime] = @Now
        WHERE [ActivitySessionId] = @ActivitySessionId
          AND [IsSessionOpen] = 1;
    END
END
GO

CREATE PROCEDURE [dbo].[usp_StopActivitySession]
(
    @ActivitySessionId BIGINT
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [UserActivitySessions]
    SET
        [LogoutTime] = SYSUTCDATETIME(),
        [IsSessionOpen] = 0
    WHERE [ActivitySessionId] = @ActivitySessionId
      AND [IsSessionOpen] = 1;

    IF @@ROWCOUNT = 0
        THROW 50026, 'Open activity session was not found.', 1;
END
GO

/* =========================================================
   DASHBOARD AND REPORT PROCEDURES
   ========================================================= */

CREATE PROCEDURE [dbo].[usp_GetAdminDashboard]
AS
BEGIN
    SET NOCOUNT ON;

    -- Result set 1: College wise registrations
    SELECT
        [College] AS [Name],
        COUNT(*) AS [Registered]
    FROM [Registration]
    GROUP BY [College]
    ORDER BY [Registered] DESC, [College];

    -- Result set 2: Department wise registrations
    SELECT
        [Department] AS [Name],
        COUNT(*) AS [Registered]
    FROM [Registration]
    GROUP BY [Department]
    ORDER BY [Registered] DESC, [Department];

    -- Result set 3: ATME College departments
    SELECT
        [Department] AS [Name],
        COUNT(*) AS [Registered]
    FROM [Registration]
    WHERE [College] = 'ATME College of Engineering'
    GROUP BY [Department]
    ORDER BY [Registered] DESC, [Department];

    -- Result set 4: Lab registration and completion
    SELECT
        L.[LabId],
        L.[LabName] AS [Lab],
        COUNT(LE.[EnrollmentId]) AS [Registered],
        SUM
        (
            CASE
                WHEN LE.[EnrollmentStatus] = 'Completed'
                THEN 1
                ELSE 0
            END
        ) AS [Completed]
    FROM [Labs] L
    LEFT JOIN [LabEnrollment] LE
        ON LE.[LabId] = L.[LabId]
    GROUP BY
        L.[LabId],
        L.[LabName]
    ORDER BY L.[LabId];
END
GO

CREATE PROCEDURE [dbo].[usp_GetUserRegistrations]
(
    @SearchText VARCHAR(250) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SET @SearchText =
        NULLIF(LTRIM(RTRIM(@SearchText)), '');

    SELECT
        R.[UserId],
        'U' + RIGHT
        (
            '0000' + CAST(R.[UserId] AS VARCHAR(10)),
            4
        ) AS [DisplayUserId],
        R.[StudentName] AS [Name],
        R.[USN],
        R.[College],
        R.[Department],
        R.[EmailId],
        R.[Semester],
        R.[RegistrationStatus] AS [Status],
        R.[CreatedAt]
    FROM [Registration] R
    WHERE
        @SearchText IS NULL
        OR R.[StudentName] LIKE '%' + @SearchText + '%'
        OR R.[USN] LIKE '%' + @SearchText + '%'
        OR R.[EmailId] LIKE '%' + @SearchText + '%'
        OR R.[College] LIKE '%' + @SearchText + '%'
        OR R.[Department] LIKE '%' + @SearchText + '%'
        OR R.[RegistrationStatus] LIKE '%' + @SearchText + '%'
    ORDER BY
        CASE
            WHEN R.[RegistrationStatus] = 'Pending'
            THEN 0
            ELSE 1
        END,
        R.[CreatedAt] DESC;
END
GO

CREATE PROCEDURE [dbo].[usp_GetUserDashboard]
(
    @UserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Result set 1: User details
    SELECT
        [UserId],
        [StudentName] AS [Name],
        [USN],
        [College],
        [Department],
        [Semester],
        [EmailId],
        [RegistrationStatus]
    FROM [Registration]
    WHERE [UserId] = @UserId;

    -- Result set 2: Registered labs
    SELECT
        L.[LabId],
        L.[LabName],
        L.[CourseCode],
        LE.[EnrollmentStatus],
        LE.[EnrolledAt],
        LE.[CompletedAt]
    FROM [LabEnrollment] LE
    INNER JOIN [Labs] L
        ON L.[LabId] = LE.[LabId]
    WHERE LE.[UserId] = @UserId
    ORDER BY L.[Semester], L.[LabId];

    -- Result set 3: Experiment progress
    SELECT
        LS.[LabId],
        E.[ExperimentId],
        E.[ExperimentNumber],
        E.[ExperimentTitle],
        LS.[Theory],
        LS.[Execution],
        LS.[Quiz],
        LS.[Assignment1],
        LS.[Assignment2],
        LS.[Assignment3],
        LS.[Assignment4],
        LS.[Assignment5],
        LS.[CompletionStatus]
    FROM [LabStatus] LS
    INNER JOIN [Experiments] E
        ON E.[ExperimentId] = LS.[ExperimentId]
    WHERE LS.[UserId] = @UserId
    ORDER BY LS.[LabId], E.[ExperimentNumber];
END
GO

CREATE PROCEDURE [dbo].[usp_GetUserActivityHistory]
(
    @UserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CAST([LoginTime] AS DATE) AS [ActivityDate],
        [LoginTime],
        [LogoutTime],
        [ActiveSeconds],
        dbo.[ufn_GetActiveDurationText]
        (
            [ActiveSeconds]
        ) AS [ActiveDuration]
    FROM [UserActivitySessions]
    WHERE [UserId] = @UserId
    ORDER BY [LoginTime] DESC;

    SELECT
        SUM([ActiveSeconds]) AS [TotalActiveSeconds],
        dbo.[ufn_GetActiveDurationText]
        (
            ISNULL(SUM([ActiveSeconds]), 0)
        ) AS [TotalActiveDuration]
    FROM [UserActivitySessions]
    WHERE [UserId] = @UserId;
END
GO

/* =========================================================
   SEED DATA
   ========================================================= */

INSERT INTO [Roles] ([RoleName])
VALUES
    ('Admin'),
    ('Student');
GO

INSERT INTO [Labs]
(
    [LabName],
    [CourseCode],
    [Semester],
    [Scheme],
    [IsAvailable]
)
VALUES
    ('C Programming Lab', '1BPOPL107', 1, 2025, 1),
    ('Data Structures Lab', '1BCSL306', 3, 2025, 0),
    ('Design and Analysis of Algorithms Lab', '1BCS404', 4, 2025, 0);
GO

DECLARE @CLabId INT;

SELECT @CLabId = [LabId]
FROM [Labs]
WHERE [CourseCode] = '1BPOPL107';

INSERT INTO [Experiments]
(
    [LabId],
    [ExperimentNumber],
    [ExperimentTitle]
)
VALUES
    (@CLabId, 1,
     'A robot needs to find how far it must travel between two points on a 2D plane. Develop a C program to calculate the straight-line distance between the given coordinates.'),
    (@CLabId, 2,
     'Develop a C program that takes a student''s marks as input and displays their grade based on the specified grading criteria.'),
    (@CLabId, 3,
     'Develop a C program that takes a unique identification input like PAN Number, AADHAR Number, APAAR ID, Driving License and Passport and checks it against stored KYC records.'),
    (@CLabId, 4,
     'Develop a C program to calculate and display the roots of a quadratic equation based on the given coefficients.'),
    (@CLabId, 5,
     'Develop a C program to approximate the value of sin(x) using a series expansion method.'),
    (@CLabId, 6,
     'Develop a C program that accepts a course description string and a keyword from the user and searches for the keyword using string functions.'),
    (@CLabId, 7,
     'Develop a C program that takes marks for three subjects as input, checks pass/fail using a function and displays the average.'),
    (@CLabId, 8,
     'In an ATM system, two account balances need to be swapped temporarily for validation. Develop a C program that accepts two balances and uses a function with pointers to swap them.'),
    (@CLabId, 9,
     'A college library has a digital bookshelf system where each book is assigned a unique Book ID. Develop a C program to quickly find whether a book with a specific Book ID is available in the shelf.'),
    (@CLabId, 10,
     'A sports teacher has recorded the scores of students in a 100-meter race. Develop a C program to sort the scores in descending order.'),
    (@CLabId, 11,
     'A small warehouse tracks product shipments and revenue. Develop a C program to calculate total revenue generated by each branch.'),
    (@CLabId, 12,
     'A basic mobile contact manager stores first and last names separately. Develop a C program without using built-in string functions to join names and calculate their lengths.'),
    (@CLabId, 13,
     'A currency exchange booth allows users to convert between two currencies. Develop a C program implementing Call by Value and Call by Reference.'),
    (@CLabId, 14,
     'A local library needs to store and display details of its books including title, author and year of publication. Design a structure and develop a C program.');
GO

/*
    Replace this temporary administrator password hash through
    ASP.NET Core before production use.
*/
DECLARE @AdminRoleId TINYINT;

SELECT @AdminRoleId = [RoleId]
FROM [Roles]
WHERE [RoleName] = 'Admin';

INSERT INTO [Registration]
(
    [College],
    [Department],
    [StudentName],
    [USN],
    [Semester],
    [EmailId],
    [PasswordHash],
    [RoleId],
    [RegistrationStatus],
    [IsActive],
    [ApprovedAt]
)
VALUES
(
    'ATME College of Engineering',
    'Computer Science and Engineering',
    'System Administrator',
    'ADMIN001',
    1,
    'admin@atmecsevlab.local',
    'REPLACE_WITH_ASPNET_CORE_PASSWORD_HASH',
    @AdminRoleId,
    'Approved',
    1,
    SYSUTCDATETIME()
);
GO

/* =========================================================
   EXAMPLE COMMANDS
   ========================================================= */

/*
-- Register a student. Generate PasswordHash in ASP.NET Core.
DECLARE @NewUserId INT;

EXEC dbo.usp_RegisterUser
    @College =
        'ATME College of Engineering',
    @Department =
        'Computer Science and Engineering',
    @StudentName = 'Demo Student',
    @USN = '1AT23CS001',
    @Semester = 4,
    @EmailId = 'student@example.com',
    @PasswordHash = 'ASPNET_CORE_GENERATED_HASH',
    @UserId = @NewUserId OUTPUT;

SELECT @NewUserId AS NewUserId;

-- Approve one registration.
EXEC dbo.usp_UpdateRegistrationStatus
    @UserId = @NewUserId,
    @Status = 'Approved',
    @AdminUserId = 1;

-- Approve selected users.
DECLARE @SelectedUsers dbo.UserIdList;
INSERT INTO @SelectedUsers ([UserId])
VALUES (2), (3), (4);

EXEC dbo.usp_BulkUpdateRegistrationStatus
    @UserIds = @SelectedUsers,
    @Status = 'Approved',
    @AdminUserId = 1;

-- Enroll the user in C Programming Lab.
DECLARE @CLab INT;
SELECT @CLab = [LabId]
FROM [Labs]
WHERE [CourseCode] = '1BPOPL107';

EXEC dbo.usp_EnrollUserInLab
    @UserId = @NewUserId,
    @LabId = @CLab;

-- The enrollment creates one LabStatus row per active experiment.
SELECT *
FROM [LabStatus]
WHERE [UserId] = @NewUserId
  AND [LabId] = @CLab;
*/

/* =========================================================
   VERIFICATION QUERIES
   ========================================================= */

SELECT * FROM [Roles];
SELECT * FROM [Registration];
SELECT * FROM [Labs];
SELECT * FROM [Experiments]
ORDER BY [LabId], [ExperimentNumber];
SELECT * FROM [LabEnrollment];
SELECT * FROM [LabStatus];
SELECT * FROM [UserActivitySessions];
SELECT * FROM [UserRequests];
GO
