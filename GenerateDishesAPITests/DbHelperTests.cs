//using GenerateDishesAPI.Data;
//using GenerateDishesAPI.Models;
//using GenerateDishesAPI.Repositories;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace GenerateDishesAPITests
//{
//	[TestClass]
//	public class DbHelperTests
//	{
//        [TestMethod]
//        public void GetUserInfo_ReturnsCorrectUserViewModel()
//        {
//           // Arrange
//        var options = new DbContextOptionsBuilder<ApplicationContext>()
//            .UseInMemoryDatabase(databaseName: "TestDatabase")
//            .Options;

//            using (var context = new ApplicationContext(options))
//            {
//                context.Users.AddRange(
//                    new ApplicationUser { Id = "1", Email = "user1@example.com" },
//                    new ApplicationUser { Id = "2", Email = "user2@example.com" }
//                );
//                context.SaveChanges();
//            }

//            using (var context = new ApplicationContext(options))
//            {
//                var service = new DbHelper(context);

//                // Act & Assert for existing user
//                var result = service.GetUserInfo("1");
//                Assert.IsNotNull(result);
//                Assert.AreEqual("user1@example.com", result.Email);
//                Assert.AreEqual("1", result.UserId);

//                // Act & Assert for non-existing user
//                var resultNonExisting = service.GetUserInfo("2");
//                Assert.IsNotNull(resultNonExisting);
//            }
//        }
//    }
    
//}
