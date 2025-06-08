namespace api_details.DataTransfer
{
    public class ConfirmChangePasswordDto
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public string NewPassword { get; set; }
    }

}
