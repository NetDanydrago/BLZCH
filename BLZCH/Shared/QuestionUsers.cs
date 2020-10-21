using System;
using System.Collections.Generic;
using System.Text;

namespace BLZCH.Shared
{
    public enum StateAnswer
    {
        Answering,
        NotAnswered
    }

    public class QuestionUsers
    {
        public ChatUser ChatUser { get; set; }
        public int Id { get; set; }
        public string  Question { get; set; }
        public StateAnswer IsAnswered { get; set; }
    }
}
