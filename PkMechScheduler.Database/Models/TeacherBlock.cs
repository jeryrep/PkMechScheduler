using System.ComponentModel.DataAnnotations.Schema;

namespace PkMechScheduler.Database.Models;

[Table("TeacherBlocks")]
public class TeacherBlock : BaseBlock
{
    public string? Courses { get; set; }
    public string? Description { get; set; }
}