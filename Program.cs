
using GenerateDishesAPI.Data;
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
				string query = "print the name of 10 different dishes(seperated with a comma)";

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

			app.MapPost("GenerateRecipe", (string dish) =>
			{

			});
			//TwoDishesTwoPictures();

			app.Run();
		}

		//static async void TwoDishesTwoPictures()
		//{
		//	ApiClient client = new ApiClient();
		//	IDictionary<string, string> kvps = await client.GeneratePicturesAsync();

		//	foreach (var kv in kvps)
		//	{
		//		await Console.Out.WriteLineAsync(kv.Key + " " + kv.Value);
		//	}
			
		//}

		//Namn email, id, lösen
	}
}
