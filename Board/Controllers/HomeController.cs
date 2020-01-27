using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Board.Models;
using Board.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBoard.Models;
using MyBoard.ViewModels;

namespace MyBoard.Controllers
{
  //[Authorize]
  public class HomeController : Controller
  {
    private readonly IWebHostEnvironment _hostEnvironment;
    public AppDbContext Context { get; }

    public HomeController(AppDbContext context, IWebHostEnvironment hostEnvironment)
    {
      _hostEnvironment = hostEnvironment;
      Context = context;
    }
    [AllowAnonymous]
    public IActionResult Index()
    {
      return View();
    }

    [AllowAnonymous]
    public async Task<IActionResult> List()
    {
      return View(await Context.Adverts.ToListAsync());
    }

    [HttpGet]
    //[AllowAnonymous]
    public IActionResult Create()
    {
      return View();
    }

    [HttpPost]
    public IActionResult Create(AdvertCreateViewModel model)
    {
      if (ModelState.IsValid)
      {
        string uniqueFileName = null;
        if (model.Photos != null && model.Photos.Count > 0)
        {

          foreach (var photo in model.Photos)
          {
            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images");
            uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            photo.CopyTo(new FileStream(filePath, FileMode.Create));
            //string photoKey = "{id}:{photoNumber}";
          }

        }

        Advert newAdvert = new Advert
        {
          Title = model.Title,
          Category = model.Category,
          ProductIsNew = model.ProductIsNew,
          Price = model.Price,
          IsNegotiatedPrice = model.IsNegotiatedPrice,
          Description = model.Description,
          PhotoPath = uniqueFileName
        };

        Context.Add(newAdvert);
        Context.SaveChanges();
        return RedirectToAction("Index", "Home");
      }

      return View();
    }

    public async Task<IActionResult> Details(int? id)
    {
      if (id != null)
      {
        Advert advert = await Context.Adverts.FirstOrDefaultAsync(p => p.Id == id);
        if (advert != null)
          return View(advert);
      }
      return NotFound();
    }

    [HttpPost]
    [AllowAnonymous]
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
      Response.Cookies.Append(
        CookieRequestCultureProvider.DefaultCookieName,
        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
        new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
      );

      return LocalRedirect(returnUrl);
    }

    //Alex
    //[HttpGet]
    //public IActionResult SetLanguage(string lang, string returnUrl)
    //{
    //  if (string.IsNullOrEmpty(lang))
    //  {
    //    lang = "ru";
    //  }

    //  if (returnUrl.Length >= 3)
    //  {
    //    returnUrl = returnUrl.Substring(3);
    //  }

    //  if (!(lang == "ru" && returnUrl.Trim() == "/"))
    //  {
    //    returnUrl = "/" + lang + returnUrl;
    //  }

    //  return Redirect(returnUrl);
    //}

    [HttpGet]

    public async Task<IActionResult> Edit(int? id)
    {
      Advert advert = await Context.Adverts.FirstOrDefaultAsync(p => p.Id == id);
      AdvertEditViewModel advertCreateViewModel = new AdvertEditViewModel
      {
        Id = advert.Id,
        Title = advert.Title,
        Category = advert.Category,
        ProductIsNew = advert.ProductIsNew,
        Price = advert.Price,
        IsNegotiatedPrice = advert.IsNegotiatedPrice,
        Description = advert.Description,
        ExistingPhotoPath = advert.PhotoPath
      };
      return View(advertCreateViewModel);
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
  }
}
