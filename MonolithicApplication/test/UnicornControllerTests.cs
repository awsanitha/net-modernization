/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 */

using System;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnicornShopLegacy.Controllers;
using UnicornShopLegacy.Interfaces;

namespace UnicornShopLegacy.Tests
{
    [TestClass]
    public class UnicornControllerTests
    {
        private IUnishopEntities unishopDbContext = null!;
        private UnicornController unicornController = null!;
        private Mock<IUnishopEntities> mockedUnicornEntities = null!;

        [TestInitialize]
        public void Init()
        {
            this.GivenUnishopDbContext();
            this.GivenUnicornController();
        }

        [TestMethod]
        public void GetUnicorns_WhenCalled_ReturnTwoUnicorns()
        {
            var unicorns = this.unicornController.GetUnicorns();

            unicorns.Should().HaveCount(2, "because 2 unicorns are added to DbContext.Unicorns");
        }

        [TestMethod]
        public void GetUnicorns_WhenCalled_ReturnIQueriableOfUnicorn()
        {
            var unicorns = this.unicornController.GetUnicorns();

            Assert.IsInstanceOfType(unicorns, typeof(IQueryable<inventory>));
        }

        [TestMethod]
        public async Task GetUnicorn_ExistingUnicornId_ReturnOkResult()
        {
            var guid = Guid.NewGuid();
            this.unishopDbContext.inventories.Add(new inventory { unicorn_id = guid });

            var actionResult = await this.unicornController.GetUnicorn(guid);
            var contentResult = actionResult as OkObjectResult;

            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Value);
            var content = contentResult.Value as inventory;
            Assert.IsNotNull(content);
            Assert.AreEqual(guid, content.unicorn_id);
        }

