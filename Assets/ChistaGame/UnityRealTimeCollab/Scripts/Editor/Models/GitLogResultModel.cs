using System;

namespace ChistaGame.RealTimeCollab.Editor.Models
{
    [Serializable]
    public class GitLogResultModel
    {
        public string commit;
        public string author;
        public string date;
        public string message;
    }
}