using System.Security.Claims;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TeachTether.API.Errors;
using TeachTether.API.Hubs;
using TeachTether.API.Middleware;
using TeachTether.Application.Authorization.Handlers;
using TeachTether.Application.Common.Interfaces;
using TeachTether.Application.Common.Services;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;
using TeachTether.Application.Mapping;
using TeachTether.Application.Services;
using TeachTether.Application.Services.DeletionHelpers;
using TeachTether.Application.Settings;
using TeachTether.Application.Validators;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;
using TeachTether.Infrastructure.Persistence.FileStorage.Common;
using TeachTether.Infrastructure.Persistence.FileStorage.Repositories;
using TeachTether.Infrastructure.Persistence.Repositories;

namespace TeachTether.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddFluentValidationAutoValidation()
            .AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend",
                policy => policy.WithOrigins("http://localhost:5173")
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"))
        );

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    ValidAudience = builder.Configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!))
                };
            });

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("RequireSchoolOwner", polBuilder =>
                polBuilder.RequireClaim(ClaimTypes.Role, UserType.SchoolOwner.ToString())
            )
            .AddPolicy("RequireTeacher", polBuilder =>
                polBuilder.RequireClaim(ClaimTypes.Role, UserType.Teacher.ToString())
            )
            .AddPolicy("RequireGuardian", polBuilder =>
                polBuilder.RequireClaim(ClaimTypes.Role, UserType.Guardian.ToString())
            )
            .AddPolicy("RequireSchoolOwnerAdminOrTeacher", policyBuilder =>
                policyBuilder.RequireAssertion(context =>
                    context.User.HasClaim(c =>
                        c.Type == ClaimTypes.Role &&
                        (
                            c.Value == UserType.SchoolOwner.ToString() ||
                            c.Value == UserType.SchoolAdmin.ToString() ||
                            c.Value == UserType.Teacher.ToString()
                        )
                    )
                )
            )
            .AddPolicy("RequireSchoolOwnerOrAdmin", policyBuilder =>
                policyBuilder.RequireAssertion(context =>
                    context.User.HasClaim(c =>
                        c.Type == ClaimTypes.Role &&
                        (c.Value == UserType.SchoolOwner.ToString() || c.Value == UserType.SchoolAdmin.ToString())
                    )
                )
            )
            .AddPolicy("RequireTeacherOrGuardian", policyBuilder =>
                policyBuilder.RequireAssertion(context =>
                    context.User.HasClaim(c =>
                        c.Type == ClaimTypes.Role &&
                        (c.Value == UserType.Teacher.ToString() || c.Value == UserType.Guardian.ToString())
                    )
                )
            );

        builder.Services.AddSignalR();

        builder.Services.AddAutoMapper(typeof(UserMappingProfile));

        builder.Services.AddScoped<IAuthorizationHandler, CanCreateAnnouncementHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanCreateMessageHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanCreateStudentRecordsHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanCreateThreadHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanDeleteThreadHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanManageAnnouncementHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanManageSchoolHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanManageSchoolEntitiesHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanModifyStudentRecordsHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanViewAnnouncementHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanViewClassAssignmentsHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanViewClassGroupHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanViewClassGroupStudentsHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanViewClassGroupSubjectsHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanViewGuardianHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanViewGuardianStudentsHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanViewRecordsOfStudentHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanViewSchoolHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanViewStudentGuardiansHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanViewStudentHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanViewStudentRecordHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanViewSubjectHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanViewTeacherHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CanViewThreadHandler>();


        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(nameof(JwtSettings)));
        builder.Services.Configure<FileStorageOptions>(opt =>
            opt.RootPath = builder.Environment.WebRootPath);


        builder.Services.AddScoped<IAnnouncementClassGroupRepository, AnnouncementClassGroupRepository>();
        builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
        builder.Services.AddScoped<IClassAssignmentRepository, ClassAssignmentRepository>();
        builder.Services.AddScoped<IClassGroupRepository, ClassGroupRepository>();
        builder.Services.AddScoped<IClassGroupStudentRepository, ClassGroupStudentRepository>();
        builder.Services.AddScoped<IClassGroupSubjectRepository, ClassGroupSubjectRepository>();
        builder.Services.AddScoped<IGuardianRepository, GuardianRepository>();
        builder.Services.AddScoped<IGuardianStudentRepository, GuardianStudentRepository>();
        builder.Services.AddScoped<IMessageAttachmentRepository, MessageAttachmentRepository>();
        builder.Services.AddScoped<IMessageRepository, MessageRepository>();
        builder.Services.AddScoped<IMessageThreadRepository, MessageThreadRepository>();
        builder.Services.AddScoped<ISchoolAdminRepository, SchoolAdminRepository>();
        builder.Services.AddScoped<ISchoolOwnerRepository, SchoolOwnerRepository>();
        builder.Services.AddScoped<ISchoolRepository, SchoolRepository>();
        builder.Services.AddScoped<IStudentAttendanceRepository, StudentAttendanceRepository>();
        builder.Services.AddScoped<IStudentBehaviorRepository, StudentBehaviorRepository>();
        builder.Services.AddScoped<IStudentGradeRepository, StudentGradeRepository>();
        builder.Services.AddScoped<IStudentRepository, StudentRepository>();
        builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
        builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IFileStorageRepository, FileStorageRepository>();

        builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<ISchoolService, SchoolService>();
        builder.Services.AddScoped<IStudentService, StudentService>();
        builder.Services.AddScoped<ISchoolAdminService, SchoolAdminService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ITeacherService, TeacherService>();
        builder.Services.AddScoped<IGuardianService, GuardianService>();
        builder.Services.AddScoped<IClassAssignmentService, ClassAssignmentService>();
        builder.Services.AddScoped<IClassGroupService, ClassGroupService>();
        builder.Services.AddScoped<IClassGroupStudentService, ClassGroupStudentService>();
        builder.Services.AddScoped<IClassGroupSubjectService, ClassGroupSubjectService>();
        builder.Services.AddScoped<IGuardianStudentService, GuardianStudentService>();
        builder.Services.AddScoped<ISubjectService, SubjectService>();
        builder.Services.AddScoped<IFileStorageService, FileStorageService>();
        builder.Services.AddScoped<IMessageAttachmentService, MessageAttachmentService>();
        builder.Services.AddScoped<IMessageService, MessageService>();
        builder.Services.AddScoped<IMessageThreadService, MessageThreadService>();
        builder.Services.AddScoped<IStudentAttendanceService, StudentAttendanceService>();
        builder.Services.AddScoped<IStudentGradeService, StudentGradeService>();
        builder.Services.AddScoped<IStudentBehaviorService, StudentBehaviorService>();
        builder.Services.AddScoped<IOwnerService, OwnerService>();
        builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
        builder.Services.AddScoped<IStudentRecordsService, StudentRecordsService>();

        builder.Services.AddScoped<IAnnouncementDeletionHelper, AnnouncementDeletionHelper>();
        builder.Services.AddScoped<IClassGroupDeletionHelper, ClassGroupDeletionHelper>();
        builder.Services.AddScoped<IClassGroupsSubjectDeletionHelper, ClassGroupsSubjectDeletionHelper>();
        builder.Services.AddScoped<IGuardianDeletionHelper, GuardianDeletionHelper>();
        builder.Services.AddScoped<IMessageDeletionHelper, MessageDeletionHelper>();
        builder.Services.AddScoped<IMessageThreadDeletionHelper, MessageThreadDeletionHelper>();
        builder.Services.AddScoped<ISchoolAdminDeletionHelper, SchoolAdminDeletionHelper>();
        builder.Services.AddScoped<ISchoolDeletionHelper, SchoolDeletionHelper>();
        builder.Services.AddScoped<IStudentDeletionHelper, StudentDeletionHelper>();
        builder.Services.AddScoped<ISubjectDeletionHelper, SubjectDeletionHelper>();
        builder.Services.AddScoped<ITeacherDeletionHelper, TeacherDeletionHelper>();


        builder.Services.AddScoped<ICredentialsGenerator, CredentialsGenerator>();
        builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();


        var app = builder.Build();

        app.MapHub<ChatHub>("/hubs/chat");

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseStatusCodePages(async context =>
        {
            var response = context.HttpContext.Response;
            var error = ApiErrorResponseFactory.FromContext(context.HttpContext, response.StatusCode);
            response.ContentType = "application/json";
            await response.WriteAsJsonAsync(error);
        });


        app.UseCors("AllowFrontend");

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();


        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}