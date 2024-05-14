using GenerateDishesAPI.Models.KvpModel;

namespace GenerateDishesAPI
{
	public class ApiClient
	{
		private readonly HttpClient _httpClient;
		private readonly string _url = "https://localhost:7231";

		public ApiClient()
		{
			_httpClient = new HttpClient();
		}

		//Börja med ingredienser 
		//Sedan stegen

		//Endpoint åt frontend för ingredienser och 
		public async Task<string[]> GenerateDishesAsync(string path)
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

		public async Task<List<valuePair>> GeneratePicturesAndDishesAsync()
		{
			ApiClient client = new ApiClient();
			List<valuePair> kvps = new List< valuePair>();
			string path = ($"{_url}/img");
			valuePair kvp;
			int counter = 0;

			//Get 10 dish-names from method that calls ChatAi
			string[] dishes = await client.GenerateDishesAsync($"{_url}/ChatAi");
			
			foreach (string dish in dishes)
			{
				//Make GET request to Picture-API
				string requestUrl = $"{path}?imgQuery={Uri.EscapeDataString(dish)}";
				HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
				string url = await response.Content.ReadAsStringAsync();

				//Save dishes with respective picture as valuePair-object and add to list
				kvp = new valuePair()
				{
					Id = counter++,
					Key = dish,
					Value = url
				};
				kvps.Add(kvp);
			}

			return kvps;
		}




	}
}
