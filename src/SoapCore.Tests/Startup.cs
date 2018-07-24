using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Claims;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using ZNetCS.AspNetCore.Authentication.Basic;
using ZNetCS.AspNetCore.Authentication.Basic.Events;

namespace SoapCore.Tests
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services.TryAddSingleton<TestService>();
			services.TryAddSingleton<AuthenticationTestServiceWithAuthorizeAttribute>();
			services.TryAddSingleton<AuthenticationTestService>();
			services.AddMvc();

			services
				.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
				.AddBasicAuthentication(
					options =>
					{
						options.Realm = "SoapCore Authentication Test";
						options.Events = new BasicAuthenticationEvents
						{
							OnValidatePrincipal = context =>
							{								
								return this.ValidatePrincipal(context);
							}
						};
					});
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			app.UseAuthentication();

			app.UseWhen(ctx => ctx.Request.Headers.ContainsKey("SOAPAction"), app2 =>
			{
				app2.UseSoapEndpoint<TestService>("/Service.svc", new BasicHttpBinding(), SoapSerializer.DataContractSerializer);
			});
			app.UseWhen(ctx => !ctx.Request.Headers.ContainsKey("SOAPAction"), app2 =>
			{
				var transportBinding = new HttpTransportBindingElement();
				var textEncodingBinding = new TextMessageEncodingBindingElement(MessageVersion.Soap12WSAddressing10, System.Text.Encoding.UTF8);
				app.UseSoapEndpoint<AuthenticationTestServiceWithAuthorizeAttribute>("/AuthenticationTestServiceWithAuthorizeAttribute.svc", new CustomBinding(transportBinding, textEncodingBinding), SoapSerializer.DataContractSerializer);
				app.UseSoapEndpoint<AuthenticationTestService>("/AuthenticationTestService.svc", new CustomBinding(transportBinding, textEncodingBinding), SoapSerializer.DataContractSerializer);
				app.UseSoapEndpoint<TestService>("/Service.svc", new CustomBinding(transportBinding, textEncodingBinding), SoapSerializer.DataContractSerializer);
			});
			app.UseMvc();
		}


		private Task ValidatePrincipal(ValidatePrincipalContext context)
		{
			var userInfos = new[]
			{
				new {UserName= "user", Password = "password", Roles = new string[] { "role1" } },
				new {UserName= "user2", Password = "password", Roles = new string[] {"role2" } },
				new {UserName= "user3", Password = "password", Roles = new string[] {"role2", "role3" } },
				new {UserName= "user4", Password = "password", Roles = new string[] {"role3" } }
			}; 

			foreach (var userInfo in userInfos)
			{
				if ((context.UserName == userInfo.UserName) && (context.Password == userInfo.Password))
				{
					var claims = new List<Claim>
									{
										new Claim(ClaimTypes.Name, context.UserName, context.Options.ClaimsIssuer)
									};

					foreach (string role in userInfo.Roles)
					{
						claims.Add(new Claim(ClaimTypes.Role, role, context.Options.ClaimsIssuer));
					}

					var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, BasicAuthenticationDefaults.AuthenticationScheme));
					context.Principal = principal;
				}
			}
			
			return Task.CompletedTask;
		}
	}
}
