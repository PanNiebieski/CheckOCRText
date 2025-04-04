//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using WeCantSpell.Hunspell;

//namespace OcrQualityAssessment;

//class Program
//{
//    static void Main(string[] args)
//    {
//        Console.OutputEncoding = Encoding.UTF8;

//        // Przykładowy tekst OCR do oceny
//        string ocrText1 = "polskie znaki";
//        string ocrText2 = "To jest przykładowy tekst z OCR, który zawiera polskie znaki";

//        CheckText(ocrText1);

//        CheckText(ocrText2);
//    }

//    private static void CheckText(string ocrText1)
//    {
//        //double score = AssessOcrQuality(ocrText1, "pl_PL");
//        //int rating = ConvertScoreToRating(score);

//        //Console.WriteLine("");
//        //Console.WriteLine($"Tekst OCR: {ocrText1}");
//        //Console.WriteLine($"Wynik oceny: {score:F2}");
//        //Console.WriteLine($"Ocena jakości (0-5): {rating}");

//        // Szczegółowa analiza
//        var report = AnalyzeOcrQualityDetailed(ocrText1, "pl_PL");
//        Console.WriteLine("\nSzczegółowy raport:");
//        Console.WriteLine(report);
//        Console.WriteLine("");
//    }

//    /// <summary>
//    /// Ocena jakości tekstu OCR przy użyciu metody słownikowej
//    /// </summary>
//    /// <param name="text">Tekst OCR do oceny</param>
//    /// <param name="language">Kod języka (np. "pl_PL" dla polskiego)</param>
//    /// <returns>Współczynnik jakości (0.0-1.0)</returns>
//    public static double AssessOcrQuality(string text, string language)
//    {
//        // Załadowanie słownika Hunspell
//        var dictionaryPath = $"dictionaries/{language}";
//        var dictionary = WordList.CreateFromFiles($"{dictionaryPath}.dic", $"{dictionaryPath}.aff");

//        // Dzielenie tekstu na słowa z zachowaniem polskich znaków
//        // Używamy wyrażenia regularnego, które uwzględnia znaki Unicode
//        var words = Regex.Split(text.ToLower(), @"[^\p{L}]+")
//                         .Where(w => !string.IsNullOrWhiteSpace(w))
//                         .ToArray();

//        if (words.Length == 0)
//            return 0.0;

//        // Zliczanie słów, które istnieją w słowniku
//        int validWords = 0;
//        foreach (var word in words)
//        {
//            if (dictionary.Check(word))
//                validWords++;
//        }

//        // Obliczanie współczynnika jakości
//        double qualityScore = (double)validWords / words.Length;

//        // Uwzględnienie długości tekstu (krótkie teksty mogą być mniej wiarygodne)
//        double lengthFactor = Math.Min(1.0, words.Length / 20.0);
//        qualityScore *= (0.7 + 0.3 * lengthFactor);

//        return qualityScore;
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
//    /// Szczegółowa analiza jakości OCR
//    /// </summary>
//    /// <param name="text">Tekst OCR do oceny</param>
//    /// <param name="language">Kod języka (np. "pl_PL" dla polskiego)</param>
//    /// <returns>Szczegółowe informacje o jakości OCR</returns>
//    public static OcrQualityReport AnalyzeOcrQualityDetailed(string text, string language)
//    {
//        var dictionaryPath = $"dictionaries/{language}";
//        var dictionary = WordList.CreateFromFiles($"{dictionaryPath}.dic", $"{dictionaryPath}.aff");

//        // Poprawione wyrażenie regularne obsługujące znaki Unicode
//        var words = Regex.Split(text.ToLower(), @"[^\p{L}]+")
//                         .Where(w => !string.IsNullOrWhiteSpace(w))
//                         .ToArray();

//        List<string> validWords = new List<string>();
//        List<string> invalidWords = new List<string>();
//        Dictionary<string, List<string>> suggestions = new Dictionary<string, List<string>>();

//        foreach (var word in words)
//        {
//            if (dictionary.Check(word))
//                validWords.Add(word);
//            else
//            {
//                invalidWords.Add(word);

//                // Pobierz sugestie poprawek dla niepoprawnych słów
//                var wordSuggestions = dictionary.Suggest(word).Take(3).ToList();
//                if (wordSuggestions.Any())
//                    suggestions[word] = wordSuggestions;
//            }
//        }

//        double qualityScore = words.Length > 0 ? (double)validWords.Count / words.Length : 0;
//        int rating = ConvertScoreToRating(qualityScore);

//        return new OcrQualityReport
//        {
//            TotalWords = words.Length,
//            ValidWords = validWords.Count,
//            InvalidWords = invalidWords.Count,
//            InvalidWordsList = invalidWords,
//            Suggestions = suggestions,
//            QualityScore = qualityScore,
//            Rating = rating
//        };
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
//    public List<string> InvalidWordsList { get; set; }
//    public Dictionary<string, List<string>> Suggestions { get; set; } = new Dictionary<string, List<string>>();
//    public double QualityScore { get; set; }
//    public int Rating { get; set; }

//    public override string ToString()
//    {
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