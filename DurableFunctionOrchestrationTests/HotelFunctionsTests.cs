using DurableFunctionOrchestration.Activities;
using DurableFunctionOrchestration.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace DurableFunctionOrchestrationTests
{
    [TestClass]
    public class HotelFunctionsTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<ILogger<HotelFunctions>> _mockLogger;
        private readonly Mock<FunctionContext> _mockFunctionContext;

        public HotelFunctionsTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockLogger = new Mock<ILogger<HotelFunctions>>();
            _mockFunctionContext = new Mock<FunctionContext>();

            var services = new Mock<IServiceProvider>();
            services
                .Setup(x => x.GetService(typeof(ILogger<HotelFunctions>)))
                .Returns(_mockLogger.Object);

            _mockFunctionContext
                .SetupGet(x => x.InstanceServices)
                .Returns(services.Object);
        }

        [TestMethod]
        public async Task RegistrationAsync_Success()
        {
            // Arrange
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var client = new HttpClient(_mockHttpMessageHandler.Object);
            client.BaseAddress = new Uri("http://localhost/");

            // Set up generic ILogger<T> mocking for any requested type
            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            var hotelFunctions = new HotelFunctions(clientFactory.Object);

            // Act
            var result = await hotelFunctions.RegistrationAsync("testUserId", _mockFunctionContext.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<HotelReservationRequest>(result);
        }
    }
}
