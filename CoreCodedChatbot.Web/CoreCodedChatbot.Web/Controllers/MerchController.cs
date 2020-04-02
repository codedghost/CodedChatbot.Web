using System.Collections.Generic;
using System.Threading.Tasks;
using CoreCodedChatbot.Web.ViewModels.Merch;
using Microsoft.AspNetCore.Mvc;
using PrintfulLib.Interfaces.ExternalClients;

namespace CoreCodedChatbot.Web.Controllers
{
    //public class MerchController : Controller
    //{
    //    private readonly IPrintfulClient _printfulClient;

    //    public MerchController(IPrintfulClient printfulClient)
    //    {
    //        _printfulClient = printfulClient;
    //    }

    //    public async Task<IActionResult> Index()
    //    {
    //        var products = await _printfulClient.GetAllProducts();

    //        return View("Merch", BuildMerchLandingViewModel(products, string.Empty));
    //    }

    //    [HttpPost("/search")]
    //    public async Task<IActionResult> Search(MerchLandingViewModel submittedModel)
    //    {
    //        var products = string.IsNullOrWhiteSpace(submittedModel.SearchTerms)
    //            ? await _printfulClient.GetAllProducts()
    //            : await _printfulClient.SearchAllProducts(submittedModel.SearchTerms);

    //        return View("Merch", BuildMerchLandingViewModel(products, submittedModel.SearchTerms));
    //    }

    //    [HttpGet("/product/{id}")]
    //    public async Task<IActionResult> ProductPage(int id)
    //    {
    //        var variants = await _printfulClient.GetVariantsById(id);

    //        return View("ProductPage", BuildProductPageViewModel(variants));
    //    }

    //    private MerchLandingViewModel BuildMerchLandingViewModel(List<GetSyncVariantsResult> getSyncProductsResult, string searchTerms)
    //    {
    //        return new MerchLandingViewModel
    //        {
    //            SearchTerms = searchTerms,
    //            SyncVariants = getSyncProductsResult
    //        };
    //    }

    //    private ProductPageViewModel BuildProductPageViewModel(GetSyncVariantsResult getSyncVariantsResult)
    //    {
    //        return new ProductPageViewModel
    //        {
    //            VariantQueryResult = getSyncVariantsResult.Result
    //        };
    //    }
    //}
}