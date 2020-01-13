using System.Collections.Generic;
using CoreCodedChatbot.Printful.Models.ApiResponse;

namespace CoreCodedChatbot.Web.ViewModels.Merch
{
    public class MerchLandingViewModel
    {
        public string SearchTerms { get; set; }
        public List<GetSyncVariantsResult> SyncVariants { get; set; }
    }
}