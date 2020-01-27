using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Board.Models;
using Microsoft.AspNetCore.Http;

namespace Board.ViewModels
{
  public class AdvertCreateViewModel
  {
    public string Title { get; set; }

    public Categor? Category { get; set; }

    public ProductNew? ProductIsNew { get; set; }

    public decimal Price { get; set; }

    public bool IsNegotiatedPrice { get; set; }

    public string Description { get; set; }

    public List<IFormFile> Photos { get; set; }
  }
}
