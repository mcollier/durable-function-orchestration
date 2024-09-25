//using FunctionApp1;
//using Microsoft.Azure.Functions.Worker;
//using Microsoft.Azure.Functions.Worker.Http;
//using Microsoft.DurableTask.Client;
//using Microsoft.Extensions.Logging;
//using Moq;
//using System.Collections.Specialized;
//using System.Net;
//using Assert = Xunit.Assert;

//namespace DurableFunctionOrchestrationTests
//{
//    [TestClass]
//    public class HttpStartTests
//    {
//        [TestMethod]
//        public async Task HttpStart_ShouldReturn202Response()
//        {
//            // Arrange
//            var mockRequest = new Mock<HttpRequestData>(MockBehavior.Strict);
//            var mockClient = new Mock<DurableTaskClient>(MockBehavior.Strict);
//            var mockContext = new Mock<FunctionContext>(MockBehavior.Strict);
//            var mockLogger = new Mock<ILogger>(MockBehavior.Loose);

//            mockContext.Setup(c => c.GetLogger(It.IsAny<string>())).Returns(mockLogger.Object);

//            var query = new NameValueCollection { { "userId", "testUser" } };
//            mockRequest.Setup(r => r.Query).Returns(query);

//            var instanceId = "testInstanceId";
//            mockClient.Setup(c => c.ScheduleNewOrchestrationInstanceAsync(nameof(OrchestratorFunctions), "testUser", new CancellationToken()))
//                      .ReturnsAsync(instanceId);
//            mockClient.Setup(c => c.CreateCheckStatusResponseAsync(mockRequest.Object, instanceId, new CancellationToken()))
//                      .ReturnsAsync(new HttpResponseData(mockRequest.Object) { StatusCode = HttpStatusCode.Accepted });

//            // Act
//            var response = await HttpFunctions.HttpStart(mockRequest.Object, mockClient.Object, mockContext.Object);

//            // Assert
//            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
//            mockClient.VerifyAll();
//            mockRequest.VerifyAll();
//            mockContext.VerifyAll();
//        }
//    }
//}