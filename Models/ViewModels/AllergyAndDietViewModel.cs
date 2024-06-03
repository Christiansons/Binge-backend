namespace GenerateDishesAPI.Models.ViewModels
{
	public class AllergyAndDietViewModel
	{
		public DietViewModel? Diet { get; set; }
		public List<string>? Allergies { get; set; }
	}
}
