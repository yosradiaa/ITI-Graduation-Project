namespace ECommerce.API
{
    public class ResultFile
{

    public bool Successed { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Thumb { get; set; }
    public string? publicId { get; set; }
}
}
