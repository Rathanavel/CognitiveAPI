using CognitiveAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CognitiveAPI.Controllers
{
    public class VideoController : ApiController
    {
        List<Video> videos = new List<Video>()
        {
             new Video() { VideoId = 1, VideoName = "Test Video Name", VideoSize = 30 },
              new Video() { VideoId = 2, VideoName = "Test Video Name", VideoSize = 30 },
               new Video() { VideoId = 3, VideoName = "Test Video Name", VideoSize = 30}
        };

        List<VideoImageFrame> videoImageFrames = new List<VideoImageFrame>()
        {
             new  VideoImageFrame(){VideoID= 1, Name = "Frame 1",Url = "http://somedummyurl1"},
             new  VideoImageFrame(){VideoID= 2, Name = "Frame 2",Url = "http://somedummyurl2"},
             new  VideoImageFrame(){VideoID= 2,Name = "Frame 2",Url = "http://somedummyurl2"},
             new  VideoImageFrame(){VideoID= 3,Name = "Frame 1",Url = "http://somedummyurl1"},
              new  VideoImageFrame(){VideoID= 3,Name = "Frame 2",Url = "http://somedummyurl2"},
               new  VideoImageFrame(){VideoID= 3,Name = "Frame 3",Url = "http://somedummyurl3"}

        };

        [Route("api/videos")]
        public List<Video> Get()
        {
            return videos;
        }

        [Route("api/video/{videoId}")]
        public Video GetVideoMetaData(int videoId)
        {
            return videos.FirstOrDefault(v => v.VideoId == videoId);
            //return new Video() { VideoId = 1, VideoName = "Test Video Name", VideoSize = 30 };
        }

        [Route("api/video/{videoId}/frames")]
        public List<VideoImageFrame> GetAllVideoMetaData(int videoId)
        {
            return videoImageFrames.FindAll(vf => vf.VideoID == videoId);
        }
    }
}
