namespace Blog
{
    public static class Configuration
    {
        public static string JWTKey = "491E6914C43B21250FC5F3C8BBBF3AFEB425B30156CE7FA7A62D967E01F0F0D3";
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
