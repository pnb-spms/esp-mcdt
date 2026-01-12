using System.Security.Cryptography;


/*******************************************************************************
 * ATENÇÃO: Código de referência desenvolvido por equipa não especialista em C#.
 * Este exemplo serve apenas como fonte de consulta para a implementação real.
 * * O código foi disponibilizado como um esforço de auxílio para apoiar as 
 * equipas de C# no desenvolvimento da lógica de encriptação do projeto.
 * * Devido à natureza exemplificativa, é indispensável realizar testes de 
 * cenários adicionais, validações de segurança e tratamento de exceções.
 *******************************************************************************/

public class Program {
    public static void Main() {
        string pdfBytes;
        // Leio o ficheiro
        if (File.Exists("exemplo_PDF.pdf"))
        {
            byte[] pdfFile = File.ReadAllBytes("exemplo_PDF.pdf");
            pdfBytes = Base64Encoding(pdfFile);
        }
        else
        {
            throw new Exception("Ficheiro não encontrado!");
        }

        //Gero aleatoriamente a chave de encriptação AES e o IV
        byte[] encryptionKey = GenerateRandomKey(32);
        byte[] iv = GenerateRandomKey(12);

        //Encripto o PDF através do algoritmo AES
        string ciphertextBase64 = aesGcmEncryptToBase64(encryptionKey, iv, pdfBytes);

        //Concateno chave AES e IV
        byte[] splitter = new byte[] { (byte)':' };
        byte[] keyPlusIv = ConcatenateByteArrayWhitSplitter(encryptionKey, splitter, iv);

        //Encripto a concatenação da chave AES e IV através do algoritmo RSA
        string encryptedAESKey = "";
        try
        {
            string publicKeyLoad = loadRsaPublicKeyPem();
            encryptedAESKey = Base64Encoding(rsaEncryptionOaepSha256(publicKeyLoad, keyPlusIv));
        }
        catch (ArgumentNullException)
        {
            Console.WriteLine("The data was not RSA encrypted");
        }        

        //Desencripto a concatenação da chave AES e IV através do algoritmo RSA
        string privateKeyLoad = loadRsaPrivateKeyPem();
        byte[] encriptedAESKeyAndIv = Base64Decoding(encryptedAESKey);
        byte[] decriptedAESKeyAndIv = rsaDecryptionOaepSha256(privateKeyLoad, encriptedAESKeyAndIv);        
        
        //Realizo a separação da chave AES e IV
        int index = Array.IndexOf(decriptedAESKeyAndIv, (byte)':');
        byte[] decodedAESKey = new byte[32];
        byte[] decodedIV = new byte[16];
        if (index != -1)
        {
            decodedAESKey = new byte[index];
            Array.Copy(decriptedAESKeyAndIv, 0, decodedAESKey, 0, index);
            int secondPartLength = decriptedAESKeyAndIv.Length - index - 1;
            decodedIV = new byte[secondPartLength];
            Array.Copy(decriptedAESKeyAndIv, index + 1, decodedIV, 0, secondPartLength);
        }        
       
        //Desencripto o PDF através do algoritmo AES
        string decryptedtext = aesGcmDecryptFromBase64(decodedAESKey, decodedIV, ciphertextBase64);
        Console.WriteLine($"\nPDF Recuperado: {decryptedtext.Substring(0, 50)}");
        Console.WriteLine($"\nPDF Original: {pdfBytes.Substring(0, 50)}");
        var result = (decryptedtext == pdfBytes);
        Console.WriteLine("PDF Iguais: " + result);
    }

    static string aesGcmEncryptToBase64(byte[] key, byte[] iv, string file) {        
        byte[] gcmTag = new byte[16];
        byte[] data = Base64Decoding(file);
        byte[] cipherText = new byte[data.Length];
        byte[] associatedData = new byte[0];
        using (var cipher = new AesGcm(key, 16))
        {
            cipher.Encrypt(iv, data, cipherText, gcmTag, associatedData);
            byte[] result = ConcatenateByteArray(cipherText, gcmTag);
            return Base64Encoding(result);
        }
    }

    static byte[] Base64Decoding(string input) {
        return Convert.FromBase64String(input);
    }

    static string aesGcmDecryptFromBase64(byte[] key, byte[] iv, string data) {        
        byte[] associatedData = new byte[0];
        byte[] resultByteArray = Base64Decoding(data);
        int splitAt = resultByteArray.Length - 16;
        byte[] encriptedPDF = resultByteArray.AsSpan(0, splitAt).ToArray();
        byte[] gcmTag = resultByteArray.AsSpan(splitAt).ToArray();       
        byte[] decryptedData = new byte[encriptedPDF.Length];
        using (var cipher = new AesGcm(key, 16)) {
            cipher.Decrypt(iv, encriptedPDF, gcmTag, decryptedData, associatedData);            
            return Base64Encoding(decryptedData);
        }
    }

