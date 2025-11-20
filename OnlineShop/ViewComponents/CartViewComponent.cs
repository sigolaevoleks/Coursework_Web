using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;

namespace OnlineShop.ViewComponents;

public class CartViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public CartViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Получаем ID корзины (из куки)
        var cartId = Request.Cookies["CartId"];
        
        int totalItems = 0;

        if (cartId != null)
        {
            // Считаем сумму всех Quantity (количества) товаров в корзине
            totalItems = await _context.CartItems
                .Where(c => c.CartId == cartId)
                .SumAsync(c => c.Quantity);
        }

        // Передаем число в представление
        return View(totalItems);
    }
}