//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text.RegularExpressions;
//using System.Text;
//using WeCantSpell.Hunspell;
//using System.Globalization;

//namespace OcrQualityAssessment;

//class Program
//{
//    static void Main(string[] args)
//    {
//        // Zapewniamy poprawne kodowanie polskich znaków w konsoli
//        Console.OutputEncoding = Encoding.UTF8;

//        // Przykładowy tekst OCR do oceny
//        string ocrText = "To jest przykładowy tekst z OCR, który zawiera polskie znaki jak";
//        //double score = AssessOcrQuality(ocrText, "pl_PL");
//        //int rating = ConvertScoreToRating(score);

//        //Console.WriteLine($"Tekst OCR: {ocrText}");
//        //Console.WriteLine($"Wynik oceny: {score:F2}");
//        //Console.WriteLine($"Ocena jakości (0-5): {rating}");

//        // Szczegółowa analiza
//        var report = AnalyzeOcrQualityDetailed(ocrText, "pl_PL");
//        Console.WriteLine("\nSzczegółowy raport:");
//        Console.WriteLine(report);
//    }

//    /// <summary>
//    /// Ocena jakości tekstu OCR przy użyciu metody słownikowej
//    /// </summary>
//    /// <param name="text">Tekst OCR do oceny</param>
//    /// <param name="language">Kod języka (np. "pl_PL" dla polskiego)</param>
//    /// <returns>Współczynnik jakości (0.0-1.0)</returns>
//    public static double AssessOcrQuality(string text, string language)
//    {
//        try
//        {
//            // Załadowanie słownika Hunspell
//            var dictionaryPath = $"dictionaries2/{language}";
//            var dictionary = WordList.CreateFromFiles($"{dictionaryPath}.dic", $"{dictionaryPath}.aff");

//            // Dzielenie tekstu na słowa z zachowaniem polskich znaków
//            var words = ExtractWords(text);

//            if (words.Length == 0)
//                return 0.0;

//            // Zliczanie słów, które istnieją w słowniku
//            int validWords = 0;
//            foreach (var word in words)
//            {
//                if (dictionary.Check(word))
//                    validWords++;
//            }

//            // Obliczanie współczynnika jakości
//            double qualityScore = (double)validWords / words.Length;

//            // Uwzględnienie długości tekstu (krótkie teksty mogą być mniej wiarygodne)
//            double lengthFactor = Math.Min(1.0, words.Length / 20.0);
//            qualityScore *= (0.7 + 0.3 * lengthFactor);

//            return qualityScore;
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Błąd podczas oceny jakości OCR: {ex.Message}");
//            return 0.0;
//        }
//    }

//    /// <summary>
//    /// Konwertuje współczynnik jakości (0.0-1.0) na ocenę w skali 0-5
//    /// </summary>
//    /// <param name="score">Współczynnik jakości (0.0-1.0)</param>
//    /// <returns>Ocena w skali 0-5</returns>
//    public static int ConvertScoreToRating(double score)
//    {
//        // Mapowanie współczynnika jakości na skalę 0-5
//        if (score < 0.2) return 0;
//        if (score < 0.4) return 1;
//        if (score < 0.6) return 2;
//        if (score < 0.75) return 3;
//        if (score < 0.9) return 4;
//        return 5;
//    }

//    /// <summary>
//    /// Wyodrębnia słowa z tekstu, zachowując znaki diakrytyczne
//    /// </summary>
//    /// <param name="text">Tekst do analizy</param>
//    /// <returns>Tablica wyodrębnionych słów</returns>
//    private static string[] ExtractWords(string text)
//    {
//        // Konwersja do małych liter z zachowaniem polskich znaków
//        text = text.ToLowerInvariant();

//        // Używamy wyrażenia regularnego obsługującego znaki Unicode
//        return Regex.Split(text, @"[^\p{L}]+")
//                    .Where(w => !string.IsNullOrWhiteSpace(w))
//                    .ToArray();
//    }

//    /// <summary>
//    /// Szczegółowa analiza jakości OCR
//    /// </summary>
//    /// <param name="text">Tekst OCR do oceny</param>
//    /// <param name="language">Kod języka (np. "pl_PL" dla polskiego)</param>
//    /// <returns>Szczegółowe informacje o jakości OCR</returns>
//    public static OcrQualityReport AnalyzeOcrQualityDetailed(string text, string language)
//    {
//        try
//        {
//            var dictionaryPath = $"dictionaries2/{language}";
//            var dictionary = WordList.CreateFromFiles($"{dictionaryPath}.dic", $"{dictionaryPath}.aff");

