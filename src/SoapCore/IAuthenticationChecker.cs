using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace SoapCore
{
	/// <summary>
	/// Checks authentication and authorization for a given operation invocation.
	/// </summary>
	interface IAuthenticationChecker
    {
		/// <summary>
		/// Checks the authentication attribute in the context of the current invocation.
		/// </summary>
		/// <param name="httpContext">
		/// The <code>HttpContext</code> of this invocation.
		/// </param>
		/// <param name="serviceType">
		/// The class that implements the service.
		/// </param>
		/// <param name="operation">
		/// The operation, that implements the service.
		/// </param>
		/// <returns>
		/// <code>true</code>, if the call is authorized, otherwise <code>false</code>.
		/// </returns>
		bool CheckAuthentication(HttpContext httpContext, MemberInfo serviceType, MemberInfo operation);
	}
}
