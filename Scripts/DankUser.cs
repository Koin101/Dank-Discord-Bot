using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Discord_Bot;

//TODO Create Birthday class to store birthday's of members in a txt file
//TODO Read and write to txt file
//TODO send message if birthday is reached

public class DankUser(ulong userId, string discordName, string nickname, BirthDay? birthDay = null)
{
    private ulong UserID { get; set; } = userId;
    public string DiscordName { get; set; } = discordName;
    public string Nickname { get; set; } = nickname;

    public BirthDay? Birthday { get; set; } = birthDay;

    

    public void SaveUserToTxt()
    {

        var path = Directory.GetCurrentDirectory();
        string fileName = "DankUsers.json";
        var writer = File.AppendText(Path.Combine(path, fileName));
        writer.WriteLine(JsonSerializer.Serialize(this));
        writer.Close();
    }
    
    
}

public struct BirthDay()
{
    public DateTime birthdayDate { get; set; }
    private TimeSpan timeUntilBirthday { get; set; }

        
    public void SetTimeUntilBirthday(DateTime birthday)
    {
        var currentTime = DateTime.Now;

        TimeSpan differenceTime = birthday.Subtract(currentTime);

        timeUntilBirthday = differenceTime;
    }
}