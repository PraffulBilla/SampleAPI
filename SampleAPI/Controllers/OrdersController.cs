using Microsoft.AspNetCore.Mvc;
using SampleAPI.Entities;
using SampleAPI.Repositories;
using SampleAPI.Requests;

namespace SampleAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrdersController> _logger;
        public OrdersController(IOrderRepository orderRepository, ILogger<OrdersController> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        [HttpGet("recentOrders")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Order>>> GetOrders()
        {
            try
            {
                var orders = await _orderRepository.GetOrdersAsync();

                if (orders == null || orders.Count == 0)
                {
                    return NotFound("No recent orders found.");
                }

                return Ok(orders);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, "Argument Exception Occured");
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e, "Key Not Found");
                return StatusCode(StatusCodes.Status404NotFound);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Something went wrong");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var newOrder = await _orderRepository.AddNewOrderAsync(request);

                if (newOrder == null)
                {
                    return BadRequest("Order creation failed.");
                }

                return CreatedAtAction(nameof(GetOrders), new { id = newOrder.Id }, newOrder);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, "Argument Execption Occured");
                return StatusCode(StatusCodes.Status409Conflict);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e, "Key Not Found");
                return StatusCode(StatusCodes.Status404NotFound);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error on Saving data");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("ordersWithinDays/{days}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Order>>> GetOrdersForSpecificDays(int days)
        {
            try
            {
                if (days <= 0)
                {
                    return BadRequest("The number of days must be greater than zero.");
                }

                var orders = await _orderRepository.GetOrdersForSpecificDaysAsync(days);

                return Ok(orders);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Something went wrong");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
