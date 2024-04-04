using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using proiect.Data;
using proiect.Models;

namespace proiect.Controllers
{
    public class ReviewsController : Controller
    {
        // PASUL 10 - useri si roluri

        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public ReviewsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }

        // stergerea unui review asociat unui produs din baza de date
        // se poate sterge review-ul doar de catre userii cu rolul Admin
        // sau de catre userii cu rolul User sau Colaborator doar daca review-ul
        // a fost lasat de acestia
        [HttpPost]
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Delete(int id)
        {
            Review rev = db.Reviews.Find(id);
            if (rev.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Reviews.Remove(rev);
                db.SaveChanges();
                TempData["message"] = "Review-ul a fost sters";
                TempData["messageType"] = "alert-success";
                return Redirect("/Products/Show/" + rev.ProductId);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti review-ul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }

        }

        // editarea unui review asociat unui produs din baza de date
        // se poate edita review-ul doar de catre userii care au lasat review-ul
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Edit(int id)
        {
            Review rev = db.Reviews.Find(id);
            if (rev.UserId == _userManager.GetUserId(User))
            {
                return View(rev);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati review-ul!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Edit(int id, Review requestReview)
        {
            Review rev = db.Reviews.Find(id);
            if (rev.UserId == _userManager.GetUserId(User))
            {
                if (ModelState.IsValid)
                {
                    rev.Content = requestReview.Content;
                    db.SaveChanges();
                    TempData["message"] = "Review-ul a fost editat";
                    TempData["messageType"] = "alert-success";
                    return Redirect("/Products/Show/" + rev.ProductId);
                }
                else
                {
                    return View(requestReview);
                }
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }

        }
    }
}
