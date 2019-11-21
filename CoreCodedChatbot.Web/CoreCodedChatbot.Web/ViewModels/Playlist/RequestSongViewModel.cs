using System.ComponentModel.DataAnnotations;
using CoreCodedChatbot.ApiContract.ResponseModels.Playlist.ChildModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CoreCodedChatbot.Web.ViewModels.Playlist
{
    public class RequestSongViewModel
    {
        public int SongRequestId { get; set; }

        public string ModalTitle { get; set; }

        public bool IsNewRequest { get; set; }

        [Display(Name = "Song Name")]
        public string Title { get; set; }

        [Display(Name = "Song Artist")]
        public string Artist { get; set; }

        public string SelectedInstrument { get; set; }

        [Display(Name = "Instrument")]
        public SelectListItem[] Instruments { get; set; }

        [Display(Name = "Use a VIP token?")]
        public bool IsVip { get; set; }

        [Display(Name = "Use a Super VIP token?")]
        public bool IsSuperVip { get; set; }

        public bool ShouldShowVip { get; set; }

        public bool ShouldShowSuperVip { get; set; }

        public static RequestSongViewModel GetNewRequestViewModel(bool shouldShowVip, bool shouldShowSuperVip)
        {
            return new RequestSongViewModel
            {
                ModalTitle = "Request a song",
                IsNewRequest = true,
                Title = string.Empty,
                Artist = string.Empty,
                Instruments = GetRequestInstruments(),
                SelectedInstrument = string.Empty,
                IsVip = false,
                IsSuperVip = false,
                ShouldShowVip = shouldShowVip,
                ShouldShowSuperVip = shouldShowSuperVip
            };
        }

        public static RequestSongViewModel GetEditRequestSongViewModel(PlaylistItem songRequest)
        {
            if (songRequest == null) return null;

            var formattedRequest = songRequest.FormattedRequest;

            return new RequestSongViewModel
            {
                ModalTitle = "Edit your request",
                IsNewRequest = false,
                SongRequestId = songRequest.songRequestId,
                Title = formattedRequest?.SongName ?? songRequest.songRequestText,
                Artist = formattedRequest?.SongArtist ?? string.Empty,
                Instruments = GetRequestInstruments(formattedRequest?.InstrumentName),
                SelectedInstrument = formattedRequest?.InstrumentName ?? "guitar",
                IsVip = songRequest.isVip,
                ShouldShowVip = false,
                IsSuperVip = songRequest.isSuperVip,
                ShouldShowSuperVip = false,
            };
        }

        private static SelectListItem[] GetRequestInstruments(string selectedInstrumentName = "guitar")
        {
            var instrumentName = string.IsNullOrWhiteSpace(selectedInstrumentName) ? "guitar" : selectedInstrumentName;
            return new[]
            {
                new SelectListItem("Guitar", "guitar", instrumentName == "guitar"),
                new SelectListItem("Bass", "bass", instrumentName == "bass"),
            };
        }
    }
}