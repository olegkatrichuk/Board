﻿using System;
using System.Globalization;
using Board.Models;
using Board.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MyBoard.Security;

namespace MyBoard
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }
    
    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddLocalization(options => options.ResourcesPath = "Resources");

      services.AddMvc()
        .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
        .AddDataAnnotationsLocalization();

      services.Configure<RequestLocalizationOptions>(options =>
      {
        var supportedCultures = new[]
        {
          new CultureInfo("ru"),
          new CultureInfo("uk"),
        };

        options.DefaultRequestCulture = new RequestCulture("ru");
        options.SupportedCultures = supportedCultures;
        options.SupportedUICultures = supportedCultures;

      });

      

      services.Configure<CookiePolicyOptions>(options =>
      {
        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
        options.CheckConsentNeeded = context => true;
        options.MinimumSameSitePolicy = SameSiteMode.None;
      });

      services.AddIdentity<IdentityUser, IdentityRole>(option =>
        {
          option.Password.RequiredLength = 7;

          option.SignIn.RequireConfirmedEmail = true;

          option.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";

          option.Lockout.MaxFailedAccessAttempts = 5;
          option.Lockout.DefaultLockoutTimeSpan=TimeSpan.FromMinutes(15);

        }).AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders()
        .AddTokenProvider<CustomEmailConfirmationTokenProvider<IdentityUser>>("CustomEmailConfirmation");

      services.AddDbContext<AppDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

      services.AddAuthentication()
        .AddGoogle(options =>
      {
        IConfigurationSection googleAuthNSection =
          Configuration.GetSection("Authentication:Google");

        options.ClientId = "secret";
        options.ClientSecret = "secret";
      })
        .AddFacebook(options =>
        {
          options.AppId = "secret";
          options.AppSecret = "secret";
        });

      services.ConfigureApplicationCookie(options =>
      {
        options.AccessDeniedPath = new PathString("/Administration/AccessDenied");
      });

      services.AddAuthorization(options =>
      {
        options.AddPolicy("DeleteRolePolicy", policy => policy.RequireClaim("Delete Role"));

        options.AddPolicy("EditRolePolicy",
          policy => policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement()));

        options.AddPolicy("AdminRolePolicy", policy => policy.RequireRole("Admin"));
      });


      services.Configure<DataProtectionTokenProviderOptions>(o =>
        o.TokenLifespan = TimeSpan.FromHours(5));

      services.Configure<CustomEmailConfirmationTokenProviderOptions>(o =>
        o.TokenLifespan = TimeSpan.FromDays(3));


      services.AddControllersWithViews(options =>
      {
        var policy = new AuthorizationPolicyBuilder()
          .RequireAuthenticatedUser()
          .Build();
        options.Filters.Add(new AuthorizeFilter(policy));
      });

      services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimsHandler>();
      services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
      app.UseRequestLocalization(locOptions.Value);

      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      
      app.UseRequestLocalization();
  
      app.UseHttpsRedirection();
      app.UseStaticFiles();
      app.UseCookiePolicy();

      app.UseRouting();

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        //endpoints.MapControllerRoute(
        //  name: "defaultWithCulture",
        //  pattern: "{culture}/{controller=Home}/{action=Index}/{id?}",
        //  defaults:
        //  new { culture = "ru" },
        //  constraints:
        //  new
        //  {
        //    culture = new CompositeRouteConstraint(
        //      new IRouteConstraint[]
        //      {new AlphaRouteConstraint(),
        //        new MaxLengthRouteConstraint(2) })
        //  },
        //  dataTokens: new { culture = "ru" });

        endpoints.MapControllerRoute(
          name: "default",
          pattern: "{controller=Home}/{action=Index}/{id?}");

        endpoints.MapControllerRoute(
          name: "defaultError",
          pattern: "{controller=Error}/{action=Error}");
        //endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

      });
    }
  }
}
