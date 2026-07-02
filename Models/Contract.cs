using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace KursMVVM.Models;

public class Contract
{
    [Key]
    public string ContractNumber { get; set; } = string.Empty;

    public int EmployeeId { get; set; }

    public string OrganizationName { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    [XmlIgnore]
    public virtual Employee Employee { get; set; } = null!;
    [XmlIgnore]
    public virtual Supply? Supply { get; set; }
}