        [TestMethod]
        public async Task GetUnicorn_NonExistingUnicornId_ReturnNotFoundResult()
        {
            var guid = Guid.NewGuid();

            var actionResult = await this.unicornController.GetUnicorn(guid);

            Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult));
        }

        // it seems there is no validation for the unicorn model
        [TestMethod]
        public async Task PutUnicorn_InvalidModel_ReturnInvalidModelState()
        {
            var guid = Guid.NewGuid();
            var unicorn = new inventory { unicorn_id = guid };
            this.unicornController.ModelState.AddModelError("fake model error", "fake model error");

            var actionResult = await this.unicornController.PutUnicorn(guid, unicorn);

            Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task PutUnicorn_DifferentId_ReturnBadRequest()
        {
            var guid = Guid.NewGuid();
            var unicornWithDiffId = new inventory { unicorn_id = Guid.NewGuid() };

            var actionResult = await this.unicornController.PutUnicorn(guid, unicornWithDiffId);

            Assert.IsInstanceOfType(actionResult, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task PutUnicorn_AsyncSaveSucceed_ReturnNoContent()
        {
            var guid = Guid.NewGuid();
            var unicornWithSameId = new inventory { unicorn_id = guid };
            this.mockedUnicornEntities.Setup(x => x.SaveChangesAsync()).Returns(Task.FromResult(0));

            var actionResult = await this.unicornController.PutUnicorn(guid, unicornWithSameId);
            var statusCodeResult = actionResult as StatusCodeResult;

            Assert.IsNotNull(statusCodeResult);
            Assert.AreEqual((int)HttpStatusCode.NoContent, statusCodeResult.StatusCode);
        }

        [TestMethod]
        public async Task PutUnicorn_AsyncSaveFailAndUnicornExists_ThrowException()
        {
            var guid = Guid.NewGuid();
            var unicornWithSameId = new inventory { unicorn_id = guid };
            this.unishopDbContext.inventories.Add(unicornWithSameId);
            this.mockedUnicornEntities.Setup(x => x.SaveChangesAsync()).Throws(new DbUpdateConcurrencyException());

            await Assert.ThrowsExceptionAsync<DbUpdateConcurrencyException>(async () => { await this.unicornController.PutUnicorn(guid, unicornWithSameId); });
        }

        [TestMethod]
        public async Task PutUnicorn_AsyncSaveFailAndUnicornNotExists_ReturnNotFound()
        {
            var guid = Guid.NewGuid();
            var unicornWithSameId = new inventory { unicorn_id = guid };
            this.mockedUnicornEntities.Setup(x => x.SaveChangesAsync()).Throws(new DbUpdateConcurrencyException());

            var actionResult = await this.unicornController.PutUnicorn(guid, unicornWithSameId);

            Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PostUnicorn_InvalidModel_ReturnInvalidModelState()
        {
            var unicorn = new inventory { unicorn_id = Guid.NewGuid() };
            this.unicornController.ModelState.AddModelError("fake model error", "fake model error");

            var actionResult = await this.unicornController.PostUnicorn(unicorn);

            Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task PostUnicorn_WhenCalled_UnicornInList()
        {
            var guid = Guid.NewGuid();
            var unicorn = new inventory { unicorn_id = guid };
            this.mockedUnicornEntities.Setup(x => x.SaveChangesAsync()).Returns(Task.FromResult(0));

            var actionResult = await this.unicornController.PostUnicorn(unicorn);
            var createdResult = actionResult as CreatedAtRouteResult;

            Assert.IsNotNull(createdResult);
            var content = createdResult.Value as inventory;
            Assert.IsNotNull(content);
            Assert.AreEqual(unicorn.unicorn_id, content.unicorn_id);
        }

        [TestMethod]
        public async Task PostUnicorn_WhenCalled_ReDirectToDefaultAPI()
        {
            var guid = Guid.NewGuid();
            var unicorn = new inventory { unicorn_id = guid };

            var actionResult = await this.unicornController.PostUnicorn(unicorn);
            var createdResult = actionResult as CreatedAtRouteResult;

            Assert.IsNotNull(createdResult);
            Assert.AreEqual("DefaultApi", createdResult.RouteName);
            Assert.AreEqual(unicorn.unicorn_id, createdResult.RouteValues!["id"]);
        }

        [TestMethod]
        public async Task DeleteUnicorn_UnicornNotFound_ReturnNotFound()
        {
            var guid = Guid.NewGuid();

            var actionResult = await this.unicornController.DeleteUnicorn(guid);

            Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task DeleteUnicorn_UnicornFound_DeleteUnicornFromList()
        {
            var guid = Guid.NewGuid();
            var unicorn = new inventory { unicorn_id = guid };
            this.unishopDbContext.inventories.Add(unicorn);
            this.mockedUnicornEntities.Setup(x => x.SaveChangesAsync()).Returns(Task.FromResult(0));

            await this.unicornController.DeleteUnicorn(guid);

            Assert.AreEqual(0, this.unishopDbContext.inventories.Count(e => e.unicorn_id == guid));
        }

        [TestMethod]
        public async Task DeleteUnicorn_UnicornFound_ReturnOk()
        {
            var guid = Guid.NewGuid();
            var unicorn = new inventory { unicorn_id = guid };
            this.unishopDbContext.inventories.Add(unicorn);
            this.mockedUnicornEntities.Setup(x => x.SaveChangesAsync()).Returns(Task.FromResult(0));

            var actionResult = await this.unicornController.DeleteUnicorn(guid);
            var contentResult = actionResult as OkObjectResult;

            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Value);
            var content = contentResult.Value as inventory;
            Assert.IsNotNull(content);
            Assert.AreEqual(guid, content.unicorn_id);
        }

        private void GivenUnishopDbContext()
        {
            var fakeSet = new FakeUnicornDbSet();
            fakeSet.AddRange(new[] { new inventory { unicorn_id = Guid.NewGuid() }, new inventory { unicorn_id = Guid.NewGuid() } });

            this.mockedUnicornEntities = new Mock<IUnishopEntities>();
            this.mockedUnicornEntities.As<IDisposable>().Setup(x => x.Dispose());
            this.mockedUnicornEntities.Setup(x => x.inventories).Returns(fakeSet);
            this.mockedUnicornEntities.Setup(x => x.SetModified(It.IsAny<object>()));
            this.mockedUnicornEntities.Setup(x => x.SaveChangesAsync()).Returns(Task.FromResult(0));

            this.unishopDbContext = this.mockedUnicornEntities.Object;
        }

        private void GivenUnicornController()
        {
            this.unicornController = new UnicornController(this.unishopDbContext);
        }
    }
}
