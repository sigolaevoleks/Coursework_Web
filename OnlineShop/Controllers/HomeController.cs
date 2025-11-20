using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Нужно для работы с базой
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context; // Ссылка на базу данных

    // Получаем базу данных через конструктор
    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // Этот метод открывает главную страницу
    public async Task<IActionResult> Index()
    {
        // 1. Берем все товары из базы
        var products = await _context.Products.ToListAsync();

        // 2. Узнаем ID корзины текущего пользователя
        var cartId = GetCartId();

        // 3. Получаем список ID товаров, которые УЖЕ лежат в корзине у этого пользователя
        // На выходе будет список чисел, например: [1, 5, 8]
        var cartProductIds = await _context.CartItems
            .Where(c => c.CartId == cartId)
            .Select(c => c.ProductId)
            .ToListAsync();

        // 4. Кладем этот список в "рюкзак" (ViewBag), чтобы использовать его на странице (в View)
        // Это нужно, чтобы понять, какую кнопку рисовать: "В корзину" или "Уже в корзине"
        ViewBag.CartProductIds = cartProductIds;

        return View(products);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Вспомогательный метод: определяет ID корзины (из куки или создает новый)
    private string GetCartId()
    {
        if (HttpContext.Request.Cookies.ContainsKey("CartId"))
        {
            return HttpContext.Request.Cookies["CartId"]!;
        }

        var newCartId = Guid.NewGuid().ToString();
        HttpContext.Response.Cookies.Append("CartId", newCartId);
        
        return newCartId; // ВОТ ЭТОЙ СТРОКИ НЕ ХВАТАЛО
    }
}