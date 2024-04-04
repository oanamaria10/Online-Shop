using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using proiect.Data;
using proiect.Models;
using System;
using System.Text.RegularExpressions;
using Ganss.Xss;

namespace proiect.Controllers
{
    public class ProductsController : Controller
    {
        private IWebHostEnvironment _env;
        private  ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ProductsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IWebHostEnvironment env
        )
        {
            db = context;
            _env = env;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // se afiseaza lista tuturor produselor impreuna cu categoria din care fac parte
        // pentru fiecare produs se afiseaza si userul care a postat produsul respectiv
        // HttpGet implicit
        public IActionResult Index()
        {
            var products = db.Products.Include("Category").Include("User").Include("Reviews").OrderBy(a => a.Date);

            ViewBag.Products = products;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            foreach (Product prod in products)
            {
                CalculateProductFinalRating(prod);
                db.SaveChanges();
            }

            var search = "";

            var sort = "";

            // MOTOR DE CAUTARE

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                // Eliminam spatiile libere
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                // Cautare in produs dupa denumire
                List<int> productsIds = db.Products.Where(at => at.Title.Contains(search))
                                                    .Select(a => a.ProductId).ToList();

                // Lista produselor care contin cuvantul cautat in denumire 
                // Rezultatele motorului de cautare sunt sortate crescator, respectiv descrescator,
                // in functie de pret si de rating 
                sort = Convert.ToString(HttpContext.Request.Query["sort"]);

                // sortarea implicita este crescatoare dupa pret

                switch (sort)
                {
                    case "priceDESC":
                        products = db.Products.Where(product => productsIds.Contains(product.ProductId))
                                                .Include("Category")
                                                .Include("User")
                                                .OrderByDescending(a => a.Price);
                        break;

                    case "finalratingASC":
                        products = db.Products.Where(product => productsIds.Contains(product.ProductId))
                                                .Include("Category")
                                                .Include("User")
                                                .OrderBy(a => a.FinalRating);
                        break;
                    case "finalratingDESC":
                        products = db.Products.Where(product => productsIds.Contains(product.ProductId))
                                                .Include("Category")
                                                .Include("User")
                                                .OrderByDescending(a => a.FinalRating);
                        break;
                    default:
                        products = db.Products.Where(product => productsIds.Contains(product.ProductId))
                                                .Include("Category")
                                                .Include("User")
                                                .OrderBy(a => a.Price);
                        break;
                }
            }
            ViewBag.SearchString = search;
            ViewBag.SortString = sort;
            // Alegem sa afisam 3 articole pe pagina 
            int _perPage = 3;
            
            // Fiind un numar variabil de articole, verificam de fiecare data utilizand
            // metoda Count()
            int totalItems = products.Count();

            // Se preia pagina curenta din View-ul asociat
            // Numarul paginii este valoarea parametrului page din ruta /Products/Index?page=valoare
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            // Pentru prima pagina offsetul o sa fie zero
            // Pentru pagina 2 o sa fie 3
            // Asadar offsetul este egal cu numarul de articole care au fost deja afisate pe paginile anterioare
            var offset = 0;

            // Se calculeaza offsetul in functie de numarul paginii la care suntem
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            // Se preiau articolele corespunzatoare pentru fiecare pagina la care ne aflam
            // in functie de offset
            var paginatedProducts = products.Skip(offset).Take(_perPage);

            // Preluam numarul ultimei pagini
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            // Trimitem articolele cu ajutorul unui ViewBag catre View-ul corespunzator
            ViewBag.Products = paginatedProducts;

            if (search != "")
            {
                if (sort != "")
                {
                    ViewBag.PaginationBaseUrl = "/Products/Index/?search=" + search + "&sort=" + sort + "&page";
                }
                else
                {
                    ViewBag.PaginationBaseUrl = "/Products/Index/?search=" + search + "&page";
                }
            }
            else
            {
                if (sort != "")
                {
                    ViewBag.PaginationBaseUrl = "/Products/Index/?search=&sort=" + sort + "&page";
                }
                else
                {
                    ViewBag.PaginationBaseUrl = "/Products/Index/?page";
                }
            }

            return View();
        }
       
