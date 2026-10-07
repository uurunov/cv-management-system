using System.ComponentModel.DataAnnotations;

namespace cv_management_app.Dtos;

public class SupportTicketRequest
{
    public string Summary { get; set; } = "";

    [AllowedValues("High", "Average", "Low")]
    public string Priority { get; set; } = "";

    public string Link { get; set; } = "";

    public string? PositionTitle { get; set; }
}