using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.ServiceModel;

namespace SoapCore.Tests.MessageFilter
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services.TryAddSingleton<TestService>();
			services.AddSoapWsSecurityFilter("yourusername", "yourpassword");
			services.AddMvc();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			app.UseSoapEndpoint<TestService>("/Service.svc", new BasicHttpBinding(), SoapSerializer.DataContractSerializer);
			app.UseMvc();
		}
	}
}
