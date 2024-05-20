using Unsplasharp;

namespace GenerateDishesAPI.Helpers
{
    public class UnsplashartAPIClient
    {
        public static async Task<string> ReturnImageUrl(string imgQuery)
        {
            UnsplasharpClient client = new UnsplasharpClient("03jvn6lavoGJKCLyVN_Tw1GR654rFZbZbfVqRb6qiCE");

            var photo = await client.SearchPhotos(imgQuery);

            var url = photo.First().Urls.Regular;

            return url;
        }
    }
}
