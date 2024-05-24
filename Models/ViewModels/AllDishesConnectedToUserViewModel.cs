namespace GenerateDishesAPI.Models.ViewModels
{
	public class AllDishesConnectedToUserViewModel
	{
		public ICollection<DishViewModel> dishViewModels { get; set; }
	}
}
