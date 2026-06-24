using CSEVirtualLabDataAccessLayer;
using CSEVirtualLabDataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace CSEVirtualLabConsoleApp
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            await RunConsoleTestsAsync();

            CloseConsole();
        }

        static async Task RunConsoleTestsAsync()
        {
            try
            {
                VirtualLabRepository repository =
                    CreateRepository();

                int userId = 2;

                // await TestUserRegistrationAsync(repository);
                // await TestLoginAsync(repository);
                // await TestRegisterLabAsync(repository, userId, 1);
                await TestUserDashboardAsync(repository, userId);
                await TestCertificateEligibilityAsync(repository, userId, 1);
                await TestCompletionCertificateDataAsync(repository, userId, 1);
                await TestLogoutAsync(repository, userId);
                //  await TestUserLogHistoryAsync(repository, userId);

                //await TestExecutionStatusAsync(repository);
                //await TestQuizStatusAsync(repository);
                //await TestAssignmentStatusAsync(repository);

                // await TestAdminDashboardAsync(repository);

                // Individual admin table tests:
                // await TestCollegeWiseRegistrationsAsync(repository);
                // await TestDepartmentWiseRegistrationsAsync(repository);
                // await TestAtmeceDepartmentWiseRegistrationsAsync(repository);
                // await TestLabCompletionStatusAsync(repository);
                // await TestUserRegistrationsForAdminAsync(repository);
                // await TestUpdateSingleRegistrationStatusAsync(repository);
                // await TestUpdateSelectedRegistrationStatusAsync(repository);
            }
            catch (Exception exception)
            {
                Console.WriteLine(
                    $"Error: {exception.Message}");

                Console.WriteLine(exception);
            }
        }

        static VirtualLabRepository CreateRepository()
        {
            string connectionString =
                @"Data Source=(localdb)\MSSQLLocalDB;
                  Initial Catalog=atmecsevlab;
                  Integrated Security=True;
                  TrustServerCertificate=True";

            var options =
                new DbContextOptionsBuilder<AtmecsevlabContext>()
                    .UseSqlServer(connectionString)
                    .Options;

            var context =
                new AtmecsevlabContext(options);

            return new VirtualLabRepository(context);
        }

        static async Task TestUserRegistrationAsync(
            VirtualLabRepository repository)
        {
            var registration = new Registration
            {
                College = "ATME College of Engineering",
                Department = "Computer Science and Engineering",
                StudentName = "Test Student",
                Usn = "1AT23CS099",
                Semester = 4,
                EmailId = "teststudent@example.com",
                PasswordHash = "TestPasswordHash"
            };

            Registration registeredUser =
                await repository.RegisterUserAsync(registration);

            Console.WriteLine("User registered successfully.");
            Console.WriteLine($"User ID: {registeredUser.UserId}");
            Console.WriteLine($"Name: {registeredUser.StudentName}");
            Console.WriteLine($"Email: {registeredUser.EmailId}");
            Console.WriteLine($"Status: {registeredUser.RegistrationStatus}");
        }

        static async Task TestLoginAsync(
            VirtualLabRepository repository)
        {
            string emailId = "teststudent@example.com";
            string password = "TestPasswordHash";

            Registration? user =
                await repository.LoginAsync(emailId);

            if (user == null)
            {
                Console.WriteLine(
                    "Invalid account or registration is not approved.");

                return;
            }

            if (user.PasswordHash != password)
            {
                Console.WriteLine("Invalid password.");
                return;
            }

            Console.WriteLine("Login successful.");
            Console.WriteLine($"Welcome, {user.StudentName}");
            Console.WriteLine($"Role: {user.Role.RoleName}");
        }

        static async Task TestRegisterLabAsync(
            VirtualLabRepository repository,
            int userId,
            int labId)
        {
            bool registered =
                await repository.RegisterLabAsync(userId, labId);

            Console.WriteLine(
                registered
                    ? "Lab registered successfully."
                    : "Lab registration failed.");
        }

        static async Task TestUserDashboardAsync(
            VirtualLabRepository repository,
            int userId)
        {
            UserDashboardDto? dashboard =
                await repository.GetUserDashboardAsync(userId);

            if (dashboard == null)
            {
                Console.WriteLine("User was not found.");
                return;
            }

            Console.WriteLine("USER DASHBOARD");
            Console.WriteLine(new string('=', 75));
            Console.WriteLine($"Name            : {dashboard.StudentName}");
            Console.WriteLine($"USN             : {dashboard.Usn}");
            Console.WriteLine($"College         : {dashboard.College}");
            Console.WriteLine($"Semester        : {dashboard.Semester}");
            Console.WriteLine($"Registered Labs : {dashboard.RegisteredLabCount}");

            foreach (UserLabDashboardDto lab in
                     dashboard.Labs.Where(item => item.IsRegistered))
            {
                Console.WriteLine();
                Console.WriteLine(new string('=', 75));
                Console.WriteLine($"LAB: {lab.LabName}");
                Console.WriteLine($"Course Code: {lab.CourseCode}");
                Console.WriteLine($"Enrollment Status: {lab.EnrollmentStatus}");
                Console.WriteLine(
                    "Certificate Download: " +
                    (lab.CanDownloadCertificate ? "Allowed" : "Not Allowed"));
                Console.WriteLine(new string('-', 75));

                Console.WriteLine(
                    $"{"Exp",-6}" +
                    $"{"Execution",-13}" +
                    $"{"Quiz",-13}" +
                    $"{"Assignments",-20}" +
                    $"{"Status",-18}");

                Console.WriteLine(new string('-', 75));

                foreach (ExperimentProgressDto experiment
                         in lab.Experiments)
                {
                    Console.WriteLine(
                        $"{experiment.ExperimentNumber,-6}" +
                        $"{experiment.Execution,-13}" +
                        $"{experiment.Quiz,-13}" +
                        $"{experiment.Assignments,-20}" +
                        $"{experiment.Status,-18}");
                }
            }

            Console.WriteLine(new string('=', 75));
        }

        static async Task TestCertificateEligibilityAsync(
            VirtualLabRepository repository,
            int userId,
            int labId)
        {
            bool canDownload =
                await repository.CanDownloadCertificateAsync(
                    userId,
                    labId);

            Console.WriteLine();
            Console.WriteLine("CERTIFICATE ELIGIBILITY TEST");
            Console.WriteLine(new string('-', 75));
            Console.WriteLine($"User ID : {userId}");
            Console.WriteLine($"Lab ID  : {labId}");
            Console.WriteLine(
                canDownload
                    ? "Result  : Certificate download is allowed."
                    : "Result  : Certificate download is not allowed.");
            Console.WriteLine(new string('-', 75));
        }

        static async Task TestCompletionCertificateDataAsync(
            VirtualLabRepository repository,
            int userId,
            int labId)
        {
            CompletionCertificateDto? certificate =
                await repository.GetCompletionCertificateDataAsync(
                    userId,
                    labId);

            Console.WriteLine();
            Console.WriteLine("COMPLETION CERTIFICATE DATA TEST");
            Console.WriteLine(new string('-', 75));

            if (certificate == null)
            {
                Console.WriteLine(
                    "Certificate data was not found.");

                Console.WriteLine(new string('-', 75));
                return;
            }

            Console.WriteLine($"User ID              : {certificate.UserId}");
            Console.WriteLine($"Name                 : {certificate.StudentName}");
            Console.WriteLine($"USN                  : {certificate.Usn}");
            Console.WriteLine($"College              : {certificate.College}");
            Console.WriteLine($"Department           : {certificate.Department}");
            Console.WriteLine($"Lab                  : {certificate.LabName}");
            Console.WriteLine($"Course Code          : {certificate.CourseCode}");
            Console.WriteLine(
                $"Experiments          : " +
                $"{certificate.CompletedExperiments} / " +
                $"{certificate.TotalExperiments}");
            Console.WriteLine(
                $"Assignments          : " +
                $"{certificate.CompletedAssignments} / " +
                $"{certificate.TotalAssignments}");
            Console.WriteLine(
                $"Quiz Score           : " +
                $"{certificate.QuizScoreOutOf10:0.0} / 10");
            Console.WriteLine(
                $"Actual Quiz Marks    : " +
                $"{certificate.TotalQuizScore} / " +
                $"{certificate.TotalQuizMaxMarks}");
            Console.WriteLine(
                $"Completion Date      : " +
                $"{certificate.CompletionDate?.ToLocalTime():dd-MM-yyyy}");
            Console.WriteLine(
                $"Completion Status    : " +
                $"{(certificate.IsCompleted ? "Completed" : "Not Completed")}");
            Console.WriteLine($"Certificate ID       : {certificate.CertificateId}");
            Console.WriteLine(new string('-', 75));
        }

        static async Task TestUserLogHistoryAsync(
            VirtualLabRepository repository,
            int userId)
        {
            long sessionId =
                await repository.StartUserSessionAsync(userId);

            Console.WriteLine(
                $"Login recorded. Session ID: {sessionId}");

            await Task.Delay(5000);

            bool loggedOut =
                await repository.EndUserSessionAsync(sessionId);

            Console.WriteLine(
                loggedOut
                    ? "Logout recorded."
                    : "Open session was not found.");

            UserLogHistoryDto history =
                await repository.GetUserLogHistoryAsync(userId);

            foreach (UserLogHistoryItemDto session in history.Sessions)
            {
                TimeSpan duration =
                    TimeSpan.FromSeconds(session.DurationSeconds);

                Console.WriteLine(
                    $"Login: " +
                    $"{session.LoginTime.ToLocalTime():dd-MM-yyyy hh:mm tt}");

                Console.WriteLine(
                    $"Logout: " +
                    $"{session.LogoutTime?.ToLocalTime():dd-MM-yyyy hh:mm tt}");

                Console.WriteLine(
                    $"Duration: {(int)duration.TotalHours} hrs " +
                    $"{duration.Minutes} minutes");
            }
        }

        static async Task TestLogoutAsync(
            VirtualLabRepository repository,
            int userId)
        {
            Console.WriteLine();
            Console.WriteLine("LOGOUT TEST");
            Console.WriteLine(new string('-', 75));

            long sessionId =
                await repository.StartUserSessionAsync(userId);

            Console.WriteLine(
                $"Login session started. Session ID: {sessionId}");

            await Task.Delay(3000);

            bool loggedOut =
                await repository.LogoutAsync(userId);

            Console.WriteLine(
                loggedOut
                    ? "Logout successful."
                    : "No open session was found for logout.");

            UserLogHistoryDto history =
                await repository.GetUserLogHistoryAsync(userId);

            UserLogHistoryItemDto? latestSession =
                history.Sessions.FirstOrDefault();

            if (latestSession != null)
            {
                TimeSpan duration =
                    TimeSpan.FromSeconds(
                        latestSession.DurationSeconds);

                Console.WriteLine(
                    $"Latest Login  : " +
                    $"{latestSession.LoginTime.ToLocalTime():dd-MM-yyyy hh:mm tt}");

                Console.WriteLine(
                    $"Latest Logout : " +
                    $"{latestSession.LogoutTime?.ToLocalTime():dd-MM-yyyy hh:mm tt}");

                Console.WriteLine(
                    $"Duration      : " +
                    $"{(int)duration.TotalHours} hrs " +
                    $"{duration.Minutes} minutes " +
                    $"{duration.Seconds} seconds");
            }

            Console.WriteLine(new string('-', 75));
        }

        static void CloseConsole()
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to close.");
            Console.ReadKey();
        }

        static async Task TestExecutionStatusAsync(
    VirtualLabRepository repository)
        {
            bool updated =
                await repository.UpdateExecutionStatusAsync(
                    userId: 2,
                    labId: 1,
                    experimentId: 1);

            Console.WriteLine(
                updated
                    ? "Execution status updated."
                    : "Progress row not found.");
        }

        static async Task TestQuizStatusAsync(
    VirtualLabRepository repository)
        {
            bool updated =
                await repository.UpdateQuizStatusAsync(
                    userId: 2,
                    labId: 1,
                    experimentId: 1,
                    quizScore: 8,
                    quizMaxMarks: 10);

            Console.WriteLine(
                updated
                    ? "Quiz status updated."
                    : "Progress row not found.");
        }

        static async Task TestAssignmentStatusAsync(
    VirtualLabRepository repository)
        {
            bool updated =
                await repository.UpdateAssignmentStatusAsync(
                    userId: 2,
                    labId: 1,
                    experimentId: 1,
                    assignmentId: 1);

            Console.WriteLine(
                updated
                    ? "Assignment status updated."
                    : "Progress row not found.");
        }

        static async Task TestAdminDashboardAsync(
            VirtualLabRepository repository)
        {
            AdminDashboardDto dashboard =
                await repository.GetAdminDashboardAsync();

            Console.WriteLine("ADMIN DASHBOARD");
            Console.WriteLine(new string('=', 90));

            PrintSummaryTable(
                "COLLEGE WISE",
                "College",
                dashboard.CollegeWise);

            PrintSummaryTable(
                "DEPARTMENT WISE",
                "Department",
                dashboard.DepartmentWise);

            PrintSummaryTable(
                "ATMECE DEPARTMENTS",
                "Department",
                dashboard.AtmeceDepartmentWise);

            PrintCompletionStatusTable(
                dashboard.CompletionStatus);

            PrintUserRegistrationsTable(
                dashboard.UserRegistrations);
        }

        static async Task TestCollegeWiseRegistrationsAsync(
            VirtualLabRepository repository)
        {
            List<AdminSummaryDto> rows =
                await repository.GetCollegeWiseRegistrationsAsync();

            PrintSummaryTable(
                "COLLEGE WISE",
                "College",
                rows);
        }

        static async Task TestDepartmentWiseRegistrationsAsync(
            VirtualLabRepository repository)
        {
            List<AdminSummaryDto> rows =
                await repository.GetDepartmentWiseRegistrationsAsync();

            PrintSummaryTable(
                "DEPARTMENT WISE",
                "Department",
                rows);
        }

        static async Task TestAtmeceDepartmentWiseRegistrationsAsync(
            VirtualLabRepository repository)
        {
            List<AdminSummaryDto> rows =
                await repository.GetAtmeceDepartmentWiseRegistrationsAsync();

            PrintSummaryTable(
                "ATMECE DEPARTMENTS",
                "Department",
                rows);
        }

        static async Task TestLabCompletionStatusAsync(
            VirtualLabRepository repository)
        {
            List<AdminLabCompletionDto> rows =
                await repository.GetLabCompletionStatusAsync();

            PrintCompletionStatusTable(rows);
        }

        static async Task TestUserRegistrationsForAdminAsync(
            VirtualLabRepository repository)
        {
            List<AdminUserRegistrationDto> rows =
                await repository.GetUserRegistrationsForAdminAsync();

            PrintUserRegistrationsTable(rows);
        }

        static async Task TestUpdateSingleRegistrationStatusAsync(
            VirtualLabRepository repository)
        {
            int userId = 2;
            int approvedBy = 1;
            string status = "Approved";

            bool updated =
                await repository.UpdateUserRegistrationStatusAsync(
                    userId,
                    status,
                    approvedBy);

            Console.WriteLine(
                updated
                    ? $"User {userId} status updated to {status}."
                    : $"User {userId} was not found.");
        }

        static async Task TestUpdateSelectedRegistrationStatusAsync(
            VirtualLabRepository repository)
        {
            var userIds =
                new List<int>
                {
                    2,
                    3
                };

            int approvedBy = 1;
            string status = "Rejected";

            int updatedCount =
                await repository.UpdateSelectedUserRegistrationStatusAsync(
                    userIds,
                    status,
                    approvedBy);

            Console.WriteLine(
                $"{updatedCount} selected user(s) updated to {status}.");
        }

        static void PrintSummaryTable(
            string title,
            string nameHeader,
            List<AdminSummaryDto> rows)
        {
            Console.WriteLine();
            Console.WriteLine(title);
            Console.WriteLine(new string('-', 90));
            Console.WriteLine($"{nameHeader,-65}{"Registered",15}");
            Console.WriteLine(new string('-', 90));

            foreach (AdminSummaryDto row in rows)
            {
                Console.WriteLine(
                    $"{row.Name,-65}{row.Registered,15}");
            }
        }

        static void PrintCompletionStatusTable(
            List<AdminLabCompletionDto> rows)
        {
            Console.WriteLine();
            Console.WriteLine("COMPLETION STATUS");
            Console.WriteLine(new string('-', 90));
            Console.WriteLine(
                $"{"Lab",-45}" +
                $"{"Registered",15}" +
                $"{"Completed",15}");
            Console.WriteLine(new string('-', 90));

            foreach (AdminLabCompletionDto row in rows)
            {
                Console.WriteLine(
                    $"{row.LabName,-45}" +
                    $"{row.Registered,15}" +
                    $"{row.Completed,15}");
            }
        }

        static void PrintUserRegistrationsTable(
            List<AdminUserRegistrationDto> rows)
        {
            Console.WriteLine();
            Console.WriteLine("USER REGISTRATIONS");
            Console.WriteLine(new string('-', 120));
            Console.WriteLine(
                $"{"UserId",-8}" +
                $"{"Name",-24}" +
                $"{"USN",-15}" +
                $"{"College",-28}" +
                $"{"Department",-18}" +
                $"{"Status",-12}");
            Console.WriteLine(new string('-', 120));

            foreach (AdminUserRegistrationDto row in rows)
            {
                Console.WriteLine(
                    $"{row.UserId,-8}" +
                    $"{TrimForConsole(row.StudentName, 23),-24}" +
                    $"{row.Usn,-15}" +
                    $"{TrimForConsole(row.College, 27),-28}" +
                    $"{TrimForConsole(row.Department, 17),-18}" +
                    $"{row.RegistrationStatus,-12}");
            }
        }

        static string TrimForConsole(
            string value,
            int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            if (value.Length <= maxLength)
            {
                return value;
            }

            return value.Substring(0, maxLength - 3) + "...";
        }
    }
}
