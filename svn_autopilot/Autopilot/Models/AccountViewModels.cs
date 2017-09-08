using System.ComponentModel.DataAnnotations;


namespace Autopilot.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
    }

    public class ManageUserViewModel
    {
        public string UserId { get; set; }
        public string Token { get; set; }
        public bool FirstLogin { get; set; }


        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [RegularExpression("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$", ErrorMessage = "Password must have at least 8 Characters and contains atleast uppercase letter, lowercase letter, numbers and special symbols.")]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [RegularExpression(".+@.+\\..+", ErrorMessage = "Please Enter Correct Email Address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public string Message { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [RegularExpression(".+@.+\\..+", ErrorMessage = "Please Enter Correct Email Address")]
        [System.Web.Mvc.Remote("CheckEmailExist", "Account", ErrorMessage = "Email already exists!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please Select BusinessCategory")]
        public string BusinessCategory { get; set; }


        public string Password { get; set; }


    }

    public class ResponseViewModel
    {

        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [RegularExpression(".+@.+\\..+", ErrorMessage = "Please Enter Correct Email Address")]
        public string Email { get; set; }

    }

    public class SendMailViewModel
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string message { get; set; }
        public string userId { get; set; }
        public int MailType { get; set; }
        public bool ChangePassword { get; set; }
    }

    enum MailType
    {
        ResetPassword,
        EmailVerification
    }

}
