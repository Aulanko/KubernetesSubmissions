

var randomWords = new List<string> {"jeje","koira","kissa","koroko","lehmä"};

var random = new Random();



const string filePath = "/usr/src/app/files/log.txt";
Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);


while (true)
{
    var currentWord = randomWords[random.Next(randomWords.Count)];
    var currentTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
    var line = $"[{currentTimestamp}] {currentWord}";
    await File.AppendAllTextAsync(filePath, line + Environment.NewLine);
    await Task.Delay(5000);
}