        static public void CalculateProductFinalRating(Product product)
        {
            var NrReviews = product.Reviews.Count;
            product.FinalRating = 0;
            foreach (Review rev in product.Reviews)
                product.FinalRating += rev.Rating;
            if (NrReviews != 0)
                product.FinalRating = product.FinalRating / NrReviews;
            else
                product.FinalRating = 0;
        
        }

        // se afiseaza un singur produs in functie de id-ul sau 
        // impreuna cu categoria din care face parte
        // in plus sunt preluate si toate review-urile asociate unui produs
        // pentru fiecare articol se afiseaza si userul care a postat produsul respectiv
        // HttpGet implicit
        public IActionResult Show(int id)
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            Product product = db.Products.Include("Category")
                                         .Include("User")
                                         .Include("Reviews")
                                         .Include("Reviews.User")
                            .Where(prod => prod.ProductId == id)
                            .First();

            CalculateProductFinalRating(product);

            SetAccessRights();

            if(product.Approved == true || User.IsInRole("Admin"))
            {
                return View(product);
            }
            else
            {
                TempData["message"] = "Produsul este in curs de aprobare!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        // adaugarea unui review asociat unui produs in baza de date
        // toate rolurile pot adauga review-uri in baza de date
        [HttpPost]
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Show([FromForm] Review review)
        {
            review.Date = DateTime.Now;

            // preluam id-ul utilizatorului care posteaza review-ul
            review.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                TempData["message"] = "Review-ul a fost adaugat!";
                TempData["messageType"] = "alert-success";
                db.Reviews.Add(review);
                db.SaveChanges();
                return Redirect("/Products/Show/" + review.ProductId);
            }

            else
            {
                Product prod = db.Products.Include("Category")
                                         .Include("User")
                                         .Include("Reviews")
                                         .Include("Reviews.User")
                                         .Where(prod => prod.ProductId == review.ProductId)
                                         .First();


                SetAccessRights();

                return View(prod);
            }
        }

        // se afiseaza formularul in care se vor completa datele unui produs
        // impreuna cu selectarea categoriei din care face parte
        // doar userii cu rolul de Colaborator sau Admin pot adauga produse in platforma
        // HttpGet implicit
        [Authorize(Roles = "Colaborator,Admin")]
        public IActionResult New()
        {
            Product product = new Product();
            // Se preia lista de categorii cu ajutorul metodei GetAllCategories()
            product.Categ = GetAllCategories();
            product.Approved = User.IsInRole("Admin");
            return View(product);
        }

