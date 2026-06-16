using CollectionHub.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace CollectionHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartApiController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartApiController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpPost("remove")]
        public IActionResult Remove([FromBody] CartItemRequest request)
        {
            _cartService.RemoveFromCart(request.ItemId);
            return Ok(new { success = true, totalItems = _cartService.GetCart().TotalItems });
        }

        [HttpPost("clear")]
        public IActionResult Clear()
        {
            _cartService.ClearCart();
            return Ok(new { success = true, totalItems = 0 });
        }
    }

    public class CartItemRequest
    {
        public int ItemId { get; set; }
    }
}