//            // Wyodrębnienie słów z tekstu
//            var words = ExtractWords(text);

//            List<string> validWords = new List<string>();
//            List<string> invalidWords = new List<string>();
//            Dictionary<string, List<string>> suggestions = new Dictionary<string, List<string>>();

//            foreach (var word in words)
//            {
//                if (dictionary.Check(word))
//                    validWords.Add(word);
//                else
//                {
//                    invalidWords.Add(word);

//                    // Pobierz sugestie poprawek dla niepoprawnych słów
//                    // i upewnij się, że kodowanie jest poprawne
//                    var wordSuggestions = dictionary.Suggest(word)
//                                                  .Take(3)
//                                                  .Select(s => NormalizePolishChars(s))
//                                                  .ToList();

//                    if (wordSuggestions.Any())
//                        suggestions[word] = wordSuggestions;
//                }
//            }

//            double qualityScore = words.Length > 0 ? (double)validWords.Count / words.Length : 0;
//            int rating = ConvertScoreToRating(qualityScore);

//            return new OcrQualityReport
//            {
//                TotalWords = words.Length,
//                ValidWords = validWords.Count,
//                InvalidWords = invalidWords.Count,
//                InvalidWordsList = invalidWords,
//                Suggestions = suggestions,
//                QualityScore = qualityScore,
//                Rating = rating
//            };
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Błąd podczas szczegółowej analizy OCR: {ex.Message}");
//            return new OcrQualityReport
//            {
//                TotalWords = 0,
//                ValidWords = 0,
//                InvalidWords = 0,
//                QualityScore = 0,
//                Rating = 0,
//                ErrorMessage = ex.Message
//            };
//        }
//    }

//    /// <summary>
//    /// Normalizuje polskie znaki, zapewniając poprawne kodowanie UTF-8
//    /// </summary>
//    /// <param name="text">Tekst do normalizacji</param>
//    /// <returns>Tekst z poprawnymi polskimi znakami</returns>
//    private static string NormalizePolishChars(string text)
//    {
//        // Mapowanie niepoprawnie zakodowanych polskich znaków na poprawne
//        var replacements = new Dictionary<string, string>
//        {
//            { "¹", "ą" }, { "æ", "ć" }, { "ê", "ę" }, { "³", "ł" },
//            { "ñ", "ń" }, { "ó", "ó" }, { "œ", "ś" }, { "Ÿ", "ź" },
//            { "¿", "ż" }, { "¥", "Ą" }, { "Æ", "Ć" }, { "Ê", "Ę" },
//            { "£", "Ł" }, { "Ñ", "Ń" }, { "Ó", "Ó" }, { "Œ", "Ś" },
//            { "¬", "Ź" }, { "¯", "Ż" }
//        };

//        foreach (var pair in replacements)
//        {
//            text = text.Replace(pair.Key, pair.Value);
//        }

//        return text;
//    }
//}

///// <summary>
///// Klasa przechowująca szczegółowe informacje o jakości OCR
///// </summary>
//public class OcrQualityReport
//{
//    public int TotalWords { get; set; }
//    public int ValidWords { get; set; }
//    public int InvalidWords { get; set; }
//    public List<string> InvalidWordsList { get; set; } = new List<string>();
//    public Dictionary<string, List<string>> Suggestions { get; set; } = new Dictionary<string, List<string>>();
//    public double QualityScore { get; set; }
//    public int Rating { get; set; }
//    public string ErrorMessage { get; set; }

//    public override string ToString()
//    {
//        if (!string.IsNullOrEmpty(ErrorMessage))
//        {
//            return $"Błąd: {ErrorMessage}";
//        }

//        var result = $"Łącznie słów: {TotalWords}\n" +
//               $"Poprawnych słów: {ValidWords}\n" +
//               $"Niepoprawnych słów: {InvalidWords}\n" +
//               $"Współczynnik jakości: {QualityScore:F2}\n" +
//               $"Ocena (0-5): {Rating}";

//        if (InvalidWords > 0)
//        {
//            result += "\n\nNiepoprawne słowa:";
//            foreach (var word in InvalidWordsList)
//            {
//                result += $"\n- {word}";
//                if (Suggestions.ContainsKey(word) && Suggestions[word].Any())
//                {
//                    result += $" (sugestie: {string.Join(", ", Suggestions[word])})";
//                }
//            }
//        }

//        return result;
//    }
//}