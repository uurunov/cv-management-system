namespace cv_management_app.Dropbox;

public class DropboxOptions
{
    public const string SectionName = "Dropbox";

    public string AppKey { get; set; } = "";
    public string AppSecret { get; set; } = "";
    public string RefreshToken { get; set; } = "";
}