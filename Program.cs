
using GenerateDishesAPI.Data;
using GenerateDishesAPI.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using OpenAI_API;
using OpenAI_API.Models;
using Unsplasharp;

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

			app.MapGet("ChatAi", async (OpenAIAPI api) =>
			{
				return await OpenAiClient.GenerateTenDishes(api);
			});

			app.MapGet("/img", async (string imgQuery) =>
			{
				return await UnsplashartAPIClient.ReturnImageUrl(imgQuery);
			});

            app.MapGet("/PicturesAndUrls", async (ApiClient client) =>
			{
				return await client.GeneratePicturesAndDishesAsync();
			});

			app.MapPost("GenerateRecipe", async (OpenAIAPI api, string dish) =>
			{
				return await OpenAiClient.GenerateRecipeAndIngredientsForDish(api, dish);
			});

            app.Run();
		}

		//Namn email, id, lösen
	}
}
