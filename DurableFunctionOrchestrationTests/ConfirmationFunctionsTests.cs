using DurableFunctionOrchestration.Activities;
using DurableFunctionOrchestration.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;

namespace DurableFunctionOrchestrationTests
{
    [TestClass]
    public class ConfirmationFunctionsTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<ILogger<ConfirmationFunctions>> _mockLogger;
        private readonly Mock<FunctionContext> _mockFunctionContext;

        public ConfirmationFunctionsTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockLogger = new Mock<ILogger<ConfirmationFunctions>>();
            _mockFunctionContext = new Mock<FunctionContext>();

            var services = new Mock<IServiceProvider>();
            services
                .Setup(x => x.GetService(typeof(ILogger<ConfirmationFunctions>)))
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

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize("successful"), Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Set up generic ILogger<T> mocking for any requested type
            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            var confirmationFunctions = new ConfirmationFunctions(clientFactory.Object);

            // Act
            var result = await confirmationFunctions.ConfirmationAsync(It.IsAny<ConfirmationRequest>(), _mockFunctionContext.Object);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }
    }
}
