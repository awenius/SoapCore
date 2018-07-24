# SoapCore

SOAP protocol middleware for ASP.NET Core.

Based on Microsoft article: [Custom ASP.NET Core Middleware Example](https://blogs.msdn.microsoft.com/dotnet/2016/09/19/custom-asp-net-core-middleware-example/).

Support ref\out params, exceptions. Works with legacy SOAP\WCF-clients.

## Getting Started

### Installing

`PM> Install-Package SoapCore`

In Startup.cs:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.TryAddSingleton<ServiceContractImpl>();
}
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
    app.UseSoapEndpoint<ServiceContractImpl>("/ServicePath.asmx", new BasicHttpBinding());
}
```

### Authentication and Authorization

SoapCore supports basic authentication and authorization.
Any authentication middleware can be used.

A service or service operation can be protected using the standard `Microsoft.AspNetCore.Authorization.AuthorizationAttribute` attribute.
However, only a subset of this attributes functionality is supported:

* A whole service may be protected by adding the `Authorize` attribute to the service implementation class.
* A service operation may be protected by adding the `Authorize` attribute to the service implementation method.
* The `Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute` attribute may be used to unprotect single methods if the whole service has been protected.

In addition, the `Role` property of the `AuthorizeAttribute` attribute may be used. In this case, the user must be in one of the specified role for the call to be permitted. 

### References

* [stackify.com/soap-net-core](https://stackify.com/soap-net-core/)

[![Build Status](https://travis-ci.com/DigDes/SoapCore.svg?branch=master)](https://travis-ci.com/DigDes/SoapCore)
