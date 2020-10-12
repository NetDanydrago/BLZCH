using System;
using System.Collections.Generic;
using System.Text;

namespace BLZCH.Shared
{
    public class QuestionUsers
    {
        public ChatUser ChatUser { get; set; }
        public string  Question { get; set; }
        public bool IsAnswered { get; set; }
    }
}
