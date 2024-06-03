private static string Decipher3DES(string arg, string key)
{
    using (var des = new TripleDESCryptoServiceProvider())
    {
        des.Key = ASCIIEncoding.ASCII.GetBytes(key);
        var IV = new byte[8];
        Buffer.BlockCopy(des.Key, des.Key.Length - 8, IV, 0, 8);
        des.IV = IV;
        using (var factory = des.CreateDecryptor())
        {
            var stream = Convert.FromBase64String(arg);
            var output =
                factory.TransformFinalBlock(stream, 0, stream.Length);
            return UTF8Encoding.UTF8.GetString(output);
        }
    }
}