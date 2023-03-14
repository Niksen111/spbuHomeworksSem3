using Microsoft.AspNetCore.Mvc.RazorPages;
using MyNUnitWeb.Data;

namespace MyNUnitWeb.Pages;

public class Assembly : PageModel
{
    private readonly TestingDataDbContext context;

    public Assembly(TestingDataDbContext context) 
        => this.context = context;

    public IList<Test> Tests { get; private set; } = new List<Test>();
    
    public void OnGet(int assemblyId)
    {
        Tests = context.Tests.Where(t => t.AssemblyId == assemblyId).Select(t => t).ToList();
    }
}