using GenerateDishesAPI.Data;
using GenerateDishesAPI.Models;
using GenerateDishesAPI.Repositories;
using Newtonsoft.Json;
using OpenAI_API;

namespace GenerateDishesAPI.Handlers
{
	public class OpenAiHandler
	{
		private readonly OpenAIAPI _api;
		private readonly DbHelpers _dbHelpers;

        public OpenAiHandler(OpenAIAPI api, DbHelpers dbHelpers)
        {
            _api = api;
			_dbHelpers = dbHelpers;
		}

        public async Task<string> GenerateDishesAsync()
        {
			string query = $"print the name of 10 different dishes(seperated with a comma)"; /*, adjust for: {preferences}*/

			var chat = _api.Chat.CreateConversation();

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
		}

		public async Task<string> GenerateIngredientsAsync(string dishName, int numOfPeople, string[]? allergies)
		{
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
			chat.AppendUserInput($"print the ingredients for dish: spaghetti carbonara, adjust for 4 people");
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
