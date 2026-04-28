namespace Heimdall.BragiCore.Configuration;

public static class DefaultCategoryRules
{
    public static IReadOnlyList<CategoryRule> Create()
    {
        return new[]
        {
            Rule("art", "Art", "ArtSubjects.txt", 10,
                ["art", "artist", "painting"],
                ["children", "artificial intelligence"],
                disableForFiction: true,
                disableForJuvenile: true),

            Rule("biology", "Biology", "BiologySubjects.txt", 20,
                ["biology", "botany", "ecology", "microbiology"],
                ["children"],
                disableForFiction: true,
                disableForJuvenile: true),

            Rule("business", "Business", "BusinessSubjects.txt", 30,
                ["business", "economy", "economic", "accounting", "accountant", "advertising", "management"],
                ["children"],
                disableForFiction: true,
                disableForJuvenile: true),

            Rule("chemistry", "Chemistry", "ChemistrySubjects.txt", 40,
                ["chemistry", "chemical", "chemist"],
                ["children"],
                disableForFiction: true,
                disableForJuvenile: true),

            Rule("computer", "Computer", "ComputerSubjects.txt", 50,
                [
                    "computer",
                    "information technology",
                    "technology",
                    "coding",
                    "cybernetics",
                    "information theory",
                    "system theory",
                    "systems theory",
                    "system analysis",
                    "system design",
                    "electronic data processing",
                    "artificial intelligence",
                    "automatic data processing",
                    "application software",
                    "android",
                    "computational complexity"
                ],
                ["children"],
                disableForFiction: true,
                disableForJuvenile: true),

            Rule("education", "Education", "EducationSubjects.txt", 60,
                ["education", "teaching", "teachers", "learning", "history", "juvenile", "picture book"],
                [],
                disableForFiction: false,
                disableForJuvenile: false),

            Rule("fiction", "Fiction", "FictionSubjects.txt", 70,
                ["fiction"],
                [],
                disableForFiction: false,
                disableForJuvenile: false),

            Rule("forensics", "Forensics", "ForensicsSubjects.txt", 80,
                ["forensics", "crime", "crimin", "criminology"],
                ["children"],
                disableForFiction: true,
                disableForJuvenile: true),

            Rule("geoscience", "Geoscience", "GeoscienceSubjects.txt", 90,
                ["geology", "hydrology", "tectonics", "topographic"],
                ["children"],
                disableForFiction: true,
                disableForJuvenile: true),

            Rule("history", "History", "HistorySubjects.txt", 100,
                ["history", "biography", "autobiography", "social"],
                ["children"],
                disableForFiction: true,
                disableForJuvenile: true),

            Rule("hper", "HPER", "HPERSubjects.txt", 110,
                ["health", "exercise", "athlete", "athletic"],
                ["children", "research methodology", "problems exercises etc"],
                disableForFiction: true,
                disableForJuvenile: true),

            Rule("humanities", "Humanities", "HumanitiesSubjects.txt", 120,
                [
                    "language",
                    "literature",
                    "communication",
                    "oral communication",
                    "written communication",
                    "nonverbal communication",
                    "rhetoric",
                    "writing",
                    "philosophy",
                    "semiotics",
                    "literacy",
                    "humanities",
                    "mass media"
                ],
                ["children"],
                disableForFiction: true,
                disableForJuvenile: true),

            Rule("idt", "IDT", "IDTSubjects.txt", 130,
                ["instructional design"],
                ["children"],
                disableForFiction: true,
                disableForJuvenile: true),

            Rule("interdis", "InterDis", "InterDisSubjects.txt", 140,
                ["geneder", "gender", "sexuality", "ethnology", "sociology"],
                ["children"],
                disableForFiction: true,
                disableForJuvenile: true),

            Rule("math", "Math", "MathSubjects.txt", 150,
                [
                    "math",
                    "trigonometry",
                    "linear algebra",
                    "calculus",
                    "geometry",
                    "statistics",
                    "analysis of variance",
                    "biometry"
                ],
                ["children"],
                disableForFiction: true,
                disableForJuvenile: true),

            Rule("music", "Music", "MusicSubjects.txt", 160,
                ["music", "instrument", "musical", "concert", "instrumental"],
                ["children"],
                disableForFiction: true,
                disableForJuvenile: true),

            Rule("nursing", "Nursing", "NursingSubjects.txt", 170,
                ["nurse", "nursing", "healthcare", "medicine", "doctor", "vaccine"],
                ["children"],
                disableForFiction: true,
                disableForJuvenile: true),

            Rule("performance", "Performance", "PerformanceSubjects.txt", 180,
                ["performing arts", "plays", "operas", "movie", "actor", "actress", "stage"],
                ["children"],
                disableForFiction: true,
                disableForJuvenile: true),

            Rule("physics", "Physics", "PhysicsSubjects.txt", 190,
                ["physics", "astronomy"],
                ["children"],
                disableForFiction: true,
                disableForJuvenile: true),

            Rule("psych", "Psych", "PsychSubjects.txt", 200,
                ["psych", "therapy", "counseling", "mental health", "adult child"],
                ["children"],
                disableForFiction: true,
                disableForJuvenile: true),

            Rule("politics", "Politics", "PoliticsSubjects.txt", 210,
                ["politics", "law", "government"],
                ["children", "vet"],
                disableForFiction: true,
                disableForJuvenile: true),

            Rule("slim", "SLIM", "SlimSubjects.txt", 220,
                [
                    "library",
                    "librarian",
                    "libraries",
                    "collection development",
                    "information science",
                    "bibliography",
                    "book collecting",
                    "books and reading",
                    "antiquarian booksellers",
                    "book collectors",
                    "book industries and trade",
                    "best books",
                    "annotating, book",
                    "book thefts",
                    "classification"
                ],
                [],
                disableForFiction: true,
                disableForJuvenile: true)
        };
    }

    private static CategoryRule Rule(
        string key,
        string displayName,
        string outputFileName,
        int sortOrder,
        IReadOnlyList<string> includeKeywords,
        IReadOnlyList<string> excludeKeywords,
        bool disableForFiction,
        bool disableForJuvenile)
    {
        return new CategoryRule
        {
            Key = key,
            DisplayName = displayName,
            OutputFileName = outputFileName,
            IncludeKeywords = includeKeywords,
            ExcludeKeywords = excludeKeywords,
            RequireAnyKeywords = Array.Empty<string>(),
            DisableForFiction = disableForFiction,
            DisableForJuvenile = disableForJuvenile,
            SortOrder = sortOrder,
            Enabled = true
        };
    }
}
