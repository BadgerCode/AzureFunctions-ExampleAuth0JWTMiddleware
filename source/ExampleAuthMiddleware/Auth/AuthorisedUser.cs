namespace ExampleAuthMiddleware.Auth
{
    public class AuthorisedUser
    {
        public bool IsAuthenticated { get; set; }
        public string? AuthToken { get; set; }
        public string? UserID { get; set; }
    }
}
