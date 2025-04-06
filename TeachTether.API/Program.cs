using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TeachTether.Infrastructure.Persistence.Data;
using TeachTether.Application.Validators;
using FluentValidation.AspNetCore;
using FluentValidation;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Infrastructure.Persistence.Repositories;
using TeachTether.Application.Mapping;
using TeachTether.Application.Settings;
using TeachTether.Application.Services;
using Microsoft.AspNetCore.Mvc;
using TeachTether.API.Errors;
using TeachTether.API.Middleware;
using Microsoft.AspNetCore.Authorization;
using TeachTether.Application.Authorization.Handlers;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Domain.Entities;
using TeachTether.API.Authorization;
using System.Diagnostics;
using TeachTether.Application.Common.Exceptions;

namespace TeachTether.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                    ValidationProblemDetailsFactory.Create(context.HttpContext, context.ModelState);
            });


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
                        System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!))
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("IsSchoolOwner", polBuilder =>
                    polBuilder.Requirements.Add(new UserTypeRequirement(UserType.SchoolOwner)));
            });

            builder.Services.AddFluentValidationAutoValidation()
                .AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
            builder.Services.AddAutoMapper(typeof(UserMappingProfile));

            builder.Services.AddScoped<IAuthorizationHandler, UserTypeHandler>();
            builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();


            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(nameof(JwtSettings)));

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

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ISchoolService, SchoolService>();

            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

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
}
