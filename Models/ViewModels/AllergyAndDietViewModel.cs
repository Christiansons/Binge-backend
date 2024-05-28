namespace GenerateDishesAPI.Models.ViewModels
{
	public class AllergyAndDietViewModel
	{
		public DietViewModel? Diet { get; set; }
		public ICollection<AllergyViewModel>? Allergies { get; set; }
	}
}
