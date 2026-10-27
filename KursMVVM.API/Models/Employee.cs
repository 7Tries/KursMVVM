using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace KursMVVM.API.Models;

public class Employee
{
    [Key]
    public int Id { get; set; }

    public int DepartmentCode { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Position { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Salary { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Bonus { get; set; }

    public string Month { get; set; } = string.Empty;

    [JsonIgnore]
    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    [JsonIgnore]
    public virtual ICollection<Supply> Supplies { get; set; } = new List<Supply>();
}
