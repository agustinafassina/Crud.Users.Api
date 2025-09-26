namespace UsersApi.Configurations.ClientsDB.Mongo
{
    public class SecurityAuth
    {
        public string Authority { get; set; }
        public string Audience { get; set; }
        public string SecretKey { get; set; }
    }
}