    static byte[] GenerateRandomKey(int size) {
        var rngCsp = RandomNumberGenerator.Create();
        byte[] nonce = new byte[size];
        rngCsp.GetBytes(nonce);
        return nonce;
    }

    static string Base64Encoding(byte[] input) {
        return Convert.ToBase64String(input);
    }

    public static byte[] rsaEncryptionOaepSha256(string publicKeyPem, byte[] plaintext) {
        RSA rsaAlg = RSA.Create();
        rsaAlg.ImportFromPem(publicKeyPem);
        var encryptedData = rsaAlg.Encrypt(plaintext, RSAEncryptionPadding.OaepSHA256);
        return encryptedData;
    }

    public static byte[] rsaDecryptionOaepSha256(string privateKeyPem, byte[] ciphertext) {
        RSA rsaAlgd = RSA.Create();
        byte[] privateKeyByte = getRsaPrivateKeyEncodedFromPem(privateKeyPem);
        int _out;
        rsaAlgd.ImportPkcs8PrivateKey(privateKeyByte, out _out);
        var decryptedData = rsaAlgd.Decrypt(ciphertext, RSAEncryptionPadding.OaepSHA256);
        return decryptedData;
    }

    private static byte[] getRsaPrivateKeyEncodedFromPem(string rsaPrivateKeyPem) {
        string rsaPrivateKeyHeaderPem = "-----BEGIN PRIVATE KEY-----\n";
        string rsaPrivateKeyFooterPem = "-----END PRIVATE KEY-----";
        string rsaPrivateKeyDataPem = rsaPrivateKeyPem.Replace(rsaPrivateKeyHeaderPem, "").Replace(rsaPrivateKeyFooterPem, "").Replace("\n", "");
        return Base64Decoding(rsaPrivateKeyDataPem);
    }

    public static byte[] ConcatenateByteArrayWhitSplitter(byte[] array1, byte[] splitter, byte[] array3) {
        // Cria um novo array com o tamanho total da soma dos dois inputs
        byte[] result = new byte[array1.Length + splitter.Length + array3.Length];

        // 1. Copia o primeiro array para o início (posição 0)
        Buffer.BlockCopy(array1, 0, result, 0, array1.Length);

        // 2. Copia o segundo array logo após o primeiro
        Buffer.BlockCopy(splitter, 0, result, array1.Length, splitter.Length);

        // 3. Copia o terceiro array após a soma do primeiro com o segundo
        Buffer.BlockCopy(array3, 0, result, array1.Length + splitter.Length, array3.Length);

        return result;
    }

    public static byte[] ConcatenateByteArray(byte[] array1, byte[] array2) {
        // Cria um novo array com o tamanho total da soma dos dois inputs
        byte[] result = new byte[array1.Length + array2.Length];

        // 1. Copia o primeiro array para o início (posição 0)
        Buffer.BlockCopy(array1, 0, result, 0, array1.Length);

        // 2. Copia o segundo array logo após o primeiro
        Buffer.BlockCopy(array2, 0, result, array1.Length, array2.Length);   

        return result;
    }

