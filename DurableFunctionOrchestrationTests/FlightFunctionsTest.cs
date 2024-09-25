using DurableFunctionOrchestration.Activities;
using DurableFunctionOrchestration.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;

namespace DurableFunctionOrchestrationTests
{
    [TestClass]
    public class FlightFunctionsTest
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<ILogger<FlightFunctions>> _mockLogger;
        private readonly Mock<FunctionContext> _mockFunctionContext;

        public FlightFunctionsTest()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockLogger = new Mock<ILogger<FlightFunctions>>();
            _mockFunctionContext = new Mock<FunctionContext>();

            var services = new Mock<IServiceProvider>();
            services
                .Setup(x => x.GetService(typeof(ILogger<FlightFunctions>)))
                .Returns(_mockLogger.Object);

            _mockFunctionContext
                .SetupGet(x => x.InstanceServices)
                .Returns(services.Object);
        }

        [TestMethod]
        public async Task FlightRegistrationAsync_Success()
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

            var flightFunctions = new FlightFunctions(clientFactory.Object);

            // Act
            var result = await flightFunctions.FlightRegistrationAsync("testUserId", _mockFunctionContext.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<FlightReservationRequest>(result);
        }
    }
}
