using System.Collections.Generic;

namespace JobHelper.WebApi.Models.JustJoinIt
{
    public class JustJoinItInputModel
    {
        public string ApiKey { get; set; }

        public string Url { get; set; }

        public List<JustJoinItInputSkillModel> Skills { get; set; }

        //maybe in future :)
        //public List<JustJoinItInputSkillModel> BlacklistSkills { get; set; }
    }
}
