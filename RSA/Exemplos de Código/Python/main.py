import rsa
import aes
import custom_base64

###########################
####### ENCRIPTAÇÃO #######
###########################

# Faço a leitura do PDF e a conversão para Base64
with open('exemplo_PDF.pdf', 'rb') as file:    
    pdf = custom_base64.base64Encoding(file.read())

# Gero aleatoriamente a chave e o vetor de inicialização
key = aes.generateRandomAesKey(32)
iv = aes.generateRandomAesKey(12)

# Faço a encriptação do PDF, através do algoritmo AES
encrypted_pdf = aes.aesGcmEncryptToBase64(pdf, key, iv)
print(encrypted_pdf)

# Efetuo a concatenação da chave gerada aleatoriamente com o vetor de inicialização, separando-o por ':'. 
## Esta chave está a ser tratada em bytes, pois o Crypto.Random gera-o assim. O algoritmo aceita tanto string, quando bytes como input.
## A escolha de bytes ou string fica a critério do utilizador, sendo necessário garantir a geração de valores criptograficamente seguros. 
## O Crypto.random já garante isso, mas há outras bibliotecas disponíveis, que podem gerar inclusive string.
join_list = [key, iv]
key_iv = b':'.join(join_list)

# Encripto a concatenação de "key:iv", usando do algoritmo RSA.
encrypted_key_iv = rsa.rsaEncryptionOaepSha256ToBase64(key_iv)

print(encrypted_key_iv)

###########################
###### DECODIFICAÇÃO ######
###########################

# Para fins de validação, podem executar este trecho do código para garantir que a encriptação correu bem.

decoded_key_iv = rsa.rsaDecryptionOaepSha256FromBase64(encrypted_key_iv)

print("Key decodificada corretamente: " + str(key_iv == decoded_key_iv))

decoded_key, decoded_iv = decoded_key_iv.split(b":")

decoded_pdf = aes.aesGcmDecryptFromBase64(decoded_key, decoded_iv, encrypted_pdf)

print("PDF decodificador corretamente: " + str(decoded_pdf == pdf))