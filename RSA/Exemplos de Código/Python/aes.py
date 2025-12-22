from Crypto.Cipher import AES
from Crypto.Random import get_random_bytes
from Crypto.Util.Padding import pad
from Crypto.Util.Padding import unpad
import custom_base64

def generateRandomAesKey(size:int):
  return get_random_bytes(size)

def aesGcmEncryptToBase64(plaintext, encryptionKey, iv):  
  aad = b""
  cipher = AES.new(encryptionKey, AES.MODE_GCM, nonce=iv)
  cipher.update(aad)
  plaintext = custom_base64.base64Decoding(plaintext)
  ciphertext, tag = cipher.encrypt_and_digest(plaintext)
  ciphertext_tag = ciphertext + tag
  ciphertext_tag_base64 = custom_base64.base64Encoding(ciphertext_tag)  
  return ciphertext_tag_base64

def aesGcmDecryptFromBase64(decryptionKey, iv, ciphertextDecryptionBase64):
  splitat = -24
  ciphertext_base64 = ciphertextDecryptionBase64[:splitat]
  gcmTag_base64 = ciphertextDecryptionBase64[splitat:]
  ciphertext = custom_base64.base64Decoding(ciphertext_base64)
  gcmTag = custom_base64.base64Decoding(gcmTag_base64)
  aad = b""
  cipher = AES.new(decryptionKey, AES.MODE_GCM, nonce=iv)
  cipher.update(aad)
  decryptedtext = cipher.decrypt_and_verify(ciphertext, gcmTag)
  return custom_base64.base64Encoding(decryptedtext)