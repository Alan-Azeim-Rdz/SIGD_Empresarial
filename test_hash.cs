/*cont*/
using System;
using System.Security.Cryptography;
using System.Text;

class Program {
    static void Main() {
        var password = "Admin@SIGD2026!";
        using var sha256 = SHA256.Create();

        var bytesUTF8 = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        Console.WriteLine("UTF8: " + BitConverter.ToString(bytesUTF8).Replace("-", ""));

        var bytesUnicode = sha256.ComputeHash(Encoding.Unicode.GetBytes(password));
        Console.WriteLine("Unicode: " + BitConverter.ToString(bytesUnicode).Replace("-", ""));

        var bytesASCII = sha256.ComputeHash(Encoding.ASCII.GetBytes(password));
        Console.WriteLine("ASCII: " + BitConverter.ToString(bytesASCII).Replace("-", ""));
    }
}
