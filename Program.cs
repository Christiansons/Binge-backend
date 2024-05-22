
using GenerateDishesAPI.Data;
using Microsoft.EntityFrameworkCore;
using OpenAI_API;
using Unsplasharp;
using GenerateDishesAPI.Models;
using System.Net;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;

namespace GenerateDishesAPI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            
            DotNetEnv.Env.Load();
			

            builder.Services.AddControllers();
            builder.Services.AddSingleton(sp => new OpenAIAPI(Environment.GetEnvironmentVariable("OPENAI_API_KEY")));
			builder.Services.AddSingleton(new ApiClient());
            builder.Services.AddDbContext<ApplicationContext>(options =>
			options.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationContext")));

			//Identity stuff


			//builder.Services.AddIdentity<User, IdentityRole>(options => { } )
			//	.AddEntityFrameworkStores<ApplicationContext>()
			//	.AddUserManager<UserManager<User>>()
			//	.AddApiEndpoints();

			//builder.Services.AddIdentityCore<User>()
			//	.AddEntityFrameworkStores<ApplicationContext>()
			//	.AddUserManager<UserManager<User>>()
			//	.AddApiEndpoints();

			builder.Services.AddIdentity<User, IdentityRole>(options =>
			{
				options.User.RequireUniqueEmail = true;
			})
				.AddEntityFrameworkStores<ApplicationContext>() // ersätt "ApplicationDbContext" så den pekar mot ditt eget context-fil
				.AddUserManager<UserManager<User>>()
				.AddRoleManager<RoleManager<IdentityRole>>()
				.AddApiEndpoints()
				.AddDefaultTokenProviders();

			builder.Services.AddAuthentication();/*.AddCookie(IdentityConstants.ApplicationScheme);*/
			builder.Services.AddAuthorization();



			builder.Services.Configure<IdentityOptions>(options =>
			{
				// Password settings.
				options.Password.RequireDigit = true;
				options.Password.RequireLowercase = true;
				options.Password.RequireNonAlphanumeric = true;
				options.Password.RequireUppercase = true;
				options.Password.RequiredLength = 6;
				options.Password.RequiredUniqueChars = 1;

				// User settings.
				options.User.AllowedUserNameCharacters =
				"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
				options.User.RequireUniqueEmail = false;
			});

			//builder.Services.ConfigureApplicationCookie(options =>
			//{
			//	// Cookie settings
			//	options.Cookie.HttpOnly = true;
			//	options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

			//	options.LoginPath = "/Identity/Account/Login";
			//	options.AccessDeniedPath = "/Identity/Account/AccessDenied";
			//	options.SlidingExpiration = true;
			//});
			//Identity stuff



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

			app.MapPost("/register", async (UserManager<User> userManager, string email, string userName, string password) =>
			{
				var newUser = new User
				{
					Email = email,
					UserName = userName
				};

				await userManager.CreateAsync(newUser, password);
				await userManager.

				return newUser;
			});

			app.MapGet("/login", async (UserManager<User> userManager, string email, string password) =>
			{
				var result = await userManager.FindByEmailAsync(email);
				result.
				//Check password
				//var result = Sign-in
				//Return result

			});

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
			app.MapPost("/SaveDishAndUrl", (string dishName, string id) =>
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

			//GET id
			//Endpoint: Remove dish from user

			//endpoint Login

			//app.MapPost("/identity/login", async (HttpClient httpClient) =>
			//{
			//	var loginResponse = await httpClient.PostAsJsonAsync("/identity/login", new { username, password });
			//}); 
			//var loginContent = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
			//var accessToken = loginContent.GetProperty("access_token").GetString();

			//httpClient.DefaultRequestHeaders.Authorization = new("Bearer", accessToken);
			//Console.WriteLine(await httpClient.GetStringAsync("/requires-auth"));

			//endpoint Register

			//endpoint Show all dishes and pictures

			app.MapPost("GenerateIngredients", async (OpenAIAPI api, string dishName, int numOfPeople, string id) =>
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
