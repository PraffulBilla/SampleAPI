using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SampleAPI.Controllers;
using SampleAPI.Entities;
using SampleAPI.Repositories;
using SampleAPI.Requests;

namespace SampleAPI.Tests.Controllers
{
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderRepository> _mockRepository;
        private readonly OrdersController _ordersController;
        private readonly Mock<ILogger<OrdersController>> _loggerMock;

        public OrdersControllerTests()
        {
            _mockRepository = new Mock<IOrderRepository>();
            _loggerMock = new Mock<ILogger<OrdersController>>();
            _ordersController = new OrdersController(_mockRepository.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetRecentOrders_ShouldReturnOkResult()
        {
            var orders = new List<Order>
            {
                new Order { Id = 1, OrderName = "Test Order Name1", OrderDescription = "Test order Desp1", EntryDate = DateTime.Now },
                new Order { Id = 2, OrderName = "Test Order Name2", OrderDescription = "Test order Desp2", EntryDate = DateTime.Now.AddDays(-2), IsDeleted = true }
            };

            _mockRepository.Setup(repo => repo.GetOrdersAsync())
                       .ReturnsAsync(orders);

            var result = await _ordersController.GetOrders();

            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            var returnOrders = okResult.Value as List<Order>;
            returnOrders.Should().HaveCount(2);
        }


        [Fact]
        public async Task GetOrders_ShouldReturnNotFound_WhenNoRecentOrdersExist()
        {
            _mockRepository.Setup(repo => repo.GetOrdersAsync())
                                .ReturnsAsync(new List<Order>());

            var result = await _ordersController.GetOrders();

            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().Be("No recent orders found.");
        }

        [Fact]
        public async Task GetOrders_ShouldReturnInternalServerError_WhenExceptionIsThrown()
        {
            _mockRepository.Setup(repo => repo.GetOrdersAsync())
                                .ThrowsAsync(new Exception("Database error"));

            var result = await _ordersController.GetOrders();

            var internalServerErrorResult = result.Result as StatusCodeResult;
            internalServerErrorResult.Should().NotBeNull();
            internalServerErrorResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task CreateOrder_ShouldReturnCreatedAtAction_WhenOrderIsValid()
        {
            var request = new CreateOrderRequest { Name = "test Order1", Description = "Test Valid order", IsInvoiced = true };
            var order = new Order { Id = 1, OrderName = request.Name, OrderDescription = request.Description, IsInvoiced = request.IsInvoiced };

            _mockRepository.Setup(repo => repo.AddNewOrderAsync(request))
                                .ReturnsAsync(order);

            var result = await _ordersController.CreateOrder(request);

            var createdAtActionResult = result.Result as CreatedAtActionResult;
            createdAtActionResult.Should().NotBeNull();
            createdAtActionResult.StatusCode.Should().Be(201);
            createdAtActionResult.Value.Should().BeEquivalentTo(order);
        }

        [Fact]
        public async Task CreateOrder_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            var request = new CreateOrderRequest { Name = "", Description = "Invalid order", IsInvoiced = true };
            _ordersController.ModelState.AddModelError("Name", "Name is required.");

            var result = await _ordersController.CreateOrder(request);

            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);

            var modelState = badRequestResult.Value as SerializableError;

            modelState.Should().ContainKey("Name");

            var nameErrors = modelState["Name"] as string[];
            nameErrors.Should().Contain("Name is required.");
        }

        [Fact]
        public async Task CreateOrder_ShouldReturnConflict_WhenArgumentExceptionIsThrown()
        {
            var request = new CreateOrderRequest { Name = "Test Order1", Description = "Conflicting order", IsInvoiced = true };

            _mockRepository.Setup(repo => repo.AddNewOrderAsync(request))
                                .ThrowsAsync(new ArgumentException("Order already exists"));

            var result = await _ordersController.CreateOrder(request);

            var conflictResult = result.Result as StatusCodeResult;
            conflictResult.Should().NotBeNull();
            conflictResult.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        }

        [Fact]
        public async Task CreateOrder_ShouldReturnInternalServerError_WhenExceptionIsThrown()
        {
            var request = new CreateOrderRequest { Name = "Test Order1", Description = "Valid order", IsInvoiced = true };

            _mockRepository.Setup(repo => repo.AddNewOrderAsync(request))
                                .ThrowsAsync(new Exception("Database error"));
            
            var result = await _ordersController.CreateOrder(request);

            var internalServerErrorResult = result.Result as StatusCodeResult;
            internalServerErrorResult.Should().NotBeNull();
            internalServerErrorResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task GetOrdersWithinDays_ShouldReturnOrders_ExcludingWeekendsAndHolidays()
        {
            int days = 5;
            var currentDate = DateTime.UtcNow.Date;
            var orders = new List<Order>
            {
                new Order { EntryDate = currentDate.AddDays(-1), IsDeleted = false },
                new Order { EntryDate = currentDate.AddDays(-3), IsDeleted = false },
                new Order { EntryDate = currentDate.AddDays(-7), IsDeleted = false }
            };

            _mockRepository
                .Setup(repo => repo.GetOrdersForSpecificDaysAsync(days))
                .ReturnsAsync(orders);

            var result = await _ordersController.GetOrdersForSpecificDays(days);

            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedOrders = okResult.Value as List<Order>;
            returnedOrders.Should().NotBeNull();
            returnedOrders.Should().ContainSingle(o => o.EntryDate.Date == currentDate.AddDays(-1).Date);
            returnedOrders.Should().ContainSingle(o => o.EntryDate.Date == currentDate.AddDays(-3).Date);
        }


        [Fact]
        public async Task GetOrdersWithinDays_ShouldReturnBadRequest_WhenDaysIsZeroOrNegative()
        {
            int days = 0;
            var result = await _ordersController.GetOrdersForSpecificDays(days);

            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            badRequestResult.Value.Should().Be("The number of days must be greater than zero.");
        }
    }

}
