using System.Threading.Tasks;
using DeepEqual.Syntax;
using Moq;
using Shouldly;
using Xunit;

using SoapCore.Tests.Serialization.Models.Xml;

namespace SoapCore.Tests.Serialization
{
	[Collection("serialization")]
	public class XmlSerializationTests : IClassFixture<ServiceFixture<ISampleService>>
	{
		private readonly ServiceFixture<ISampleService> fixture;

		public XmlSerializationTests(ServiceFixture<ISampleService> fixture)
		{
			this.fixture = fixture;
		}

		[Theory]
		[MemberData(nameof(ServiceFixture<ISampleService>.SoapSerializersList), MemberType = typeof(ServiceFixture<ISampleService>))]
		public void TestPingSerialization(SoapSerializer soapSerializer)
		{
			var sampleServiceClient = this.fixture.GetSampleServiceClient(soapSerializer);

			const string input_value = "input_value";
			const string output_value = "output_value";

			this.fixture.serviceMock
				.Setup(x => x.Ping(It.IsAny<string>()))
				.Callback(
					(string s_service) =>
					{
						// check input paremeters serialization
						s_service.ShouldBe(input_value);
					})
				.Returns(output_value);

			var pingResult_client = sampleServiceClient.Ping(input_value);

			// check output paremeters serialization
			pingResult_client.ShouldBe(output_value);
		}

		delegate void EnumMethodCallback(out SampleEnum e);

		[Theory]
		[MemberData(nameof(ServiceFixture<ISampleService>.SoapSerializersList), MemberType = typeof(ServiceFixture<ISampleService>))]
		public void TestEnumMethodSerialization(SoapSerializer soapSerializer)
		{
			var sampleServiceClient = this.fixture.GetSampleServiceClient(soapSerializer);

			const SampleEnum output_value = SampleEnum.C;

			this.fixture.serviceMock
				.Setup(x => x.EnumMethod(out It.Ref<SampleEnum>.IsAny))
				.Callback(new EnumMethodCallback(
					(out SampleEnum e_service) =>
					{
						// sample response
						e_service = output_value;
					}))
				.Returns(true);

			var enumMethodResult_client = sampleServiceClient.EnumMethod(out var e_client);

			// check output paremeters serialization
			enumMethodResult_client.ShouldBe(true);
			e_client.ShouldBe(output_value);
		}

		delegate void VoidMethodCallback(out string s);

		[Theory]
		[MemberData(nameof(ServiceFixture<ISampleService>.SoapSerializersList), MemberType = typeof(ServiceFixture<ISampleService>))]
		public void TestVoidMethodSerialization(SoapSerializer soapSerializer)
		{
			var sampleServiceClient = this.fixture.GetSampleServiceClient(soapSerializer);

			const string output_value = "output_value";

			this.fixture.serviceMock
				.Setup(x => x.VoidMethod(out It.Ref<string>.IsAny))
				.Callback(new VoidMethodCallback(
					(out string s_service) =>
					{
						// sample response
						s_service = output_value;
					}));

			sampleServiceClient.VoidMethod(out var s_client);

			// check output paremeters serialization
			s_client.ShouldBe(output_value);
		}

		[Theory]
		[MemberData(nameof(ServiceFixture<ISampleService>.SoapSerializersList), MemberType = typeof(ServiceFixture<ISampleService>))]
		public async Task TestAsyncMethodSerialization(SoapSerializer soapSerializer)
		{
			var sampleServiceClient = this.fixture.GetSampleServiceClient(soapSerializer);

			const int output_value = 123;

			this.fixture.serviceMock
				.Setup(x => x.AsyncMethod())
				.Returns(() => Task.Run(() => output_value));

			var asyncMethodResult_client = await sampleServiceClient.AsyncMethod();

			// check output paremeters serialization
			asyncMethodResult_client.ShouldBe(output_value);
		}

