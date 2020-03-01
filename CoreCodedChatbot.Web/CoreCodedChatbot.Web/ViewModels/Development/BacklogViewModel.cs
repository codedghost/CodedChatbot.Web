using System.Collections.Generic;
using CoreCodedChatbot.ApiContract.ResponseModels.DevOps.ChildModels;

namespace CoreCodedChatbot.Web.ViewModels.Development
{
    public class BacklogViewModel
    {
        public List<DevOpsWorkItem> WorkItems { get; set; }
    }
}