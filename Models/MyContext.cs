#pragma warning disable CS8618

using Microsoft.EntityFrameworkCore;
namespace BankAccounts.Models;


public class MyContext : DbContext 
{   
    public DbSet<User> Users { get; set; } 
    public DbSet<Transaction> Transactions { get; set; } 

    
    public MyContext(DbContextOptions options) : base(options) { }    


    
}
