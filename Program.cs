
using GenerateDishesAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using OpenAI_API;
using OpenAI_API.Models;
using GenerateDishesAPI.Repositories;
using Unsplasharp;
using Microsoft.Identity.Client;
using GenerateDishesAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;
using Newtonsoft.Json;
using System.Linq;

namespace GenerateDishesAPI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();
            DotNetEnv.Env.Load();

            builder.Services.AddControllers();
            builder.Services.AddSingleton(sp => new OpenAIAPI(Environment.GetEnvironmentVariable("OPENAI_API_KEY")));
			builder.Services.AddSingleton(new ApiClient());
            builder.Services.AddDbContext<ApplicationContext>(options =>
			options.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationContext")));

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseAuthorization();


			app.MapControllers();

			
			//var LoginChecker = new DbHelpers();
			//bool loginCheckBool = LoginChecker.LoginCheck();
			//LoginChecker.CreateAccount();


      app.MapGet("ChatAi", async (OpenAIAPI api/*, string preferences*/) =>
			{
				string query = $"print the name of 10 different dishes(seperated with a comma)"; /*, adjust for: {preferences}*/

				var chat = api.Chat.CreateConversation();

				//Something went wrong here so its a bit different from how i did it but it still works
				chat.Model = OpenAI_API.Models.Model.ChatGPTTurbo;

				chat.RequestParameters.Temperature = 1;

                chat.AppendSystemMessage("You are a food recepie database and will give out food recepies");
				//Lägg till appends för json
                chat.AppendUserInput(query);
				chat.AppendExampleChatbotOutput("Beef wellington, Homemade pizza, Spaghetti carbonara, Birria tacos, Meatloaf, Fried chicken sandwich, Pulled pork, Escargot, Sushi, Mango sticky rice");
				chat.AppendUserInput(query);
				var answer = await chat.GetResponseFromChatbotAsync();

                return answer;

            });

			app.MapGet("/img", async (string imgQuery) =>
			{
				UnsplasharpClient client = new UnsplasharpClient("03jvn6lavoGJKCLyVN_Tw1GR654rFZbZbfVqRb6qiCE");
				var photo = await client.SearchPhotos(imgQuery);

				var url = photo.First().Urls.Regular;

				return url;
			});

			app.MapGet("/PicturesAndUrls", async (ApiClient client) =>
			{
				return await client.GeneratePicturesAndDishesAsync();
			});

			//Save dish to user
			app.MapPost("/SaveDishAndUrl", (string dishName, int id) =>
			{
				try
				{	
					using(ApplicationContext _context = new ApplicationContext())
					{
						User user = _context.Users.Where(u => u.Id == id).FirstOrDefault();
						if (user == null)
						{
							return Results.Problem("User not found");
						}

						Dish dish = new Dish()
						{
							DishName = dishName
						};

						if(user.Dishes == null)
						{
							user.Dishes = new List<Dish>() { dish };
						}
						_context.SaveChanges();

						return Results.StatusCode((int)HttpStatusCode.Created);
					}
				}
				catch (Exception ex)
					
					{
						return Results.BadRequest(ex.Message);
					}
			});


			//Endpoint: Remove dish from user

			//endpoint Login

			//endpoint Register

			//endpoint Show all dishes and pictures

			app.MapPost("GenerateIngredients", async (OpenAIAPI api, string dishName, int numOfPeople, int id) =>
			{
				string query = $"print the ingredients for dish: {dishName}, adjust for {numOfPeople} people";

				var chat = api.Chat.CreateConversation();
				chat.Model = OpenAI_API.Models.Model.ChatGPTTurbo;
				chat.RequestParameters.Temperature = 1;

				chat.AppendSystemMessage("You are a food recipe database and will give out food recepies in JSON-format");
				//Lägg till appends för json
				chat.AppendUserInput($"print the ingredients for dish: spaghetti carbonara, adjust for 4 people");
				chat.AppendExampleChatbotOutput(@"[{""ingredient"": ""400g pasta""}, {""ingredient"": ""4 eggs""}, {""ingredient"": ""30g pecorino cheese""},{""ingredient"": ""200g pancetta""},{""ingredient"": ""30g parmesan cheese""},{""ingredient"": ""salt""},{""ingredient"": ""pepper""}]");
				chat.AppendUserInput(query);
				var answer = await chat.GetResponseFromChatbotAsync();
				
				dynamic ingredients = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(answer));

				using (var context = new ApplicationContext())
				{
					User user = context.Users.Where(u => u.Id == id).FirstOrDefault();


					Dish savedDish = context.Dishes
					.Where(d => d.DishName == dishName)
					.FirstOrDefault();
					
					foreach (var Ingredient in ingredients)
					{
						savedDish.ingredients.Add(new Ingredient()
						{
							IngredientName = Ingredient.ingredient
						});
					}
					context.SaveChanges();
				}

				return ingredients;
			});

			app.MapPost("GenerateInstructions", async (OpenAIAPI api, string ingredients, string dish) =>
			{
				string query = $"generate cooking instructions for dish: {dish} with the ingredients: {ingredients}";

				var chat = api.Chat.CreateConversation();
				chat.Model = OpenAI_API.Models.Model.ChatGPTTurbo;
				chat.RequestParameters.Temperature = 1;

				chat.AppendSystemMessage("You are a food recipe database and will give out food recepies in JSON-format");
				//Lägg till appends för json
				chat.AppendUserInput(query);
				var answer = await chat.GetResponseFromChatbotAsync();

				return answer;
			});

			//endpoint Save complete dish

			//endpoint Return complete dish

			//endpoint if serving size changes, remove recipe from database, call Save and return dish with new number

			//



			app.Run();
		}

		//Namn email, id, lösen
	}
}
