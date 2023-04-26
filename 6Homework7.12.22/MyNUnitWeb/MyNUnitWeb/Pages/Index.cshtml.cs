using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyNUnit;

namespace MyNUnitWeb.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }

    public async Task Run()
    {
        var files = "Path-to-uploaded-files";
        var result = await Task.Run(() => TestsRunner.RunTests(files));
        // Add new record to db
        // 
    }
}