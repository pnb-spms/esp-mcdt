function encrypt($original_base64, $key)
{
	$iv = substr($key,-8,8);
	$method = "DES-EDE-CBC";
	$options = 0;
	
	return openssl_encrypt($original_base64, $method, $key, $options, $iv);
}