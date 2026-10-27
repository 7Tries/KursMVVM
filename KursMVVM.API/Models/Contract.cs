using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KursMVVM.API.Models;

public class Contract
{
    [Key]
    public string ContractNumber { get; set; } = string.Empty;

    public int EmployeeId { get; set; }

    public string OrganizationName { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    [JsonIgnore]
    public virtual Employee Employee { get; set; } = null!;
    [JsonIgnore]
    public virtual Supply? Supply { get; set; }
}
