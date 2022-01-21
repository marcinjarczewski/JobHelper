using JobHelper.WebApi.Enums;
using JobHelper.WebApi.Helpers;
using JobHelper.WebApi.Pracujpl;
using Microsoft.AspNetCore.Cors;
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
    public class PracujplController : ControllerBase
    {
        [HttpPost]
        public object Get(PracujplInputSkillModel model)
        {
            if (model == null)
            {
                return new JsonResult(new { error = "model can't be empty." });
            }

            if (string.IsNullOrEmpty(model.Url))
            {
                return new JsonResult(new { error = "url can't be empty." });
            }

            if (!model.Url.StartsWith("https://www.pracuj.pl/"))
            {
                return new JsonResult(new { error = "Only pracuj.pl sites are allowed." });
            }

            var result = new PracujplResultModel();
            string lowerBody;
            using (WebClient wc = new WebClient())
            {
                var data = wc.DownloadString(model.Url);
                lowerBody = data.ToLower();
            }
            result.Body = lowerBody;
            var language = HtmlHelper.GetLanguage(lowerBody);
            result.OfferLanguage = language.ToString();

            //Search for text in the main section to increase the chance of finding the correct description
            var requirementIndex = lowerBody.IndexOf("data-scroll-id=\"requirements-1\"");
            if (requirementIndex == -1)
            {
                requirementIndex = lowerBody.IndexOf("data-test=\"section-offerView\"");
            }
            if (requirementIndex > -1)
            {
                result.EnglishEvaluation = HtmlHelper.GetEnglishLevel(language, lowerBody.Substring(requirementIndex));
            }
            return result;
        }
    }
}
