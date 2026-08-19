/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 */

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnicornShopLegacy.Interfaces;

namespace UnicornShopLegacy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BasketController : ControllerBase
    {
        private IUnishopEntities unishopEntitiesContext;

        public BasketController(IUnishopEntities databaseContext)
        {
            this.unishopEntitiesContext = databaseContext;
        }

        // GET: api/Basket
        [HttpGet]
        public IQueryable<basket> GetUnicornBaskets()
        {
            return this.unishopEntitiesContext.baskets;
        }

        // GET: api/Basket/f29b70d8-2994-4cea-861e-61903801dd98
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUnicornBasket(Guid id)
        {
            var unicornBasket = from ub in this.unishopEntitiesContext.baskets
                                where ub.user_id == id
                                select ub;

            if (!unicornBasket.Any())
            {
                return this.NotFound();
            }

            return this.Ok(unicornBasket);
        }

        // PUT: api/Basket/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUnicornBasket(Guid id, basket unicornBasket)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            if (id != unicornBasket.basket_id)
            {
                return this.BadRequest();
            }

            this.unishopEntitiesContext.SetModified(unicornBasket);

            try
            {
                await this.unishopEntitiesContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!this.UnicornBasketExists(id))
                {
                    return this.NotFound();
                }
                else
                {
                    throw;
                }
            }

            return this.StatusCode((int)HttpStatusCode.NoContent);
        }

        // POST: api/Basket
        [HttpPost]
        public async Task<IActionResult> PostUnicornBasket(basket unicornBasket)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            unicornBasket.basket_id = Guid.NewGuid();
            this.unishopEntitiesContext.baskets.Add(unicornBasket);
            await this.unishopEntitiesContext.SaveChangesAsync();

            return this.CreatedAtRoute("DefaultApi", new { id = unicornBasket.basket_id }, unicornBasket);
        }

        // DELETE: api/Basket/1d6d0345-b3e5-4e0f-87a3-0a98b9a17073
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUnicornBasket(Guid id)
        {
            basket? unicornBasket = await this.unishopEntitiesContext.baskets.FindAsync(id);
            if (unicornBasket == null)
            {
                return this.NotFound();
            }

            this.unishopEntitiesContext.baskets.Remove(unicornBasket);
            await this.unishopEntitiesContext.SaveChangesAsync();

            return this.Ok(unicornBasket);
        }

        private bool UnicornBasketExists(Guid id)
        {
            return this.unishopEntitiesContext.baskets.Count(e => e.basket_id == id) > 0;
        }
    }
}
