using DurableFunctionOrchestration.Activities;
using DurableFunctionOrchestration.Models;
using FunctionApp1;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;

namespace DurableFunctionOrchestrationTests
{
    [TestClass]
    public class OrchestratorFunctionsTests
    {
        [TestMethod]
        public async Task RunOrchestrator_ReturnsConfirmationResult()
        {
            // Arrange
            var context = new Mock<TaskOrchestrationContext>();
            var logger = new Mock<ILogger>();
            context.Setup(c => c.CreateReplaySafeLogger(It.IsAny<string>())).Returns(logger.Object);
            var userId = "testUser";
            var hotel = new HotelReservationRequest
            {
                Id = "testHotelId",
                Name = "testHotelName",
                CheckIn = DateTime.Now,
                CheckOut = DateTime.Now.AddDays(1),
                Address = "testHotelAddress",
            };
            var flight = new FlightReservationRequest
            {
                Id = "testFlightId",
                Name = "testFlightName",
                From = "testFlightFrom",
                To = "testFlightTo",
                Departure = DateTime.Now,
                Arrival = DateTime.Now.AddDays(1),
            };

            var hotelResult = new HotelReservationResult { Status = "SUCCESS", Reservation = hotel };
            var flightResult = new FlightReservationResult { Status = "SUCCESS", Reservation = flight };

            var confirmationResult = "testConfirmationResult";

            context.Setup(c => c.GetInput<string>()).Returns(userId);
            context.Setup(c => c.CallActivityAsync<HotelReservationResult>(nameof(HotelFunctions.RegistrationAsync), userId, It.IsAny<TaskOptions>()))
                .ReturnsAsync(hotelResult);
            context.Setup(c => c.CallActivityAsync<FlightReservationResult>(nameof(FlightFunctions.FlightRegistrationAsync), userId, It.IsAny<TaskOptions>()))
                .ReturnsAsync(flightResult);
            context.Setup(c => c.CallActivityAsync<string>(
                nameof(ConfirmationFunctions.ConfirmationAsync), It.IsAny<ConfirmationRequest>(), It.IsAny<TaskOptions>())).ReturnsAsync(confirmationResult);

            // Act
            var result = await OrchestratorFunctions.RunOrchestrator(context.Object);

            // Assert
            Assert.AreEqual(confirmationResult, result);
        }
    }
}
