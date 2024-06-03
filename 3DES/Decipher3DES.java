public static String decipher3DES(String encryptedText, String key) {
    try {
        // Configuração do algoritmo de criptografia e modo de operação
        String algorithm = "TripleDES/CBC/PKCS5Padding"; // 3DES com CBC e PKCS5Padding

        // Gera uma chave adequada para 3DES (precisa ter 24 bytes)
        key = key + key; // Key Length: Twice
        byte[] keyBytes = key.getBytes(StandardCharsets.UTF_8);
        byte[] truncatedKey = new byte[24];
        System.arraycopy(keyBytes, 0, truncatedKey, 0, Math.min(keyBytes.length, 24));

        // Gera o IV com os últimos 8 bytes da chave
        byte[] iv = key.substring(key.length() - 8).getBytes(StandardCharsets.UTF_8);

        SecretKey secretKey = new SecretKeySpec(truncatedKey, "TripleDES");
        IvParameterSpec ivSpec = new IvParameterSpec(iv);

        // Inicializa o Cipher com a chave e o IV para o modo de decodificação
        Cipher cipher = Cipher.getInstance(algorithm);
        cipher.init(Cipher.DECRYPT_MODE, secretKey, ivSpec);

        // Decodifica a string Base64 para obter os bytes criptografados
        byte[] encryptedBytes = Base64.getDecoder().decode(encryptedText);

        // Descriptografa os bytes criptografados
        byte[] decryptedBytes = cipher.doFinal(encryptedBytes);

        // Retorna a string decodificada usando a codificação UTF-8
        return new String(decryptedBytes, StandardCharsets.UTF_8);
    } catch (Exception e) {
        e.printStackTrace();
        return null;
    }
}