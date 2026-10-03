namespace cv_management_app.Dtos;

public class SalesforceFormRequest
{
    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public string Company { get; set; } = "";

    public string? Phone { get; set; }

    public string? JobTitle { get; set; }

    public string? Description { get; set; }
}