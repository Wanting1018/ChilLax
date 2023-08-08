using ChilLaxFrontEnd.Models.DTO;
using System.ComponentModel.DataAnnotations;

namespace ChilLaxFrontEnd.ViewModels
{
    public class LoginViewModel
    {
        //登入

        public int Id { get; set; }

        [Required(ErrorMessage = "請輸入帳號")]
        public string txtAccount { get; set; }

        [Required(ErrorMessage = "請輸入密碼")]
        public string txtPassword { get; set; }

        //註冊
        [Required(ErrorMessage = "請輸入帳號")]
        [RegularExpression("^[a-zA-Z0-9]{6,20}$", ErrorMessage = "帳號格式不正確，請輸入6至20個英文字母或數字")]
        public string txtRegisterAccount { get; set; }

        [Required(ErrorMessage = "請輸入密碼")]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z\\d]).{8,20}$", ErrorMessage = "密碼格式不正確，請輸入8至20個字元，包含大小寫字母、數字和特殊符號")]
        [DataType(DataType.Password)]
        public string txtRegisterPassword { get; set; }

        [Required(ErrorMessage = "請輸入相同密碼")]
        [DataType(DataType.Password)]
        [Compare("txtRegisterPassword", ErrorMessage = "確認密碼是否相同")]
        public string txtRegisterPasswordChk { get; set; }

        [Required(ErrorMessage = "請輸入姓名")]
        public string memberName { get; set; }

        [Required(ErrorMessage = "請輸入手機")]
        [RegularExpression("^09\\d{8}$", ErrorMessage = "手機號碼格式不正確")]
        public string memberPhone { get; set; }

        [Required(ErrorMessage = "請輸入信箱")]
        [RegularExpression("^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$", ErrorMessage = "信箱格式不正確")]
        public string memberEmail { get; set; }

        public bool memberGender { get; set; }

        [Required(ErrorMessage = "請輸入生日")]
        [DataType(DataType.Date)]
        [Range(typeof(DateTime), "1/1/1900", "1/1/2024", ErrorMessage = "日期區間，只能在1950年以後~2025年之前")]
        public DateTime memberBirth { get; set; }
        public string memberAddress { get; set; }

        public string Provider { get; set; }

        public string ProviderUserId { get; set; }

        public List<MemberOrder> memberOrder { get; set; }

    }
}
