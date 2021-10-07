using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;

public class Raffle {
    private static string key = "6W{cj/oFO&Howu}";

    private static string[] animals = new string[] {"whale", "hake", "shark", "dolphin", "marlin", "trout", "tuna", "cod", "carp", "grouper"};
    private static string[] adjectives = new string[] {"curious", "timid", "angry", "active", "moody", "cynical", "hopeful", "nervous", "sleepy", "brave"};

    private static string formURL = "https://docs.google.com/forms/d/e/1FAIpQLSfThYMTfwARKs3o3vt_jaXiUTWPhps9pHGaj9yiQvOJXMVeRg/viewform";

    private static string GenerateMessage() {
        var random = new Random();
        return adjectives[random.Next(adjectives.Length)] + "-" + animals[random.Next(animals.Length)];
    }

    public static string GetRaffleURL(string message) {
        var date = DateTime.Now.ToString("yyyy-MM-dd");
        var hmac = new HMACSHA256(Encoding.ASCII.GetBytes(key));

        var msgPack = Encoding.ASCII.GetBytes(date + message);
        var hash = hmac.ComputeHash(msgPack);
        var hashStr = Convert.ToBase64String(hash);

        var token = WebUtility.UrlEncode(message + "|" + hashStr);
        var url = $"{formURL}?usp=pp_url&entry.1613413500={token}&entry.1275246369={date}";

        return url;
    }
}