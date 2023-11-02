#pragma warning disable CS8618
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BankAccounts.Models;
public class LoginUser
{
    // No other fields!
    [Required]
    public string EmailLogin { get; set; }
    [Required]
    [DataType(DataType.Password)]
    public string PasswordLogin { get; set; }
}
