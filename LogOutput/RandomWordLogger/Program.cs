

var randomWords = new List<string> {"jeje","koira","kissa","koroko","lehmä"};

var random = new Random();

while (true)
{
    var randomIndex = random.Next(randomWords.Count);
    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
    Console.WriteLine($"[{timestamp}] {randomWords[randomIndex]}");
    Thread.Sleep(5000);
}