		[Theory]
		[InlineData(SoapSerializer.XmlSerializer, null, null)]
		[InlineData(SoapSerializer.XmlSerializer, true, 1)]
		[InlineData(SoapSerializer.XmlSerializer, false, 2)]
		[InlineData(SoapSerializer.DataContractSerializer, null, null)]
		[InlineData(SoapSerializer.DataContractSerializer, true, 1)]
		[InlineData(SoapSerializer.DataContractSerializer, false, 2)]
		public void TestNullableMethodSerialization(SoapSerializer soapSerializer, bool? input_value, int? output_value)
		{
			var sampleServiceClient = this.fixture.GetSampleServiceClient(soapSerializer);

			this.fixture.serviceMock
				.Setup(x => x.NullableMethod(It.IsAny<bool?>()))
				.Callback(
					(bool? arg_service) =>
					{
						// check input paremeters serialization
						arg_service.ShouldBe(input_value);
					})
				.Returns(output_value);

			var nullableMethodResult_client = sampleServiceClient.NullableMethod(input_value);

			// check output paremeters serialization
			nullableMethodResult_client.ShouldBe(output_value);
		}

		[Theory]
		// not compatible with DataContractSerializer
		[InlineData(SoapSerializer.XmlSerializer)]
		public void TestPingComplexModelSerialization(SoapSerializer soapSerializer)
		{
			var sampleServiceClient = this.fixture.GetSampleServiceClient(soapSerializer);

			this.fixture.serviceMock
				.Setup(x => x.PingComplexModel(It.IsAny<ComplexModel2>()))
				.Callback(
					(ComplexModel2 inputModel_service) =>
					{
						// check input paremeters serialization
						inputModel_service.ShouldDeepEqual(ComplexModel2.CreateSample2());
					})
				.Returns(ComplexModel1.CreateSample3);

			var pingComplexModelResult_client =
				sampleServiceClient
					.PingComplexModel(ComplexModel2.CreateSample2());

			// check output paremeters serialization
			pingComplexModelResult_client.ShouldDeepEqual(ComplexModel1.CreateSample3());
		}

		delegate void PingComplexModelOutAndRefCallback(
			ComplexModel1 inputModel,
			ref ComplexModel2 responseModelRef1,
			ComplexObject data1,
			ref ComplexModel1 responseModelRef2,
			ComplexObject data2,
			out ComplexModel2 responseModelOut1,
			out ComplexModel1 responseModelOut2);

		[Theory]
		// not compatible with DataContractSerializer
		[InlineData(SoapSerializer.XmlSerializer)]
		public void TestPingComplexModelOutAndRefSerialization(SoapSerializer soapSerializer)
		{
			var sampleServiceClient = this.fixture.GetSampleServiceClient(soapSerializer);

			this.fixture.serviceMock
				.Setup(x => x.PingComplexModelOutAndRef(
					It.IsAny<ComplexModel1>(),
					ref It.Ref<ComplexModel2>.IsAny,
					It.IsAny<ComplexObject>(),
					ref It.Ref<ComplexModel1>.IsAny,
					It.IsAny<ComplexObject>(),
					out It.Ref<ComplexModel2>.IsAny,
					out It.Ref<ComplexModel1>.IsAny))
				.Callback(new PingComplexModelOutAndRefCallback(
					(ComplexModel1 inputModel_service,
						ref ComplexModel2 responseModelRef1_service,
						ComplexObject data1_service,
						ref ComplexModel1 responseModelRef2_service,
						ComplexObject data2_service,
						out ComplexModel2 responseModelOut1_service,
						out ComplexModel1 responseModelOut2_service) =>
					{
						// check input paremeters serialization
						inputModel_service.ShouldDeepEqual(ComplexModel1.CreateSample2());
						responseModelRef1_service.ShouldDeepEqual(ComplexModel2.CreateSample1());
						responseModelRef2_service.ShouldDeepEqual(ComplexModel1.CreateSample2());
						data1_service.ShouldDeepEqual(ComplexObject.CreateSample1());
						data2_service.ShouldDeepEqual(ComplexObject.CreateSample2());
						// sample response
						responseModelRef1_service = ComplexModel2.CreateSample2();
						responseModelRef2_service = ComplexModel1.CreateSample1();
						responseModelOut1_service = ComplexModel2.CreateSample3();
						responseModelOut2_service = ComplexModel1.CreateSample1();
					}))
				.Returns(true);

			var responseModelRef1_client = ComplexModel2.CreateSample1();
			var responseModelRef2_client = ComplexModel1.CreateSample2();

			var pingComplexModelOutAndRefResult_client =
				sampleServiceClient.PingComplexModelOutAndRef(
					ComplexModel1.CreateSample2(),
					ref responseModelRef1_client,
					ComplexObject.CreateSample1(),
					ref responseModelRef2_client,
					ComplexObject.CreateSample2(),
					out var responseModelOut1_client,
					out var responseModelOut2_client);

			// check output paremeters serialization
			pingComplexModelOutAndRefResult_client.ShouldBeTrue();
			responseModelRef1_client.ShouldDeepEqual(ComplexModel2.CreateSample2());
			responseModelRef2_client.ShouldDeepEqual(ComplexModel1.CreateSample1());
			responseModelOut1_client.ShouldDeepEqual(ComplexModel2.CreateSample3());
			responseModelOut2_client.ShouldDeepEqual(ComplexModel1.CreateSample1());
		}

