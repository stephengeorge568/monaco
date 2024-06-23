namespace server.models
{
    public class Operation
    {
        public string Text { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public int RevisionId { get; set; }
        public string OriginatorId { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public bool IsSimpleInsert { 
            get 
            {
                return StartColumn == EndColumn && StartLine == EndLine;
            }
        }

        public new string ToString
        {
            get
            {
                return $"{StartColumn}, {EndColumn}, {StartLine}, {EndLine} | {RevisionId} | {Text}";
            }
        }

        public Operation DeepCopy()
        {
            return new Operation
            {
                Id = this.Id,
                Text = this.Text,
                StartColumn = this.StartColumn,
                EndColumn = this.EndColumn,
                StartLine = this.StartLine,
                EndLine = this.EndLine,
                RevisionId = this.RevisionId,
                OriginatorId = this.OriginatorId
            };
        }
    }   
}