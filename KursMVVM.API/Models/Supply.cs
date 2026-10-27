using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace KursMVVM.API.Models;

public class Supply
{
    [Key]
    public string ContractNumber { get; set; } = string.Empty;

    public string EquipmentType { get; set; } = string.Empty;

    public string? Comment { get; set; }

    [ForeignKey("Employee")]
    public int EmployeeId { get; set; }

    [JsonIgnore]
    public virtual Contract Contract { get; set; } = null!;
    [JsonIgnore]
    public virtual Employee Employee { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();
}
