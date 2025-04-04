//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text.RegularExpressions;
//using WeCantSpell.Hunspell;

//namespace OcrQualityAssessment
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            // Przykładowy tekst OCR do oceny
//            string ocrText = "To jest przykładowy tekst z OCR, który może zawierać pewne błędy takie jak zniekształcone slowa lub nieprawidłowe znaki...";

//            double score = AssessOcrQuality(ocrText, "pl_PL");
//            int rating = ConvertScoreToRating(score);

//            Console.WriteLine($"Tekst OCR: {ocrText}");
//            Console.WriteLine($"Wynik oceny: {score:F2}");
//            Console.WriteLine($"Ocena jakości (0-5): {rating}");
//        }

//        /// <summary>
//        /// Ocena jakości tekstu OCR przy użyciu metody słownikowej
//        /// </summary>
//        /// <param name="text">Tekst OCR do oceny</param>
//        /// <param name="language">Kod języka (np. "pl_PL" dla polskiego)</param>
//        /// <returns>Współczynnik jakości (0.0-1.0)</returns>
//        public static double AssessOcrQuality(string text, string language)
//        {
//            // Załadowanie słownika Hunspell
//            var dictionaryPath = $"dictionaries/{language}";
//            var dictionary = WordList.CreateFromFiles($"{dictionaryPath}.dic", $"{dictionaryPath}.aff");

//            // Dzielenie tekstu na słowa i usuwanie znaków specjalnych
//            var words = Regex.Split(text.ToLower(), @"\W+")
//                             .Where(w => !string.IsNullOrWhiteSpace(w))
//                             .ToArray();

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

//        /// <summary>
//        /// Konwertuje współczynnik jakości (0.0-1.0) na ocenę w skali 0-5
//        /// </summary>
//        /// <param name="score">Współczynnik jakości (0.0-1.0)</param>
//        /// <returns>Ocena w skali 0-5</returns>
//        public static int ConvertScoreToRating(double score)
//        {
//            // Mapowanie współczynnika jakości na skalę 0-5
//            if (score < 0.2) return 0;
//            if (score < 0.4) return 1;
//            if (score < 0.6) return 2;
//            if (score < 0.75) return 3;
//            if (score < 0.9) return 4;
//            return 5;
//        }

//        /// <summary>
//        /// Szczegółowa analiza jakości OCR
//        /// </summary>
//        /// <param name="text">Tekst OCR do oceny</param>
//        /// <param name="language">Kod języka (np. "pl_PL" dla polskiego)</param>
//        /// <returns>Szczegółowe informacje o jakości OCR</returns>
//        public static OcrQualityReport AnalyzeOcrQualityDetailed(string text, string language)
//        {
//            var dictionaryPath = $"dictionaries/{language}";
//            var dictionary = WordList.CreateFromFiles($"{dictionaryPath}.dic", $"{dictionaryPath}.aff");

//            var words = Regex.Split(text.ToLower(), @"\W+")
//                             .Where(w => !string.IsNullOrWhiteSpace(w))
//                             .ToArray();

//            List<string> validWords = new List<string>();
//            List<string> invalidWords = new List<string>();

//            foreach (var word in words)
//            {
//                if (dictionary.Check(word))
//                    validWords.Add(word);
//                else
//                    invalidWords.Add(word);
//            }

//            double qualityScore = words.Length > 0 ? (double)validWords.Count / words.Length : 0;
//            int rating = ConvertScoreToRating(qualityScore);

//            return new OcrQualityReport
//            {
//                TotalWords = words.Length,
//                ValidWords = validWords.Count,
//                InvalidWords = invalidWords.Count,
//                InvalidWordsList = invalidWords,
//                QualityScore = qualityScore,
//                Rating = rating
//            };
//        }
//    }

//    /// <summary>
//    /// Klasa przechowująca szczegółowe informacje o jakości OCR
//    /// </summary>
//    public class OcrQualityReport
//    {
//        public int TotalWords { get; set; }
//        public int ValidWords { get; set; }
//        public int InvalidWords { get; set; }
//        public List<string> InvalidWordsList { get; set; }
//        public double QualityScore { get; set; }
//        public int Rating { get; set; }

//        public override string ToString()
//        {
//            return $"Łącznie słów: {TotalWords}\n" +
//                   $"Poprawnych słów: {ValidWords}\n" +
//                   $"Niepoprawnych słów: {InvalidWords}\n" +
//                   $"Współczynnik jakości: {QualityScore:F2}\n" +
//                   $"Ocena (0-5): {Rating}";
//        }
//    }
//}