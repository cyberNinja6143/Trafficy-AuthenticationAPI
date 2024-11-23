using AuthenticationAPI.Models.ConfigModels;
using AuthenticationAPI.Services.EmailService;
using AuthenticationAPI.Services.HouseKeeping;
using AuthenticationAPI.Services.PasswordHashers;
using AuthenticationAPI.Services.TokenGenerators;
using AuthenticationAPI.Services.TokenGenerators.JWTClaimsBinder;
using AuthenticationAPI.Services.UserRepositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var configuration = builder.Configuration;
//Objects to share with other classes
AuthenticationConfiguration authenticationConfiguration = new AuthenticationConfiguration();
EmailCred emailCred = new EmailCred();

//bind objects from appsetting.json to the objects created above
builder.Configuration.GetSection("Authentication").Bind(authenticationConfiguration);
builder.Configuration.GetSection("EmailCredentials").Bind(emailCred);

// Configure services
string connectionString = builder.Configuration.GetConnectionString("sqlite");
builder.Services.AddControllers();
builder.Services.AddTransient<Cleaner>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserRepository, SqliteUserRepository>();
builder.Services.AddDbContextFactory<SecondAuthenticationDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddDbContext<AuthenticationDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddSingleton<AccessTokenGenerator>();
builder.Services.AddSingleton<TokenValidator>();
builder.Services.AddSingleton<JWTClaimUserBinder>();
builder.Services.AddSingleton(emailCred);
builder.Services.AddSingleton(authenticationConfiguration);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters()
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationConfiguration.AccessTokenSecret)),
        ValidIssuer = authenticationConfiguration.Issuer,
        ValidAudience = authenticationConfiguration.Audience,
        ValidateIssuer = true,
        ValidateAudience = true,
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

//DB Migration
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();
    context.Database.Migrate();
}

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Authentication}/{action=Index}/{id?}");
app.MapControllers();

app.Run();
