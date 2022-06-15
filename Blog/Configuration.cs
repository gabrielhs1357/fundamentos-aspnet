namespace Blog
{
    public static class Configuration
    {
        public static string JWTKey;
        public static string ApiKeyName; // Campo que iremos consultar se existe na requisição
        public static string APIKey;
        public static SmtpConfiguration Smtp;

        public class SmtpConfiguration
        {
            public string Host { get; set; }
            public int Port { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
