using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proiect.Controllers;
using proiect.Data;
using proiect.Models;


namespace ProiectDAW.Controllers
{
    [Authorize]
    public class BasketController : Controller
    {
        private IWebHostEnvironment _env;
        private ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public BasketController(
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

        public async Task<ActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var basketProducts = GetBasketProducts(user);

            return View(basketProducts);
        }

        public async Task<ActionResult> New(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            await AddProductToBasket(user, id);

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Empty()
        {
            var user = await _userManager.GetUserAsync(User);
            await EmptyBasket(user);

            return RedirectToAction("Index");
        }

        private List<int> ParseProductIds(string products)
        {
            if (string.IsNullOrEmpty(products))
                return new List<int>();

            return products.Split(',').Select(int.Parse).ToList();
        }

        private List<Product> GetBasketProducts(ApplicationUser user)
        {
            var products = new List<Product>();
            int totalPrice = 0;

            if (user != null)
            {
                var productIds = ParseProductIds(user.Basket);
                foreach (int productId in productIds)
                {
                    var product = db.Products.Include(p => p.Reviews).FirstOrDefault(p => p.ProductId == productId);

                    if (product != null)
                    {
                        ProductsController.CalculateProductFinalRating(product);
                        products.Add(product);
                        totalPrice += product.Price;
                    }
                }
            }

            ViewBag.Products = products;
            ViewBag.TotalPrice = totalPrice;

            return products;
        }

        private async Task AddProductToBasket(ApplicationUser user, int productId)
        {
            var productIds = ParseProductIds(user.Basket);
            productIds.Add(productId);
            user.Basket = string.Join(",", productIds);

            await _userManager.UpdateAsync(user);
        }

        private async Task EmptyBasket(ApplicationUser user)
        {
            user.Basket = "";
            await _userManager.UpdateAsync(user);
        }

        //stergem un produs din cos
        [HttpPost]
        public async Task<ActionResult> Remove(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            await RemoveProductFromBasket(user, id);

            return RedirectToAction("Index");
        }

        private async Task RemoveProductFromBasket(ApplicationUser user, int productId)
        {
            var productIds = ParseProductIds(user.Basket);
            productIds.Remove(productId);
            user.Basket = string.Join(",", productIds);

            await _userManager.UpdateAsync(user);
        }

    }
}
