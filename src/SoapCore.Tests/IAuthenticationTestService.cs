using System.ServiceModel;
using Microsoft.AspNetCore.Authorization;

namespace SoapCore.Tests
{
	[ServiceContract]
	interface IAuthenticationTestServiceWithAuthorizeAttribute
	{
		[OperationContract]
		string Ping(string s);

		[OperationContract]
		string AnonymousPing(string s);
	}


	[ServiceContract]
	interface IAuthenticationTestService
	{
		[OperationContract]
		string Ping(string s);

		[OperationContract]
		string PingWithRoles(string s);

		[OperationContract]
		string PingWithRolesSpaces(string s);
	}
}
