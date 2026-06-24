using CSEVirtualLabDataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSEVirtualLabDataAccessLayer
{
    public class VirtualLabRepository
    {
        private const string AutoApproveRegistrationsKey =
            "AutoApproveRegistrations";

        private AtmecsevlabContext context;
        public VirtualLabRepository(AtmecsevlabContext context)
        {
            this.context = context;
        }

        //Registration
        public async Task<Registration> RegisterUserAsync(
            Registration registration)
        {
            registration.EmailId =
                registration.EmailId.Trim().ToLower();

            string userType =
                registration.UserType?.Trim() ?? string.Empty;

            if (
                userType != "Student" &&
                userType != "Faculty" &&
                userType != "Other")
            {
                throw new InvalidOperationException(
                    "User type must be Student, Faculty, or Other.");
            }

            registration.UserType = userType;

            if (userType == "Student")
            {
                if (
                    string.IsNullOrWhiteSpace(registration.College) ||
                    string.IsNullOrWhiteSpace(registration.Usn) ||
                    registration.Semester is null)
                {
                    throw new InvalidOperationException(
                        "College, USN, and Semester are required for students.");
                }

                registration.Usn =
                    registration.Usn.Trim().ToUpper();
                registration.Designation = null;
                registration.Organization = null;
            }
            else if (userType == "Faculty")
            {
                if (
                    string.IsNullOrWhiteSpace(registration.College) ||
                    string.IsNullOrWhiteSpace(registration.Designation))
                {
                    throw new InvalidOperationException(
                        "College and Designation are required for faculty.");
                }

                registration.Usn = null;
                registration.Semester = null;
                registration.Organization = null;
            }
            else
            {
                if (
                    string.IsNullOrWhiteSpace(registration.Organization) ||
                    string.IsNullOrWhiteSpace(registration.Designation))
                {
                    throw new InvalidOperationException(
                        "Organization and Designation are required for other professionals.");
                }

                registration.College = null;
                registration.Usn = null;
                registration.Semester = null;
            }

            bool emailExists =
                await context.Registrations.AnyAsync(
                    user =>
                        user.EmailId == registration.EmailId);

            if (emailExists)
            {
                throw new InvalidOperationException(
                    "Email ID is already registered.");
            }

            if (
                registration.Usn != null &&
                await context.Registrations.AnyAsync(
                    user => user.Usn == registration.Usn))
            {
                throw new InvalidOperationException(
                    "USN is already registered.");
            }

            Role studentRole =
                await context.Roles.FirstOrDefaultAsync(
                    role => role.RoleName == "Student");

            if (studentRole == null)
            {
                throw new InvalidOperationException(
                    "Student role is not configured.");
            }

            registration.College =
                string.IsNullOrWhiteSpace(registration.College)
                    ? null
                    : registration.College.Trim();

            registration.Organization =
                string.IsNullOrWhiteSpace(registration.Organization)
                    ? null
                    : registration.Organization.Trim();

            registration.Designation =
                string.IsNullOrWhiteSpace(registration.Designation)
                    ? null
                    : registration.Designation.Trim();

            registration.Department =
                registration.Department.Trim();

            registration.StudentName =
                registration.StudentName.Trim();

            registration.RoleId =
                studentRole.RoleId;

            bool autoApproveRegistrations =
                await GetAutoApproveRegistrationsAsync();

            registration.RegistrationStatus =
                autoApproveRegistrations
                    ? "Approved"
                    : "Pending";

            registration.IsActive = autoApproveRegistrations;
            registration.CreatedAt = DateTime.UtcNow;
            registration.UpdatedAt = null;
            registration.ApprovedAt =
                autoApproveRegistrations
                    ? DateTime.UtcNow
                    : null;
            registration.ApprovedBy = null;

            context.Registrations.Add(registration);

            await context.SaveChangesAsync();

            return registration;
        }


        //Login
        public async Task<Registration?> LoginAsync(string emailId)
        {
            string email = emailId.Trim().ToLowerInvariant();

            return await context.Registrations
                .AsNoTracking()
                .Include(user => user.Role)
                .FirstOrDefaultAsync(user =>
                    user.EmailId == email &&
                    user.RegistrationStatus == "Approved" &&  user.IsActive);
        }

        //Profile Update
        public async Task<Registration?> UpdateUserProfileAsync(
            int userId,
            string studentName,
            string college,
            byte semester)
        {
            Registration? user =
                await context.Registrations
                    .FirstOrDefaultAsync(registration =>
                        registration.UserId == userId &&
                        registration.IsActive);

            if (user == null)
            {
                return null;
            }

            user.StudentName =
                studentName.Trim();

            user.College =
                college.Trim();

            user.Semester =
                semester;

            user.UpdatedAt =
                DateTime.UtcNow;

            await context.SaveChangesAsync();

            return user;
        }

        //Change Password
        public async Task<bool> ChangePasswordAsync(
            int userId,
            string currentPassword,
            string newPassword)
        {
            Registration? user =
                await context.Registrations
                    .FirstOrDefaultAsync(registration =>
                        registration.UserId == userId &&
                        registration.IsActive);

            if (user == null)
            {
                return false;
            }

            if (user.PasswordHash != currentPassword)
            {
                throw new InvalidOperationException(
                    "Current password is incorrect.");
            }

            user.PasswordHash =
                newPassword;

            user.UpdatedAt =
                DateTime.UtcNow;

            await context.SaveChangesAsync();

            return true;
        }

        //For User Dashboard Data
        public async Task<UserDashboardDto?> GetUserDashboardAsync(int userId)
        {
            Registration? user = await context.Registrations
                .AsNoTracking()
                .FirstOrDefaultAsync(registration =>
                    registration.UserId == userId);

            if (user == null)
            {
                return null;
            }

            List<LabEnrollment> enrollments =
                await context.LabEnrollments
                    .AsNoTracking()
                    .Where(enrollment =>
                        enrollment.UserId == userId &&
                        enrollment.EnrollmentStatus != "Cancelled")
                    .ToListAsync();

            List<int> enrolledLabIds = enrollments
                .Select(enrollment => enrollment.LabId)
                .ToList();

            List<Lab> labs = await context.Labs
                .AsNoTracking()
                .Where(lab => lab.IsActive)
                .OrderBy(lab => lab.Semester)
                .ThenBy(lab => lab.LabName)
                .ToListAsync();

            List<LabStatus> progressRows =
                await context.LabStatuses
                    .AsNoTracking()
                    .Where(status =>
                        status.UserId == userId &&
                        enrolledLabIds.Contains(status.LabId))
                    .ToListAsync();

            List<Experiment> experiments =
                await context.Experiments
                    .AsNoTracking()
                    .Where(experiment =>
                        experiment.IsActive &&
                        enrolledLabIds.Contains(experiment.LabId))
                    .OrderBy(experiment => experiment.ExperimentNumber)
                    .ToListAsync();

            var dashboard = new UserDashboardDto
            {
                UserId = user.UserId,
                StudentName = user.StudentName,
                Usn = user.Usn ?? string.Empty,
                College =
                    user.College ??
                    user.Organization ??
                    string.Empty,
                Semester = user.Semester ?? 0,
                RegisteredLabCount = enrollments.Count
            };

            foreach (Lab lab in labs)
            {
                LabEnrollment? enrollment = enrollments
                    .FirstOrDefault(item => item.LabId == lab.LabId);

                var labDashboard = new UserLabDashboardDto
                {
                    LabId = lab.LabId,
                    LabName = lab.LabName,
                    CourseCode = lab.CourseCode,
                    IsRegistered = enrollment != null,
                    EnrollmentStatus =
                        enrollment?.EnrollmentStatus ?? "Not Registered"
                };

                if (enrollment != null)
                {
                    List<Experiment> labExperiments = experiments
                        .Where(item => item.LabId == lab.LabId)
                        .OrderBy(item => item.ExperimentNumber)
                        .ToList();

                    foreach (Experiment experiment in labExperiments)
                    {
                        LabStatus? status = progressRows
                            .FirstOrDefault(item =>
                                item.LabId == lab.LabId &&
                                item.ExperimentId ==
                                    experiment.ExperimentId);

                        labDashboard.Experiments.Add(
                            new ExperimentProgressDto
                            {
                                ExperimentId =
                                    experiment.ExperimentId,

                                ExperimentNumber =
                                    experiment.ExperimentNumber,

                                ExperimentTitle =
                                    experiment.ExperimentTitle,

                                Execution = status?.Execution == true
                                    ? "Completed"
                                    : "Pending",

                                Quiz = status?.Quiz == true
                                    ? "Completed"
                                    : "Pending",

                                Assignments = status == null
                                    ? "Pending"
                                    : GetAssignmentStatus(status),

                                Status = IsExperimentCompleted(status)
                                    ? "Completed"
                                    : "Not Completed"
                            });
                    }

                    labDashboard.CanDownloadCertificate =
                        IsLabCompleted(
                            lab.LabId,
                            labExperiments,
                            progressRows);

                    labDashboard.ProgressPercentage =
                        CalculateLabProgressPercentage(
                            lab.LabId,
                            labExperiments,
                            progressRows);
                }

                dashboard.Labs.Add(labDashboard);
            }

            return dashboard;
        }

        private static int CalculateLabProgressPercentage(
            int labId,
            List<Experiment> labExperiments,
            List<LabStatus> progressRows)
        {
            if (labExperiments.Count == 0)
            {
                return 0;
            }

            int earnedPercentagePoints = 0;

            foreach (Experiment experiment in labExperiments)
            {
                LabStatus? status =
                    progressRows.FirstOrDefault(item =>
                        item.LabId == labId &&
                        item.ExperimentId ==
                            experiment.ExperimentId);

                if (status == null)
                {
                    continue;
                }

                if (status.Execution)
                {
                    earnedPercentagePoints += 10;
                }

                if (status.Quiz)
                {
                    earnedPercentagePoints += 15;
                }

                if (status.Assignment1)
                {
                    earnedPercentagePoints += 15;
                }

                if (status.Assignment2)
                {
                    earnedPercentagePoints += 15;
                }

                if (status.Assignment3)
                {
                    earnedPercentagePoints += 15;
                }

                if (status.Assignment4)
                {
                    earnedPercentagePoints += 15;
                }

                if (status.Assignment5)
                {
                    earnedPercentagePoints += 15;
                }
            }

            double progress =
                (double)earnedPercentagePoints /
                labExperiments.Count;

            return Math.Clamp(
                (int)Math.Round(
                    progress,
                    MidpointRounding.AwayFromZero),
                0,
                100);
        }

        public async Task<bool> CanDownloadCertificateAsync(
            int userId,
            int labId)
        {
            bool isRegistered =
                await context.LabEnrollments
                    .AsNoTracking()
                    .AnyAsync(enrollment =>
                        enrollment.UserId == userId &&
                        enrollment.LabId == labId &&
                        enrollment.EnrollmentStatus != "Cancelled");

            if (!isRegistered)
            {
                return false;
            }

            List<Experiment> experiments =
                await context.Experiments
                    .AsNoTracking()
                    .Where(experiment =>
                        experiment.LabId == labId &&
                        experiment.IsActive)
                    .ToListAsync();

            if (experiments.Count == 0)
            {
                return false;
            }

            List<LabStatus> progressRows =
                await context.LabStatuses
                    .AsNoTracking()
                    .Where(status =>
                        status.UserId == userId &&
                        status.LabId == labId)
                    .ToListAsync();

            return IsLabCompleted(
                labId,
                experiments,
                progressRows);
        }

        public async Task<LabReportDto?> GetLabReportDataAsync(
            int userId,
            int labId)
        {
            Registration? user =
                await context.Registrations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(registration =>
                        registration.UserId == userId);

            if (user == null)
            {
                return null;
            }

            Lab? lab =
                await context.Labs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(item =>
                        item.LabId == labId &&
                        item.IsActive);

            if (lab == null)
            {
                return null;
            }

            LabEnrollment? enrollment =
                await context.LabEnrollments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(item =>
                        item.UserId == userId &&
                        item.LabId == labId &&
                        item.EnrollmentStatus != "Cancelled");

            if (enrollment == null)
            {
                return null;
            }

            List<Experiment> experiments =
                await context.Experiments
                    .AsNoTracking()
                    .Where(experiment =>
                        experiment.LabId == labId &&
                        experiment.IsActive)
                    .OrderBy(experiment =>
                        experiment.ExperimentNumber)
                    .ToListAsync();

            List<LabStatus> progressRows =
                await context.LabStatuses
                    .AsNoTracking()
                    .Where(status =>
                        status.UserId == userId &&
                        status.LabId == labId)
                    .ToListAsync();

            bool canDownloadCertificate =
                IsLabCompleted(
                    labId,
                    experiments,
                    progressRows);

            var report =
                new LabReportDto
                {
                    UserId = user.UserId,
                    StudentName = user.StudentName,
                    Usn = user.Usn ?? string.Empty,
                    Department = user.Department,
                    College =
                        user.College ??
                        user.Organization ??
                        string.Empty,
                    Semester = user.Semester ?? 0,
                    LabId = lab.LabId,
                    LabName = lab.LabName,
                    CourseCode = lab.CourseCode,
                    EnrollmentStatus = enrollment.EnrollmentStatus,
                    DateOfRegistration = enrollment.EnrolledAt,
                    DateOfCompletion = enrollment.CompletedAt,
                    CanDownloadCertificate = canDownloadCertificate
                };

            foreach (Experiment experiment in experiments)
            {
                LabStatus? status =
                    progressRows.FirstOrDefault(item =>
                        item.ExperimentId == experiment.ExperimentId);

                report.Experiments.Add(
                    new LabReportExperimentDto
                    {
                        ExperimentId = experiment.ExperimentId,
                        ExperimentNumber = experiment.ExperimentNumber,
                        PartName = experiment.ExperimentNumber <= 8
                            ? "PART-A"
                            : "PART-B",
                        PartExperimentNumber =
                            experiment.ExperimentNumber <= 8
                                ? experiment.ExperimentNumber
                                : experiment.ExperimentNumber - 8,
                        ExperimentTitle = experiment.ExperimentTitle,
                        Execution = status?.Execution == true
                            ? "Completed"
                            : "Pending",
                        Quiz = GetQuizReportStatus(status),
                        Assignments = status == null
                            ? "Pending"
                            : GetAssignmentStatus(status),
                        CompletionStatus =
                            IsExperimentCompleted(status)
                                ? "Completed"
                                : "Not Completed"
                    });
            }

            List<int> quizScores =
                progressRows
                    .Where(status => status.QuizScore.HasValue)
                    .Select(status => status.QuizScore!.Value)
                    .ToList();

            report.AverageQuizScore =
                quizScores.Count == 0
                    ? 0
                    : Math.Round(
                        quizScores.Average(),
                        2);

            return report;
        }

        public async Task<CompletionCertificateDto?>
            GetCompletionCertificateDataAsync(
                int userId,
                int labId)
        {
            Registration? user =
                await context.Registrations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(registration =>
                        registration.UserId == userId);

            if (user == null)
            {
                return null;
            }

            Lab? lab =
                await context.Labs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(item =>
                        item.LabId == labId &&
                        item.IsActive);

            if (lab == null)
            {
                return null;
            }

            LabEnrollment? enrollment =
                await context.LabEnrollments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(item =>
                        item.UserId == userId &&
                        item.LabId == labId &&
                        item.EnrollmentStatus != "Cancelled");

            if (enrollment == null)
            {
                return null;
            }

            List<Experiment> experiments =
                await context.Experiments
                    .AsNoTracking()
                    .Where(experiment =>
                        experiment.LabId == labId &&
                        experiment.IsActive)
                    .OrderBy(experiment =>
                        experiment.ExperimentNumber)
                    .ToListAsync();

            List<LabStatus> progressRows =
                await context.LabStatuses
                    .AsNoTracking()
                    .Where(status =>
                        status.UserId == userId &&
                        status.LabId == labId)
                    .ToListAsync();

            int completedExperiments =
                progressRows.Count(IsExperimentCompleted);

            int completedAssignments =
                progressRows.Sum(GetCompletedAssignmentCount);

            int totalAssignments =
                experiments.Count * 5;

            int totalQuizScore =
                progressRows
                    .Where(status => status.QuizScore.HasValue)
                    .Sum(status => status.QuizScore!.Value);

            int totalQuizMaxMarks =
                progressRows
                    .Where(status => status.QuizMaxMarks.HasValue)
                    .Sum(status => status.QuizMaxMarks!.Value);

            double quizScoreOutOf10 =
                totalQuizMaxMarks == 0
                    ? 0
                    : Math.Round(
                        ((double)totalQuizScore / totalQuizMaxMarks) * 10,
                        1);

            bool isCompleted =
                IsLabCompleted(
                    labId,
                    experiments,
                    progressRows);

            DateTime? completionDate =
                enrollment.CompletedAt;

            if (completionDate == null && isCompleted)
            {
                completionDate =
                    progressRows
                        .Where(IsExperimentCompleted)
                        .Max(status =>
                            status.UpdatedAt ?? status.CreatedAt);
            }

            return new CompletionCertificateDto
            {
                UserId = user.UserId,
                StudentName = user.StudentName,
                Usn = user.Usn ?? string.Empty,
                College =
                    user.College ??
                    user.Organization ??
                    string.Empty,
                Department = user.Department,
                LabId = lab.LabId,
                LabName = lab.LabName,
                CourseCode = lab.CourseCode,
                TotalExperiments = experiments.Count,
                CompletedExperiments = completedExperiments,
                TotalAssignments = totalAssignments,
                CompletedAssignments = completedAssignments,
                TotalQuizScore = totalQuizScore,
                TotalQuizMaxMarks = totalQuizMaxMarks,
                QuizScoreOutOf10 = quizScoreOutOf10,
                CompletionDate = completionDate,
                IsCompleted = isCompleted,
                CertificateId =
                    $"ATME-CSE-VLAB-{DateTime.UtcNow.Year}-{user.UserId:D4}-{lab.LabId:D2}"
            };
        }

        //Helper Method

        private static string GetAssignmentStatus(
    LabStatus status)
        {
            int completedAssignments = 0;

            if (status.Assignment1) completedAssignments++;
            if (status.Assignment2) completedAssignments++;
            if (status.Assignment3) completedAssignments++;
            if (status.Assignment4) completedAssignments++;
            if (status.Assignment5) completedAssignments++;

            if (completedAssignments == 5)
            {
                return "Completed";
            }

            if (completedAssignments > 0)
            {
                return $"{completedAssignments}/5 Completed";
            }

            return "Pending";
        }//Helper method ends

        private static int GetCompletedAssignmentCount(
            LabStatus status)
        {
            int completedAssignments = 0;

            if (status.Assignment1) completedAssignments++;
            if (status.Assignment2) completedAssignments++;
            if (status.Assignment3) completedAssignments++;
            if (status.Assignment4) completedAssignments++;
            if (status.Assignment5) completedAssignments++;

            return completedAssignments;
        }

        private static string GetQuizReportStatus(
            LabStatus? status)
        {
            if (status == null || !status.Quiz)
            {
                return "Pending";
            }

            if (status.QuizScore.HasValue &&
                status.QuizMaxMarks.HasValue)
            {
                return $"{status.QuizScore}/{status.QuizMaxMarks}";
            }

            return "Completed";
        }

        private static bool IsExperimentCompleted(
            LabStatus? status)
        {
            return status != null &&
                status.Execution &&
                status.Quiz &&
                status.Assignment1 &&
                status.Assignment2 &&
                status.Assignment3 &&
                status.Assignment4 &&
                status.Assignment5;
        }

        private static void RefreshCompletionStatus(
            LabStatus status)
        {
            status.CompletionStatus =
                IsExperimentCompleted(status)
                    ? "Completed"
                    : "Not Completed";
        }

        private static bool IsLabCompleted(
            int labId,
            List<Experiment> experiments,
            List<LabStatus> progressRows)
        {
            List<int> experimentIds =
                experiments
                    .Where(experiment => experiment.LabId == labId)
                    .Select(experiment => experiment.ExperimentId)
                    .ToList();

            if (experimentIds.Count == 0)
            {
                return false;
            }

            return experimentIds.All(experimentId =>
                progressRows.Any(status =>
                    status.LabId == labId &&
                    status.ExperimentId == experimentId &&
                    IsExperimentCompleted(status)));
        }

        //REGISTER FOR THE SELECTED LAB

        public async Task<bool> RegisterLabAsync(
    int userId,
    int labId)
        {
            bool validUser = await context.Registrations
                .AnyAsync(user =>
                    user.UserId == userId &&
                    user.RegistrationStatus == "Approved" &&
                    user.IsActive);

            if (!validUser)
            {
                throw new InvalidOperationException(
                    "User is not approved or active.");
            }

            Lab? lab = await context.Labs
                .FirstOrDefaultAsync(item =>
                    item.LabId == labId &&
                    item.IsActive &&
                    item.IsAvailable);

            if (lab == null)
            {
                throw new InvalidOperationException(
                    "The selected lab is currently unavailable.");
            }

            bool alreadyRegistered =
                await context.LabEnrollments.AnyAsync(enrollment =>
                    enrollment.UserId == userId &&
                    enrollment.LabId == labId &&
                    enrollment.EnrollmentStatus != "Cancelled");

            if (alreadyRegistered)
            {
                throw new InvalidOperationException(
                    "User is already registered for this lab.");
            }

            List<int> experimentIds =
                await context.Experiments
                    .Where(experiment =>
                        experiment.LabId == labId &&
                        experiment.IsActive)
                    .Select(experiment => experiment.ExperimentId)
                    .ToListAsync();

            if (experimentIds.Count == 0)
            {
                throw new InvalidOperationException(
                    "No active experiments are available for this lab.");
            }

            await using var transaction =
                await context.Database.BeginTransactionAsync();

            try
            {
                LabEnrollment? cancelledEnrollment =
                    await context.LabEnrollments
                        .FirstOrDefaultAsync(enrollment =>
                            enrollment.UserId == userId &&
                            enrollment.LabId == labId &&
                            enrollment.EnrollmentStatus == "Cancelled");

                if (cancelledEnrollment != null)
                {
                    cancelledEnrollment.EnrollmentStatus = "Enrolled";
                    cancelledEnrollment.EnrolledAt = DateTime.UtcNow;
                    cancelledEnrollment.CompletedAt = null;
                }
                else
                {
                    context.LabEnrollments.Add(
                        new LabEnrollment
                        {
                            UserId = userId,
                            LabId = labId,
                            EnrollmentStatus = "Enrolled",
                            EnrolledAt = DateTime.UtcNow
                        });
                }

                List<int> existingExperimentIds =
                    await context.LabStatuses
                        .Where(status =>
                            status.UserId == userId &&
                            status.LabId == labId)
                        .Select(status => status.ExperimentId)
                        .ToListAsync();

                foreach (int experimentId in experimentIds)
                {
                    if (existingExperimentIds.Contains(experimentId))
                    {
                        continue;
                    }

                    context.LabStatuses.Add(
                        new LabStatus
                        {
                            UserId = userId,
                            LabId = labId,
                            ExperimentId = experimentId,
                            Theory = false,
                            Execution = false,
                            Quiz = false,
                            Assignment1 = false,
                            Assignment2 = false,
                            Assignment3 = false,
                            Assignment4 = false,
                            Assignment5 = false,
                            CreatedAt = DateTime.UtcNow
                        });
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        //When user logged-in
        public async Task<long> StartUserSessionAsync(int userId)
        {
            bool validUser = await context.Registrations
                .AnyAsync(user =>
                    user.UserId == userId &&
                    user.RegistrationStatus == "Approved" &&
                    user.IsActive);

            if (!validUser)
            {
                throw new InvalidOperationException(
                    "User is not approved or active.");
            }

            DateTime currentTime = DateTime.UtcNow;

            var session = new UserActivitySession
            {
                UserId = userId,
                LoginTime = currentTime,
                LastActivityTime = currentTime,
                LastHeartbeatTime = currentTime,
                ActiveSeconds = 0,
                IsSessionOpen = true
            };

            context.UserActivitySessions.Add(session);

            await context.SaveChangesAsync();

            return session.ActivitySessionId;
        }


        //When user logged-out
        public async Task<bool> EndUserSessionAsync(long activitySessionId)
        {
            UserActivitySession? session =
                await context.UserActivitySessions
                    .FirstOrDefaultAsync(item =>
                        item.ActivitySessionId == activitySessionId &&
                        item.IsSessionOpen);

            if (session == null)
            {
                return false;
            }

            session.LogoutTime = DateTime.UtcNow;
            session.IsSessionOpen = false;

            await context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> LogoutAsync(int userId)
        {
            UserActivitySession? session =
                await context.UserActivitySessions
                    .Where(item =>
                        item.UserId == userId &&
                        item.IsSessionOpen)
                    .OrderByDescending(item =>
                        item.LoginTime)
                    .FirstOrDefaultAsync();

            if (session == null)
            {
                return false;
            }

            session.LogoutTime = DateTime.UtcNow;
            session.IsSessionOpen = false;

            await context.SaveChangesAsync();

            return true;
        }

        //Log History

        public async Task<UserLogHistoryDto>
    GetUserLogHistoryAsync(int userId)
        {
            List<UserActivitySession> sessionRecords =
                await context.UserActivitySessions
                    .AsNoTracking()
                    .Where(session => session.UserId == userId)
                    .OrderByDescending(session => session.LoginTime)
                    .ToListAsync();

            List<UserLogHistoryItemDto> sessions =
                sessionRecords.Select(session =>
                {
                    DateTime endTime =
                        session.LogoutTime ?? DateTime.UtcNow;

                    int durationSeconds = Math.Max(
                        0,
                        (int)(endTime - session.LoginTime)
                            .TotalSeconds);

                    return new UserLogHistoryItemDto
                    {
                        ActivitySessionId =
                            session.ActivitySessionId,

                        LoginTime = session.LoginTime,
                        LogoutTime = session.LogoutTime,
                        DurationSeconds = durationSeconds,
                        IsSessionOpen = session.IsSessionOpen
                    };
                }).ToList();

            return new UserLogHistoryDto
            {
                TotalDurationSeconds =
                    sessions.Sum(item => item.DurationSeconds),

                Sessions = sessions
            };
        }


        //Adding methods to update the status of Quiz, Assignments and Execution

        public async Task<bool> UpdateExecutionStatusAsync(
    int userId,
    int labId,
    int experimentId)
        {
            LabStatus? status =
                await context.LabStatuses.FirstOrDefaultAsync(item =>
                    item.UserId == userId &&
                    item.LabId == labId &&
                    item.ExperimentId == experimentId);

            if (status == null)
            {
                return false;
            }

            status.Execution = true;
            status.UpdatedAt = DateTime.UtcNow;
            RefreshCompletionStatus(status);

            await context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateQuizStatusAsync(
    int userId,
    int labId,
    int experimentId,
    int quizScore,
    int quizMaxMarks)
        {
            LabStatus? status =
                await context.LabStatuses.FirstOrDefaultAsync(item =>
                    item.UserId == userId &&
                    item.LabId == labId &&
                    item.ExperimentId == experimentId);

            if (status == null ||
                status.Quiz ||
                status.QuizScore.HasValue ||
                status.QuizSubmittedAt.HasValue)
            {
                return false;
            }

            status.Quiz = true;
            status.QuizScore = quizScore;
            status.QuizMaxMarks = quizMaxMarks;
            status.QuizSubmittedAt = DateTime.UtcNow;
            status.UpdatedAt = DateTime.UtcNow;
            RefreshCompletionStatus(status);

            await context.SaveChangesAsync();

            return true;
        }

        public async Task<QuizAttemptDto?> GetQuizAttemptAsync(
            int userId,
            int labId,
            int experimentId)
        {
            return await context.LabStatuses
                .AsNoTracking()
                .Where(status =>
                    status.UserId == userId &&
                    status.LabId == labId &&
                    status.ExperimentId == experimentId)
                .Select(status => new QuizAttemptDto
                {
                    HasAttempted =
                        status.Quiz ||
                        status.QuizScore.HasValue ||
                        status.QuizSubmittedAt.HasValue,
                    QuizScore = status.QuizScore,
                    QuizMaxMarks = status.QuizMaxMarks,
                    SubmittedAt = status.QuizSubmittedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateAssignmentStatusAsync(
    int userId,
    int labId,
    int experimentId,
    int assignmentId)
        {
            LabStatus? status =
                await context.LabStatuses.FirstOrDefaultAsync(item =>
                    item.UserId == userId &&
                    item.LabId == labId &&
                    item.ExperimentId == experimentId);

            if (status == null)
            {
                return false;
            }

            switch (assignmentId)
            {
                case 1:
                    status.Assignment1 = true;
                    break;

                case 2:
                    status.Assignment2 = true;
                    break;

                case 3:
                    status.Assignment3 = true;
                    break;

                case 4:
                    status.Assignment4 = true;
                    break;

                case 5:
                    status.Assignment5 = true;
                    break;

                default:
                    throw new ArgumentException(
                        "AssignmentId must be between 1 and 5.");
            }

            status.UpdatedAt = DateTime.UtcNow;
            RefreshCompletionStatus(status);

            await context.SaveChangesAsync();

            return true;
        }

        // Admin Dashboard - College Wise Registration Count
        public async Task<List<AdminSummaryDto>>
            GetCollegeWiseRegistrationsAsync()
        {
            return await context.Registrations
                .AsNoTracking()
                .Where(user => user.College != null)
                .GroupBy(user => user.College)
                .Select(group => new AdminSummaryDto
                {
                    Name = group.Key,
                    Registered = group.Count()
                })
                .OrderByDescending(item => item.Registered)
                .ThenBy(item => item.Name)
                .ToListAsync();
        }

        // Admin Dashboard - Department Wise Registration Count
        public async Task<List<AdminSummaryDto>>
            GetDepartmentWiseRegistrationsAsync()
        {
            return await context.Registrations
                .AsNoTracking()
                .GroupBy(user => user.Department)
                .Select(group => new AdminSummaryDto
                {
                    Name = group.Key,
                    Registered = group.Count()
                })
                .OrderByDescending(item => item.Registered)
                .ThenBy(item => item.Name)
                .ToListAsync();
        }

        // Admin Dashboard - ATMECE Department Wise Registration Count
        public async Task<List<AdminSummaryDto>>
            GetAtmeceDepartmentWiseRegistrationsAsync()
        {
            return await context.Registrations
                .AsNoTracking()
                .Where(user =>
                    user.College != null &&
                    user.College.Contains("ATME"))
                .GroupBy(user => user.Department)
                .Select(group => new AdminSummaryDto
                {
                    Name = group.Key,
                    Registered = group.Count()
                })
                .OrderByDescending(item => item.Registered)
                .ThenBy(item => item.Name)
                .ToListAsync();
        }

        // Admin Dashboard - Lab Completion Status
        public async Task<List<AdminLabCompletionDto>>
            GetLabCompletionStatusAsync()
        {
            List<Lab> labs =
                await context.Labs
                    .AsNoTracking()
                    .Where(lab => lab.IsActive)
                    .OrderBy(lab => lab.Semester)
                    .ThenBy(lab => lab.LabName)
                    .ToListAsync();

            List<LabEnrollment> enrollments =
                await context.LabEnrollments
                    .AsNoTracking()
                    .Where(enrollment =>
                        enrollment.EnrollmentStatus != "Cancelled")
                    .ToListAsync();

            List<Experiment> experiments =
                await context.Experiments
                    .AsNoTracking()
                    .Where(experiment => experiment.IsActive)
                    .ToListAsync();

            List<LabStatus> labStatuses =
                await context.LabStatuses
                    .AsNoTracking()
                    .ToListAsync();

            var completionStatus =
                new List<AdminLabCompletionDto>();

            foreach (Lab lab in labs)
            {
                List<LabEnrollment> labEnrollments =
                    enrollments
                        .Where(enrollment =>
                            enrollment.LabId == lab.LabId)
                        .ToList();

                int experimentCount =
                    experiments.Count(experiment =>
                        experiment.LabId == lab.LabId);

                int completedCount = 0;

                foreach (LabEnrollment enrollment in labEnrollments)
                {
                    int completedExperimentCount =
                        labStatuses.Count(status =>
                            status.UserId == enrollment.UserId &&
                            status.LabId == lab.LabId &&
                            IsExperimentCompleted(status));

                    if (
                        experimentCount > 0 &&
                        completedExperimentCount == experimentCount)
                    {
                        completedCount++;
                    }
                }

                completionStatus.Add(
                    new AdminLabCompletionDto
                    {
                        LabId = lab.LabId,
                        LabName = lab.LabName,
                        Registered = labEnrollments.Count,
                        Completed = completedCount
                    });
            }

            return completionStatus;
        }

        // Admin Dashboard - User Registration Table
        public async Task<List<AdminUserRegistrationDto>>
            GetUserRegistrationsForAdminAsync()
        {
            return await context.Registrations
                .AsNoTracking()
                .Include(user => user.Role)
                .Where(user => user.Role.RoleName == "Student")
                .OrderByDescending(user => user.CreatedAt)
                .Select(user => new AdminUserRegistrationDto
                {
                    UserId = user.UserId,
                    UserType = user.UserType,
                    StudentName = user.StudentName,
                    Usn = user.Usn ?? string.Empty,
                    College = user.College ?? string.Empty,
                    Organization = user.Organization ?? string.Empty,
                    Department = user.Department,
                    Semester = user.Semester,
                    Designation = user.Designation ?? string.Empty,
                    EmailId = user.EmailId,
                    RegistrationStatus = user.RegistrationStatus,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    ApprovedAt = user.ApprovedAt
                })
                .ToListAsync();
        }

        public async Task<bool> GetAutoApproveRegistrationsAsync()
        {
            string? value =
                await context.SystemSettings
                    .AsNoTracking()
                    .Where(setting =>
                        setting.SettingKey ==
                        AutoApproveRegistrationsKey)
                    .Select(setting => setting.SettingValue)
                    .FirstOrDefaultAsync();

            return bool.TryParse(value, out bool enabled) &&
                enabled;
        }

        public async Task SetAutoApproveRegistrationsAsync(
            bool enabled)
        {
            SystemSetting? setting =
                await context.SystemSettings
                    .FirstOrDefaultAsync(item =>
                        item.SettingKey ==
                        AutoApproveRegistrationsKey);

            if (setting == null)
            {
                setting = new SystemSetting
                {
                    SettingKey =
                        AutoApproveRegistrationsKey,
                    SettingValue =
                        enabled.ToString().ToLowerInvariant(),
                    UpdatedAt = DateTime.UtcNow
                };

                context.SystemSettings.Add(setting);
            }
            else
            {
                setting.SettingValue =
                    enabled.ToString().ToLowerInvariant();
                setting.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
        }

        public async Task<UserRequest> CreateUserRequestAsync(
            int userId,
            string requestType,
            string description)
        {
            string type = requestType.Trim();

            if (
                type != "Query" &&
                type != "Feedback" &&
                type != "Defects/Bugs")
            {
                throw new ArgumentException(
                    "Request type must be Query, Feedback, or Defects/Bugs.");
            }

            Registration? user =
                await context.Registrations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(item =>
                        item.UserId == userId &&
                        item.IsActive);

            if (user == null)
            {
                throw new InvalidOperationException(
                    "The active user was not found.");
            }

            var request = new UserRequest
            {
                UserId = user.UserId,
                EmailId = user.EmailId,
                RequestType = type,
                Description = description.Trim(),
                SubmittedAt = DateTime.UtcNow,
                Status = "Open"
            };

            context.UserRequests.Add(request);
            await context.SaveChangesAsync();

            return request;
        }

        public async Task<List<AdminUserRequestDto>>
            GetUserRequestsForAdminAsync()
        {
            return await context.UserRequests
                .AsNoTracking()
                .OrderBy(request =>
                    request.Status == "Closed")
                .ThenByDescending(request =>
                    request.SubmittedAt)
                .Select(request => new AdminUserRequestDto
                {
                    RequestId = request.RequestId,
                    UserId = request.UserId,
                    EmailId = request.EmailId,
                    RequestType = request.RequestType,
                    Description = request.Description,
                    SubmittedAt = request.SubmittedAt,
                    Status = request.Status,
                    ClosedAt = request.ClosedAt,
                    Remarks = request.Remarks ?? string.Empty
                })
                .ToListAsync();
        }

        public async Task<bool> UpdateUserRequestAsync(
            long requestId,
            string status,
            string remarks)
        {
            string normalizedStatus =
                status.Trim();

            if (
                normalizedStatus != "Open" &&
                normalizedStatus != "Closed")
            {
                throw new ArgumentException(
                    "Request status must be Open or Closed.");
            }

            string normalizedRemarks =
                remarks?.Trim() ?? string.Empty;

            if (
                normalizedStatus == "Closed" &&
                string.IsNullOrWhiteSpace(normalizedRemarks))
            {
                throw new ArgumentException(
                    "Remarks are required before closing a request.");
            }

            UserRequest? request =
                await context.UserRequests
                    .FirstOrDefaultAsync(item =>
                        item.RequestId == requestId);

            if (request == null)
            {
                return false;
            }

            request.Status = normalizedStatus;
            request.Remarks =
                string.IsNullOrWhiteSpace(normalizedRemarks)
                    ? null
                    : normalizedRemarks;
            request.ClosedAt =
                normalizedStatus == "Closed"
                    ? request.ClosedAt ?? DateTime.UtcNow
                    : null;
            request.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return true;
        }

        // Admin Dashboard - Complete Data
        public async Task<AdminDashboardDto>
            GetAdminDashboardAsync()
        {
            return new AdminDashboardDto
            {
                AutoApproveRegistrations =
                    await GetAutoApproveRegistrationsAsync(),

                CollegeWise =
                    await GetCollegeWiseRegistrationsAsync(),

                DepartmentWise =
                    await GetDepartmentWiseRegistrationsAsync(),

                AtmeceDepartmentWise =
                    await GetAtmeceDepartmentWiseRegistrationsAsync(),

                CompletionStatus =
                    await GetLabCompletionStatusAsync(),

                UserRegistrations =
                    await GetUserRegistrationsForAdminAsync(),

                UserRequests =
                    await GetUserRequestsForAdminAsync()
            };
        }

        // Admin Dashboard - Update One User Registration Status
        public async Task<string?> GetUserEmailAsync(
            int userId)
        {
            return await context.Registrations
                .AsNoTracking()
                .Where(user => user.UserId == userId)
                .Select(user => user.EmailId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<string>> GetUserEmailsAsync(
            List<int> userIds)
        {
            if (userIds == null || userIds.Count == 0)
            {
                return new List<string>();
            }

            return await context.Registrations
                .AsNoTracking()
                .Where(user => userIds.Contains(user.UserId))
                .Select(user => user.EmailId)
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .ToListAsync();
        }

        public async Task<bool> UpdateUserRegistrationStatusAsync(
            int userId,
            string registrationStatus,
            int approvedBy)
        {
            string status =
                registrationStatus.Trim();

            if (
                status != "Approved" &&
                status != "Rejected")
            {
                throw new ArgumentException(
                    "Registration status must be Approved or Rejected.");
            }

            Registration? user =
                await context.Registrations
                    .FirstOrDefaultAsync(registration =>
                        registration.UserId == userId);

            if (user == null)
            {
                return false;
            }

            user.RegistrationStatus = status;
            user.IsActive = status == "Approved";
            user.ApprovedBy = approvedBy;
            user.ApprovedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return true;
        }

        // Admin Dashboard - Update Selected User Registration Status
        public async Task<int> UpdateSelectedUserRegistrationStatusAsync(
            List<int> userIds,
            string registrationStatus,
            int approvedBy)
        {
            string status =
                registrationStatus.Trim();

            if (
                status != "Approved" &&
                status != "Rejected")
            {
                throw new ArgumentException(
                    "Registration status must be Approved or Rejected.");
            }

            if (userIds == null || userIds.Count == 0)
            {
                return 0;
            }

            DateTime currentTime =
                DateTime.UtcNow;

            List<Registration> users =
                await context.Registrations
                    .Where(user => userIds.Contains(user.UserId))
                    .ToListAsync();

            foreach (Registration user in users)
            {
                user.RegistrationStatus = status;
                user.IsActive = status == "Approved";
                user.ApprovedBy = approvedBy;
                user.ApprovedAt = currentTime;
                user.UpdatedAt = currentTime;
            }

            await context.SaveChangesAsync();

            return users.Count;
        }

        // Admin Dashboard - Approve One User
        public async Task<bool> ApproveUserRegistrationAsync(
            int userId,
            int approvedBy)
        {
            return await UpdateUserRegistrationStatusAsync(
                userId,
                "Approved",
                approvedBy);
        }

        // Admin Dashboard - Reject One User
        public async Task<bool> RejectUserRegistrationAsync(
            int userId,
            int approvedBy)
        {
            return await UpdateUserRegistrationStatusAsync(
                userId,
                "Rejected",
                approvedBy);
        }

        // Admin Dashboard - Approve Selected Users
        public async Task<int> ApproveSelectedUsersAsync(
            List<int> userIds,
            int approvedBy)
        {
            return await UpdateSelectedUserRegistrationStatusAsync(
                userIds,
                "Approved",
                approvedBy);
        }

        // Admin Dashboard - Reject Selected Users
        public async Task<int> RejectSelectedUsersAsync(
            List<int> userIds,
            int approvedBy)
        {
            return await UpdateSelectedUserRegistrationStatusAsync(
                userIds,
                "Rejected",
                approvedBy);
        }

        // Admin Dashboard - Delete User Registration
        public async Task<bool> DeleteUserRegistrationAsync(
            int userId)
        {
            Registration? user =
                await context.Registrations
                    .Include(item => item.Role)
                    .FirstOrDefaultAsync(item =>
                        item.UserId == userId);

            if (user == null)
            {
                return false;
            }

            if (user.Role?.RoleName != "Student")
            {
                throw new InvalidOperationException(
                    "Administrator accounts cannot be deleted here.");
            }

            await using var transaction =
                await context.Database
                    .BeginTransactionAsync();

            try
            {
                await context.Registrations
                    .Where(item =>
                        item.ApprovedBy == userId)
                    .ExecuteUpdateAsync(setters =>
                        setters
                            .SetProperty(
                                item => item.ApprovedBy,
                                (int?)null)
                            .SetProperty(
                                item => item.ApprovedAt,
                                (DateTime?)null));

                await context.UserRequests
                    .Where(item =>
                        item.UserId == userId)
                    .ExecuteDeleteAsync();

                await context.UserActivitySessions
                    .Where(item =>
                        item.UserId == userId)
                    .ExecuteDeleteAsync();

                await context.LabStatuses
                    .Where(item =>
                        item.UserId == userId)
                    .ExecuteDeleteAsync();

                await context.LabEnrollments
                    .Where(item =>
                        item.UserId == userId)
                    .ExecuteDeleteAsync();

                context.Registrations.Remove(user);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }









    }
}
