using Autofac;
using Infrastructure;
using Infrastructure.Mappings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Presentation.API.Auth;
using Presentation.API.DI;

namespace Presentation.API
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }
		public ILifetimeScope AutofacContainer { get; private set; }

		readonly string AllowDevAndProdOrigins = "_allowDevAndProdOrigins";

		// ConfigureContainer is where you can register things directly
		// with Autofac. This runs after ConfigureServices so the things
		// here will override registrations made in ConfigureServices.
		// Don't build the container; that gets done for you by the factory.
		public void ConfigureContainer(ContainerBuilder builder)
		{
			builder.RegisterModule(new AutofacCompositionRootModule());
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();

			// cors
			services.AddCors(options =>
			{
				options.AddPolicy(AllowDevAndProdOrigins,
					builder =>
					{
						builder.WithOrigins("http://localhost:4000",
											"https://guitar-channels.com")
												.AllowAnyHeader()
												.AllowAnyMethod()
												.AllowCredentials();
					});
			});

			// add authentication options, default is JWT
			services
				.AddAuthentication("JWT")
					.AddScheme<AuthenticationSchemeOptions, JWTCookieAuthenticationHandler>("JWT", null);

			services.AddAuthorization();

			// caching
			services.AddMemoryCache();

			// database
			services.AddMongoDb();

			// add automapper
			services.AddDataMappings();

			// inject youtube api
			services.AddYoutubeAPI();

			// allow access to httpcontext via DI
			services.AddHttpContextAccessor();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			// global cors policy
			app.UseCors(AllowDevAndProdOrigins);

			app.UseHttpsRedirection();
			app.UseRouting();

			// place auth middleware init between routing and endpoints
			// https://docs.microsoft.com/en-us/aspnet/core/migration/22-to-30?view=aspnetcore-3.0&tabs=visual-studio#migrate-startupconfigure
			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
