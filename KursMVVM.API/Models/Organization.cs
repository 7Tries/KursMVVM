using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace KursMVVM.API.Models;

public class Organization
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Supply")]
    public string ContractNumber { get; set; } = string.Empty;

    public string CountryCode { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Website { get; set; }

    [JsonIgnore]
    public virtual Supply Supply { get; set; } = null!;
}
