private static string Cipher3DES(this string arg, string key)
{
    using (var tdes = new TripleDESCryptoServiceProvider())
    {
        tdes.Key = ASCIIEncoding.ASCII.GetBytes(key);
        var IV = new byte[8];
        Buffer.BlockCopy(tdes.Key, tdes.Key.Length - 8, IV, 0, 8);
        tdes.IV = IV;
        tdes.Padding = PaddingMode.PKCS7;
        tdes.Mode = CipherMode.CBC;
        using (var factory = tdes.CreateEncryptor())
        {
            var toEncryptArray = ASCIIEncoding.ASCII.GetBytes(arg);
            var resultArray =
                factory
                    .TransformFinalBlock(toEncryptArray,
                    0,
                    toEncryptArray.Length);
            return Convert.ToBase64String(resultArray);
        }
    }
}
