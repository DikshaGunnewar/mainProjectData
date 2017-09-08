using ServiceLayer.EnumStore;
using ServiceLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    public class AlgoService:IAlgoService
    {
        #region Initializations
        private readonly ITwitterServices _twitterService;
        private readonly ISpotifyServices _spotifyService;
        private readonly IInstagramServices _instagramService;
        private readonly IPinterestServices _pinterestService;
        private readonly IDeezerServices _deezerService ;

        private readonly IUserService _userService;
        public AlgoService(ITwitterServices twitterService, 
            IUserService userService, 
            ISpotifyServices spotifyService, 
            IInstagramServices instagramService, 
            IPinterestServices pinterestService,
            IDeezerServices deezerService)
        {
            _twitterService = twitterService;
            _userService = userService;
            _deezerService = deezerService;
            _pinterestService = pinterestService;
            _instagramService = instagramService;
            _spotifyService = spotifyService;
        }
        #endregion

        public async void BeginAlgo(){
            try 
            {
                var AllAccounts = _userService.GetAllAccounts();
                foreach (var item in AllAccounts)
                {
                    if (item.Status == true && (item.IsSubscribed == true || item.IsTrail == true)) { //check service status
                        if (item.Provider == "Twitter") {
                          await _twitterService.scheduleAlgo(item);
                          await _twitterService.CheckForConversion(item);
                        
                        }else if (item.Provider == "Deezer") {
                            await _deezerService.DeezerAlgoForUser(item);
                            _deezerService.CheckForConversion(item);

                        }
                        else if (item.Provider == "Spotify")
                        {
                            await _spotifyService.SpotifyAlgoForUser(item);
                            _spotifyService.CheckForConversion(item);

                        }
                        else if (item.Provider == "Instagram")
                        {
                            _instagramService.scheduleAlgo(item);

                        }
                        else if (item.Provider == "Pinterest")
                        {
                            _pinterestService.ScheduleAlgo(item);

                        }


                        //switch (item.Provider)
                        //{
                        //    case "Twitter":
                        //        _twitterService.scheduleAlgo(item);
                        //        break;
                        //    //case "Spotify":
                        //    //    _spotifyService.SpotifyAlgoForUser(item);
                        //    //    _spotifyService.CheckForConversion(item);
                        //    //    break;
                        //    //case "Instagram":
                        //    //    _instagramService.scheduleAlgo(item);
                        //    //    break;
                        //    //case "Pinterest":
                        //    //    _pinterestService.ScheduleAlgo(item);
                        //    //    break;
                        //    case "Deezer":
                        //        _deezerService.DeezerAlgoForUser(item);
                        //        _deezerService.CheckForConversion(item);
                        //        break;
                                
                        //}
                        
                        
                        //if (item.Provider == SocialMediaProviders.Spotify.ToString())
                        //{
                        //    _twitterService.scheduleAlgo(item);
                        //    _twitterService.CheckForConversion(item);
                            
                        //}
                    }
                    
                }

            }
            catch (Exception)
            {
                
                throw;
            }
        }

    }
}
