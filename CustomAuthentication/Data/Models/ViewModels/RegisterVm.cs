using System.ComponentModel.DataAnnotations;

namespace CustomAuthentication.Data.Models.ViewModels
{
    public class RegisterVm
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }

    }
}
