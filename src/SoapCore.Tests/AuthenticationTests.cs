using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SoapCore.Tests
{
	[TestClass]
	public class AuthenticationTests
	{
		TestServer CreateTestHost()
		{
			var webHostBuilder = new WebHostBuilder()
				.ConfigureLogging((hostingContext, logging) =>
				{					
					logging.AddConsole();
					logging.SetMinimumLevel(LogLevel.Debug);
				})
				.UseStartup<Startup>();
			return new TestServer(webHostBuilder);
		}
		

		[TestMethod]
		public void TestFailedAuthenticationWithAuthorizeAttributeAtClass()
		{
			var action = @"<Ping xmlns=""http://tempuri.org/""><s>abc</s></Ping>";
			HttpResponseMessage result = CallService(action, "/AuthenticationTestServiceWithAuthorizeAttribute.svc", "", "");

			Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, result.StatusCode);
			Assert.AreEqual("Unauthorized", result.ReasonPhrase);					
		}

		[TestMethod]
		public void TestFailedAuthenticationWithAuthorizeAttributeAtMethod()
		{
			var action = @"<Ping xmlns=""http://tempuri.org/""><s>abc</s></Ping>";
			HttpResponseMessage result = CallService(action, "/AuthenticationTestService.svc", "", "");

			Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, result.StatusCode);
			Assert.AreEqual("Unauthorized", result.ReasonPhrase);
		}

		[TestMethod]
		public void TestSuccessfulAuthenticationWithAuthorizeAttributeAtClass()
		{
			var action = @"<Ping xmlns=""http://tempuri.org/""><s>abc</s></Ping>";
			HttpResponseMessage result = CallService(action, "/AuthenticationTestServiceWithAuthorizeAttribute.svc", "user", "password");

			Assert.AreEqual(System.Net.HttpStatusCode.OK, result.StatusCode);
			Assert.AreEqual("OK", result.ReasonPhrase);
		}

		[TestMethod]
		public void TestAnonymusWithAuthorizeAttributeAtClass()
		{
			var action = @"<AnonymousPing xmlns=""http://tempuri.org/""><s>abc</s></AnonymousPing>";
			HttpResponseMessage result = CallService(action, "/AuthenticationTestServiceWithAuthorizeAttribute.svc", "", "");

			Assert.AreEqual(System.Net.HttpStatusCode.OK, result.StatusCode);
			Assert.AreEqual("OK", result.ReasonPhrase);
		}

		[TestMethod]
		public void TestSuccessfulAuthenticationWithAuthorizeAttributeAtMethod()
		{
			var action = @"<Ping xmlns=""http://tempuri.org/""><s>abc</s></Ping>";
			HttpResponseMessage result = CallService(action, "/AuthenticationTestService.svc", "user", "password");

			Assert.AreEqual(System.Net.HttpStatusCode.OK, result.StatusCode);
			Assert.AreEqual("OK", result.ReasonPhrase);
		}

		[TestMethod]
		public void TestUnsuccessfulWithRoles()
		{
			var action = @"<PingWithRoles xmlns=""http://tempuri.org/""><s>abc</s></PingWithRoles>";
			HttpResponseMessage result = CallService(action, "/AuthenticationTestService.svc", "user", "password");

			Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, result.StatusCode);
			Assert.AreEqual("Unauthorized", result.ReasonPhrase);
		}

		[TestMethod]
		public void TestSuccessfulWithRoles()
		{
			var action = @"<PingWithRoles xmlns=""http://tempuri.org/""><s>abc</s></PingWithRoles>";
			HttpResponseMessage result = CallService(action, "/AuthenticationTestService.svc", "user3", "password");

			Assert.AreEqual(System.Net.HttpStatusCode.OK, result.StatusCode);
			Assert.AreEqual("OK", result.ReasonPhrase);

			result = CallService(action, "/AuthenticationTestService.svc", "user3", "password");
			Assert.AreEqual(System.Net.HttpStatusCode.OK, result.StatusCode);
			Assert.AreEqual("OK", result.ReasonPhrase);

			result = CallService(action, "/AuthenticationTestService.svc", "user4", "password");
			Assert.AreEqual(System.Net.HttpStatusCode.OK, result.StatusCode);
			Assert.AreEqual("OK", result.ReasonPhrase);

			action = @"<PingWithRolesSpaces xmlns=""http://tempuri.org/""><s>abc</s></PingWithRolesSpaces>";
			result = CallService(action, "/AuthenticationTestService.svc", "user4", "password");
			Assert.AreEqual(System.Net.HttpStatusCode.OK, result.StatusCode);
			Assert.AreEqual("OK", result.ReasonPhrase);
		}

		/// <summary>
		/// Invokes a Soap service.
		/// </summary>
		/// <param name="action">
		/// The method that shall be called, XML formatted.
		/// </param>
		/// <param name="serviceName">
		/// The name of the service that shall be called.
		/// </param>
		/// <param name="userName">
		/// The username that shall be passed as HTTP basic authentication.
		/// </param>
		/// <param name="password">
		/// The password that shall be passed as HTTP basic authentication.
		/// </param>
		/// <returns>
		/// The response of the service invocation.
		/// </returns>
		private HttpResponseMessage CallService(string action, string serviceName, string userName, string password)
		{
			HttpResponseMessage result = null;
			var body = @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
  <soap12:Body>
    {0}
  </soap12:Body>
</soap12:Envelope>
";
			body = String.Format(body, action);
			var bodyBytes = Encoding.UTF8.GetBytes(body);
			using (var host = CreateTestHost())
			using (var client = host.CreateClient())
			using (var content = new StringContent(body, Encoding.UTF8, "application/soap+xml"))
			{
				using (var res = host.CreateRequest(serviceName)
					.And(msg => msg.Content = content)
					.And(msg => SetAuthenticationHeader(msg.Headers, userName, password))
					.PostAsync().Result
				)
				{
					result = res;
				}
			}

			return result;
		}


		private void SetAuthenticationHeader(HttpRequestHeaders headers, string userName, string password)
		{
			String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(userName + ":" + password));
			headers.Add("Authorization", "Basic " + encoded);
		}
	}
}
