namespace GenerateDishesAPI.Models
{
	public class Allergy
	{
		public int Id { get; set; }
		public string AllergyName { get; set; }

		public string ApplicationUserId { get; set; }
		public virtual ApplicationUser user { get; set; }
	}
}
