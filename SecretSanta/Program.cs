using SecretSanta;
using SendGrid.Helpers.Mail;
using SendGrid;

// Create Participants
var participants = new List<Participant>(new[]
    {
        new Participant { Name = "John", Email = "john@yourmail.com"},
        new Participant { Name = "Jane", Email = "jane@yourmail.com"},
        new Participant { Name = "Aunty", Email = "aunty@yourmail.com"},
        new Participant { Name = "Uncle", Email = "uncle@yourmail.com"},
        new Participant { Name = "Kiddo", Email = "kiddo@yourmail.net"},
        new Participant { Name = "Gramps", Email = "gramps@yourmail.com"},
    });

// Add relations (for secret santa exclusion)
participants.Where(p => p.Name == "John").FirstOrDefault()!.Partner = participants.Where(p => p.Name == "Jane").FirstOrDefault();
participants.Where(p => p.Name == "Jane").FirstOrDefault()!.Partner = participants.Where(p => p.Name == "John").FirstOrDefault();
participants.Where(p => p.Name == "Uncle").FirstOrDefault()!.Partner = participants.Where(p => p.Name == "Aunty").FirstOrDefault();
participants.Where(p => p.Name == "Aunty").FirstOrDefault()!.Partner = participants.Where(p => p.Name == "Uncle").FirstOrDefault();

// Set a variable to the Documents path.
string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

// Using streamwriter to local file for backup
using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "SecretSanta2023.txt")))
{
    // Determine SecretSantas
    foreach (var p in participants)
    {
        var pot = participants.Where(t => !t.IsDrawn).ToArray();
        var rng = new Random();
        var pick = rng.Next(pot.Length);

        // Reroll rng if pick is invalid (partner / self)
        while (p.Name == pot[pick].Name || (p.Partner != null && pot[pick].Name == p.Partner.Name))
        {
            pick = rng.Next(pot.Length);

            // Can lead to bad combination, but prevents infinite loop. 
            if (pot.Length == 1) break;
        }

        p.SecretSanta = pot[pick];
        pot[pick].IsDrawn = true;

        Console.WriteLine($"Picked someone for {p.Name}");
        
        // Encoding secret santa, so it can be manually provided from local backup without spoiling information
        var secretSantaBytes = System.Text.Encoding.UTF8.GetBytes(p.SecretSanta!.Name!);
        outputFile.WriteLine($"{p.Name}: {Convert.ToBase64String(secretSantaBytes)}");
    }
}


// Failsafe validation
if (participants.Any(p => p.SecretSanta == null) || participants.Any(p => p.Partner != null && p.Partner.Name == p.SecretSanta!.Name) || participants.Any(p => p.Name == p.SecretSanta!.Name))
{
    // Did not bother with auto reruns until valid solution. Just rerun in this unlikely case
    Console.WriteLine("Bad combination, please rerun.");
    return;
}

Console.WriteLine($"Solution is valid, sending mails...");

//SendGrid Block
var apiKey = "<SendGrid API Key";
var client = new SendGridClient(apiKey);

var from = new EmailAddress("your-sendgrid@sender.adress", "Sender Name");
var subject = "<Mail Subject>";

foreach (var p in participants)
{
    var to = new EmailAddress(p.Email);
    var htmlContent = $"Ho ho ho {p.Name},<br /><br />du bist dieses Jahr der Weihnachtswichtel für <strong>{p.SecretSanta!.Name}</strong>!";
    var plainTextContent = $"Ho ho ho {p.Name}, du bist dieses Jahr der Weihnachtswichtel für {p.SecretSanta!.Name}!";

    var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
    var response = await client.SendEmailAsync(msg);
}

Console.WriteLine($"All done, Merry X-Mas");

