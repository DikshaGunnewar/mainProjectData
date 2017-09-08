using EntitiesLayer.Entities;
using EntitiesLayer.ViewModel;
using RepositoryLayer.Infrastructure;
using RepositoryLayer.Repositories;
using RestSharp;
using RestSharp.Deserializers;
using ServiceLayer.EnumStore;
using ServiceLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLayer.Helper;
using System.IO;
using Newtonsoft.Json;
using TweetSharp;
using System.Data.Entity;
using Hangfire;

namespace ServiceLayer.Services
{
    public class PinterestServices : IPinterestServices
    {
        #region Initialization
        private readonly IRepository<SocialMedia> _socialMedia;
       private readonly IRepository<AccessDetails> _accessDetail;
       private readonly IRepository<PinterestScheduledPin> _scheduleTaskRepo;
       private readonly IRepository<PinterestFollowingBoardMapping> _boardMapRepo;
       private readonly IRepository<Activities> _activityRepo;
       private readonly IRepository<Tags> _tagRepo;

       private readonly IUserService _userService;  
       private readonly IUnitOfWork _unitOfWork;
       private readonly string consumerKey;
       private readonly string consumerSecret;
       private readonly string ApiURL ;
       private readonly string RedirectURL;

       public PinterestServices(IRepository<SocialMedia> socialMedia, IUnitOfWork unitOfWork,
           IRepository<AccessDetails> accessDetail, IRepository<PinterestScheduledPin> scheduleTaskRepo, IUserService userService,
            IRepository<PinterestFollowingBoardMapping> boardMapRepo, IRepository<Activities> activityRepo, IRepository<Tags> tagRepo)
       {
           _socialMedia = socialMedia;
           _unitOfWork = unitOfWork;
           _accessDetail = accessDetail;
           _scheduleTaskRepo = scheduleTaskRepo;
           _userService = userService;
           _boardMapRepo = boardMapRepo;
           _activityRepo = activityRepo;
           _tagRepo = tagRepo;
           consumerKey = ConfigurationSettings.AppSettings["PinID"];
           consumerSecret = ConfigurationSettings.AppSettings["PinSecret"];
           ApiURL = "https://api.pinterest.com/";
           RedirectURL = ConfigurationSettings.AppSettings["PinRedirectUrl"];

      }
        #endregion

       /// <summary>
       /// Method to get the uri for authorizing user in Pinterest
       /// </summary>
       /// <returns></returns>
       public string Authorize() {
           try
           {
               
               var request = new RestRequest("oauth/", Method.GET);
               request.AddHeader("Content-Type", "application/json");
               request.AddParameter("scope", "read_public,write_public,read_relationships,write_relationships");
               request.AddParameter("client_id", consumerKey);
               request.AddParameter("redirect_uri", RedirectURL);
               request.AddParameter("response_type", "code");
               // request.AddParameter("host_id", "QwJrNRmITnC3ZenQzXtedg");
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               var uri = response.ResponseUri.AbsoluteUri;
              
               return uri;
           }
           catch (Exception)
           {
               
               throw;
           }
           
       }

        /// <summary>
       /// Method to get OAuth token for authenticate user
        /// </summary>
        /// <param name="Code"></param>
        /// <returns></returns>
       public OAuthTokens GetPinToken(string Code)
       {
           var request = new RestRequest("v1/oauth/token", Method.POST);
           //request.AddHeader("Content-Type", "application/json");
           //request.AddHeader("Authorization", "OAuth " + token);
           request.AddParameter("grant_type", "authorization_code");
           request.AddParameter("client_id", consumerKey);
           //request.AddParameter("scope", "read_public");
           //request.AddParameter("redirect_uri", RedirectURL);
           request.AddParameter("client_secret", consumerSecret);
           request.AddParameter("code", Code);
            // request.AddParameter("host_id", "QwJrNRmITnC3ZenQzXtedg");
           var response = WebServiceHelper.WebRequest(request, ApiURL);
           JsonDeserializer deserial = new JsonDeserializer();
           var result = deserial.Deserialize<OAuthTokens>(response);
          
           return result;
       }

