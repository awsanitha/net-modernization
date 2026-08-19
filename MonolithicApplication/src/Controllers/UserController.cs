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
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UnicornShopLegacy.Interfaces;

namespace UnicornShopLegacy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUnishopEntities unishopEntitiesContext;

        public UserController(IUnishopEntities databaseContext)
        {
            this.unishopEntitiesContext = databaseContext;
        }

        // GET: api/User
        [HttpGet]
        public IQueryable<user> GetUsers()
        {
            return this.unishopEntitiesContext.users;
        }

        // POST: api/User
        [HttpPost]
        public async Task<ActionResult<user>> PostUser([FromBody] user user)
        {
            var search = from u in this.unishopEntitiesContext.users
                         where u.email == user.email
                         select u;

            if (string.IsNullOrWhiteSpace(user.email) || !user.email.Contains('@') || !user.email.Contains('.') || search.Count() != 0)
            {
                return this.BadRequest();
            }

            byte[] salt = RandomNumberGenerator.GetBytes(16);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(user.password, salt, 10000, HashAlgorithmName.SHA1, 20);
            byte[] hashBytes = new byte[36];

            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            user.password = Convert.ToBase64String(hashBytes);

            user.user_id = Guid.NewGuid();
            this.unishopEntitiesContext.users.Add(user);
            await this.unishopEntitiesContext.SaveChangesAsync();

            return this.CreatedAtRoute("DefaultApi", new { id = user.user_id }, user);
        }

        // POST: api/user/login
        [Route("api/user/login")]
        [HttpPost]
        public async Task<ActionResult<user>> PostLogin([FromBody] user login)
        {
            var search = from u in this.unishopEntitiesContext.users
                         where u.email == login.email
                         select u;

            if (!search.Any())
            {
                return this.NotFound();
            }

            var user = search.First();

            byte[] hashBytes = Convert.FromBase64String(user.password);

            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(login.password, salt, 10000, HashAlgorithmName.SHA1, 20);

            bool match = true;
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    match = false;
                }
            }

            if (!match)
            {
                return this.BadRequest();
            }

            return this.Ok(user);
        }

        private bool UserExists(Guid id)
        {
            return this.unishopEntitiesContext.users.Count(e => e.user_id == id) > 0;
        }
    }
}
