using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Vjezba.DAL;
using Vjezba.Model;
using Vjezba.Web.Models;

namespace Vjezba.Web.Controllers
{
    [Authorize]
    public class ClientController : BaseController
    {
        private ClientManagerDbContext _dbContext;
        private UserManager<AppUser> _userManager;

        public ClientController(ClientManagerDbContext dbContext, UserManager<AppUser> userManager) : base(userManager)
        {
            this._dbContext = dbContext;
            this._userManager = userManager;
        }

        [AllowAnonymous]
        [HttpPost]
        public PartialViewResult IndexAjax(ClientFilterModel filter)
        {
            IEnumerable<Client> clientQuery = this._dbContext.Clients.Include(c => c.City).ToList();

            if (!string.IsNullOrWhiteSpace(filter.FullName))
                clientQuery = clientQuery.Where(p => p.FullName.ToLower().Contains(filter.FullName.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.Address))
                clientQuery = clientQuery.Where(p => p.Address.ToLower().Contains(filter.Address.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.Email))
                clientQuery = clientQuery.Where(p => p.Email.ToLower().Contains(filter.Email.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.City))
                clientQuery = clientQuery.Where(p => p.City != null && p.City.Name.ToLower().Contains(filter.City.ToLower()));

            var model = clientQuery.ToList();
            return PartialView("_ClientTable", model);
        }

        [AllowAnonymous]
        public IActionResult Index(string query = null)
        {
            if(query == null)
            {
                query = "";
            }

            var clientQuery = this._dbContext.Clients
                .Include(c => c.City)
                .Where(c => c.FullName.ToLower().Contains(query));

        

            return View(clientQuery.ToList());
        }

        public async Task<IActionResult> UploadAttachment(int clientId, List<IFormFile> file)
        {
            long size = file.Sum(f => f.Length);
            var client = this._dbContext.Clients.Where(p => p.ID == clientId).First();

            foreach (var formFile in file)
            {
                if (formFile.Length > 0)
                {
                    var filePath = Path.Combine("Uploads", System.DateTime.Now.Ticks + formFile.FileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        this._dbContext.Attachments.Add(new Attachment() { AttachmentPath = filePath, ClientID = clientId });
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            var ok = await this.TryUpdateModelAsync(client);

            if (ok)
            {
                this._dbContext.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpDelete]
        public ActionResult DeleteAttachment(int id)
        {
            var att = this._dbContext.Attachments.Where(a => a.ID == id).FirstOrDefault();
            if (att == null)
            {
                return BadRequest();
            }

            int idd = att.ClientID;


            if (System.IO.File.Exists(att.AttachmentPath))
            {
                try
                {
                    System.IO.File.Delete(att.AttachmentPath);
                }
                catch (System.IO.IOException e)
                {
                    return BadRequest();
                }
            }

            this._dbContext.Attachments.Remove(att);
            this._dbContext.SaveChanges();

            return GetAttachments(idd);
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult GetAttachments(int id)
        {
            Client klijent = this._dbContext.Clients.Include(c => c.City).Where(c => c.ID == id).Include(a => a.Attachments).FirstOrDefault();
            return PartialView("_FileTable", klijent);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Index(string queryName, string queryAddress)
        {
            IEnumerable<Client> clientQuery = this._dbContext.Clients.Include(c => c.City).ToList();

            ////Primjer iterativnog građenja upita - dodaje se "where clause" samo u slučaju da je parametar doista proslijeđen.
            ////To rezultira optimalnijim stablom izraza koje se kvalitetnije potencijalno prevodi u SQL
            if (!string.IsNullOrWhiteSpace(queryName))
                clientQuery = clientQuery.Where(p => p.Address.ToLower().Contains(queryName));
                    
            if (!string.IsNullOrWhiteSpace(queryAddress))
                clientQuery = clientQuery.Where(p => p.Address.ToLower().Contains(queryAddress));

            var model = clientQuery.ToList();
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult AdvancedSearch(ClientFilterModel filter)
        {
            IEnumerable<Client> clientQuery = this._dbContext.Clients.Include(c => c.City).ToList();

            ////Primjer iterativnog građenja upita - dodaje se "where clause" samo u slučaju da je parametar doista proslijeđen.
            ////To rezultira optimalnijim stablom izraza koje se kvalitetnije potencijalno prevodi u SQL
            if (!string.IsNullOrWhiteSpace(filter.FullName))
                clientQuery = clientQuery.Where(p => p.FullName.ToLower().Contains(filter.FullName.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.Address))
                clientQuery = clientQuery.Where(p => p.Address.ToLower().Contains(filter.Address.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.Email))
                clientQuery = clientQuery.Where(p => p.Email.ToLower().Contains(filter.Email.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.City))
                clientQuery = clientQuery.Where(p => p.City != null && p.City.Name.ToLower().Contains(filter.City.ToLower()));

            var model = clientQuery.ToList();
            return View("Index", model);
        }

        public IActionResult Details(int? id = null)
        {
            Client model = id != null ? this._dbContext.Clients
                .Include(c => c.City).Where(c => c.ID == id).FirstOrDefault() : null;

            return View(model);

        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            ViewBag.PossibleCategories = fillDropdownValues();

            return View();
        }

        [HttpPost]
        public IActionResult Create(Client client)
        {
            if(this.ModelState.IsValid)
            {
                client.CreatedById = base.UserId;
                this._dbContext.Clients.Add(client);
                this._dbContext.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(client);
        }

        [Authorize(Roles = "Admin,Manager")]
        [ActionName("Edit")]
        public ActionResult EditGet(int id)
        {
            Client klijent = this._dbContext.Clients.Include(c => c.City).Where(c => c.ID == id).Include(a => a.Attachments).FirstOrDefault();

            if (klijent == null)
                return NotFound();

            ViewBag.PossibleCategories = fillDropdownValues();
            ViewData.Add("Cities", fillDropdownValues());
            return View(klijent);
        }

        [HttpPost]
        [ActionName("Edit")]
        public async Task<ActionResult> EditPost(int id)
        {
            Client klijent = this._dbContext.Clients.Include(c => c.City).Where(c => c.ID == id).First();
            klijent.UpdatedById = base.UserId;
            var ok = await this.TryUpdateModelAsync(klijent);

            if (ok)
            {
                this._dbContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        private List<SelectListItem> fillDropdownValues()
        {
            var selectItems = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();

            var listItem = new SelectListItem();
            listItem.Text = "- odaberite -";
            listItem.Value = "";
            selectItems.Add(listItem);

            foreach (var city in _dbContext.Cities.ToArray())
            {
                listItem = new SelectListItem();
                listItem.Text = city.Name;
                listItem.Value = city.ID.ToString();
                selectItems.Add(listItem);
            }

            return selectItems;
        }
    }
}