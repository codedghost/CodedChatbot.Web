using System;
using System.Linq;
using CoreCodedChatbot.Secrets;
using My.JDownloader.Api;
using My.JDownloader.Api.Models.LinkgrabberV2;

namespace CoreCodedChatbot.Web.Services
{
    public interface IDownloadChartService
    {
        void Download(string downloadUrl, int directoryId);
    }

    public class DownloadChartService : IDownloadChartService
    {
        private readonly ISecretService _secretService;

        public DownloadChartService(
            ISecretService secretService
        )
        {
            _secretService = secretService;
        }

        public void Download(string downloadUrl, int directoryId)
        {
            var jdownloaderUsername = _secretService.GetSecret<string>("JDownloaderUsername");
            var jdownloaderPassword = _secretService.GetSecret<string>("JDownloaderPassword");
            var jdownloaderAppKey = _secretService.GetSecret<string>("JDownloaderAppKey");

            var jdownloader = new JDownloaderHandler(jdownloaderUsername, jdownloaderPassword, jdownloaderAppKey);

            var devices = jdownloader.GetDevices();

            var mainDevice = devices.FirstOrDefault();

            if (mainDevice == null) throw new Exception("JDownloader instance not running or not discoverable");

            var handler = jdownloader.GetDeviceHandler(mainDevice);

            handler.LinkgrabberV2.AddLinks(new AddLinkRequestObject
            {
                DestinationFolder = directoryId.ToString(),
                AutoExtract = true,
                AutoStart = true,
                Links = downloadUrl
            });

            handler.DownloadController.Start();
        }
    }
}