using CSEVirtualLabDataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace CSEVirtualLabWebAPI.Services
{
    public static class DatabaseSchemaInitializer
    {
        public static async Task EnsureApplicationTablesAsync(
            IServiceProvider services)
        {
            using var scope =
                services.CreateScope();

            var context =
                scope.ServiceProvider
                    .GetRequiredService<AtmecsevlabContext>();

            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[dbo].[UserRequests]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[UserRequests]
                    (
                        [RequestId] BIGINT IDENTITY(1,1)
                            CONSTRAINT [PK_UserRequests] PRIMARY KEY,
                        [UserId] INT NOT NULL
                            CONSTRAINT [FK_UserRequests_Registration]
                            REFERENCES [dbo].[Registration]([UserId]),
                        [EmailId] VARCHAR(256) NOT NULL,
                        [RequestType] VARCHAR(30) NOT NULL
                            CONSTRAINT [CK_UserRequests_Type]
                            CHECK ([RequestType] IN
                                ('Query', 'Feedback', 'Defects/Bugs')),
                        [Description] NVARCHAR(2000) NOT NULL,
                        [SubmittedAt] DATETIME2(0) NOT NULL
                            CONSTRAINT [DF_UserRequests_SubmittedAt]
                            DEFAULT SYSUTCDATETIME(),
                        [Status] VARCHAR(10) NOT NULL
                            CONSTRAINT [DF_UserRequests_Status]
                            DEFAULT 'Open'
                            CONSTRAINT [CK_UserRequests_Status]
                            CHECK ([Status] IN ('Open', 'Closed')),
                        [ClosedAt] DATETIME2(0) NULL,
                        [Remarks] NVARCHAR(2000) NULL,
                        [UpdatedAt] DATETIME2(0) NULL
                    );

                    CREATE INDEX [IX_UserRequests_Status_SubmittedAt]
                        ON [dbo].[UserRequests]([Status], [SubmittedAt]);

                    CREATE INDEX [IX_UserRequests_User_SubmittedAt]
                        ON [dbo].[UserRequests]([UserId], [SubmittedAt]);
                END

                IF OBJECT_ID(N'[dbo].[SystemSettings]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[SystemSettings]
                    (
                        [SettingKey] VARCHAR(100) NOT NULL
                            CONSTRAINT [PK_SystemSettings] PRIMARY KEY,
                        [SettingValue] VARCHAR(500) NOT NULL,
                        [UpdatedAt] DATETIME2(0) NOT NULL
                            CONSTRAINT [DF_SystemSettings_UpdatedAt]
                            DEFAULT SYSUTCDATETIME()
                    );
                END

                IF NOT EXISTS
                (
                    SELECT 1
                    FROM [dbo].[SystemSettings]
                    WHERE [SettingKey] = 'AutoApproveRegistrations'
                )
                BEGIN
                    INSERT INTO [dbo].[SystemSettings]
                        ([SettingKey], [SettingValue])
                    VALUES
                        ('AutoApproveRegistrations', 'false');
                END

                IF COL_LENGTH('dbo.Registration', 'UserType') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[Registration]
                    ADD [UserType] VARCHAR(20) NOT NULL
                        CONSTRAINT [DF_Registration_UserType]
                        DEFAULT 'Student';
                END

                IF COL_LENGTH('dbo.Registration', 'Organization') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[Registration]
                    ADD [Organization] VARCHAR(250) NULL;
                END

                IF COL_LENGTH('dbo.Registration', 'Designation') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[Registration]
                    ADD [Designation] VARCHAR(150) NULL;
                END

                IF EXISTS
                (
                    SELECT 1
                    FROM sys.key_constraints
                    WHERE [name] = 'UQ_Registration_USN'
                        AND [parent_object_id] =
                            OBJECT_ID('dbo.Registration')
                )
                BEGIN
                    ALTER TABLE [dbo].[Registration]
                    DROP CONSTRAINT [UQ_Registration_USN];
                END

                IF EXISTS
                (
                    SELECT 1
                    FROM sys.indexes
                    WHERE [name] = 'UQ_Registration_USN'
                        AND [object_id] =
                            OBJECT_ID('dbo.Registration')
                )
                BEGIN
                    DROP INDEX [UQ_Registration_USN]
                    ON [dbo].[Registration];
                END

                ALTER TABLE [dbo].[Registration]
                ALTER COLUMN [USN] VARCHAR(30) NULL;

                ALTER TABLE [dbo].[Registration]
                ALTER COLUMN [College] VARCHAR(250) NULL;

                ALTER TABLE [dbo].[Registration]
                ALTER COLUMN [Semester] TINYINT NULL;

                IF NOT EXISTS
                (
                    SELECT 1
                    FROM sys.indexes
                    WHERE [name] = 'UQ_Registration_USN'
                        AND [object_id] =
                            OBJECT_ID('dbo.Registration')
                )
                BEGIN
                    CREATE UNIQUE INDEX [UQ_Registration_USN]
                    ON [dbo].[Registration]([USN])
                    WHERE [USN] IS NOT NULL;
                END
                """);
        }
    }
}
