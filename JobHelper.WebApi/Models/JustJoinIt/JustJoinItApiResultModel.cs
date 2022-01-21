using System;
using System.Collections.Generic;

namespace JobHelper.WebApi.JustJoinIt
{
    public class JustJoinItApiResultModel
    {
        public string company_size { get; set; }

        public string body { get; set; }

        public List<JustJoinItApiSkillModel> skills { get; set; }
    }
}
