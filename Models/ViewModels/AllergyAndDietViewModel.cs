namespace GenerateDishesAPI.Models.ViewModels
{
	public class AllergyAndDietViewModel
	{
		public ICollection<DietViewModel>? Diets { get; set; }
		public ICollection<AllergyViewModel>? Allergies { get; set; }
	}
}
