namespace UsersApi.Services.Exceptions
{
    public class DuplicateEmailException : Exception
    {
        public DuplicateEmailException(string email)
            : base($"A user with email '{email}' already exists.")
        {
            Email = email;
        }

        public string Email { get; }
    }
}
