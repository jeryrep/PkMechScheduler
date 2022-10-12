using System.ComponentModel.DataAnnotations.Schema;

namespace PkMechScheduler.Database.Models;

[Table("StudentBlocks")]
public class StudentBlock : BaseBlock
{
    public string? Initials { get; set; }
}