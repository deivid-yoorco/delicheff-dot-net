using Nop.Core;
using System;

namespace Teed.Plugin.CornerForm.Domain
{
    public class CornerFormResult : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public string QuestionText { get; set; }
        public string AnswerText { get; set; }
        public int CustomerId { get; set; }
    }
}
