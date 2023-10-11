using System.Collections.Generic;

namespace JobHelper.WebApi.Models.JustJoinIt
{
    public class JustJoinItResultModel
    {
        public string CompanySize { get; set; }

        public string OfferLanguage { get; set; }

        public List<JustJoinItResultSkillModel> Skills { get; set; }

        public string EnglishEvaluation { get; set; }
    }
}
