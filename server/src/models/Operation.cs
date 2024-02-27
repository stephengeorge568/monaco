using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.models
{
    public class Operation
    {
        public string? Text { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public int RevisionId { get; set; }
        public string? OriginatorId { get; set; }
        public bool IsSimpleInsert { 
            get 
            {
                return StartColumn == EndColumn && StartLine == EndLine;
            }
        }
    }   
}