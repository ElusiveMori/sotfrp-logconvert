using System.Text.RegularExpressions;


string[] files = Directory.GetFiles(Directory.GetCurrentDirectory());
string extractor =          """^\s+call Preload\( "(.*?)" \)\s*?$""";
string lineStartExtractor = """^\|c\w{2}(\w{6})(.*?)\|r: (.*?)$""";
string colorExtractor =     """\|c\w{2}(\w{6})""";
string timestampExtractor = """Temp (\d+?)\.pld""";

List<string> colorNames = new List<string>
{
    "red",
    "blue",
    "teal",
    "purple",
    "yellow",
    "orange",
    "green",
    "pink",
    "grey",
    "light blue",
    "dark green",
    "brown",
    "maroon",
    "navy",
    "turquoise",
    "violet",
    "wheat",
    "peach",
    "mint",
    "lavender",
    "coal",
    "snow",
    "emerald",
    "peanut",
    "black"
};

var rgbColorList = new List<(int, int, int)>
{
    (255, 2, 2),
    (0, 65, 255),
    (27, 229, 184),
    (83, 0, 128),
    (255, 252, 0),
    (254, 137, 13),
    (31, 191, 0),
    (228, 90, 175),
    (148, 149, 150),
    (125, 190, 241),
    (15, 97, 69),
    (77, 41, 3),
    (155, 0, 0),
    (0, 0, 195),
    (0, 234, 255),
    (190, 0, 254),
    (235, 205, 135),
    (248, 164, 139),
    (191, 255, 128),
    (220, 185, 235),
    (40, 40, 40),
    (235, 240, 255),
    (0, 120, 30),
    (164, 111, 51),
    (0, 0, 0)
};


var hexColorList = new List<string>();

foreach (var rgbColor in rgbColorList)
{
    string hexColor = $"{rgbColor.Item1:X2}{rgbColor.Item2:X2}{rgbColor.Item3:X2}";
    hexColorList.Add(hexColor);
}

Dictionary<string, string> colorTable = new Dictionary<string, string>();

for (int i = 0; i < hexColorList.Count; i++)
{
    Console.WriteLine($"{hexColorList[i]} - {colorNames[i]}");
    colorTable[hexColorList[i]] = colorNames[i];
}

string RewriteColorsRegex(string content)
{
    content = content.Replace("|r", "(#FFFFFF)");

    return Regex.Replace(content, colorExtractor, (Match match) =>
    {
        string color = match.Groups[1].Value;
        return $"(#{color})";
    });
}

foreach (string file in files)
{

    var timestampMatch = Regex.Match(file, timestampExtractor);
    if (!timestampMatch.Success)
    {
        Console.WriteLine($"Skipping {file}");
        continue;
    }

    var timestampValue = long.Parse(timestampMatch.Groups[1].Value);
    var date = DateTimeOffset.FromUnixTimeSeconds(timestampValue).DateTime;

    string fileContent = File.ReadAllText(file);

    string[] lines = fileContent.Split(Environment.NewLine);
    string newFileContent = "";

    foreach (string line in lines)
    {
        Match extractedMatch = Regex.Match(line, extractor);
        if (!extractedMatch.Success) {
            continue;
        }

        var extracted = extractedMatch.Groups[1].Value;

        Match match = Regex.Match(extracted, lineStartExtractor);
        if (match.Success)
        {
            string color = match.Groups[1].Value;
            string name = match.Groups[2].Value;
            string text = match.Groups[3].Value;

            if (colorTable.ContainsKey(color.ToUpper()))
            {
                color = colorTable[color.ToUpper()];
            }

            name = RewriteColorsRegex(name);
            text = RewriteColorsRegex(text);

            newFileContent += $"\n({color}) {name}: {text}";
        } else {
            newFileContent += RewriteColorsRegex(extracted);
        }
    }

    string newFileName = "Roleplay log " + date.ToString("yyyy-MM-dd HH-mm-ss") + ".txt";
    File.WriteAllText(newFileName, newFileContent);
}