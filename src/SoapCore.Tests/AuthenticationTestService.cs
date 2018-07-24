using Microsoft.AspNetCore.Authorization;


namespace SoapCore.Tests
{
	[Authorize]
	public class AuthenticationTestServiceWithAuthorizeAttribute : IAuthenticationTestServiceWithAuthorizeAttribute
	{
		public string Ping(string s)
		{
			return s;
		}


		[AllowAnonymous]
		public string AnonymousPing(string s)
		{
			return s;
		}
	}

	public class AuthenticationTestService: IAuthenticationTestService
	{
		[Authorize]
		public string Ping(string s)
		{
			return s;
		}

		[Authorize(Roles ="role2,role3")]
		public string PingWithRoles(string s)
		{
			return s;
		}

		[Authorize(Roles = "role2,  role3")]
		public string PingWithRolesSpaces(string s)
		{
			return s;
		}
	}
}
