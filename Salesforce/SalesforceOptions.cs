namespace cv_management_app.Salesforce;

public class SalesforceOptions
{
    public const string SectionName = "Salesforce";
    public string MyDomain { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string ApiVersion { get; set; } = "";
}