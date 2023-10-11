using System.Collections.Generic;

namespace JobHelper.WebApi.Models.JustJoinIt
{
    public class JustJoinItApiOfferModel
    {
        public string companySize { get; set; }

        public string body { get; set; }

        public List<JustJoinItApiSkillModel> requiredSkills { get; set; }
    }
}
