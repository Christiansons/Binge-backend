namespace GenerateDishesAPI
{
	public class ApiClient
	{
		private readonly HttpClient _httpClient;
		private string url = "https://localhost:7231";

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

		public async Task<IDictionary<string,string>> GeneratePicturesAsync()
		{
			ApiClient client = new ApiClient();
			string[] dishes = await client.GenerateDishesAsync(url + "/ChatAi");
			string path = url + "/img";

			IDictionary<string,string> kvp = new Dictionary<string,string>();

			foreach (string dish in dishes)
			{
				//Make GET request to Picture-API
				string requestUrl = $"{path}?imgQuery={Uri.EscapeDataString(dish)}";

				HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);

				string responseBody = await response.Content.ReadAsStringAsync();

				kvp.Add(dish, responseBody);
			}

			return kvp;
		}




	}
}
