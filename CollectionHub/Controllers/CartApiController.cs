using CollectionHub.Models;
using CollectionHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace CollectionHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    // <summary>
    // Controlador da API responsável pela gestão do carrinho de compras do utilizador autenticado.
    // </summary>
    public class CartApiController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartApiController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // <summary>
        // Devolve o carrinho guardado na sessão atual.
        // </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ShoppingCart), StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            return Ok(_cartService.GetCart());
        }

        // <summary>
        // Actualiza a quantidade de cada item no carrinho. Se a quantidade for zero, o item é removido.
        // </summary>
        [HttpPost("update-quantity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult UpdateQuantity([FromBody] CartQuantityRequest request)
        {
            _cartService.UpdateQuantity(request.ItemId, request.Quantity);
            var cart = _cartService.GetCart();

            return Ok(new
            {
                success = true,
                totalItems = cart.TotalItems,
                total = cart.Total
            });
        }

        // <summary>
        // Remove um item do carrinho da sessão atual.
        // </summary>
        [HttpPost("remove")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Remove([FromBody] CartItemRequest request)
        {
            _cartService.RemoveFromCart(request.ItemId);
            var cart = _cartService.GetCart();

            return Ok(new
            {
                success = true,
                totalItems = cart.TotalItems,
                total = cart.Total
            });
        }

        // <summary>
        // Limpa todos os items do carrinho da sessão atual.
        // </summary>
        [HttpPost("clear")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Clear()
        {
            _cartService.ClearCart();
            return Ok(new { success = true, totalItems = 0, total = 0 });
        }
    }

    // <summary>
    // Representa cart item request no domínio da aplicação.
    // </summary>
    public class CartItemRequest
    {
        // <summary>
        // Obtém ou define item identificador.
        // </summary>
        public int ItemId { get; set; }
    }

    // <summary>
    // Representa cart quantity request no domínio da aplicação.
    // </summary>
    public class CartQuantityRequest : CartItemRequest
    {
        // <summary>
        // Obtém ou define quantidade.
        // </summary>
        public int Quantity { get; set; }
    }
}
