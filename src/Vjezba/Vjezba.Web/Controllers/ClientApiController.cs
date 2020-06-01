using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using Vjezba.DAL;
using Vjezba.Model;
using Vjezba.Web.Models;

namespace Vjezba.Web.Controllers
{
    [Route("api/client")]
    [ApiController]
    public class ClientApiController : BaseController
    {
        private ClientManagerDbContext _dbContext;

        public ClientApiController(ClientManagerDbContext dbContext, UserManager<AppUser> userManager) : base(userManager)
        {
            this._dbContext = dbContext;
        }

        public IActionResult Get()
        {
            var clients = this._dbContext.Clients
                .GetDTOs()
                .ToList();
            return Ok(clients);
        }

        [Authorize]
        [Route("{id}")]
        public IActionResult Get(int id)
        {
            var client = this._dbContext.Clients
                .GetDTOs()
                .Where(p => p.ID == id)
                .FirstOrDefault();

            if (client == null)
            {
                return NotFound();
            }
            return Ok(client);
        }

        [Route("pretraga/{q}")]
        public IActionResult Get(string q)
        {
            var clients = this._dbContext.Clients
                .GetDTOs()
               .Where(p => 
                    p.FullName.ToLower().Contains(q.ToLower()) || 
                    p.Email.ToLower().Contains(q.ToLower()) ||
                    p.Address.ToLower().Contains(q.ToLower())
                )
                .ToList();

            if (clients == null)
            {
                return NotFound();
            }
            return Ok(clients);
        }

        [Authorize(Roles = "Admin,Manager")]
        [Route("{id}")]
        [HttpPut]
        public async Task<ActionResult<Client>> Put(int id, [FromBody] Client model)
        {
            Client klijent = this._dbContext.Clients.Include(c => c.City).Where(c => c.ID == id).First();

            var ok = await this.TryUpdateModelAsync(klijent);

            if (ok)
            {
                this._dbContext.SaveChanges();
                return Ok();
            }
            else return NotFound();
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public IActionResult Post([FromBody] Client model)
        {
            if (model == null)
                return BadRequest();
            this._dbContext.Clients.Add(model);
            this._dbContext.SaveChanges();

            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var client = this._dbContext.Clients.Where(p => p.ID == id).First();
            this._dbContext.Clients.Remove(client);
            this._dbContext.SaveChanges();
            return Ok(); 
        } 

    }

        public class ClientDTO
    {
        public int ID { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }
        public string Address { get; set; }

        public CityDTO City { get; set; }

    }

    public class CityDTO
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public static class MyExtensions
    {
        public static IQueryable<ClientDTO> GetDTOs(this DbSet<Client> clients)
        {
            return clients.Select(p => new ClientDTO()
            {
                ID = p.ID,
                FullName = $"{p.FirstName} {p.LastName}",
                Email = p.Email,
                Address = p.Address,
                City = new CityDTO()
                {
                    ID = p.City.ID,
                    Name = p.City.Name
                }
            });
        }
    }
}