using System;
using System.Collections.Generic;

namespace JobHelper.WebApi.JustJoinIt
{
    public class JustJoinItInputModel
    {
        public string Url { get; set; }

        public bool EvaluateEnglish { get; set; }

        public List<JustJoinItInputSkillModel> Skills { get; set; }

        //maybe in future :)
        //public List<JustJoinItInputSkillModel> BlacklistSkills { get; set; }
    }
}
