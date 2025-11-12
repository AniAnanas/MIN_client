using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Shared.Interfaces
{
    public interface ILanguageService
    {
        string GetLanguage();
        string GetLanguageCode();
        string DetectLanguage(string text);
        string Translate(string text);
        string TranslateTo(string text, string code);
        string GetText(string text);
    }
}
