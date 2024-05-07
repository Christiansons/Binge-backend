namespace GenerateDishesAPI
{
	public class ApiClient
	{
		private readonly HttpClient _httpClient;

		public ApiClient(string apiEndpoint)
		{
			_httpClient = new HttpClient();
		}

		public async Task<string[]> GenerateTenDishesAsync(string path)
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




	}
}