    private static string loadRsaPrivateKeyPem() {
        // Apenas para fins didáticos
        return "-----BEGIN PRIVATE KEY-----\n" +
                "MIIJQwIBADANBgkqhkiG9w0BAQEFAASCCS0wggkpAgEAAoICAQDLwbicdXfjFLgz\n" +
                "OVif9yVztdT6Bie3snuArqG7JJ4H3uUMrv3VnPts4TZmgAbXeduRR4RIky3vsBTA\n" +
                "JCkF3+TeBE0ct9Ytg6AV9DkP3um54brt/u42v1Fv9yOY7zv5pP/B/+BgkwmjW8bH\n" +
                "D0e2HtjOJmJeTBlecRP00PW/zOCARs7MOsuRqYyN9ap7yd7CM2tJigEUFBWsHR3c\n" +
                "VS3Wv+eWjIyTpxIZtMww+XlWBrR7qM4n/ymG7D+Sf1tsCvR5xCwhQrmMvY7AoUvh\n" +
                "9z57Kz9+my/wHUB8tz1iYAnBMSheE+s77cRGWhZlwlJB9diARhowuwPS9ejdjFVW\n" +
                "5hKv8lmTTOX/M1tQ9GZ4yb4hnm6EPWXGj4eXfk/VIFJPZdEpnCPFRUmmDEXe3h6O\n" +
                "lA2FzPrUVHyEnfJz3amyfb4mPE4jeyxnc9Vq/rFUg2k/0pJx/G4R8xczut/d9ziA\n" +
                "AbhNLbiG60AJUE9gT8BOW/n6XMLl90l/Bg3S9qLhP86uorQy9IftiGaYQQ3odog3\n" +
                "B2fK5maKV+6V9UNgJmbhsKS2VvrfXPlPjp5L5VUGgHXULgRb4Ek43CkGXmsoV9Rn\n" +
                "RtzDzQjaeliJBX8Tz+8gzYsEOL9kGpxkO6GKIxPsbwaQ8cnJmKs5h/+6BfbqnO4F\n" +
                "zc7YsJxDn6tiZ87lf66L2tQjMmIDLQIDAQABAoICAC8ozLJFy/Q+iO2uMbGAkeYI\n" +
                "qV0fDJVLVaNwh9VZQcxfjhMT84M7/MN0EtPBgRPhS+0BqP/lNMYbsonQNMB71CyA\n" +
                "wVpKwGMzalt6dbSTKvSPZuUL0pXQTSiFgnM0t+RtiJadwxCHJ71sEgNjqXzhAdwg\n" +
                "7TaXFW4S7QQGU0LLyBBYkyeY2iICJJp8yDMzg3/eR9AzBVHg3i1CZfVnr57bNt5U\n" +
                "9Hp+OkXB31rcevVqwt42MVT3jWSBjKs7F+1XUcNXIMGoAGsvOnmiO35267q1RVJn\n" +
                "174S/yh+ftIkmU1iM84mfRUXUIzZ8AIoakKDBNOXphsuRdHPgdC5WM0unhekF5AC\n" +
                "9Ii9RXMAqdXCN7IWZflYuaJmtUDUUXy1OeCobkaZqiBjz0t0EUEzGRcVqJZ3FIMe\n" +
                "TzkQnlbosQbNs9PDRUhQYhXknzonV9cHxKuxHSjQcMveJ/qaWq7PUxlDQ0ilyCw6\n" +
                "KGtsiyJx4ofLEzpJIjwBzIFlsoyV2j+d5NnS8OOcc9gK3/3Hic7PxnaexiSylXUs\n" +
                "OOYmA8J48ydAIWkzqBKeMAT9v/vOa+uUN0fL1nTgUVGRsU6xjEYd0CRLoRbbLmJL\n" +
                "UpNakPV1UZTDSuTfWSL/HVppvufuG7ZOq+kmAJRXuYRHOOmUy1LsNH2wLJIMrxRf\n" +
                "B8ru9aN+WQlxnlT7xqUxAoIBAQDuRkvYUfzCKrDDUZvHmzcxk93CVrkbpFFgYZ71\n" +
                "38RhVoAeTQfgjkmdOEe3ZpHR365DodNtI0qVSDCp7AuUq01W0Vf8lCnR7JBpj+2O\n" +
                "g9zvhP5ffZGoYTMM4QStlMU3DbuYahvR5GzG2GXdw9kFMEsaSLoyeiQx9S0hUTvc\n" +
                "DZGlTBDVtHrwIgOnqYCH1el43d9eqV97NhPOfLFjrJ8BNKP2ItEnC55c189Y/oFO\n" +
                "oBWxlqqIAMGiMCnYYtpu59M3zpD9fDB03fJWBJAJLm3Ryw5Q8FYHArUnV7+Et/6A\n" +
                "5/YOC2bwAjSpOB+WroAkMnnHjDuh5w98pNaYKL0VzSmOTDW5AoIBAQDa6hDxOxhD\n" +
                "iQdwmTD4nubk73v9PMIKBrWig3iccu0vHFJKy/2USLRcRxbPBWDTqABftxEWwtUD\n" +
                "08gs+MDTLZ2EQMLh8PuHyW0dWx+ntc9b6ywmgDY7Awgk7pspQwRN6GdjiHXlH0zM\n" +
                "HolJoWuR38xfJFepfqp7sB4Wf9DehRpYnkW+59+0sFh09gwWIDbDU2y+sFV0wzVC\n" +
                "rmpExcZkNUMAHPpWPZl8GyA3Rhwljcvtu/vV4pOWYlccBlYw5qUnmTrAJLY/SyIY\n" +
                "k7kdJBJis4Vz7j0N/eoa0ftohuTWGTyFLw7xLrZnUgRBDKGOvPcNkMkY05b/GnTG\n" +
                "UQc0N5z5u/MVAoIBAQCEQb6tpuyOdhXBhJXcWLptKW9JBMwgnLeSsP0D6x3dDZ91\n" +
                "sjoyGaigvPGJykQcY3UeE0AA7zrh270vRFc1QVRfYTned/syi4WpInuwxF6pEIAF\n" +
                "TvS/qpwkhO4Mz2wcX+IF+rfQbGM4iDsaui/G4oR9ZWr8w7E9jaJsQOPiBCIYxzz8\n" +
                "8f3fNnIi6UHvo/vA3B61M3xNLroVvj7UXaewiqCL6c2AR8GyNRPhiPsLQm5oMYIc\n" +
                "lZblSKihdFVchA9Ihb8TBJD8/Yka19N/VScd8QDUSmLl50nYOauTpnF86cQ5DXko\n" +
                "tYC3gPB/ugJklSPlcGBVQKTnAbQfp1U1ObjcRE75AoIBAQDTGssRxdoFLnxVMfVO\n" +
                "m/WvKd6Ye2HnR0cbG8tQm2dsedRFuuejemUrY4Z0Rwxzr1wE8UXyfBntIeq0q4FP\n" +
                "ZHq00brdJYV6EV0+5OPm1+XKD0Q5MISbbbYJ32sTZEQfWRFyo2KFfCST5pKfxaE1\n" +
                "P+sWiLZchgCjXFf3fHSQUSzx7lb13ScWWMRErg1DvPbyb127mdIe0ixnTEjAXzSM\n" +
                "5+vfjRI/3hFH4+SySNLVyCKy9AfL3z5R1C2KofW2HIWBZTuJlR2Gdc7AgpwNpP5x\n" +
                "3grAnCqWdpiuGnZl+0dry1RYXuP64wAVLLtbe3HEcyePFmNGpKzE/t9Da1oWGdcv\n" +
                "4PZtAoIBAHJmr1KQPisnImiu+I0dlvxdCLIj+wrTX7nOoAVAG0A2mbffC7E9+jmS\n" +
                "O7tHv9MrGYN92+X2qfT0sp9GxpfXJ0goOCFqP45W4VU38jlEeGAVBj+AEqjy14jE\n" +
                "HUu8/JuB0lBlBWc71IZTAgoKQg84KxCelwytqYI+vhN3qglgF0OAKGtRZcmfT4MX\n" +
                "NdDHWcIJWUOIJHk4PiSFPXH5q3pdrEwDYZBHM+AOSJFWIQF4XaM7E3xbbToy9PXF\n" +
                "b64m5ztYKLmcgqNfcYF3onwjwDOh5cr28RsUVf+vUf8sAj/QkeLPt9LODksRKHsD\n" +
                "g4ZBA9KDwbe3hYjngteeBDjXVhN0PiU=\n" +
                "-----END PRIVATE KEY-----";
    }

