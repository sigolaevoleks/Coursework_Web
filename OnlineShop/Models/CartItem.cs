using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models;

public class CartItem
{
    public int Id { get; set; }

    // Какой товар выбрали
    public Product Product { get; set; } 
    public int ProductId { get; set; }

    // Сколько штук
    public int Quantity { get; set; }

    // ID корзины (чтобы различать покупателей). 
    // Сюда будем писать случайный GUID или email пользователя.
    public string CartId { get; set; } 
    
    // Дата добавления (чтобы потом можно было чистить старые корзины)
    public DateTime DateCreated { get; set; } = DateTime.Now;
}