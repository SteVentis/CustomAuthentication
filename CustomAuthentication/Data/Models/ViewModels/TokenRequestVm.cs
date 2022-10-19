using System.ComponentModel.DataAnnotations;

namespace CustomAuthentication.Data.Models.ViewModels
{
    public class TokenRequestVm
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}