       public string SaveAccountDeatils(OAuthTokens tokens, string userId, string Email)
       {
           try
           {
               AccessDetails accessToken = new AccessDetails() { AccessToken = tokens.access_token };
               var returnMessage = string.Empty;
               PinterestUser profile = GetUserprofile(accessToken);
                var checkAccountIsAvail = _socialMedia.Get().Include(x=>x.AccessDetails).Where(x =>x.SMId == profile.data.id.ToString() && x.IsDeleted == false).FirstOrDefault();
                if (checkAccountIsAvail == null)
                {
                    SocialMedia socialDetails = new SocialMedia()
                 {
                     UserId = userId,
                     Provider = SocialMediaProviders.Pinterest.ToString(),
                     AccessDetails = new AccessDetails { AccessToken = tokens.access_token },

                     SMId = profile.data.id,
                     Status = true,
                     UserName = profile.data.username,
                     AccSettings = new AccSettings()
                 };
                    socialDetails.AccSettings.UserManagement.Add(new UserManagement { Email = Email, userId = userId, Role = "Owner" });
                    _socialMedia.Add(socialDetails);
                }
                else if (checkAccountIsAvail.UserId == userId)
                {
                    checkAccountIsAvail.AccessDetails.AccessToken = tokens.access_token;
                    checkAccountIsAvail.IsInvalid = false;
                    checkAccountIsAvail.Status = true;
                    returnMessage = "Already added.";
                }
                else
                {
                    checkAccountIsAvail.AccessDetails.AccessToken = tokens.access_token;
                    checkAccountIsAvail.IsInvalid = false;
                    checkAccountIsAvail.Status = true;
                    returnMessage = "Cannot add this account, as already added by other user.";
                }
               _unitOfWork.Commit();
               return returnMessage;
           }
           catch (Exception)
           {
               return "Something went wrong.";
           }
       }

       public PinterestUser GetUserprofile(AccessDetails Token)
       {
           try
           {

               var request = new RestRequest("v1/me", Method.GET);
               //request.AddHeader("Content-Type", "application/json");
               //request.AddHeader("Authorization", "OAuth " + token);
               request.AddParameter("access_token", Token.AccessToken);
               request.AddParameter("fields", "first_name,id,last_name,url,username");

               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               PinterestUser result = deserial.Deserialize<PinterestUser>(response);
               return result;
           }
           catch (Exception)
           {

               throw;
           }
       }

       public List<Boards> GetUsersFollowingBoard(AccessDetails Token) {
           try
           {
               List<Boards> followingBoardList = new List<Boards>();
               var request = new RestRequest("v1/me/following/boards/", Method.GET);
               request.AddParameter("access_token", Token.AccessToken);
               var response = WebServiceHelper.WebRequest(request, ApiURL);

               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<PinterestUsersBoards>(response);
               foreach (var item in result.data)
	            {
                    followingBoardList.Add(new Boards { id = item.id, name = item.name, url = item.url });
	            }

               if (result.page.cursor != null) {
                   while (result.page.cursor == null) {
                       request.AddParameter("cursor", result.page.cursor);
                       response = WebServiceHelper.WebRequest(request, ApiURL);
                       result = deserial.Deserialize<PinterestUsersBoards>(response);
                       foreach (var item in result.data)
                       {
                           followingBoardList.Add(new Boards { id = item.id, name = item.name, url = item.url });
                       }
                   }
               }
               return followingBoardList;
               
           }
           catch (Exception)
           {
               
               throw;
           }
       }

       public bool CreatePin(PinterestScheduledPin pinInfo,AccessDetails Token) {
           try
           {
               var request = new RestRequest("v1/pins/", Method.POST);
               //request.AddHeader("Content-Type", "application/json");
               //request.AddHeader("Authorization", "OAuth " + token);
               request.AddParameter("access_token", Token.AccessToken);
               request.AddParameter("board", pinInfo.board);
               request.AddParameter("note", pinInfo.note);
               request.AddParameter("image_url", pinInfo.image_url);
               //request.AddBody("link", Token.AccessToken);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               if (response.StatusCode.ToString() == "Created")
               {
                   return true;
               }
               else
               {
                   return false;
               }
           }
           catch (Exception)
           {
               return false;
           }
       }

