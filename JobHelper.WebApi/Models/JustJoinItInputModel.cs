using System;
using System.Collections.Generic;

namespace JobHelper.WebApi
{
    public class JustJoinItInputModel
    {
        public string Url { get; set; }

        public bool EvaluateEnglish { get; set; }

        public List<JustJoinItInputSkillModel> Skills { get; set; }

        public List<JustJoinItInputSkillModel> BlacklistSkills { get; set; }
    }
}
