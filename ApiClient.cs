using GenerateDishesAPI.Handlers;
using GenerateDishesAPI.Models;
using GenerateDishesAPI.Models.DTOs;
using GenerateDishesAPI.Repositories;
using System.IO;
using System.Web;
using Unsplasharp.Models;

namespace GenerateDishesAPI
{
    public class ApiClient
	{
		private readonly HttpClient _httpClient;
		private readonly DbHelpers _dbHelpers;
		
		private readonly string _url = "https://azurefoodapi.azurewebsites.net";

		public ApiClient(DbHelpers dbHelpers)
		{
			_httpClient = new HttpClient();
			_dbHelpers = dbHelpers;
		}

		//Börja med ingredienser 
		//Sedan stegen

		//Endpoint åt frontend för ingredienser och 

		public async Task<string[]> GetDishesAsync(string path)
		{
			try
			{
				// Make a GET request to the API endpoint
				HttpResponseMessage response = await _httpClient.GetAsync(path);

				// Check if the request was successful
				response.EnsureSuccessStatusCode();

				// Read the response content as a string
				string responseBody = await response.Content.ReadAsStringAsync();

				// Split the response by any delimiter to get individual names
				string[] names = responseBody.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

				return names;
			}
			catch (HttpRequestException ex)
			{
				// Handle any errors occurred during the HTTP request
				Console.WriteLine($"HTTP request error: {ex.Message}");
				return null;
			}
		}

		public async Task<List<UrlAndDishNameDTO>> GetPicturesAndDishesAsync(string userId)
		{
			List<UrlAndDishNameDTO> dtos = new List< UrlAndDishNameDTO>();
			UrlAndDishNameDTO dto;
			int counter = 0;

			//Get 10 dish-names from method that calls ChatAi
			
			string[] dishes = await GetDishesAsync($"{_url}/ChatAi/{userId}");
			
			
			foreach (string dish in dishes)
			{
				//Make GET request to Picture-API
				string requestUrl = $"{_url}/img?imgQuery={Uri.EscapeDataString(dish)}";
				HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
				string url = await response.Content.ReadAsStringAsync();

				//Save dishes with respective picture as valuePair-object and add to list
				dto = new UrlAndDishNameDTO()
				{
					Id = counter++,
					Key = dish,
					Value = url
				};
				dtos.Add(dto);
			}
			
			return dtos;
		}

		public async Task<string[]> GetIngredientsAsync(string dishName, int numOfPeople, string userId)
		{
			string baseUrl = $"{_url}/GenerateIngredients/{dishName}/{numOfPeople}/{userId}";

			// Make a GET request to the API endpoint
			HttpResponseMessage response = await _httpClient.GetAsync(baseUrl);

			// Check if the request was successful
			response.EnsureSuccessStatusCode();

			//if (response.IsSuccessStatusCode)

			// Read the response content as a string
			string responseBody = await response.Content.ReadAsStringAsync();

			// Split the response by any delimiter to get individual names
			string[] ingredients = responseBody.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			return ingredients;
		}

		public async Task<CompleteDishDTO> GetIngredientsAndRecipeAsync(string dishName, int numOfPeople, string userId)
		{

			//Calls GetIngredientsAsync to get the ingredients
			string[] ingredients = await GetIngredientsAsync(dishName, numOfPeople, userId);

			string baseUrl = $"{_url}/GenerateInstructions/{dishName}";
			var uriBuilder = new UriBuilder(baseUrl);
			var query = HttpUtility.ParseQueryString(uriBuilder.Query);

			// Adding allergies as query parameters
			foreach (var ingredient in ingredients)
			{
				query["ingredients"] = ingredient;
			}

			uriBuilder.Query = query.ToString();
			string finalUrl = uriBuilder.ToString();

			//Calls endpoint that generates Instructions
			HttpResponseMessage response = await _httpClient.GetAsync(finalUrl);
			
			// Check if the request was successful
			response.EnsureSuccessStatusCode();
			
			// Read the response content as a string
			string instructions = await response.Content.ReadAsStringAsync();
			
			_dbHelpers.SaveIngredientsAndRecipe(userId, dishName, ingredients, instructions);
			
			
			
			CompleteDishDTO dish = _dbHelpers.GetCompleteDishFromDb(userId, dishName);

			return dish;
			 
		}


	}
}
