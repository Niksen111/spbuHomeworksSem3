using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyNUnitWeb.Data;

namespace MyNUnitWeb.Pages;

public class History : PageModel
{
    public void OnGet()
    {
        
    }

    public List<Assembly> GetAssemblies(int assemblyCount)
    {
        using var context = new TestingDataDbContext(new DbContextOptions<TestingDataDbContext>());
        return context.Assemblies.OrderByDescending(assembly => assembly.AssemblyId).Take(assemblyCount).ToList();
    }
    
    public List<Test> GetTests(int assemblyId)
    {

        using var context = new TestingDataDbContext(new DbContextOptions<TestingDataDbContext>());
        return context.Tests.Where(test => test.AssemblyId == assemblyId).Select(test => test).ToList();
    }
}