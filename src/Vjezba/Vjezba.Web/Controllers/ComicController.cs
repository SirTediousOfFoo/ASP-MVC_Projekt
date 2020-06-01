using Vjezba.DAL;
using Vjezba.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using Vjezba.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Vjezba.Web.Controllers
{
    public class ComicController : Controller
    {
        private AwfulDbContext _dbContext;
        private UserManager<AppUser> _userManager;
        private readonly IHostingEnvironment _environment;

        public ComicController(AwfulDbContext dbContext, UserManager<AppUser> userManager, IHostingEnvironment environment)
        {
            this._dbContext = dbContext;
            this._userManager = userManager;
            this._environment = environment;
        }

        [AllowAnonymous]
        [Route("{id?}")]
        public IActionResult Index(int? id)
        {
            Comic comic;

            if (id.HasValue)
            {
                comic = this._dbContext.Comics.Where(c => c.ID == id).Include(c => c.Comments).Include(c => c.Category).FirstOrDefault();
            }
            else
            {
                comic = this._dbContext.Comics.Include(c => c.Comments).Include(c => c.Category).Last();
            }

            if (comic == null)
            {
                return NotFound();
            }

            return View(comic);
        }


        [Route("admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminPanel()
        {

            FillDropdownValues();

            return View();
        }

        [HttpPost]
        [Route("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminPanel(ComicUploadModel comic)
        {
            var file = comic.File;
            var uploads = Path.Combine(_environment.WebRootPath, "Uploads");
            if (file.Length > 0)
            {
                using (var fileStream = new FileStream(Path.Combine(uploads, file.FileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }

            if (this.ModelState.IsValid)
            {
                this._dbContext.Comics.Add(
                    new Comic
                    {
                        CreatorID = this._userManager.GetUserId(User),
                        PublishDate = DateTime.Now,
                        ContentPath = "Uploads/" + file.FileName,
                        Title = comic.Title,
                        Description = comic.Description,
                        CategoryID = comic.CategoryID
                    });
                this._dbContext.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            
            return View(comic);
        }

        [AllowAnonymous]
        public IActionResult Navigate(string where, int id)
        {
            if (where == "prev")
            {
                var comic = this._dbContext.Comics.Where(c => c.ID < id).Include(c => c.Comments).Include(c => c.Category).LastOrDefault();
                if (comic != null)
                    return PartialView("_Comic", comic);
                else
                    return PartialView("_Comic", this._dbContext.Comics.Include(c => c.Comments).Include(c => c.Category).First());
            }
            else if (where == "next")
            {
                var comic = this._dbContext.Comics.Where(c => c.ID > id).Include(c => c.Comments).Include(c => c.Category).FirstOrDefault();
                if (comic != null)
                    return PartialView("_Comic", comic);
                else
                    return PartialView("_Comic", this._dbContext.Comics.Include(c => c.Comments).Include(c => c.Category).Last());
            }
            else if (where == "first")
            {
                var comic = this._dbContext.Comics.Include(c => c.Comments).Include(c => c.Category).First();
                if (comic != null)
                    return PartialView("_Comic", comic);
            }
            else if (where == "last")
            {
                var comic = this._dbContext.Comics.Include(c => c.Comments).Include(c => c.Category).Last();
                if (comic != null)
                    return PartialView("_Comic", comic);
            }
            return NotFound();
        }

        [Route("archive")]
        public IActionResult Archive()
        {
            List<Comic> comics = new List<Comic>();

                comics = this._dbContext.Comics.Include(c => c.Category).Take(3).ToList();

            return View(comics);
        }

        [Route("archive/{id?}")]
        public IActionResult Archive(int? id)
        {
            FillDropdownValues();

            List<Comic> comics = new List<Comic>();
            if (!id.HasValue || id.Value == 0)
            {
                comics = this._dbContext.Comics.Include(c => c.Category).Take(3).ToList();
            }
            else if (id.Value > 0)
            {
                comics = this._dbContext.Comics.Include(c => c.Category).Skip(3 + 3 * (id.Value - 1)).Take(3).ToList();
            }

            return View(comics);
        }

        public IActionResult ComicList(int position, string where)
        {
            if (where != "admin")
            {
                ViewData["Hidden"] = "true";
            }
            else ViewData["Hidden"] = "false";

            List<Comic> comics = new List<Comic>();
            if (position == 0)
            {
                comics = this._dbContext.Comics.Include(c => c.Category).Take(3).ToList();
            }
            else if(position > 0)
            {
                comics = this._dbContext.Comics.Include(c => c.Category).Skip(3 * position).Take(3).ToList();
            }

            return PartialView("_ComicList", comics);
        }


        [AllowAnonymous]
        [HttpPost]
        public IActionResult Find(string title, string description)
        {
            ViewData["Hidden"] = "true";
            var comics = this._dbContext.Comics.Include(c => c.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                comics = comics.Where(c => c.Title.ToLower().Contains(title.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(description))
            {
                comics = comics.Where(c => c.Description.ToLower().Contains(description.ToLower()));
            }

            List<Comic> model = comics.ToList();
            return PartialView("_ComicList", model);
        }

        public IActionResult GetEditModal(int id)
        {
            FillDropdownValues();
            ViewBag.Edit = "true";
            return PartialView("_CreateOrEdit", this._dbContext.Comics.Where(c=>c.ID==id).First());
        }

        [Authorize]
        [HttpDelete]
        public IActionResult DeleteComment(int id)
        {
            var todelete = this._dbContext.Comments.Where(c => c.ID == id).First();
            this._dbContext.Remove(todelete);
            this._dbContext.SaveChanges();
            return Json("Success");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete]
        public IActionResult RemoveComic(int id)
        {
            var todelete = this._dbContext.Comics.Where(c => c.ID == id).First();
            this._dbContext.Remove(todelete);
            this._dbContext.SaveChanges();
            return Json("Success");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult EditComic(Comic comic)
        {
            var edited = this._dbContext.Comics.Where(c => c.ID == comic.ID).First();
            edited.Description = comic.Description;
            edited.CategoryID = comic.CategoryID;
            edited.Title = comic.Title;

            if (ModelState.IsValid)
            {
                this._dbContext.Update(edited);
                this._dbContext.SaveChanges();
                return RedirectToAction("AdminPanel");
            }

            return NotFound();
        }

        [Authorize]
        [HttpPost]
        public IActionResult AddComment(string comment, int modelID)
        {
            if (comment != null)
            {
                Comment nc = new Comment
                {
                    ComicID = modelID,
                    Content = comment,
                    UserID = this._userManager.GetUserId(User),
                    TimePosted = DateTime.Now,
                    PosterName = this._dbContext.AppUsers.Where(u => u.Id == this._userManager.GetUserId(User)).Select(u => u.FirstName).First()
                };
                
                this._dbContext.Comments.Add(nc);
                this._dbContext.SaveChanges();
                return PartialView("_miniComment", nc);
            }
            else return Json("Error");
        }

        private void FillDropdownValues()
        {
            var selectItems = new List<SelectListItem>();

            //Polje je opcionalno
            var listItem = new SelectListItem();
            listItem.Text = "- pick one -";
            listItem.Value = "";
            selectItems.Add(listItem);

            foreach (var category in this._dbContext.Categories)
            {
                listItem = new SelectListItem(category.Tag, category.ID.ToString());
                selectItems.Add(listItem);
            }

            ViewBag.Categories = selectItems;
        }
    }
}