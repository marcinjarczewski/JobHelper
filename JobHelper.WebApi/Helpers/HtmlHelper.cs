using JobHelper.WebApi.Enums;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace JobHelper.WebApi.Helpers
{
    public static class HtmlHelper
    {
        /// <summary>
        /// Get text between tags from body or div
        /// </summary>
        /// <param name="lowerBody">the string that contains HTML</param>
        /// <param name="index">index of char(begin of the searched word)</param>
        /// <returns>Text between closest tags</returns>
        public static string GetTextBetweenTags(string lowerBody, int index)
        {
            //search for firstTag after text
            var closeTagIndex = lowerBody.IndexOf("</", index, StringComparison.Ordinal);
            if (closeTagIndex <= index)
            {
                return "";
            }
            //search for end of closing tag
            var closeTagEndIndex = lowerBody.IndexOf(">", closeTagIndex, StringComparison.Ordinal);
            if (closeTagEndIndex <= closeTagIndex)
            {
                return "";
            }
            //get tag name
            var closeTagName = lowerBody.Substring(closeTagIndex + "</".Length, closeTagEndIndex - closeTagIndex - "</".Length);
            //calculate begin tab position
            var beginTagIndex = lowerBody.Substring(0, closeTagEndIndex).LastIndexOf($"<{closeTagName}", StringComparison.Ordinal);
            if (beginTagIndex < 0)
            {
                return "";
            }
            //get text between tags
            var result = lowerBody.Substring(beginTagIndex + $"<{closeTagName}".Length, closeTagIndex - beginTagIndex - $"<{closeTagName}".Length);
            return result;
        }

        /// <summary>
        /// Search for "english" and return nearby text
        /// </summary>
        /// <param name="language">Searched text language</param>
        /// <param name="lowerBody">the string that contains HTML</param>
        /// <returns>Text between closest tags</returns>
        public static string GetEnglishLevel(LanguageEnum language, string lowerBody)
        {
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
            }
            // Usuń sekwencje Unicode w formacie \uXXXX
            lowerBody = Regex.Replace(lowerBody, @"\\u[0-9a-fA-F]{4}", "");
            // Usuń sekwencje Unicode w formacie \u00XX
            lowerBody = Regex.Replace(lowerBody, @"\\u00[0-9a-fA-F]{2}", "");
            // Usuń inne znaki kontrolne i niepożądane znaki
            lowerBody = Regex.Replace(lowerBody, @"[\u0000-\u001F\u007F-\u009F]", "");
            // Usuń znaczniki HTML i znaki nowej linii
            lowerBody = Regex.Replace(lowerBody, @"(/strong|\\n|/li|\<|\>)", "");
            
            int index = lowerBody.IndexOf(toSearch, StringComparison.Ordinal);
            if (index <= 0)
            {
                return "";
            }

            int minIndex = Math.Max(0, index - 30);

            return lowerBody.Substring(minIndex, 150);
        }

        /// <summary>
        /// Try to guess the website language based on simple heuristics
        /// </summary>
        /// <param name="body"></param>
        /// <returns>Language or unknown</returns>
        public static LanguageEnum GetLanguage(string body)
        {
            int polishPoints = body.Split(" się ").Count() * 2 - 2 + body.Split(" dla ").Count() * 2 - 2 + +body.Split(" z ").Count() - 1;
            int englishPoints = body.Split(" the ").Count() * 5 - 5;
            if (polishPoints > englishPoints)
            {
                return LanguageEnum.Polish;
            }

            if (polishPoints < englishPoints)
            {
                return LanguageEnum.English;
            }

            return LanguageEnum.Unknown;
        }

    }
}