       public bool SaveSchedulePin(PinterestScheduledPin PinInfo){
           try
           {
               if (PinInfo.Id == 0)
               {
                   var task = _scheduleTaskRepo.AddAndReturn(PinInfo);
                   _unitOfWork.Commit();
                   double secondsToAdd = ( PinInfo.ScheduleDate -DateTime.UtcNow ).TotalSeconds;
                   task.jobId = BackgroundJob.Schedule<IPinterestServices>(
                                     x => x.PostSchedulePin(task.Id,PinInfo.SocialId),
                                    TimeSpan.FromSeconds(secondsToAdd));

               }
               else {
                   //PinInfo.image_url = GetASchedulePin(PinInfo.Id).image_url;
                   var temp = _scheduleTaskRepo.Get().Where(x => x.Id == PinInfo.Id).FirstOrDefault();
                   BackgroundJob.Delete(temp.jobId);                   
                   temp.image_url = PinInfo.image_url;
                   temp.note = PinInfo.note;
                   temp.jobId = PinInfo.jobId;
                   temp.ScheduleDate = PinInfo.ScheduleDate;
                   temp.link = PinInfo.link;
                   temp.board = PinInfo.board;
                   
               }
               _unitOfWork.Commit();
              
               return true;
           }
           catch (Exception)
           {
               BackgroundJob.Delete(PinInfo.jobId);
               return false;
           }
       }

       public List<PinterestSchedulePinVm> GetSchedulePin(int socialId)
       {
           try
           {
               var task = _scheduleTaskRepo.Get().Where(x => x.SocialId == socialId).OrderBy(x=>x.ScheduleDate).ToList();
               List<PinterestSchedulePinVm> pinList = new List<PinterestSchedulePinVm>();
               var userBoards = GetAllBoard(_userService.GetAccount(socialId).AccessDetails);
               foreach (var item in task)
               {
                   var boardName = string.Empty;
                   foreach (var board in userBoards)
	                {
		                if(item.board == board.id){boardName = board.name;}
	                }

                   pinList.Add(new PinterestSchedulePinVm { 
                       ScheduleDate = item.ScheduleDate.ToShortDateString() + " " + item.ScheduleDate.ToShortTimeString(), 
                       boardName = boardName,
                       image_url = item.image_url,
                       Id = item.Id, note = item.note,
                       link = item.link,
                       SocialId = item.SocialId });
               }
               return pinList;
           }
           catch (Exception)
           {
               
               throw;
           }
       }

       public List<Pins> GetBoardsPin(string boardId, AccessDetails Token)
       {
           try
           {
               List<Pins> pins = new List<Pins>();

               var request = new RestRequest("/v1/boards/"+boardId+"/pins/", Method.GET);
               request.AddParameter("access_token", Token.AccessToken);
               request.AddParameter("fields", "link,id,note,url,image,creator");

               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               PinterestPin result = deserial.Deserialize<PinterestPin>(response);
               foreach (var item in result.data)
               {
                   pins.Add(new Pins { id = item.id, creator = item.creator,image = item.image,note=item.note,url=item.url});
               }

               if (result.page.cursor != null)
               {
                   while (result.page.cursor == null)
                   {
                       request.AddParameter("cursor", result.page.cursor);
                       response = WebServiceHelper.WebRequest(request, ApiURL);
                       result = deserial.Deserialize<PinterestPin>(response);
                       foreach (var item in result.data)
                       
                           pins.Add(new Pins { id = item.id, creator = item.creator, image = item.image, note = item.note, url = item.url });
                       }
                   }

               return pins;
           }
           catch (Exception)
           {
               
               throw;
           }
       }

       public PinterestScheduledPin GetASchedulePin(int schedulePinId){
           try
           {
               var pinInfo = _scheduleTaskRepo.Get().Where(x => x.Id == schedulePinId).FirstOrDefault();
               return pinInfo;
           }
           catch (Exception)
           {
               throw;
           }
       }

       public List<UserVM> GetUserFollowers(AccessDetails Token)
       {
           try
           {
               List<UserVM> followers = new List<UserVM>();
               var request = new RestRequest("v1/me/following/users/", Method.GET);
               request.AddParameter("access_token", Token.AccessToken);

               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               UserFollowersList result = deserial.Deserialize<UserFollowersList>(response);
               foreach (var item in result.data)
               {
                   followers.Add(new UserVM { id = item.id, first_name = item.first_name, last_name= item.last_name});
               }

               if (result.page.cursor != null)
               {
                   while (result.page.cursor == null)
                   {
                       request.AddParameter("cursor", result.page.cursor);
                       response = WebServiceHelper.WebRequest(request, ApiURL);
                       result = deserial.Deserialize<UserFollowersList>(response);
                       foreach (var item in result.data)
                       {
                           followers.Add(new UserVM { id = item.id, first_name = item.first_name, last_name = item.last_name });
                       }
                   }
               }
               return followers;
           }
           catch (Exception)
           {
               throw;
           }
       }

