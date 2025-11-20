using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models;

public class Order
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите имя")]
    public string Name { get; set; } = string.Empty; // ФИО

    [Required(ErrorMessage = "Введите адрес доставки")]
    public string Address { get; set; } = string.Empty; // Адрес

    [Required(ErrorMessage = "Введите телефон")]
    [Phone(ErrorMessage = "Некорректный номер")]
    public string Phone { get; set; } = string.Empty; // Телефон

    public decimal TotalPrice { get; set; } // Сумма заказа
    
    public DateTime OrderDate { get; set; } = DateTime.Now; // Когда заказали
}