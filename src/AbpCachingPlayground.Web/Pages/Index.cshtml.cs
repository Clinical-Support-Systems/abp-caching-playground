using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace AbpCachingPlayground.Web.Pages;

public class IndexModel : AbpCachingPlaygroundPageModel
{
    public void OnGet()
    {

    }

    public async Task OnPostLoginAsync()
    {
        await HttpContext.ChallengeAsync("oidc");
    }
}
