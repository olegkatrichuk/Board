using System.ComponentModel.DataAnnotations;

namespace Board.Models
{
  public class Advert
  {
    public int Id { get; set; }
    [Required]
    public string Title { get; set; }

    public Categor? Category { get; set; }

    public ProductNew? ProductIsNew { get; set; }

    public decimal Price { get; set; }
   
    public bool IsNegotiatedPrice { get; set; }

    public string Description { get; set; }

    public string PhotoPath { get; set; }
  }
}
