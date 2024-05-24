
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
using GenerateDishesAPI.Handlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using GenerateDishesAPI.Models.DTOs;

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
			builder.Services.AddScoped < ApiClient>();
			builder.Services.AddScoped<UserManager<ApplicationUser>, UserManager<ApplicationUser>>();
			builder.Services.AddScoped<DbHelpers>();
			builder.Services.AddScoped<OpenAiHandler>();
			builder.Services.AddScoped<UnsplashHandler>();

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
			
				app.UseSwagger();
				app.UseSwaggerUI();
			

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

			app.MapGet("/login", async (UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> SignInManager, string email, string password) =>
			{
				var user = await userManager.FindByEmailAsync(email);

				//Check password
				var passwordCheck = await userManager.CheckPasswordAsync(user, password);
				if (!passwordCheck)
				{
					return "No bueno";
				}

				// Sign-in
				var result = await SignInManager.PasswordSignInAsync(user, password, isPersistent: false, false);
				var token = await userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, "login");
				
				//Return result

				return (user.Id); //Skapa DTO
			});

			app.MapGet("ChatAi", async (OpenAiHandler aiHandler) =>
			{
				return await aiHandler.GenerateDishesAsync();
            });	

			app.MapGet("/img", async (UnsplashHandler unsplashHandler, string imgQuery) =>
			{
				return await unsplashHandler.GeneratePictureUrlAsync(imgQuery);
			});

			app.MapGet("/PicturesAndUrls", async (ApiClient client) =>
			{
				return await client.GetPicturesAndDishesAsync();
			});
			
			app.MapPost("/SaveDishAndUrl", (DbHelpers dbHelper, string dishName, string url, string userId) =>
			{
				return dbHelper.SaveDishAndUrl(dishName, url, userId);
			});

			app.MapGet("GenerateIngredients/{dishName}/{numOfPeople}", async (OpenAiHandler aiHandler, string dishName, int numOfPeople, string[]? allergies) =>
			{
				return await aiHandler.GenerateIngredientsAsync(dishName, numOfPeople, allergies);
			});

			app.MapGet("GenerateInstructions/{dishName}", async (OpenAiHandler aiHandler, string dishName, string[] ingredients) =>
			{
				return await aiHandler.GenerateRecipeAsync(dishName, ingredients);
			});

			app.MapPost("/SparaHelaDishen", async (DbHelpers helper, string userId, string dishName, string[] ingredients, string recipe) =>
			{
				helper.SaveIngredientsAndRecipe(userId, dishName, ingredients, recipe);
			});

			app.MapGet("GetIngredientsAndRecipe", async (DbHelpers dbHelper, ApiClient client, string dishName, int numOfPeople, string[]? allergies, string userId) =>
			{
				if (dbHelper.CheckIfRecipeAdded(dishName, userId))
				{
					CompleteDishDTO dish = dbHelper.GetCompleteDishFromDb(userId, dishName);
					return dish;
				}
				return await client.GetIngredientsAndRecipeAsync(dishName, numOfPeople, allergies, userId);

			});

			app.MapDelete("/DeleteDish", (DbHelpers dbhelper, string dishName, string userId) =>
			{
				dbhelper.DeleteDishFromDb(dishName, userId);
			});

			//Endpoint if serving size changes, removes old recipe and ingredients and generates new recipe with uppdated serving size
			app.MapGet("/UpdateDish", async (DbHelpers dbHelper, ApiClient client, string dishName, int numOfPeople, string[]? allergies, string userId) =>
			{
				dbHelper.DeleteRecipeFromDb(dishName, userId);
				return await client.GetIngredientsAndRecipeAsync(dishName, numOfPeople, allergies, userId);
			});

			//Show all dishes and pictures conneted to user
			app.MapGet("/AllDishesAndUrlsConnectedToUser", (DbHelpers dbhelper, string userId) =>
			{
				return dbhelper.GetAllDishesConnectedToUser(userId);
			});


			//endpoint Show all dishes and pictures

			//Save dish to user


			app.Run();
		}

		//Namn email, id, lösen
	}
}