    private static string loadRsaPublicKeyPem() {
        // this is a sample key - don't worry !
        return "-----BEGIN PUBLIC KEY-----\n" +
                "MIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAy8G4nHV34xS4MzlYn/cl\n" +
                "c7XU+gYnt7J7gK6huySeB97lDK791Zz7bOE2ZoAG13nbkUeESJMt77AUwCQpBd/k\n" +
                "3gRNHLfWLYOgFfQ5D97pueG67f7uNr9Rb/cjmO87+aT/wf/gYJMJo1vGxw9Hth7Y\n" +
                "ziZiXkwZXnET9ND1v8zggEbOzDrLkamMjfWqe8newjNrSYoBFBQVrB0d3FUt1r/n\n" +
                "loyMk6cSGbTMMPl5Vga0e6jOJ/8phuw/kn9bbAr0ecQsIUK5jL2OwKFL4fc+eys/\n" +
                "fpsv8B1AfLc9YmAJwTEoXhPrO+3ERloWZcJSQfXYgEYaMLsD0vXo3YxVVuYSr/JZ\n" +
                "k0zl/zNbUPRmeMm+IZ5uhD1lxo+Hl35P1SBST2XRKZwjxUVJpgxF3t4ejpQNhcz6\n" +
                "1FR8hJ3yc92psn2+JjxOI3ssZ3PVav6xVINpP9KScfxuEfMXM7rf3fc4gAG4TS24\n" +
                "hutACVBPYE/ATlv5+lzC5fdJfwYN0vai4T/OrqK0MvSH7YhmmEEN6HaINwdnyuZm\n" +
                "ilfulfVDYCZm4bCktlb631z5T46eS+VVBoB11C4EW+BJONwpBl5rKFfUZ0bcw80I\n" +
                "2npYiQV/E8/vIM2LBDi/ZBqcZDuhiiMT7G8GkPHJyZirOYf/ugX26pzuBc3O2LCc\n" +
                "Q5+rYmfO5X+ui9rUIzJiAy0CAwEAAQ==\n" +
                "-----END PUBLIC KEY-----";
    }
}