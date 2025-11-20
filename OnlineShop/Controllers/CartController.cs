using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.Controllers;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ----------------- РАБОТА С КОРЗИНОЙ -----------------

    // Показать корзину
    public async Task<IActionResult> Index()
    {
        var cartId = GetCartId();

        // Выбираем товары ТОЛЬКО для текущего пользователя (cartId)
        var cartItems = await _context.CartItems
            .Include(c => c.Product) // Подгружаем данные о товаре (название, цена)
            .Where(c => c.CartId == cartId)
            .ToListAsync();

        return View(cartItems);
    }

    // Добавить товар в корзину
    public async Task<IActionResult> AddToCart(int productId)
    {
        var cartId = GetCartId();

        // Проверяем, есть ли уже такой товар в корзине у этого юзера
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.CartId == cartId && c.ProductId == productId);

        if (cartItem == null)
        {
            // Если нет - создаем новый
            cartItem = new CartItem
            {
                ProductId = productId,
                CartId = cartId,
                Quantity = 1
            };
            _context.CartItems.Add(cartItem);
        }
        else
        {
            // Если есть - увеличиваем количество
            cartItem.Quantity++;
        }

        await _context.SaveChangesAsync();

        // Возвращаемся обратно в каталог (или в корзину)
        return RedirectToAction("Index", "Home"); 
    }

    // Удалить один товар
    public async Task<IActionResult> RemoveItem(int id)
    {
        var cartItem = await _context.CartItems.FindAsync(id);

        if (cartItem != null)
        {
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }

    // Очистить всю корзину
    public async Task<IActionResult> ClearCart()
    {
        var cartId = GetCartId();
        var cartItems = _context.CartItems.Where(c => c.CartId == cartId);

        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    // ----------------- ОФОРМЛЕНИЕ ЗАКАЗА (НОВОЕ) -----------------

    // 1. Показать страницу оформления (GET)
    [HttpGet]
    public IActionResult Checkout()
    {
        return View();
    }

    // 2. Обработать нажатие кнопки "Купить" (POST)
    [HttpPost]
    public async Task<IActionResult> Checkout(Order order)
    {
        var cartId = GetCartId();
        var cartItems = await _context.CartItems
            .Include(c => c.Product)
            .Where(c => c.CartId == cartId)
            .ToListAsync();

        // Если корзина пуста - не даем оформить
        if (cartItems.Count == 0)
        {
            ModelState.AddModelError("", "Корзина пуста!");
            return View(order);
        }

        if (ModelState.IsValid)
        {
            // 1. Считаем итоговую сумму
            order.TotalPrice = cartItems.Sum(x => x.Quantity * x.Product.Price);
            order.OrderDate = DateTime.Now;

            // 2. Сохраняем заказ в БД
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 3. Очищаем корзину (товары куплены)
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            // 4. Переходим на страницу "Спасибо"
            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }

        return View(order);
    }

    // Страница подтверждения
    public IActionResult OrderConfirmation(int id)
    {
        return View(id); // Передаем ID заказа, чтобы показать номер
    }

    // ----------------- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ -----------------

    // Метод для идентификации пользователя (через Куки)
    private string GetCartId()
    {
        // Если у пользователя уже есть кука с ID корзины - берем её
        if (HttpContext.Request.Cookies.ContainsKey("CartId"))
        {
            return HttpContext.Request.Cookies["CartId"]!;
        }

        // Если нет - придумываем новый случайный GUID и записываем в куки
        var newCartId = Guid.NewGuid().ToString();
        HttpContext.Response.Cookies.Append("CartId", newCartId);
        return newCartId;
    }
}