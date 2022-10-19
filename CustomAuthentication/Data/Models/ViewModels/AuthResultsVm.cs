using System;

namespace CustomAuthentication.Data.Models.ViewModels
{
    public class AuthResultsVm
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