       public List<Boards> GetAllBoard(AccessDetails Token)
       {
           try
           {
               var request = new RestRequest("v1/me/boards/", Method.GET);
               request.AddParameter("access_token", Token.AccessToken);
               request.AddParameter("fields", "privacy,id,name,url");

               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               PinterestBoard result = deserial.Deserialize<PinterestBoard>(response);
               return result.data.Where(x=>x.privacy == "public").ToList();
           }
           catch (Exception)
           {
               throw;
           }
       }

       public bool FollowUser(string userId,AccessDetails Token) {
           try
           {
               var request = new RestRequest("v1/me/following/users/", Method.POST);
               request.AddParameter("access_token", Token.AccessToken);
               request.AddParameter("user",userId);

               var response = WebServiceHelper.WebRequest(request, ApiURL);
               if (response.StatusCode.ToString() == "OK")
               {
                   return true;
               }
               else {
                   return false;
               }
             
           }
           catch (Exception)
           {

               return false;
           }
       }

       public UserInformation GetUserInfo(string userId, AccessDetails Token)
       {
           try
           {
               var request = new RestRequest("/v1/users/" + userId + "/", Method.GET);
               request.AddParameter("access_token", Token.AccessToken);
               request.AddParameter("fields", "first_name,image,id,last_name,username");

               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               
               UserInformation result = deserial.Deserialize<UserInformation>(response);
               //result.data.image.original.url = response.['data'].['image'].['60x60'].ToString();
               return result;

           }
           catch (Exception)
           {

               throw;
           }
       }

       public Pins GetUserRecentPin(AccessDetails Token) {
           try
           {
                var request = new RestRequest("/v1/me/pins/", Method.GET);
               request.AddParameter("access_token", Token.AccessToken);
               //request.AddParameter("fields", "first_name,image,id,last_name,username");
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               
               PinterestPin result = deserial.Deserialize<PinterestPin>(response);
               //result.data.image.original.url = response.['data'].['image'].['60x60'].ToString();
               return result.data[0];
           }
           catch (Exception)
           {
               
               throw;
           }
       }

       public Pins GetPinInfo(string pinId,AccessDetails Token)
       {
           try
           {
               var request = new RestRequest("/v1/pins/" + pinId + "", Method.GET);
               request.AddParameter("access_token", Token.AccessToken);
               request.AddParameter("fields", "image,id,note");
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();

               PinInfoVm result = deserial.Deserialize<PinInfoVm>(response);
               //result.data.image.original.url = response.['data'].['image'].['60x60'].ToString();
               return result.data;
           }
           catch (Exception)
           {

               throw;
           }
       }

       public bool RemoveSchedulePin(int schedulePinId) {
           try
           {    var pinInfo = GetASchedulePin(schedulePinId);
               _scheduleTaskRepo.Remove(pinInfo);
               _unitOfWork.Commit();
               BackgroundJob.Delete(pinInfo.jobId);
               return true;
           }
           catch (Exception)
           {
               return false;
           }
       
       }

       public List<Boards> CheckBoardConnection(int socailId, string myBoardId, List<Boards> followingBoard)
       {
           try
           {
               List<Boards> list = new List<Boards>();
               var boardMaps = _boardMapRepo.Get().Where(x => x.SocialId == socailId &&  x.IsDeleted == false).ToList();


                if (boardMaps.Count() != 0)
                {
                    foreach (var item in followingBoard)
                    {
                            foreach (var map in boardMaps)
                            {

                            if (item.id == map.FollowingBoardId && map.MyBoardId == myBoardId)
                            {
                                //list.Add(new Boards { id = item.id, name = item.name, IsMap = true, url = item.url });
                                item.IsMap = true;
                            }
                            else if(item.id == map.FollowingBoardId && map.MyBoardId != myBoardId)
                            {
                                //list.Add(new Boards { id = item.id, name = item.name, IsMap = false, url = item.url });
                                item.MapWithOtherBoard = true;
                                //followingBoard.Remove(item);
                            }
                        }

                    }
                 }



                return followingBoard;
           }
           catch (Exception)
           {
               
               throw;
           }
       }

