namespace LoginSystem.Application.DTOs
{


    public class ResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object? Data { get; set; }
    }

    public class RegisterDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }
    }

  
    
        public class LoginDto
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
    
}