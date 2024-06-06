using Unsplasharp;

namespace GenerateDishesAPI.Handlers
{
	public interface IUnsplashHandler
	{
		Task<string> GeneratePictureUrlAsync(string dishName);
	}

	public class UnsplashHandler : IUnsplashHandler
	{
		public async Task<string> GeneratePictureUrlAsync(string dishName)
		{
			UnsplasharpClient client = new UnsplasharpClient("03jvn6lavoGJKCLyVN_Tw1GR654rFZbZbfVqRb6qiCE");

			//if (client.RateLimitRemaining == 0)
			//{
			//	return "https://media.npr.org/assets/img/2019/09/27/nuts-1_custom-4d5a40d1dfed103fe42fed775960a3e15a27627c.jpg?s=1300&c=85&f=webp";
			//} 
			var photo = await client.SearchPhotos(dishName);

			var url = photo.First().Urls.Regular;

			return url;
		}
	}
}
