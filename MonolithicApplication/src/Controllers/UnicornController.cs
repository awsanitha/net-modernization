/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this
 * software and associated documentation files (the "Software"), to deal in the Software
 * without restriction, including without limitation the rights to use, copy, modify,
 * merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
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
    public class UnicornController : ControllerBase
    {
        private readonly IUnishopEntities unishopEntitiesContext;

        public UnicornController(IUnishopEntities databaseContext)
        {
            this.unishopEntitiesContext = databaseContext;
        }

        // GET: api/Unicorn
        [HttpGet]
        public IQueryable<inventory> GetUnicorns()
        {
            return this.unishopEntitiesContext.inventories;
        }

        // GET: api/Unicorn/1d6d0345-b3e5-4e0f-87a3-0a98b9a17073
        [HttpGet("{id}")]
        public async Task<ActionResult<inventory>> GetUnicorn(Guid id)
        {
            inventory? unicorn = await this.unishopEntitiesContext.inventories.FindAsync(id);
            if (unicorn == null)
            {
                return this.NotFound();
            }

            return this.Ok(unicorn);
        }

        // PUT: api/Unicorn/1d6d0345-b3e5-4e0f-87a3-0a98b9a17073
        [HttpPut("{id}")]
        public async Task<ActionResult> PutUnicorn(Guid id, inventory unicorn)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            if (id != unicorn.unicorn_id)
            {
                return this.BadRequest();
            }

            this.unishopEntitiesContext.SetModified(unicorn);

            try
            {
                await this.unishopEntitiesContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!this.UnicornExists(id))
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

        // POST: api/Unicorn
        [HttpPost]
        public async Task<ActionResult<inventory>> PostUnicorn(inventory unicorn)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            unicorn.unicorn_id = Guid.NewGuid();
            this.unishopEntitiesContext.inventories.Add(unicorn);
            await this.unishopEntitiesContext.SaveChangesAsync();

            return this.CreatedAtRoute("DefaultApi", new { id = unicorn.unicorn_id }, unicorn);
        }

        // DELETE: api/Unicorn/1d6d0345-b3e5-4e0f-87a3-0a98b9a17073
        [HttpDelete("{id}")]
        public async Task<ActionResult<inventory>> DeleteUnicorn(Guid id)
        {
            inventory? unicorn = await this.unishopEntitiesContext.inventories.FindAsync(id);
            if (unicorn == null)
            {
                return this.NotFound();
            }

            this.unishopEntitiesContext.inventories.Remove(unicorn);
            await this.unishopEntitiesContext.SaveChangesAsync();

            return this.Ok(unicorn);
        }

        private bool UnicornExists(Guid id)
        {
            return this.unishopEntitiesContext.inventories.Count(e => e.unicorn_id == id) > 0;
        }
    }
}
