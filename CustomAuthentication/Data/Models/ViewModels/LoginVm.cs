using System.ComponentModel.DataAnnotations;

namespace CustomAuthentication.Data.Models.ViewModels
{
    public class LoginVm
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
