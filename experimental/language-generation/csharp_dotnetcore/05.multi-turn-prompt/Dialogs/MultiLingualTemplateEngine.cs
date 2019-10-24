using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.LanguageGeneration;
using ActivityBuilder = Microsoft.Bot.Builder.Dialogs.Adaptive.Generators.ActivityGenerator;

namespace Microsoft.BotBuilderSamples
{
    public class MultiLingualTemplateEngine
    {
        public MultiLingualTemplateEngine(Dictionary<string, string[]> lgFilespPerLocale)
        {

            // template engine instance per locale per set of files.


        }

        public Dictionary<string, string[]> filesPerLocale;
        private Dictionary<string, TemplateEngine> enginesPerLocale;

        public Activity GenerateActivityForLocale(string locale, string templateName, object data = null)
        {

            return ActivityBuilder.GenerateFromLG(enginesPerLocale[locale].EvaluateTemplate(templateName, data));
        }
    }
}