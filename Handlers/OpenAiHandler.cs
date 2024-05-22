using OpenAI_API;

namespace GenerateDishesAPI.Handlers
{
	public class OpenAiHandler
	{
		private readonly OpenAIAPI _api;

        public OpenAiHandler(OpenAIAPI api)
        {
            _api = api;
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

    }
}
