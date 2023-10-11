using JobHelper.WebApi.Helpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using JobHelper.WebApi.Models.JustJoinIt;

namespace JobHelper.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JustJoinItController : ControllerBase
    {
        [HttpPost]
        public object Get(JustJoinItInputModel model)
        {
            if(model == null)
            {
                return new JsonResult(new { error = "model can't be empty." });
            }

            if(string.IsNullOrEmpty(model.Url))
            {
                return new JsonResult(new { error =  "url can't be empty."});
            }

            if (!model.Url.StartsWith("https://justjoin.it/offers"))
            {
                return new JsonResult(new { error = "Only justjoin.it sites are allowed." });
            }

            var apiUrl = model.Url.Replace("https://justjoin.it/offers", "https://justjoin.it/api/offers");


            JustJoinItApiResultModel response;
            using (WebClient wc = new WebClient())
            {
                var data = wc.DownloadString(apiUrl);
                response = JsonConvert.DeserializeObject<JustJoinItApiResultModel>(data);
            }

            var result = new JustJoinItResultModel();
            result.CompanySize = response?.pageProps?.offer?.companySize;
            var body = response?.pageProps?.offer?.body.ToLower();
            var language = HtmlHelper.GetLanguage(body);
            result.OfferLanguage = language.ToString();

            if (model.Skills != null)
            {
                result.Skills = CheckSkills(model.Skills, body, response?.pageProps?.offer?.requiredSkills ?? new List<JustJoinItApiSkillModel>());
            }

            result.EnglishEvaluation = HtmlHelper.GetEnglishLevel(language, body);

            return result;
        }

        private List<JustJoinItResultSkillModel> CheckSkills(List<JustJoinItInputSkillModel> skills, string body, List<JustJoinItApiSkillModel> responseSkills)
        {
            var lowerBody = body.ToLower();
            var result = new List<JustJoinItResultSkillModel>();
            foreach (var skill in skills)
            {
                var resultEntry = new JustJoinItResultSkillModel { Name = skill.Name };
                var responseSkill = responseSkills.FirstOrDefault(r => r.name.ToLower().Contains(skill.Name.ToLower()));
                if(responseSkill != null)
                {
                    resultEntry.Level = responseSkill.level;
                }
                else if(skill.SearchInDescription)
                {
                   resultEntry.IsInDescription = lowerBody.IndexOf(skill.Name.ToLower()) >= 0;
                }

                result.Add(resultEntry);
            }

            return result;
        }
    }
}