        // se adauga produsul in baza de date
        // doar userii cu rolul de Colaborator sau Admin pot adauga produse in platforma
        [Authorize(Roles = "Colaborator,Admin")]
        [HttpPost]
        public async Task<IActionResult> New(Product product, IFormFile image)
        {
            var sanitizer = new HtmlSanitizer();

            if (ModelState.IsValid)
            {
                if (image != null && image.Length > 0)
                {
                    // Generam calea de stocare a fisierului
                    var storagePath = Path.Combine(
                            _env.WebRootPath, // Preluam calea folderului wwwroot
                            "images", // Adaugam calea folderului images
                            image.FileName // Numele fisierului
                    );

                    // Generam calea de afisare a fisierului care va fi stocata in baza de date
                    var databaseFileName = "/images/" + image.FileName;

                    // Uploadam fisierul la calea de storage
                    using (var fileStream = new FileStream(storagePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    // Salvam storagePath-ul in baza de date
                    product.Image = databaseFileName;
                }

                product.Description = sanitizer.Sanitize(product.Description);
                product.Date = DateTime.Now;
                // preluam id-ul utilizatorului care adauga produsul
                // User este instanta curenta care adauga produsul
                product.UserId = _userManager.GetUserId(User);
                product.Approved = User.IsInRole("Admin"); 

                // Adaugam produsul in baza de date
                db.Products.Add(product);
                db.SaveChanges();

                if(product.Approved == true)
                {
                    TempData["message"] = "Produsul a fost adaugat";
                    TempData["messageType"] = "alert-success";
                }
                else
                {
                    TempData["message"] = "Produsul este in curs de aprobare";
                    TempData["messageType"] = "alert-success";
                }
                return RedirectToAction("Index");
            }
            else
            {
                // Adaugam lista de categorii la model in caz de eroare
                product.Categ = GetAllCategories();
                return View(product);
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Approve()
        {
            var products = db.Products.Include("Category").Include("User");
            ViewBag.Products = products;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Approve(int id)
        {
            Product product = db.Products.Find(id);
            product.Approved = true;
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                TempData["message"] = "Produsul a fost aprobat!";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Produsul nu este valid!";
                TempData["messageType"] = "alert-danger";
                return View();
            }
        }

        // se editeaza un produs existent in baza de date impreuna cu categoria din care face parte
        // categoria se selecteaza dintr-un dropdown
        // doar userii cu rolul de Colaborator sau Admin pot edita produse
        // adminii pot edita orice produs din baza de date
        // colaboratorii pot edita doar produsule proprii din baza de date
        // HttpGet implicit
        [Authorize(Roles = "Colaborator,Admin")]
        public ActionResult Edit(int id)
        {
            Product product = db.Products.Find(id);
            product.Categ = GetAllCategories();
            if (product.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(product);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui produs care nu va apartine!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }   
            
        }

        // se adauga produsul modificat in baza de date
        // verificam rolul utilizatorilor care au dreptul sa editeze (Colaborator sau Admin)
        [Authorize(Roles = "Colaborator,Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product requestProduct, IFormFile image)
        {
            Product product = db.Products.Find(id);

            var sanitizer = new HtmlSanitizer();

            if (ModelState.IsValid)
            {
                if (product.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    if (image != null && image.Length > 0)
                    {
                        // Generam calea de stocare a fisierului
                        var storagePath = Path.Combine(
                                _env.WebRootPath, // Preluam calea folderului wwwroot
                                "images", // Adaugam calea folderului images
                                image.FileName // Numele fisierului
                        );

                        // Generam calea de afisare a fisierului care va fi stocata in baza de date
                        var databaseFileName = "/images/" + image.FileName;

                        // Uploadam fisierul la calea de storage
                        using (var fileStream = new FileStream(storagePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                        }

                        // Salvam storagePath-ul in baza de date
                        product.Image = databaseFileName;
                    }
                    product.Title = requestProduct.Title;
                    requestProduct.Description = sanitizer.Sanitize(requestProduct.Description);
                    product.Description = requestProduct.Description;
                    product.Price = requestProduct.Price;
                    product.CategoryId = requestProduct.CategoryId;
                    TempData["message"] = "Produsul a fost modificat";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui produs care nu va apartine!";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                if(image == null)
                {
                    TempData["message"] = "Incarcati o imagine!";
                }
                requestProduct.Categ = GetAllCategories();
                return View(requestProduct);
            }
        }

        // se sterge un produs din baza de date 
        // utilizatorii cu rolul de Colaborator sau Admin pot sterge produse
        // colaboratorii pot sterge doar produsele adaugate de ei
        // adminii pot sterge orice produs din baza de date
        [HttpPost]
        public ActionResult Delete(int id)
        {
            // pentru a functiona stergerea in cascada trebuie sa ii scriem explicit si sa ii dam lista de review-uri
            Product product = db.Products.Include("Reviews")
                                         .Where(art => art.ProductId == id)
                                         .First();
            if (product.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Products.Remove(product);
                db.SaveChanges();
                if(product.Approved == false)
                {
                    TempData["message"] = "Produsul a fost respins";
                    TempData["messageType"] = "alert-success";
                }
                else
                {
                    TempData["message"] = "Produsul a fost sters";
                    TempData["messageType"] = "alert-success";
                }
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un produs care nu va apartine.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        // conditiile de afisare a butoanelor de editare si stergere
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Colaborator"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista goala
            var selectList = new List<SelectListItem>();
            // extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;
            // iteram prin categorii
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                selectList.Add(new SelectListItem
                {
                    Value = category.CategoryId.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }
            // returnam lista de categorii
            return selectList;
        }
    }
}
