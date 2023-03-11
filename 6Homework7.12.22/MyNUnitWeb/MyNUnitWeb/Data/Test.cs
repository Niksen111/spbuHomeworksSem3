namespace MyNUnitWeb.Data;

using System.ComponentModel.DataAnnotations.Schema;

public class Test
{
    [Column("id_test")]
    public int TestId { get; set; }

    [Column("id_assembly")]
    public int AssemblyId { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("is_passed")]
    public bool IsPassed { get; set; }

    [Column("running_time")]
    public int RunningTime { get; set; }

    [Column("reason_for_ignoring")]
    public string? ReasonForIgnoring { get; set; }

    [Column("comment")]
    public string? Comment { get; set; }

    public virtual Assembly Assembly { get; set; }
}