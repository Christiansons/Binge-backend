using Unsplasharp;

namespace GenerateDishesAPI.Handlers
{
	public class UnsplashHandler
	{
		public async Task<string> GeneratePictureUrlAsync(string dishName)
		{
			UnsplasharpClient client = new UnsplasharpClient("03jvn6lavoGJKCLyVN_Tw1GR654rFZbZbfVqRb6qiCE");
			var photo = await client.SearchPhotos(dishName);

			var url = photo.First().Urls.Regular;

			return url;
		}
	}
}
