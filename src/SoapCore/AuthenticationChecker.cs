using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Security.Claims;

namespace SoapCore
{
	/// <inheritdoc/>
	internal class AuthenticationChecker: IAuthenticationChecker
    {
		/// <summary>
		/// Constructs a new instance.
		/// </summary>
		internal AuthenticationChecker()
		{		
		}


		/// <inheritdoc/>
		public bool CheckAuthentication(HttpContext httpContext, MemberInfo serviceType, MemberInfo operation)
		{
			AuthorizeAttribute authorizeAttribute = serviceType.GetCustomAttribute<AuthorizeAttribute>();
			bool result = true;

			// First check the AuthorizeAttribute at the type.
			if (authorizeAttribute != null)
			{
				result = CheckAuthorizationAttribute(authorizeAttribute, httpContext.User);

				// There could be an "AllowAnonymous" attribute at the method which would "override" the
				// "Authorize" attribute at the class.
				if (!result && operation.GetCustomAttribute<AllowAnonymousAttribute>() != null)
				{
					result = true;
				}
			}

			// Then check the AuthorizeAttribute at the method.
			authorizeAttribute = operation.GetCustomAttribute<AuthorizeAttribute>();

			if (authorizeAttribute != null)
			{
				result = CheckAuthorizationAttribute(authorizeAttribute, httpContext.User);
			}

			return result;
		}


		/// <summary>
		/// Checks, whether the user is authenticated and if he or she has one of the
		/// required roles if roles are specified.
		/// </summary>
		/// <param name="attribute">
		/// The <code>AuthorizeAttribute</code>, that shall be checked.
		/// </param>
		/// <param name="principal">
		/// The user, that shall be checked.
		/// </param>
		/// <returns>
		/// <code>true</code> if the user is authenticated and authorized,
		/// otherwise <code>false</code>.
		/// </returns>
		private bool CheckAuthorizationAttribute(AuthorizeAttribute attribute, ClaimsPrincipal principal)
		{
			bool result = false;

			if (principal.Identity.IsAuthenticated)
			{
				string[] allowedRoles = attribute.Roles?.Split(new char[] { ',', ' ' });

				if (allowedRoles != null && allowedRoles.Length > 0)
				{
					foreach (string role in allowedRoles)
					{
						if (principal.IsInRole(role))
						{
							result = true;
							break;
						}
					}
				}
				else
				{
					// No roles specified.
					result = true;
				}
			}

			return result;
		}
    }
}