		[Theory]
		// not compatible with DataContractSerializer
		[InlineData(SoapSerializer.XmlSerializer)]
		public void TestPingComplexModelOldStyleSerialization(SoapSerializer soapSerializer)
		{
			var sampleServiceClient = this.fixture.GetSampleServiceClient(soapSerializer);

			this.fixture.serviceMock
				.Setup(x => x.PingComplexModelOldStyle(It.IsAny<PingComplexModelOldStyleRequest>()))
				.Callback(
					(PingComplexModelOldStyleRequest request_service) =>
					{
						// check input paremeters serialization
						request_service.inputModel.ShouldDeepEqual(ComplexModel1.CreateSample2());
						request_service.responseModelRef1.ShouldDeepEqual(ComplexModel2.CreateSample1());
						request_service.responseModelRef2.ShouldDeepEqual(ComplexModel1.CreateSample2());
						request_service.data1.ShouldDeepEqual(ComplexObject.CreateSample1());
						request_service.data2.ShouldDeepEqual(ComplexObject.CreateSample2());
					})
				.Returns(
					() => new PingComplexModelOldStyleResponse
					{
						// sample response
						PingComplexModelOldStyleResult = true,
						responseModelRef1 = ComplexModel2.CreateSample2(),
						responseModelRef2 = ComplexModel1.CreateSample1(),
						responseModelOut1 = ComplexModel2.CreateSample3(),
						responseModelOut2 = ComplexModel1.CreateSample1()
					});

			var pingComplexModelOldStyleResult_client =
				sampleServiceClient.PingComplexModelOldStyle(
					new PingComplexModelOldStyleRequest
					{
						inputModel = ComplexModel1.CreateSample2(),
						responseModelRef1 = ComplexModel2.CreateSample1(),
						data1 = ComplexObject.CreateSample1(),
						responseModelRef2 = ComplexModel1.CreateSample2(),
						data2 = ComplexObject.CreateSample2(),
					});

			// check output paremeters serialization
			pingComplexModelOldStyleResult_client
				.PingComplexModelOldStyleResult
				.ShouldBeTrue();
			pingComplexModelOldStyleResult_client
				.responseModelRef1
				.ShouldDeepEqual(ComplexModel2.CreateSample2());
			pingComplexModelOldStyleResult_client
				.responseModelRef2
				.ShouldDeepEqual(ComplexModel1.CreateSample1());
			pingComplexModelOldStyleResult_client
				.responseModelOut1
				.ShouldDeepEqual(ComplexModel2.CreateSample3());
			pingComplexModelOldStyleResult_client
				.responseModelOut2
				.ShouldDeepEqual(ComplexModel1.CreateSample1());
		}

		[Theory]
		// not compatible with DataContractSerializer
		[InlineData(SoapSerializer.XmlSerializer)]
		public void TestEmptyParamsMethodSerialization(SoapSerializer soapSerializer)
		{
			var sampleServiceClient = this.fixture.GetSampleServiceClient(soapSerializer);

			this.fixture.serviceMock
				.Setup(x => x.EmptyParamsMethod())
				.Returns(ComplexModel1.CreateSample2);

			var emptyParamsMethodResult_client =
				sampleServiceClient
					.EmptyParamsMethod();

			// check output paremeters serialization
			emptyParamsMethodResult_client.ShouldDeepEqual(ComplexModel1.CreateSample2());
		}
	}
}
