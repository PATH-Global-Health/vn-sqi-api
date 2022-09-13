using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class Pagination
    {
        public int PageIndex { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
    }
    public class PaginationResultModel : ResultModel
    {
        public Pagination Pagination { get; set; }
    }

    public class ResultModel
    {
        public string ErrorMessage { get; set; }
        public object Data { get; set; }
        public bool Succeed { get; set; }
        public object ResponseFailed { get; set; }

    }

    public class ResultMessage 
    {
        public bool IsSuccessStatus { get; set; }
        public object Response{ get; set; }
      

    }
}
