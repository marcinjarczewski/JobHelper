using JobHelper.WebApi.Enums;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

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

            var result = new JustJoinItResultModel();
            JustJoinItApiResultModel response;
            using (WebClient wc = new WebClient())
            {
                var data = wc.DownloadString(apiUrl);
                response = JsonConvert.DeserializeObject<JustJoinItApiResultModel>(data);
            }
            result.CompanySize = response.company_size;
            var body = response.body.ToLower();
            var language = GetLanguage(body);
            result.OfferLanguage = language.ToString();

            if(model.Skills != null)
            {
                result.Skills = CheckSkills(model.Skills, body, response.skills);
            }
            result.EnglishEvaluation = GetEnglishLevel(language, body);

            return result;
        }

        private LanguageEnum GetLanguage(string body)
        {
            int polishPoints = body.Split(" się ").Count() *2 - 2 + body.Split(" dla ").Count() * 2 - 2 + +body.Split(" z ").Count() - 1;
            int englishPoints = body.Split(" the ").Count() * 5 - 5;
            if(polishPoints > englishPoints)
            {
                return LanguageEnum.Polish;
            }
            if (polishPoints < englishPoints)
            {
                return LanguageEnum.English;
            }

            return LanguageEnum.Unknown; 
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

        private string GetEnglishLevel(LanguageEnum language, string body)
        {
            var lowerBody = body.ToLower();
            string toSearch = "";
            switch (language)
            {
                case LanguageEnum.Unknown:
                    return "";
                case LanguageEnum.Polish:
                    toSearch = "angielski";
                    break;
                case LanguageEnum.English:
                    toSearch = "english";
                    break;
                default:
                    break;
            }
            var index = lowerBody.IndexOf(toSearch);
            if(index <= 0)
            {
                return "";
            }
            //search for firstTag after text
            var closeTagIndex = lowerBody.IndexOf("</", index);
            if(closeTagIndex <= index)
            {
                return "";
            }
            //search for end of closing tag
            var closeTagEndIndex = lowerBody.IndexOf(">", closeTagIndex);
            if (closeTagEndIndex <= closeTagIndex)
            {
                return "";
            }
            //get tag name
            var closeTagName = lowerBody.Substring(closeTagIndex + "</".Length, closeTagEndIndex - closeTagIndex - "</".Length);
            //calculate begin tab position
            var beginTagIndex = lowerBody.Substring(0, closeTagEndIndex).LastIndexOf($"<{closeTagName}>");
            if(beginTagIndex < 0)
            {
                return "";
            }
            //get text between tags
            var result = lowerBody.Substring(beginTagIndex + $"<{closeTagName}>".Length, closeTagIndex - beginTagIndex - $"<{closeTagName}>".Length);
            return result;
        }
    }
}
