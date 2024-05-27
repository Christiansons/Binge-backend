namespace GenerateDishesAPI.Models
{
	public class Diet
	{
		public int Id { get; set; }
		public string DietName {  get; set; }

		public string ApplicationUserId { get; set; }
		public virtual ApplicationUser user { get; set; }
	}
}