       public bool SaveBoardMapping(int socialId, string myBoardId, string followingBoardIds)
       {
           try
           {
               var boardIds = followingBoardIds.Split(',');
               var boardMaps = _boardMapRepo.Get().Where(x => x.SocialId == socialId && x.MyBoardId == myBoardId).ToList();
               foreach (var item in boardMaps)
               {
                   RemoveMapping(item);
               }
               foreach (var item in boardIds)
               {
                   var check = _boardMapRepo.Get().Where(x => x.SocialId == socialId && x.MyBoardId == myBoardId && x.FollowingBoardId == item).FirstOrDefault();
                  if (check== null) {
                      _boardMapRepo.Add(new PinterestFollowingBoardMapping { FollowingBoardId = item, MyBoardId = myBoardId, SocialId = socialId,IsDeleted = false});                   
                  }
                  else
                  {
                      check.IsDeleted = false;
                  }

               }
               _unitOfWork.Commit();
               return true;
           }
           catch (Exception)
           {
               return false;
           }
       }

       public void RemoveMapping(PinterestFollowingBoardMapping mapObject) {
           try
           {
               //var map = _boardMapRepo.Get().Where(x => x.SocialId == socialId && x.MyBoardId == myBoardId && x.FollowingBoardId == followingBoardIds).FirstOrDefault();
               mapObject.IsDeleted = true;
               _unitOfWork.Commit();
           }
           catch (Exception)
           {
               
               throw;
           }
       }

       public bool CheckBoardIsMap(int sociaId, string myBoardId, string followingBoardIds) {
           try
           {
               var check = _boardMapRepo.Get().Where(x => x.SocialId == sociaId && x.MyBoardId == myBoardId && x.FollowingBoardId == followingBoardIds ).FirstOrDefault();
               bool status;
               if (check == null)
               {
                   status = false;
               }
               else {
                   status = true;
               }
               return status;
           }
           catch (Exception)
           {
               
               throw;
           }
       }

       public List<RecentActivityViewModel> RecentActivities(int socialId, AccessDetails _accessToken)
       {
           try
           {
               var DateLimit = DateTime.UtcNow.AddDays(-2);
               var activity = _activityRepo.Get().Where(x => x.socialId == socialId && x.ActivityDate >= DateLimit).OrderByDescending(x => x.ActivityDate).Take(10).ToList();
               List<RecentActivityViewModel> activityDetails = new List<RecentActivityViewModel>();

               foreach (var item in activity)
               {
                   //var Detail = GetUserInfo(item.SMId, _accessToken);
                   if (item.Activity == Activity.PinIt.ToString())
                   {
                       var pinDetails = GetPinInfo(item.PostId, _accessToken);

                       activityDetails.Add(new RecentActivityViewModel { Activity = item.Activity, ActivityDate = item.ActivityDate.ToLocalTime(), UserName = item.Username, Content = pinDetails.image.original.url, Tag = item.Tag });
                   }
                   else if (item.Activity == Activity.Follow.ToString())
                   {
                       activityDetails.Add(new RecentActivityViewModel { Activity = item.Activity, ActivityDate = item.ActivityDate.ToLocalTime(), UserName = item.Username, Content = item.Username, Tag = item.Tag });
                   }

               }
               return activityDetails;
           }
           catch (Exception)
           {

               throw;
           }
       }

       public void ScheduleAlgo(SocialMediaVM acc) {
           try
           {
               //var tasks = _scheduleTaskRepo.Get().Where(x => x.SocialId == acc.Id).ToList();
               List<Tags> tags = _tagRepo.Get().Where(x => x.AccSettingId == acc.AccSettings.Id).ToList();
               FollowingBoardPinTask(acc,tags);
           }
           catch (Exception)
           {
               
               throw;
           }
       }

       #region Algo methods
       public void PostSchedulePin(int schedulePinId, int socialId)
       {
           try
           {
               var accessDetials = _socialMedia.Get().Include(x => x.AccessDetails).Where(x => x.Id == socialId).FirstOrDefault().AccessDetails;
               var task = _scheduleTaskRepo.Get().Where(x => x.Id == schedulePinId).FirstOrDefault();
               task.image_url = ConfigurationSettings.AppSettings["ImageBaseURL"] + "/Images/SchedulePins/" + task.image_url;
               PinterestScheduledPin pinInfo = new PinterestScheduledPin() { board = task.board, note = task.note, image_url = task.image_url };
               if (CreatePin(pinInfo, accessDetials))
               {
                   var usersBoard = GetAllBoard(accessDetials);
                   var userName = GetUserprofile(accessDetials).data.username;
                   var boardName = string.Empty;
                   foreach (var item in usersBoard)
                   {
                       if (pinInfo.board == item.id)
                       {
                           boardName = item.name;
                       }
                   }
                   _userService.SaveActivity(new Activities { socialId = socialId, Activity = Activity.PinIt.ToString(), ActivityDate = DateTime.UtcNow, Tag = '@' + boardName, Username = userName });
               }
           }
           catch (Exception)
           {
               
               throw;
           }
       }

