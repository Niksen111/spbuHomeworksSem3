using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyNUnitWeb.Data;

namespace MyNUnitWeb.Pages;

public class History : PageModel
{
    private readonly TestingDataDbContext context;
    
    public History(TestingDataDbContext context) 
        => this.context = context;

    public IList<Data.Assembly> Assemblies { get; private set; } = new List<Data.Assembly>();
    
    public List<Data.Assembly> GetAssemblies(int assemblyCount = -1)
    {
        using var context = new TestingDataDbContext(new DbContextOptions<TestingDataDbContext>());
        if (assemblyCount == -1)
        {
            return context.Assemblies.OrderByDescending(assembly => assembly.AssemblyId).Select(t => t).ToList();
        }
        
        return context.Assemblies.OrderByDescending(assembly => assembly.AssemblyId).Take(assemblyCount).ToList();
    }
    
    public void OnGet()
    {
        Assemblies = GetAssemblies();
    }
}