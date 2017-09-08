using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer.Entities
{
    public class SuggestedTracks
    {
        public int Id { get; set; }
        public string TrackName{ get; set; }
        public string TrackId { get; set; }
        public int SocialId { get; set; }
        public string Provider{ get; set; }
        public bool IsAdded { get; set; }
        public bool IsIgnored { get; set; }
        public string SourceName { get; set; }
        public string SourceType { get; set; }
        public string uri { get; set; }

    }
}
