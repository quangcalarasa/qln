using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

public class Security
{
    public static string getMd5(string pass)
    {
        return
            BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(new UTF8Encoding().GetBytes(pass))).
                ToLower().Replace("-", "");
    }

    public static string Encrypt(string key, string data)
    {
        data = data.Trim();

        byte[] keydata = Encoding.ASCII.GetBytes(key);

        string md5String = BitConverter.ToString(new
                                                        MD5CryptoServiceProvider().ComputeHash(keydata)).Replace("-",
                                                                                                                "").
            ToLower();

        byte[] tripleDesKey = Encoding.ASCII.GetBytes(md5String.Substring(0, 24));

        TripleDES tripdes = TripleDES.Create();

        tripdes.Mode = CipherMode.ECB;

        tripdes.Key = tripleDesKey;

        tripdes.GenerateIV();

        var ms = new MemoryStream();

        var encStream = new CryptoStream(ms, tripdes.CreateEncryptor(),
                                            CryptoStreamMode.Write);

        encStream.Write(Encoding.ASCII.GetBytes(data), 0, Encoding.ASCII.GetByteCount(data));

        encStream.FlushFinalBlock();

        byte[] cryptoByte = ms.ToArray();

        ms.Close();

        encStream.Close();

        return Convert.ToBase64String(cryptoByte, 0, cryptoByte.GetLength(0)).Trim();
    }

    public static string Decrypt(string key, string data)
    {
        byte[] keydata = Encoding.ASCII.GetBytes(key);

        string md5String = BitConverter.ToString(new
                                                        MD5CryptoServiceProvider().ComputeHash(keydata)).Replace("-",
                                                                                                                "").
            Replace(" ", "+").ToLower();

        byte[] tripleDesKey = Encoding.ASCII.GetBytes(md5String.Substring(0, 24));

        TripleDES tripdes = TripleDES.Create();

        tripdes.Mode = CipherMode.ECB;

        tripdes.Key = tripleDesKey;

        byte[] cryptByte = Convert.FromBase64String(data);

        var ms = new MemoryStream(cryptByte, 0, cryptByte.Length);

        ICryptoTransform cryptoTransform = tripdes.CreateDecryptor();

        var decStream = new CryptoStream(ms, cryptoTransform,
                                            CryptoStreamMode.Read);

        var read = new StreamReader(decStream);

        return (read.ReadToEnd());
    }

    //public static string GetIP()
    //{
    //    string IP = "";
    //    if (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
    //    {
    //        IP = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
    //    }
    //    if (IP == "")
    //    {
    //        IP = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
    //    }
    //    return IP;
    //}  

    //public static bool IsValidURIAddress(string uri)
    //{
    //    bool success = false;
    //    try
    //    {
    //        ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
    //        var myRequest = (HttpWebRequest) WebRequest.Create(uri);
    //        myRequest.Method = "GET";
    //        myRequest.ContentType = "application/x-www-form-urlencoded";
    //        myRequest.KeepAlive = false;
    //        myRequest.AllowAutoRedirect = false;
    //        using (var myResponse = (HttpWebResponse) myRequest.GetResponse())
    //        {
    //            if (myResponse.StatusCode == HttpStatusCode.OK)
    //            {
    //                var sb = new StringBuilder();
    //                var buf = new byte[8192];
    //                Stream resStream = myResponse.GetResponseStream();
    //                string tempString = null;
    //                int count = 0;

    //                do
    //                {
    //                    count = resStream.Read(buf, 0, buf.Length);
    //                    if (count != 0)
    //                    {
    //                        tempString = Encoding.ASCII.GetString(buf, 0, count);
    //                        sb.Append(tempString);
    //                    }
    //                } while (count > 0);
    //                if (sb.ToString() ==
    //                    string.Format("pagate-site-verification: {0}", uri.Split('/')[uri.Split('/').Length - 1]))
    //                    success = true;
    //                else success = false;
    //            }
    //            else
    //                success = false;
    //        }
    //    }
    //    catch
    //    {
    //    }

    //    return success;
    //}

    public static string RandomString(int size, bool lowerCase)
    {
        var builder = new StringBuilder();
        var random = new Random();
        char ch;
        for (int i = 0; i < size; i++)
        {
            ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26*random.NextDouble() + 65)));
            builder.Append(ch);
        }
        if (lowerCase)
            return builder.ToString().ToLower();
        return getMd5(builder.ToString());
    }

    public static int RandomNumber(int min, int max)
    {
        var random = new Random();
        return random.Next(min, max);
    }

    public static string EncryptRSA(string publickey, string data)
    {
        data = data.Trim();
        string encryptedValue = string.Empty;
        var csp = new CspParameters(1);

        var rsa = new RSACryptoServiceProvider(csp);
        rsa.FromXmlString(publickey);

        byte[] bytesToEncrypt = Encoding.UTF8.GetBytes(data);
        byte[] bytesEncrypted = rsa.Encrypt(bytesToEncrypt, false);
        encryptedValue = Convert.ToBase64String(bytesEncrypted);
        return encryptedValue;
    }

    public static string DecryptRSA(string privateKey, string data)
    {
        data = data.Trim();
        string decryptedValue = string.Empty;
        var csp = new CspParameters(1);

        var rsa = new RSACryptoServiceProvider(csp);
        rsa.FromXmlString(privateKey);
        byte[] valueToDecrypt = Convert.FromBase64String(data);
        byte[] plainTextValue = rsa.Decrypt(valueToDecrypt, false);

        // Extract our decrypted byte array into a string value to return to our user
        decryptedValue = Encoding.UTF8.GetString(plainTextValue);
        return decryptedValue;
    }

    public static string CreateSignRSA(string data, string privateKey)
    {
        //RSACryptoServiceProvider rsaCryptoIPT = new RSACryptoServiceProvider(1024);

        CspParameters _cpsParameter;
        RSACryptoServiceProvider rsaCryptoIPT;
        _cpsParameter = new CspParameters();
        _cpsParameter.Flags = CspProviderFlags.UseMachineKeyStore;
        rsaCryptoIPT = new RSACryptoServiceProvider(1024, _cpsParameter);

        rsaCryptoIPT.FromXmlString(privateKey);
        return
            Convert.ToBase64String(rsaCryptoIPT.SignData(new ASCIIEncoding().GetBytes(data),
                                                            new SHA1CryptoServiceProvider()));
    }

    public static bool CheckSignRSA(string data, string sign, string publicKey)
    {
        try
        {
            var rsacp = new RSACryptoServiceProvider();
            rsacp.FromXmlString(publicKey);
            return rsacp.VerifyData(Encoding.UTF8.GetBytes(data), "SHA1", Convert.FromBase64String(sign));
        }
        catch (Exception ex)
        {            
            return false;
        }
    }

    public static bool Checkkey(string key)
    {
        try
        {
            var rsacp = new RSACryptoServiceProvider();
            rsacp.FromXmlString(key);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static String sha256_hash(String value)
    {
        StringBuilder Sb = new StringBuilder();

        using (SHA256 hash = SHA256Managed.Create())
        {
            Encoding enc = Encoding.UTF8;
            Byte[] result = hash.ComputeHash(enc.GetBytes(value));

            foreach (Byte b in result)
                Sb.Append(b.ToString("x2"));
        }

        return Sb.ToString();
    }

    public static string HmacSha256Digest(string message, string secret)
    {
        ASCIIEncoding encoding = new ASCIIEncoding();
        byte[] keyBytes = encoding.GetBytes(secret);
        byte[] messageBytes = encoding.GetBytes(message);
        System.Security.Cryptography.HMACSHA256 cryptographer = new System.Security.Cryptography.HMACSHA256(keyBytes);

        byte[] bytes = cryptographer.ComputeHash(messageBytes);

        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }
}

//public class TrustAllCertificatePolicy : ICertificatePolicy
//{
//    #region ICertificatePolicy Members

//    public bool CheckValidationResult(ServicePoint sp,
//                                        X509Certificate cert, WebRequest req, int problem)
//    {
//        return true;
//    }

//    #endregion
//}
