using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;

namespace SoapCore
{
	public class ServiceDescription
	{
		public Type ServiceType { get; private set; }
		public IEnumerable<ContractDescription> Contracts { get; private set; }
		public IEnumerable<OperationDescription> Operations => Contracts.SelectMany(c => c.Operations);

		public ServiceDescription(Type serviceType)
		{
			ServiceType = serviceType;

			var types = Enumerable.Empty<Type>().Concat(ServiceType.GetInterfaces());
			types = types.Concat(new[] { ServiceType });

			var contracts = new List<ContractDescription>();
			foreach (var contractType in types)
			{
				foreach (var serviceContract in contractType.GetTypeInfo().GetCustomAttributes<ServiceContractAttribute>())
				{
					contracts.Add(new ContractDescription(this, contractType, serviceType, serviceContract));
				}
			}

			Contracts = contracts;
		}
	}
}
