using Ecommerce.Models.AuthModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Data;

public class ApplicationDbContext : IdentityDbContext<ApiUser>
{
    public ApplicationDbContext(DbContextOptions options): base(options)
    {
        
    }
}