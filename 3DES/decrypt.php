function decrypt($ciphertext, $key)
{
	$iv = substr($key,-8,8);
	$method = "DES-EDE-CBC";
	$options = 0;

	return openssl_decrypt($ciphertext, $method, $key, $options, $iv);
}