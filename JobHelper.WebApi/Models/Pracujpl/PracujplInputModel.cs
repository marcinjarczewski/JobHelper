using System;
using System.Collections.Generic;

namespace JobHelper.WebApi.Pracujpl
{
    public class PracujplInputModel
    {
        public string Url { get; set; }

        public bool EvaluateEnglish { get; set; }

        public List<PracujplInputSkillModel> Skills { get; set; }

        public List<PracujplInputSkillModel> BlacklistSkills { get; set; }
    }
}
