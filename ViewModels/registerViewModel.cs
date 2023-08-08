using ChilLaxFrontEnd.Models.DTO;
using System.ComponentModel.DataAnnotations;

namespace ChilLaxFrontEnd.ViewModels
{
	public class registerViewModel
	{
		public int memberId { get; set; }
		public string memberName { get; set; }
		public string memberTel { get; set; }
		public string memberEmail { get; set; }
		public bool memberSex { get; set; }

		[DataType(DataType.Date)]
		public DateTime memberBirthday { get; set; }
		public string memberAddress { get; set; }
        //public List<MemberOrder> memberOrder { get; set; }

    }
}
