## 📄 README: Validação de Codificação de Arquivos Java (TripleDES)

Este aplicativo Java foi desenvolvido para **validar se um arquivo PDF foi codificado corretamente** utilizando o algoritmo **Triple DES (3DES)** para a Partilha de Resultados de MCDT com a SPMS. Ele lê o texto cifrado de um arquivo de entrada (`received.txt`), solicita a chave de decifração e tenta decodificar o conteúdo.

O resultado esperado, para um PDF decodificado corretamente, é que o conteúdo inicie com a assinatura padrão de um arquivo PDF: **`%PDF`** (que no código Java é simplificado para `JVBER`, pois é o início comum de um PDF codificado em Base64).

-----

## 🚀 Como Utilizar

### Pré-requisitos

  * **Java Runtime Environment (JRE)** ou **Java Development Kit (JDK)** instalado (versão 8 ou superior recomendada).

### 1\. Preparação do Arquivo de Entrada

O aplicativo espera que o **texto cifrado (criptografado e codificado em Base64)** esteja contido num único arquivo chamado **`received.txt`** na mesma pasta do executável (o `.jar` ou o `.bat`).

  * **Conteúdo de `received.txt`**: O resultado da encriptação (uma longa *string* codificada em Base64).

### 2\. Execução

Existem duas formas principais de executar a validação:

#### A. Via Terminal/Linha de Comando (Multiplataforma)

Utilize o comando `java -jar` na pasta onde se encontra o ficheiro `.jar`:

```bash
java -jar validate-pdf-1.0-SNAPSHOT.jar
```

#### B. Via Executável de Lote (Apenas Windows)

Execute o ficheiro `exec.bat` na pasta:

```bash
exec.bat
```

### 3\. Entrada da Chave

Após a execução, o aplicativo solicitará que insira a chave 3DES:

```
Digite a chave 3DES:
```

  * **Requisito da Chave**: A chave 3DES fornecida deve ter exatamente **16 caracteres** de comprimento.

-----

## ⚙️ Funcionamento e Lógica de Validação

O aplicativo realiza os seguintes passos:

1.  **Leitura**: Lê todo o conteúdo do ficheiro `received.txt`.
2.  **Validação da Chave**: Verifica se a chave fornecida tem 16 caracteres. Se não tiver, termina com erro.
3.  **Decifração (3DES)**:
      * A chave de 16 caracteres é duplicada (`key + key`) para criar uma chave de 32 bytes, que é truncada para **24 bytes**, o requisito para uma chave 3DES (Triple DES).
      * Um **Vetor de Inicialização (IV)** é gerado usando os últimos 8 bytes da chave original.
      * O algoritmo de decifração usado é o **TripleDES/CBC/PKCS5Padding**.
      * O texto de entrada é decodificado de **Base64** e, em seguida, decifrado.
4.  **Validação do Resultado**: O texto decifrado é verificado. Se for um PDF válido, o seu conteúdo decifrado deve começar com a *string* **`JVBER`** (que é a representação em texto do `%PDF` após a decodificação Base64 e decifração).

-----

## 🚦 Códigos de Retorno do Aplicativo (Saída no Terminal)

O aplicativo pode retornar os seguintes resultados:

| Mensagem de Saída | Tipo de Saída | Significado |
| :--- | :--- | :--- |
| **PDF OK** | Saída Padrão (`System.out`) | O ficheiro `received.txt` foi decifrado corretamente com a chave fornecida e o resultado **inicia com a assinatura esperada de um PDF** (`JVBER`), indicando que a codificação foi bem-sucedida. |
| **ERRO\! Há algo errado com o PDF. O resultado da decodificação foi ...** | Saída de Erro (`System.err`) | O deciframento foi concluído, mas o conteúdo decodificado **não inicia com a assinatura de PDF** (`JVBER`). Indica que a chave estava correta, mas o conteúdo original não era um PDF válido ou foi corrompido. |
| **ERRO\! Chave 3DES invalida\!** | Saída de Erro (`System.err`) | A chave fornecida pelo utilizador não tinha os **16 caracteres** exigidos. |
| **Entrada inválida.** | Saída de Erro (`System.err`) | O ficheiro **`received.txt` estava vazio** ou não foi encontrado/lido corretamente. |
| **NÃO FOI POSSÍVEL DECODIFICAR COM A CHAVE INFORMADA.** | Saída de Erro (`System.err`) | Um erro ocorreu durante a decifração (por exemplo, `BadPaddingException`). Isso geralmente significa que a **chave 3DES fornecida estava incorreta** ou que o texto cifrado em `received.txt` está malformado. |