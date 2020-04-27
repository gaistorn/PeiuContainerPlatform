using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Model.ExchangeModel
{
    public class AttachableBoardModel : BoardModel
    {
        public AttachFileModel[] AttachFiles { get; set; }
    }

    public class AttachableViewBoardModel : BoardModel 
    { 
        public string Username { get; set; }
        public int ViewCount { get; set; }

        public ViewFileModel[] VIewFileModels { get; set; }

    }

    public class BoardModel
    {
        public string Title { get; set; }
        public string Contents { get; set; }
        public int BoardType { get; set; }
        public DateTime Createts { get; set; }
        
    }

    public class ViewFileModel
    {
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public int KBSize { get; set; }
        public string FileId { get; set; }

    }



}