       public async void FollowingBoardPinTask(SocialMediaVM acc,List<Tags> tags) {
           try
           {
               var followingBoards = GetUsersFollowingBoard(acc.AccessDetails);
               var boardMapping = _boardMapRepo.Get().Where(x => x.SocialId == acc.Id).ToList();
               var usersFollowers = GetUserFollowers(acc.AccessDetails);
               var myBoards = GetAllBoard(acc.AccessDetails);
               if (followingBoards.Count() > 0) {
                   int count = 0;
                   int pinterestPinPerFollowingBoard = Int16.Parse(ConfigurationSettings.AppSettings["pinterestPinPerFollowingBoard"].ToString());
                   int limitForPinningFollowPerAccount = Int16.Parse(ConfigurationSettings.AppSettings["pinterestPinPerFollowingBoard"].ToString());                  
                   bool BreakLoop= false; 
                   foreach (var board in followingBoards)
                   {
                       if (BreakLoop) { break; }
                           var allPins = GetBoardsPin(board.id, acc.AccessDetails);//API
                           var pinsCreated = _activityRepo.Get().Where(x => x.socialId == acc.Id && x.Activity == Activity.PinIt.ToString()).ToList();//Table
                           //Get Missing Pins
                           var pins = allPins.Where(p => !pinsCreated.Any(f => f.OriginalPostId == p.id))
                                                .Select(m => new { m.id,m.image,m.creator,m.link,m.note }).ToList();
                           if (pins.Count() > 0)
                           {
                               foreach (var pin in pins.Take(pinterestPinPerFollowingBoard))
                               {    //Follow creator user of a pin in following board
                                   bool allow = true;
                                   foreach (var item in tags.ToList().Where(x => x.IsBlocked == true).ToList()) 
                                   {
                                       if (pin.note.Contains(item.TagName) == true) {
                                           allow = false;
                                       }
                                   }
                                   if (allow == true) {
                                       var creatorUserName = GetUserInfo(pin.creator.id, acc.AccessDetails).data.username;

                                       if (usersFollowers.Where(x => x.id== pin.creator.id).FirstOrDefault() == null)
                                       {
                                           var result = FollowUser(pin.creator.id, acc.AccessDetails);
                                           if (result == true)
                                           {
                                               _userService.SaveActivity(new Activities { SMId = pin.creator.id, socialId = acc.Id, Activity = Activity.Follow.ToString(), ActivityDate = DateTime.UtcNow, Tag = '@' + board.name, Username = creatorUserName });
                                               count++;
                                               await Task.Delay(100000);                                      
                                           }
                                       }

                                       //creating pin from following boards
                                       if (_activityRepo.Get().Where(x => x.socialId == acc.Id && x.OriginalPostId == pin.id).FirstOrDefault() == null)
                                       {
                                           string boardIdToPin = string.Empty;
                                           var check = boardMapping.Where(x => x.FollowingBoardId == board.id).FirstOrDefault();
                                           if (check == null)
                                           {
                                               Random r = new Random();
                                               boardIdToPin = myBoards[r.Next(myBoards.Count())].id;
                                           }
                                           else
                                           {
                                               boardIdToPin = check.MyBoardId;
                                           }
                                           PinterestScheduledPin pinInfo = new PinterestScheduledPin() { board = boardIdToPin, note = pin.note, image_url = pin.image.original.url };
                                           if (CreatePin(pinInfo, acc.AccessDetails))
                                           {
                                               var newPinDetail = GetUserRecentPin(acc.AccessDetails);
                                               _userService.SaveActivity(new Activities { SMId = pin.creator.id, socialId = acc.Id, Activity = Activity.PinIt.ToString(), ActivityDate = DateTime.UtcNow, Tag = '@' + board.name, OriginalPostId = pin.id, PostId = newPinDetail.id, Username = creatorUserName });
                                               count++;
                                               await Task.Delay(100000);                                      
                                           }

                                       }
                                   }

                                   if (count > limitForPinningFollowPerAccount)
                                   {
                                       BreakLoop = true;
                                   }
                               }
                           }
                       
                   }
               }




           }
           catch (Exception)
           {
               
               throw;
           }
       }
       #endregion
    }
}
