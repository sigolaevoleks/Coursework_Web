using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите бренд (Apple, Samsung...)")]
    public string Brand { get; set; } = string.Empty; // Apple, Samsung, Xiaomi

    [Required(ErrorMessage = "Укажите название модели")]
    public string Model { get; set; } = string.Empty; // iPhone 15, Galaxy S24

    [Required]
    [Range(0, 1000000, ErrorMessage = "Цена должна быть положительной")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    public string? ImageUrl { get; set; } // Ссылка на фото

    public string? Description { get; set; } // Описание характеристик
    
    public int Stock { get; set; } // Сколько штук на складе
}