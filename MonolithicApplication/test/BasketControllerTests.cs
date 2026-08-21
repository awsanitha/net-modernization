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
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnicornShopLegacy.Controllers;
using UnicornShopLegacy.Interfaces;

namespace UnicornShopLegacy.Tests
{
    [TestClass]
    public class BasketControllerTests
    {
        private IUnishopEntities unishopDbContext = null!;
        private BasketController basketController = null!;

        [TestMethod]
        public void GetUnicornBasketsTest()
        {
            this.GivenUnishopDbContext();
            this.GivenBasketController();

            var unicornBaskets = this.basketController.GetUnicornBaskets();
            Assert.IsNotNull(unicornBaskets);
            Assert.AreEqual(unicornBaskets.Count(), 3);
        }

        [TestMethod]
        public void GetUnicornBasketSuccessTest()
        {
            this.GivenUnishopDbContext();
            var user_uuid_to_get = Guid.NewGuid();
            this.unishopDbContext.baskets.Add(new basket { user_id = user_uuid_to_get });
            this.GivenBasketController();

            var result = this.basketController.GetUnicornBasket(user_uuid_to_get).GetAwaiter().GetResult();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var confirmed_result = result as OkObjectResult;
            Assert.IsNotNull(confirmed_result);
            var baskets = confirmed_result.Value as IQueryable<basket>;
            Assert.IsNotNull(baskets);
            Assert.AreEqual(user_uuid_to_get, baskets.FirstOrDefault()!.user_id);
        }

        [TestMethod]
        public void GetUnicornBasketWithInvalidUUIDTest()
        {
            this.GivenUnishopDbContext();
            this.GivenBasketController();

            var user_uuid_to_get = Guid.NewGuid();

            var result = this.basketController.GetUnicornBasket(user_uuid_to_get).GetAwaiter().GetResult();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void PutUnicornBasketSuccessTest()
        {
            this.GivenUnishopDbContext();
            this.GivenBasketController();

            var basket_uuid_to_put = Guid.NewGuid();

            var result = this.basketController.PutUnicornBasket(basket_uuid_to_put, new basket { basket_id = basket_uuid_to_put }).GetAwaiter().GetResult();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(StatusCodeResult));
            var confirmed_result = result as StatusCodeResult;
            Assert.IsNotNull(confirmed_result);
            Assert.AreEqual((int)HttpStatusCode.NoContent, confirmed_result.StatusCode);
        }

        [TestMethod]
        public void PutUnicornBasketTestIdsNotMatching()
        {
            this.GivenUnishopDbContext();
            this.GivenBasketController();
            var result = this.basketController.PutUnicornBasket(Guid.NewGuid(), new basket { basket_id = Guid.NewGuid() }).GetAwaiter().GetResult();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public void PostUnicornBasketSuccessTest()
        {
            this.GivenUnishopDbContext();
            this.GivenBasketController();

            var basketItem = new basket { basket_id = Guid.NewGuid() };
            var result = this.basketController.PostUnicornBasket(basketItem).GetAwaiter().GetResult();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(CreatedAtRouteResult));
            var confirmed_result = result as CreatedAtRouteResult;
            Assert.IsNotNull(confirmed_result);
            Assert.AreEqual(confirmed_result.RouteName, "DefaultApi");
            Assert.AreEqual(confirmed_result.RouteValues!["id"], confirmed_result.Value is basket b ? b.basket_id : Guid.Empty);
            Assert.AreEqual(((basket)confirmed_result.Value!).basket_id, basketItem.basket_id);
        }

        [TestMethod]
        public void PostUnicornBasketInvalidModelTest()
        {
            this.GivenUnishopDbContext();
            this.GivenBasketController();

            this.basketController.ModelState.AddModelError("invalidModelFakeError", "Fake model error for testing");
            var result = this.basketController.PostUnicornBasket(new basket() { }).GetAwaiter().GetResult();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public void DeleteUnicornBasketTest()
        {
            this.GivenUnishopDbContext();
            this.GivenBasketController();
            var uuid_to_delete = Guid.NewGuid();
            this.unishopDbContext.baskets.Add(new basket { basket_id = uuid_to_delete });
            var result = this.basketController.DeleteUnicornBasket(uuid_to_delete).GetAwaiter().GetResult();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var confirmed_result = result as OkObjectResult;
            Assert.IsNotNull(confirmed_result);
            Assert.AreEqual(uuid_to_delete, ((basket)confirmed_result.Value!).basket_id);
        }

        public void DeleteInvalidUnicornBasketTest()
        {
            this.GivenUnishopDbContext();
            this.GivenBasketController();
            var uuid_to_delete = Guid.NewGuid();
            var result = this.basketController.DeleteUnicornBasket(uuid_to_delete).GetAwaiter().GetResult();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        private void GivenUnishopDbContext()
        {
            var fakeSet = new FakeBasketDbSet();
            fakeSet.AddRange(new[] { new basket { }, new basket { }, new basket { } });
            var mock = new Mock<IUnishopEntities>();
            mock.As<IDisposable>().Setup(x => x.Dispose());
            mock.Setup(x => x.baskets).Returns(fakeSet);
            mock.Setup(x => x.SetModified(It.IsAny<object>()));

            this.unishopDbContext = mock.Object;
        }

        private void GivenBasketController()
        {
            this.basketController = new BasketController(this.unishopDbContext);
        }
    }
}
