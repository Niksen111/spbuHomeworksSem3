namespace MyNUnitWeb.Data;

using System.ComponentModel.DataAnnotations.Schema;

public class Assembly
{
    [Column("id_assembly")]
    public int AssemblyId { get; set; }
    
    [Column("name")]
    public string Name { get; set; }

    [Column("test_count")]
    public int TestsCount { get; set; }

    [Column("passed")]
    public int Passed { get; set; }

    [Column("failed")]
    public int Failed { get; set; }

    [Column("ignored")]
    public int Ignored { get; set; }

    [Column("datetime")]
    public string? Datetime { get; set; }

    public virtual ICollection<Test> Tests { get; set; }
}