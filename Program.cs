
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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

			app.MapGet("ChatAi", async (OpenAIAPI api/*, string preferences*/) =>
			{
				string query = $"print the name of 10 different dishes(seperated with a comma)"; /*, adjust for: {preferences}*/

				var chat = api.Chat.CreateConversation();

				//Something went wrong here so its a bit different from how i did it but it still works
				chat.Model = OpenAI_API.Models.Model.ChatGPTTurbo;

				chat.RequestParameters.Temperature = 1;

                chat.AppendSystemMessage("You are a food recepie database and will give out food recepies");
				//L�gg till appends f�r json
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


			app.MapPost("GenerateIngredients", async (OpenAIAPI api, string dish, int numOfPeople) =>
			{
				string query = $"print the ingredients for dish: {dish}, adjust for {numOfPeople} people";

				var chat = api.Chat.CreateConversation();
				chat.Model = OpenAI_API.Models.Model.ChatGPTTurbo;
				chat.RequestParameters.Temperature = 1;

				chat.AppendSystemMessage("You are a food recipe database and will give out food recepies in JSON-format");
				//L�gg till appends f�r json
				chat.AppendUserInput($"print the ingredients for dish: spaghetti carbonara, adjust for 4 people");
				chat.AppendExampleChatbotOutput(@"[{""ingredient"": ""400g pasta""}, {""ingredient"": ""4 eggs""}, {""ingredient"": ""30g pecorino cheese""},{""ingredient"": ""200g pancetta""},{""ingredient"": ""30g parmesan cheese""},{""ingredient"": ""salt""},{""ingredient"": ""pepper""}]");
				chat.AppendUserInput(query);
				var answer = await chat.GetResponseFromChatbotAsync();
				//l�gg till
				return answer;
			});

			app.MapPost("GenerateInstructions", async (OpenAIAPI api, string ingredients, string dish) =>
			{
				string query = $"generate cooking instructions for dish: {dish} with the ingredients: {ingredients}";

				var chat = api.Chat.CreateConversation();
				chat.Model = OpenAI_API.Models.Model.ChatGPTTurbo;
				chat.RequestParameters.Temperature = 1;

				chat.AppendSystemMessage("You are a food recipe database and will give out food recepies in JSON-format");
				//L�gg till appends f�r json
				chat.AppendUserInput(query);
				var answer = await chat.GetResponseFromChatbotAsync();

				return answer;
			});


			app.Run();
		}

		//Namn email, id, l�sen
	}
}
