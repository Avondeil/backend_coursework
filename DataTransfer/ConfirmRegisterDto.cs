namespace api_details.DataTransfer
{
    public class ConfirmLoginDto
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public bool RememberMe { get; set; }
    }

}
