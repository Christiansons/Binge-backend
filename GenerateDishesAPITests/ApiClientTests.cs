//using GenerateDishesAPI;
//using GenerateDishesAPI.Repositories;
//using Moq.Protected;
//using Moq;
//using System.Net;
//using Xunit;

//namespace GenerateDishesAPITests
//{
//    [TestClass]
//    public class ApiClientTests
//    {



//        [TestMethod]
//        public async Task GetDishesAsync_Gets10DishesAndReturnsThemCorrectlyFormated()
//        {
//            //Arrange

//            //Create a Mock Httpmessagehandler to simulate a call from the OpenAi API to get the names of 10 dishes
//            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
//            mockHttpMessageHandler.Protected()
//            .Setup<Task<HttpResponseMessage>>(
//                "SendAsync",
//                ItExpr.IsAny<HttpRequestMessage>(),
//                ItExpr.IsAny<CancellationToken>())
//            .ReturnsAsync(new HttpResponseMessage
//            {
//                StatusCode = HttpStatusCode.OK,
//                Content = new StringContent("Pizza,Pasta,Salad,Sushi,Burger,Steak,Sandwich,Tacos,Nachos,Spaghetti")
//            });

//            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

//            var mockDbHelper = new Mock<IDbHelper>();

//            var ApiClient = new ApiClient(mockDbHelper.Object, httpClient);

//            // Act
//            string[] result = await ApiClient.GetDishesAsync("https://api.example.com/dishes");

//            // Assert
//            Xunit.Assert.NotNull(result);
//            Xunit.Assert.Equal(10, result.Length);
//        }

        
//    }
    
//}