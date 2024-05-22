
using GenerateDishesAPI.Data;
using Microsoft.EntityFrameworkCore;
using OpenAI_API;
using Unsplasharp;
using GenerateDishesAPI.Models;
using System.Net;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;
using Unsplasharp.Models;
using GenerateDishesAPI.Repositories;

namespace GenerateDishesAPI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
            
            DotNetEnv.Env.Load();

            builder.Services.AddControllers();
            builder.Services.AddSingleton(sp => new OpenAIAPI(Environment.GetEnvironmentVariable("OPENAI_API_KEY")));
			builder.Services.AddSingleton(new ApiClient());
			builder.Services.AddScoped<UserManager<ApplicationUser>, UserManager<ApplicationUser>>();
			builder.Services.AddScoped<DbHelpers>();

			builder.Services.AddDbContext<ApplicationContext>(options =>
			options.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationContext")));

			//Identity stuff
			
			builder.Services.AddAuthentication();
			builder.Services.AddAuthorization();

			builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
				.AddEntityFrameworkStores<ApplicationContext>()
				.AddUserManager<UserManager<ApplicationUser>>();

			//ALDORS LÖSNING
			//builder.Services.AddIdentity<User, IdentityRole>(options =>
			//{
			//	options.User.RequireUniqueEmail = true;
			//})
			//	.AddEntityFrameworkStores<ApplicationContext>() // ersätt "ApplicationDbContext" så den pekar mot ditt eget context-fil
			//	.AddUserManager<UserManager<User>>()
			//	.AddRoleManager<RoleManager<IdentityRole>>()
			//	.AddApiEndpoints()
			//	.AddDefaultTokenProviders();

			//CONFIGURE OPTIONS
			//builder.Services.Configure<IdentityOptions>(options =>
			//{
			//	// Password settings.
			//	options.Password.RequireDigit = true;
			//	options.Password.RequireLowercase = true;
			//	options.Password.RequireNonAlphanumeric = true;
			//	options.Password.RequireUppercase = true;
			//	options.Password.RequiredLength = 6;
			//	options.Password.RequiredUniqueChars = 1;

			//	// User settings.
			//	options.User.AllowedUserNameCharacters =
			//	"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
			//	options.User.RequireUniqueEmail = false;
			//});

			//COOKIES
			//builder.Services.ConfigureApplicationCookie(options =>
			//{
			//	// Cookie settings
			//	options.Cookie.HttpOnly = true;
			//	options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

			//	options.LoginPath = "/Identity/Account/Login";
			//	options.AccessDeniedPath = "/Identity/Account/AccessDenied";
			//	options.SlidingExpiration = true;
			//});

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

			app.MapIdentityApi<ApplicationUser>();

			//app.MapPost("/register", async (UserManager<ApplicationUser> userManager, string email, string userName, string password) =>
			//{
			//	var newUser = new ApplicationUser
			//	{
			//		Email = email,
			//		UserName = userName
			//	};

			//	await userManager.CreateAsync(newUser, password);

			//	return newUser;
			//});

			//app.MapGet("/login", async (UserManager<ApplicationUser> userManager, string email, string password) =>
			//{
			//	var result = await userManager.FindByEmailAsync(email);	
			//	//Check password
			//	//var result = Sign-in
			//	//Return result

			//});

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

			
			app.MapPost("/SaveDishAndUrl", (DbHelpers dbHelper, string dishName, string url, string id) =>
			{
				return dbHelper.SaveDishAndUrl(dishName, url, id);
			});

			

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
					ApplicationUser user = context.Users.Where(u => u.Id == id).FirstOrDefault();


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

			//GET id
			//Endpoint: Remove dish from user

			//endpoint Show all dishes and pictures

			//Save dish to user


			app.Run();
		}

		//Namn email, id, lösen
	}
}
