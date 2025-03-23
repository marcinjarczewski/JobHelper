using System;
using JobHelper.WebApi.Helpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using JobHelper.WebApi.Models.JustJoinIt;
using System.Text.RegularExpressions;

namespace JobHelper.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JustJoinItController : ControllerBase
    {
        public class JsonResponseModel
        {
            public string requiredSkills { get; set; }
            public string body { get; set; }
        }

        [HttpPost]
        public object Get(JustJoinItInputModel model)
        {
            if (model == null)
            {
                return new JsonResult(new { error = "model can't be empty." });
            }

            if (string.IsNullOrEmpty(model.Url))
            {
                return new JsonResult(new { error = "url can't be empty." });
            }

            if (!model.Url.StartsWith("https://justjoin.it/job-offer/"))
            {
                return new JsonResult(new { error = "Only justjoin.it sites are allowed." });
            }

            string apiUrl = model.Url + "";

            try
            {
                JustJoinItApiResultModel response;
                var result = new JustJoinItResultModel();
                string body  = "";
                // Znajdź wartość, która najprawdopodobniej zawiera dane JSON
                string lastMatchingValue = null;

                List<JustJoinItApiSkillModel> skills = new List<JustJoinItApiSkillModel>();

                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
                    var data = wc.DownloadString(apiUrl);
                    SaveDataToLocalFolder(data);

                    // Wyciąganie elementów self.__next_f.push([1, ...)
                    var matches = Regex.Matches(data, @"self\.__next_f\.push\(\[1.*?""(.*?)""\]\)");
                    var extractedValues = matches.Cast<Match>().Select(m => m.Groups[1].Value).ToList();
                    
                    int bestScore = 9999999;
                    
                    foreach (var value in extractedValues)
                    {
                        if (value.IndexOf(':') >= 0 && value.IndexOf(':') < 15)
                        {
                            continue;
                        }

                        // Sprawdź procent cyfr w tekście
                        int digitCount = value.Count(char.IsDigit);
                        double digitPercentage = (double)digitCount / value.Length * 100;
                       
                        // Pomiń wartości z dużą ilością cyfr (co najmniej 15%)
                        if (digitPercentage >= 25)
                            continue;
                            
                        int score = 0;
                        
                        // Punkty za zawartość kluczowych pól
                        if (value.Contains("requiredSkills")) score += 5;
                        if (value.Contains("body")) score += 5;
                        if (value.Contains("companySize")) score += 5;
                        
                        // Punkty za struktury JSON
                        score += Regex.Matches(value, @"\{").Count;
                        score += Regex.Matches(value, @"\}").Count;
                        score += Regex.Matches(value, @"\[").Count;
                        score += Regex.Matches(value, @"\]").Count;
                        score += Regex.Matches(value, @"\\""[\w]+\\"":").Count * 2; // Klucze JSON
                        
                        // Sprawdź, czy wartość wygląda na poprawny JSON po oczyszczeniu
                        try {
                            var testJson = value.Replace("\\\"", "\"").Replace("\\\\", "\\");
                            if (testJson.Contains("{") && testJson.Contains("}"))
                                score += 10;
                        } catch {}
                        
                        // Uwzględnij długość tekstu - preferujemy dłuższe teksty
                        score = score / (int)Math.Log10(value.Length + 10);
                        
                        if (score < bestScore)
                        {
                            bestScore = score;
                            lastMatchingValue = value;
                        }
                    }
                    
                    // Szukanie elementów w extractedValues
                    var filteredValues = new List<string>();
                    foreach (var value in extractedValues)
                    {
                        // Sprawdzenie, czy value zawiera klucze "requiredSkills" i "body"
                        if (value.Contains("requiredSkills") && value.Contains("body"))
                        {
                            // Wyodrębnienie requiredSkills
                            var requiredSkillsMatch = Regex.Match(value, @"\\""requiredSkills\\"":\[(\{.*?\}(?:,\{.*?\})*)\]");
                            var bodyMatch = Regex.Match(value, @"body\\"":\\""(.*?)\\""");
                            var companySizeMatch = Regex.Match(value, @"\\""companySize\\"":\\""(.*?)\\""");

                            if (requiredSkillsMatch.Success && bodyMatch.Success && companySizeMatch.Success)
                            {
                                var requiredSkillsJson = requiredSkillsMatch.Groups[1].Value;
                                var body1 = bodyMatch.Groups[1].Value;
                                result.CompanySize = companySizeMatch.Success ? companySizeMatch.Groups[1].Value : null;

                                try
                                {
                                    // Przygotowanie stringa JSON do deserializacji
                                    var cleanedJson = requiredSkillsJson.Replace("\\\"", "\"");
                                    // Deserializacja requiredSkills
                                    skills = JsonConvert.DeserializeObject<List<JustJoinItApiSkillModel>>("[" + cleanedJson + "]");
                                }
                                catch (Exception ex)
                                {
                                    // Możesz dodać logowanie błędu
                                    // Console.WriteLine($"Błąd deserializacji: {ex.Message}");
                                    continue;
                                }
                                // Możesz teraz dodać do filteredValues
                                filteredValues.Add(value);
                            }
                        }
                    }
                }

                body = lastMatchingValue?.ToLower() ?? string.Empty;
                var language = HtmlHelper.GetLanguage(body);
                result.OfferLanguage = language.ToString();

                if (model.Skills != null)
                {
                    result.Skills = CheckSkills(model.Skills, body, skills ?? new List<JustJoinItApiSkillModel>());
                }

                result.EnglishEvaluation = HtmlHelper.GetEnglishLevel(language, body);

                // Save data to local folder
                

                return result;
            }
            catch
            {
                return "Processing server error";
            }
        }

        private List<JustJoinItResultSkillModel> CheckSkills(List<JustJoinItInputSkillModel> skills, string body, List<JustJoinItApiSkillModel> responseSkills)
        {
            var lowerBody = body.ToLower();
            var result = new List<JustJoinItResultSkillModel>();
            foreach (var skill in skills)
            {
                var resultEntry = new JustJoinItResultSkillModel { Name = skill.Name };
                var responseSkill = responseSkills.FirstOrDefault(r => r.name.ToLower().Contains(skill.Name.ToLower()));
                if (responseSkill != null)
                {
                    resultEntry.Level = responseSkill.level;
                }
                else if (skill.SearchInDescription)
                {
                    resultEntry.IsInDescription = lowerBody.IndexOf(skill.Name.ToLower(), StringComparison.Ordinal) >= 0;
                }

                result.Add(resultEntry);
            }

            return result;
        }

        private void SaveDataToLocalFolder(string result)
        {
            string folderPath = @"D:\test\logs";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, $"result_{DateTime.Now:yyyyMMddHHmmss}.json");
            //string jsonData = JsonConvert.SerializeObject(result, Formatting.Indented);
            System.IO.File.WriteAllText(filePath, result);
        }
    }
}
