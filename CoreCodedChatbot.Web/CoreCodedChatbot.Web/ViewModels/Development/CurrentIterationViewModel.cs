using System.Collections.Generic;
using CoreCodedChatbot.ApiContract.ResponseModels.DevOps.ChildModels;

namespace CoreCodedChatbot.Web.ViewModels.Development
{
    public class CurrentIterationViewModel
    {
        public List<DevOpsWorkItem> WorkItems { get; set; }
    }
}