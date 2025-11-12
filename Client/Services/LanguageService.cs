using Client.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Services;

public static class Language //: ILanguageService //хз как реализовать пока, реализовать в поздних версиях
{
    public static string GetText(string text) => text;

    public static string DetectLanguage(string text) { return string.Empty; }

    public static string GetLanguage() { return string.Empty; }

    public static string GetLanguageCode() { return string.Empty; }

    public static string Translate(string text) { return string.Empty; }

    public static string TranslateTo(string text, string code) { return string.Empty; }
}
