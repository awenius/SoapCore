using System;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;

namespace SoapCore
{
	public class ContractDescription
	{
		public ServiceDescription Service { get; private set; }
		public string Name { get; private set; }
		public string Namespace { get; private set; }
		public Type ContractType { get; private set; }
		public IEnumerable<OperationDescription> Operations { get; private set; }

		public ContractDescription(	ServiceDescription service,
									Type contractType,
									Type implementingType,
									ServiceContractAttribute attribute)
		{
			Service = service;
			ContractType = contractType;
			Namespace = attribute.Namespace ?? "http://tempuri.org/"; // Namespace defaults to http://tempuri.org/
			Name = attribute.Name ?? ContractType.Name; // Name defaults to the type name

			var operations = new List<OperationDescription>();
			foreach (var operationMethodInfo in ContractType.GetTypeInfo().DeclaredMethods)
			{
				foreach (var operationContract in operationMethodInfo.GetCustomAttributes<OperationContractAttribute>())
				{
					MethodInfo implementingMethod = GetImplementingMethod(contractType, implementingType, operationMethodInfo);
					operations.Add(new OperationDescription(this, operationMethodInfo, implementingMethod, operationContract));
				}
			}
			Operations = operations;
		}


		/// <summary>
		/// Returns the method that actually implements the action.
		/// </summary>
		/// <param name="contractType">
		/// The type of the interface that declares the service.
		/// </param>
		/// <param name="implementingType">
		/// The type of the class that implements the service.
		/// </param>
		/// <param name="operationMethodInfo">
		/// The method from the interface type that describes the method.
		/// </param>
		/// <remarks>
		/// Usually, the implementingType parameter should be the type of the class
		/// that actually implements the service.
		/// But we also support passing the type of the interface as implementingType.
		/// In this case, the authorization features cannot be used.
		/// </remarks>
		/// <returns>
		/// The method from the implementing class that relates to the passed interface method.
		/// </returns>
		private MethodInfo GetImplementingMethod(Type contractType, Type implementingType, MethodInfo operationMethodInfo)
		{
			MethodInfo result = operationMethodInfo;

			if (contractType != implementingType)
			{
				InterfaceMapping mapping = implementingType.GetInterfaceMap(contractType);

				for (int ii = 0; ii < mapping.InterfaceMethods.Length; ii++)
				{
					if (mapping.InterfaceMethods[ii] == operationMethodInfo)
					{
						result = mapping.TargetMethods[ii];
						break;
					}
				}
			}

			return result;
		}
	}
}
