using System.Text;
using System.Text.RegularExpressions;
using WeCantSpell.Hunspell;

// Zapewniamy poprawne kodowanie polskich znaków w konsoli
Console.OutputEncoding = Encoding.UTF8;

// Przykładowy tekst OCR do oceny
string ocrText = "To jest przykładowy tekst z OCR, który zawiera polskie znaki. A to telefon : 555 555 111";

// Test słownika - sprawdzenie podstawowych polskich słów
// Jeśli to nie działa to nic nie ma sensu
TestDictionary("pl_PL");

double score = AssessOcrQuality(ocrText, "pl_PL");
int rating = ConvertScoreToRating(score);

Console.WriteLine($"Tekst OCR: {ocrText}");
Console.WriteLine($"Wynik oceny: {score:F2}");
Console.WriteLine($"Ocena jakości (0-5): {rating}");

// Szczegółowa analiza
var report = AnalyzeOcrQualityDetailed(ocrText, "pl_PL");
Console.WriteLine("\nSzczegółowy raport:");
Console.WriteLine(report);

/// <summary>
/// Testuje słownik na podstawowych polskich słowach
/// </summary>
/// <param name="language">Kod języka słownika</param>
static void TestDictionary(string language)
{
    try
    {
        Console.WriteLine("Testowanie słownika...");
        var dictionaryPath = $"dictionaries_UTF-8/{language}";

        // Sprawdź czy pliki słownika istnieją
        if (!File.Exists($"{dictionaryPath}.dic"))
            Console.WriteLine($"BŁĄD: Plik słownika {dictionaryPath}.dic nie istnieje!");

        if (!File.Exists($"{dictionaryPath}.aff"))
            Console.WriteLine($"BŁĄD: Plik słownika {dictionaryPath}.aff nie istnieje!");

        if (!File.Exists($"{dictionaryPath}.dic") || !File.Exists($"{dictionaryPath}.aff"))
        {
            Console.WriteLine("Nie można znaleźć plików słownika. Sprawdź ścieżkę i nazwy plików.");
            return;
        }

        var dictionary = WordList.CreateFromFiles($"{dictionaryPath}.dic", $"{dictionaryPath}.aff");

        // Spróbuj:
        //var dictionaryData = File.ReadAllText($"{dictionaryPath}.dic", Encoding.UTF8);
        //var affixData = File.ReadAllText($"{dictionaryPath}.aff", Encoding.UTF8);
        //var dictionary = WordList.CreateFromStreams(
        //    new MemoryStream(Encoding.UTF8.GetBytes(dictionaryData)),
        //    new MemoryStream(Encoding.UTF8.GetBytes(affixData))
        //);

        // Lista podstawowych polskich słów do testowania
        var testWords = new[] {
                "dom", "kot", "pies", "przykładowy", "zawiera", "tekst",
                "polski", "język", "słownik", "komputer"
            };

        Console.WriteLine("Wyniki testu słownika:");
        foreach (var word in testWords)
        {
            bool isWordRecognized = dictionary.Check(word);
            Console.WriteLine($"  - {word}: {(isWordRecognized ? "rozpoznane" : "nierozpoznane")}");

            if (!isWordRecognized)
            {
                var suggestions = dictionary.Suggest(word).Take(3);
                Console.WriteLine($"    Sugestie: {string.Join(", ", suggestions)}");
            }
        }

        Console.WriteLine("Test słownika zakończony.\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Błąd podczas testowania słownika: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
}

/// <summary>
/// Ocena jakości tekstu OCR przy użyciu metody słownikowej
/// </summary>
/// <param name="text">Tekst OCR do oceny</param>
/// <param name="language">Kod języka (np. "pl_PL" dla polskiego)</param>
/// <returns>Współczynnik jakości (0.0-1.0)</returns>
static double AssessOcrQuality(string text, string language)
{
    try
    {
        // Załadowanie słownika Hunspell
        var dictionaryPath = $"dictionaries_UTF-8/{language}";
        var dictionary = WordList.CreateFromFiles($"{dictionaryPath}.dic", $"{dictionaryPath}.aff");

        // Dzielenie tekstu na słowa z zachowaniem polskich znaków
        var words = ExtractWords(text);

        if (words.Length == 0)
            return 0.0;

        // Lista rozpoznanych i nierozpoznanych słów dla diagnostyki
        List<string> recognizedWords = new List<string>();
        List<string> unrecognizedWords = new List<string>();

        // Zliczanie słów, które istnieją w słowniku
        int validWords = 0;
        foreach (var word in words)
        {
            if (dictionary.Check(word))
            {
                validWords++;
                recognizedWords.Add(word);
            }
            else
            {
                unrecognizedWords.Add(word);
            }
        }

        // Diagnostyka - wypisz rozpoznane i nierozpoznane słowa
        Console.WriteLine("\nDiagnostyka słownika:");
        Console.WriteLine($"Rozpoznane słowa ({recognizedWords.Count}): {string.Join(", ", recognizedWords)}");
        Console.WriteLine($"Nierozpoznane słowa ({unrecognizedWords.Count}): {string.Join(", ", unrecognizedWords)}");

        // Obliczanie współczynnika jakości
        double qualityScore = (double)validWords / words.Length;

        // Uwzględnienie długości tekstu (krótkie teksty mogą być mniej wiarygodne)
        double lengthFactor = Math.Min(1.0, words.Length / 20.0);
        qualityScore *= (0.7 + 0.3 * lengthFactor);

        return qualityScore;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Błąd podczas oceny jakości OCR: {ex.Message}");
        return 0.0;
    }
}

/// <summary>
/// Konwertuje współczynnik jakości (0.0-1.0) na ocenę w skali 0-5
/// </summary>
/// <param name="score">Współczynnik jakości (0.0-1.0)</param>
/// <returns>Ocena w skali 0-5</returns>
static int ConvertScoreToRating(double score)
{
    // Mapowanie współczynnika jakości na skalę 0-5
    if (score < 0.2) return 0;
    if (score < 0.4) return 1;
    if (score < 0.6) return 2;
    if (score < 0.75) return 3;
    if (score < 0.9) return 4;
    return 5;
}

/// <summary>
/// Wyodrębnia słowa z tekstu, zachowując znaki diakrytyczne
/// </summary>
/// <param name="text">Tekst do analizy</param>
/// <returns>Tablica wyodrębnionych słów</returns>
static string[] ExtractWords(string text)
{
    // Konwersja do małych liter z zachowaniem polskich znaków
    text = text.ToLowerInvariant();

    // Używamy wyrażenia regularnego obsługującego znaki Unicode
    return Regex.Split(text, @"[^\p{L}]+")
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .ToArray();
}

/// <summary>
/// Szczegółowa analiza jakości OCR
/// </summary>
/// <param name="text">Tekst OCR do oceny</param>
/// <param name="language">Kod języka (np. "pl_PL" dla polskiego)</param>
/// <returns>Szczegółowe informacje o jakości OCR</returns>
static OcrQualityReport AnalyzeOcrQualityDetailed(string text, string language)
{
    try
    {
        var dictionaryPath = $"dictionaries_UTF-8/{language}";

        // Używamy niestandardowego słownika, jeśli istnieje
        var dictionaryToUse = dictionaryPath;
        if (!File.Exists($"{dictionaryPath}.dic") || !File.Exists($"{dictionaryPath}.aff"))
        {
            Console.WriteLine($"UWAGA: Słownik {dictionaryPath} nie istnieje, próba użycia słownika systemowego...");
            // Może być potrzebna inna lokalizacja słownika systemowego
            dictionaryToUse = "pl";
        }

        var dictionary = WordList.CreateFromFiles($"{dictionaryToUse}.dic", $"{dictionaryToUse}.aff");

        // Wyodrębnienie słów z tekstu
        var words = ExtractWords(text);

        List<string> validWords = new List<string>();
        List<string> invalidWords = new List<string>();
        Dictionary<string, List<string>> suggestions = new Dictionary<string, List<string>>();

        // Zbiór znanych słów kluczowych - słownikowe rozszerzenie
        HashSet<string> knownKeywords = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                "ocr", "pdf", "xml", "jpeg", "png", "tiff"
            };

        foreach (var word in words)
        {
            // Specjalne traktowanie dla akronimów i znanych słów kluczowych
            if (knownKeywords.Contains(word))
            {
                validWords.Add(word);
                continue;
            }

            if (dictionary.Check(word))
                validWords.Add(word);
            else
            {
                invalidWords.Add(word);

                // Pobierz sugestie poprawek dla niepoprawnych słów
                var wordSuggestions = dictionary.Suggest(word)
                                              .Take(4)
                                              .ToList();

                if (wordSuggestions.Any())
                    suggestions[word] = wordSuggestions;

                // Jeśli słowo ma sugestię samego siebie, to oznacza problem ze słownikiem
                if (wordSuggestions.Contains(word))
                {
                    Console.WriteLine($"UWAGA: Słowo '{word}' nie jest rozpoznawane przez słownik, " +
                                     $"ale jest sugerowane jako poprawka dla samego siebie!");
                }
            }
        }

        double qualityScore = words.Length > 0 ? (double)validWords.Count / words.Length : 0;
        int rating = ConvertScoreToRating(qualityScore);

        return new OcrQualityReport
        {
            TotalWords = words.Length,
            ValidWords = validWords.Count,
            InvalidWords = invalidWords.Count,
            InvalidWordsList = invalidWords,
            ValidWordsList = validWords,
            Suggestions = suggestions,
            QualityScore = qualityScore,
            Rating = rating,
            UsedDictionary = dictionaryToUse
        };
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Błąd podczas szczegółowej analizy OCR: {ex.Message}");
        return new OcrQualityReport
        {
            TotalWords = 0,
            ValidWords = 0,
            InvalidWords = 0,
            QualityScore = 0,
            Rating = 0,
            ErrorMessage = ex.Message
        };
    }
}

/// <summary>
/// Klasa przechowująca szczegółowe informacje o jakości OCR
/// </summary>
public class OcrQualityReport
{
    public int TotalWords { get; set; }
    public int ValidWords { get; set; }
    public int InvalidWords { get; set; }
    public List<string> ValidWordsList { get; set; } = new List<string>();
    public List<string> InvalidWordsList { get; set; } = new List<string>();
    public Dictionary<string, List<string>> Suggestions { get; set; } = new Dictionary<string, List<string>>();
    public double QualityScore { get; set; }
    public int Rating { get; set; }
    public string UsedDictionary { get; set; }
    public string ErrorMessage { get; set; }

    public override string ToString()
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            return $"Błąd: {ErrorMessage}";
        }

        var result = $"Łącznie słów: {TotalWords}\n" +
               $"Poprawnych słów: {ValidWords}\n" +
               $"Niepoprawnych słów: {InvalidWords}\n" +
               $"Współczynnik jakości: {QualityScore:F2}\n" +
               $"Ocena (0-5): {Rating}\n" +
               $"Użyty słownik: {UsedDictionary}";

        if (ValidWords > 0)
        {
            result += "\n\nPoprawne słowa: " + string.Join(", ", ValidWordsList);
        }

        if (InvalidWords > 0)
        {
            result += "\n\nNiepoprawne słowa:";
            foreach (var word in InvalidWordsList)
            {
                result += $"\n- {word}";
                if (Suggestions.ContainsKey(word) && Suggestions[word].Any())
                {
                    result += $" (sugestie: {string.Join(", ", Suggestions[word])})";
                }
            }
        }

        return result;
    }
}