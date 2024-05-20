using GenerateDishesAPI.Models;
using OpenAI_API;
using Unsplasharp;

namespace GenerateDishesAPI.Helpers
{
    public class OpenAiClient
    {
        public static async Task<string> GenerateTenDishes(OpenAIAPI api)
        {
            string query = "print the name of 10 different dishes(seperated with a comma)";

            var chat = api.Chat.CreateConversation();

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

        public static async Task<string> GenerateRecipeAndIngredientsForDish(OpenAIAPI api, string dish)
        {
            string query = $"print the ingredients and cooking instructions for dish: {dish}";

            var chat = api.Chat.CreateConversation();

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
