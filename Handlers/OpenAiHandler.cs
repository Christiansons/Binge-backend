using GenerateDishesAPI.Data;
using GenerateDishesAPI.Models;
using GenerateDishesAPI.Repositories;
using Newtonsoft.Json;
using OpenAI_API;

namespace GenerateDishesAPI.Handlers
{
	public interface IOpenAiHandler
	{
		Task<string> GenerateDishesAsync(string userId);
		Task<string> GenerateIngredientsAsync(string dishName, int numOfPeople, string userId);
		Task<string> GenerateRecipeAsync(string dishName, string[] ingredients);
	}

	public class OpenAiHandler : IOpenAiHandler
	{
		private readonly OpenAIAPI _api;
		private readonly IDbHelper _dbHelpers;

        public OpenAiHandler(OpenAIAPI api, IDbHelper dbHelpers)
        {
            _api = api;
			_dbHelpers = dbHelpers;
		}

        public async Task<string> GenerateDishesAsync(string userId)
        {
			string diet = _dbHelpers.GetUserDiet(userId);

			string query = $"print the name of 10 different dishes(seperated with a comma), adjust for: {diet} diet"; /*, adjust for: {preferences}*/

			var chat = _api.Chat.CreateConversation();

			//Something went wrong here so its a bit different from how i did it but it still works
			chat.Model = OpenAI_API.Models.Model.ChatGPTTurbo;

			chat.RequestParameters.Temperature = 1;

			chat.AppendSystemMessage("You are a food recepie database and will give out food recepies");
			//Lägg till appends för json
			chat.AppendUserInput("print the name of 10 different dishes(seperated with a comma), adjust for: no diet");
			chat.AppendExampleChatbotOutput("Beef wellington,Homemade pizza,Spaghetti carbonara,Birria tacos,Meatloaf,Fried chicken sandwich,Pulled pork,Escargot,Sushi,Mango sticky rice");
			chat.AppendUserInput("print the name of 10 different dishes(seperated with a comma), adjust for: vegan diet");
			chat.AppendExampleChatbotOutput("Vegan lasagna,Vegan Pad Thai,Lentil Curry,Vegan Tacos,Vegan Chili,Stuffed Bell Peppers,Vegan Sushi Rolls,Roasted Vegetable Pasta,Chickpea Salad Sandwich");
			chat.AppendUserInput("print the name of 10 different dishes(seperated with a comma), adjust for: no diet");
			chat.AppendExampleChatbotOutput("Beef Bourguignon,Paella,Moussaka,Tacos al Pastor,Pad Thai,Ratatouille,Bibimbap,Butter Chicken,Falafel");
			chat.AppendUserInput("print the name of 10 different dishes(seperated with a comma), adjust for: vegetarian diet");
			chat.AppendExampleChatbotOutput("Vegetable Stir-Fry,Homemade vegetarian pizza,Lentil Soup,Vegetarian tacos,Paneer Butter Masala,Vegetable Samosas,Mushroom Risotto, Escargot,Vegetarian Sushi,Greek Salad");
			chat.AppendUserInput(query);
			var answer = await chat.GetResponseFromChatbotAsync();
			return answer;
		}

		public async Task<string> GenerateIngredientsAsync(string dishName, int numOfPeople, string userId)
		{
			string[]? allergies = _dbHelpers.GetUserAllergies(userId);

			string allergiesInString = "";
			if (allergies.Length < 1 || allergies == null)
			{
				allergies = new string[]
				{
					"none"
				};
			}
			else
			{
				foreach (var allergy in allergies)
				{
					allergiesInString += $"{allergy}, ";
				}
			}

			string query = $"print the ingredients for dish: {dishName}, adjust for {numOfPeople} people, adjust for these: '{allergiesInString}' allergies";

			var chat = _api.Chat.CreateConversation();
			chat.Model = OpenAI_API.Models.Model.ChatGPTTurbo;
			chat.RequestParameters.Temperature = 1;

			chat.AppendSystemMessage("You are a food recipe database and will give out food recepies, seperated with a comma");
			chat.AppendUserInput($"print the ingredients for dish: spaghetti carbonara, adjust for 4 people, adjust for these: 'none' allergies");
			chat.AppendExampleChatbotOutput("400g pasta, 4 eggs, 30g pecorino cheese, 200g pancetta, 30g parmesan cheese, salt, pepper");
			chat.AppendUserInput(query);
			var answer = await chat.GetResponseFromChatbotAsync();
			return answer;
		}

		public async Task<string> GenerateRecipeAsync(string dishName, string[] ingredients)
		{
			string query = $"generate cooking instructions for dish: {dishName} with the ingredients: {ingredients} (dont include ingredients in answer)";

			var chat = _api.Chat.CreateConversation();
			chat.Model = OpenAI_API.Models.Model.ChatGPTTurbo;
			chat.RequestParameters.Temperature = 1;

			chat.AppendSystemMessage("You are a food recipe database and will give out food recepies");
			//Lägg till appends för json
			chat.AppendUserInput(query);
			var answer = await chat.GetResponseFromChatbotAsync();

			return answer;
		}

    }
}
