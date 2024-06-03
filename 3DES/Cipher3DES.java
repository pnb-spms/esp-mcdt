public static String cipher3DES(String arg, String key) {
    try {
        // Configuração do algoritmo de criptografia e modo de operação
        String algorithm = "TripleDES/CBC/PKCS5Padding"; // 3DES com CBC e PKCS5Padding

        // Gera uma chave adequada para 3DES (precisa ter 24 bytes)
        key = key + key; // Key Length: Twice
        byte[] keyBytes = key.getBytes(StandardCharsets.UTF_8);
        byte[] truncatedKey = new byte[24];
        System.arraycopy(keyBytes, 0, truncatedKey, 0, Math.min(keyBytes.length, 24));

        // Gera o IV com os últimos 8 bytes da chave
        byte[] iv = key.substring(key.length()-8, key.length()).getBytes(StandardCharsets.UTF_8);

        SecretKey secretKey = new SecretKeySpec(truncatedKey, "TripleDES");
        IvParameterSpec ivSpec = new IvParameterSpec(iv);

        // Inicializa o Cipher com a chave e o IV
        Cipher cipher = Cipher.getInstance(algorithm);
        cipher.init(Cipher.ENCRYPT_MODE, secretKey, ivSpec);

        // Criptografa o texto
        byte[] encryptedBytes = cipher.doFinal(arg.getBytes(StandardCharsets.UTF_8));

        // Codifica em base64 para armazenamento seguro ou transmissão
        return Base64.getEncoder().encodeToString(encryptedBytes);
    } catch (Exception e) {
        e.printStackTrace();
        return null;
    }
}