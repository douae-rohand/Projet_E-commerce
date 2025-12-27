using System;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        string password = "SuperAdmin123";
        string hashedPassword = HashPassword(password, Encoding.UTF8).ToLower();
        Console.WriteLine("Mot de passe: " + password);
        Console.WriteLine("Hash: " + hashedPassword);
    }

    private static string HashPassword(string password, Encoding encoding)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(encoding.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}</content>
<parameter name="filePath">c:\Users\hp\Documents\Projet_E-commerce\generate_hash.cs