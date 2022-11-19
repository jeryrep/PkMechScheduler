using System.ComponentModel.DataAnnotations.Schema;

namespace MechScraper.Models;

[Table("TeacherBlocks")]
public class TeacherBlock : BaseBlock
{
    public string? Courses { get; set; }
    public string? Description { get; set; }
}