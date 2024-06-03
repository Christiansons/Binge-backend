namespace GenerateDishesAPI.Models.DTOs
{
	public class DietAndAllergyPostRequest
	{
		public string userId { get; set; }
		public string[]? allergies { get; set; }
		public string? diet { get; set; }
	}
}
