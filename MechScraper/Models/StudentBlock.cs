using System.ComponentModel.DataAnnotations.Schema;

namespace MechScraper.Models;

[Table("StudentBlocks")]
public class StudentBlock : BaseBlock
{
    public string? Initials { get; set; }
}