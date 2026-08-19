/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 */

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnicornShopLegacy.Controllers;
using UnicornShopLegacy.Interfaces;

namespace UnicornShopLegacy.Tests
{
    [TestClass]
    public class UserControllerTests
    {
        private IUnishopEntities unishopDbContext = null!;
        private UserController userController = null!;

        [TestMethod]
        public void GetUsersTest()
        {
            this.GivenUnishopDbContext();
            this.GivenUserController();

            var users = this.userController.GetUsers();
            Assert.IsNotNull(users);
            Assert.AreEqual(users.Count(), 3);
        }

        [TestMethod]
        public void SignUPTestSuccess()
        {
            this.GivenUnishopDbContext();
            this.GivenUserController();

            var user = new user { user_id = Guid.NewGuid(), email = "456@gmail.com", password = "123456" };
            var result = this.userController.PostUser(user).GetAwaiter().GetResult();
            Assert.IsNotNull(result);

            Assert.IsInstanceOfType(result, typeof(CreatedAtRouteResult));
            var confirmed_result = result as CreatedAtRouteResult;
            Assert.IsNotNull(confirmed_result);
            Assert.AreEqual(confirmed_result.RouteName, "DefaultApi");

            var content = confirmed_result.Value as user;
            Assert.IsNotNull(content);
            Assert.AreEqual(confirmed_result.RouteValues!["id"], content.user_id);
            Assert.AreEqual(content.user_id, user.user_id);
        }

        [TestMethod]
        public void SignUpTestDuplicateEmail_ShouldFail()
        {
            this.GivenUnishopDbContext();
            this.GivenUserController();

            var user = new user { user_id = Guid.NewGuid(), email = "qwertyuio@gmail.com", password = "123456" };
            var result = this.userController.PostUser(user).GetAwaiter().GetResult();

            result = this.userController.PostUser(user).GetAwaiter().GetResult();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public void LoginTestSuccess()
        {
            this.GivenUnishopDbContext();
            this.GivenUserController();

            var user_temp = new user { user_id = Guid.NewGuid(), email = "56@gmail.com", password = "123456" };
            this.userController.PostUser(user_temp).GetAwaiter().GetResult();

            var user_temp_new = new user { user_id = Guid.NewGuid(), email = "56@gmail.com", password = "123456" };
            var result = this.userController.PostLogin(user_temp_new).GetAwaiter().GetResult();
            Assert.IsNotNull(result);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public void LoginTestUserNotExist_ShouldFail()
        {
            this.GivenUnishopDbContext();
            this.GivenUserController();

            var user_temp_new = new user { user_id = Guid.NewGuid(), email = "56@gmail.com", password = "123456" };
            var result = this.userController.PostLogin(user_temp_new).GetAwaiter().GetResult();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void LoginTestPasswordIncorrect_ShouldFail()
        {
            this.GivenUnishopDbContext();
            this.GivenUserController();

            var user_temp = new user { user_id = Guid.NewGuid(), email = "56@gmail.com", password = "123456" };
            this.userController.PostUser(user_temp).GetAwaiter().GetResult();

            var user_temp_new = new user { user_id = Guid.NewGuid(), email = "56@gmail.com", password = "12345" };
            var result = this.userController.PostLogin(user_temp_new).GetAwaiter().GetResult();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        private void GivenUnishopDbContext()
        {
            var fakeSet = new FakeUserDbSet();
            fakeSet.AddRange(new[] { new user { }, new user { }, new user { } });
            var mock = new Mock<IUnishopEntities>();
            mock.As<IDisposable>().Setup(x => x.Dispose());
            mock.Setup(x => x.users).Returns(fakeSet);
            mock.Setup(x => x.SetModified(It.IsAny<object>()));
            mock.Setup(x => x.SaveChangesAsync()).Returns(System.Threading.Tasks.Task.FromResult(0));

            this.unishopDbContext = mock.Object;
        }

        private void GivenUserController()
        {
            this.userController = new UserController(this.unishopDbContext);
        }
    }
}
