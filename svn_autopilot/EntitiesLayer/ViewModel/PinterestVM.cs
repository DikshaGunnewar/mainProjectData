using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer.ViewModel
{
    class PinterestVM
    {
    }
    public class PinterestUser{
        public UserVM data { get; set; }
       
    }
    public class UserVM{
        public string id{ get; set; }
        public string first_name{ get; set; }
        public string last_name{ get; set; }
        public string url{ get; set; }
        public string username{ get; set; }
        public ImageVm image { get; set; }
        }

    public class PinterestBoard {
        public List<Boards> data { get; set; }
        public PinterestBoard()
        {
            data = new List<Boards>();
        }
    }

    public class Boards {
        public string url { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string privacy { get; set; }
        public bool IsMap { get; set; }
        public bool MapWithOtherBoard { get; set; }

    }

    public class PinterestSchedulePinVm {
        public int Id { get; set; }
        public int SocialId { get; set; }
        public string note { get; set; }
        public string link { get; set; }
        public string image_url { get; set; }
        public string board { get; set; }
        public string boardName { get; set; }
        public string ScheduleDate { get; set; }
    }
    public class PinterestUsersBoards
    {
        public List<Boards> data { get; set; }
        public PageOptions page { get; set; }
    }
    public class PageOptions {
        public string cursor { get; set; }
        public string next { get; set; }
    }
    public class PinterestPin
    {
        public List<Pins> data { get; set; }
        public PageOptions page { get; set; }
        public PinterestPin()
        {
            data = new List<Pins>();
        }
    }
    public class PinInfoVm
    {
        public Pins data { get; set; }
     
    }

    public class Pins
    {
        public string url { get; set; }
        public string note { get; set; }
        public string link { get; set; }
        public string id { get; set; }
        public UserVM creator { get; set; }
        public ImageVm image { get; set; }
    }
    public class ImageVm
    {
        public ImageDetails original { get; set; }

       
    }
    public class ImageDetails {
        public string url { get; set; }
    }

    public class UserFollowersList { 
        public List<UserVM> data { get; set; }
        public PageOptions page { get; set; }
        public UserFollowersList()
        {
            data = new List<UserVM>();
        }
    }

    public class UserInformation
    {
        public UserVM data{ get; set; }
    }

}
