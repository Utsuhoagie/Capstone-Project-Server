namespace Capstone.Responses.ExceptionHandling
{
    public class CustomValidationFailure
    {
        public string PropertyName { get; set; } = string.Empty;

        public List<string> Errors { get; set; } = new List<string>();
    }
}
