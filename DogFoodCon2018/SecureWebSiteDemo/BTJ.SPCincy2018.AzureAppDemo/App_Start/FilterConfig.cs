using System.Web;
using System.Web.Mvc;

namespace BTJ.SPCincy2018.AzureAppDemo
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
