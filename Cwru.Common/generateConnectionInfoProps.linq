<Query Kind="Program" />

void Main()
{
	var parameters = new string[] {
		"ServiceUri, Service Uri, Url, Server",
		"Domain",
		"UserName, User Name, UserId, User Id",
		"Password",
		"HomeRealmUri, Home Realm Uri",
		"AuthenticationType, AuthType",
		"RequireNewInstance",
		"ClientId, AppId, ApplicationId",
		"RedirectUri, ReplyUrl",
		"TokenCacheStorePath",
		"LoginPrompt",
		"SkipDiscovery",
		"Thumbprint, CertificateThumbprint",
		"StoreName, CertificateStoreName",
		"ServiceUri, Service Uri, Url, Server",
		"UserName, User Name, UserId, User Id",
		"Password",
		"HomeRealmUri, Home Realm Uri",
		"AuthenticationType, AuthType",
		"RequireNewInstance",
		"ClientId, AppId, ApplicationId",
		"ClientSecret, Secret",
		"RedirectUri, ReplyUrl",
		"TokenCacheStorePath",
		"LoginPrompt",
		"StoreName, CertificateStoreName",
		"Thumbprint, CertThumbprint",
		"Integrated Security",
	};
	
	var builder = new StringBuilder();
	foreach(var parameter in parameters) {
		if(string.IsNullOrWhiteSpace(parameter)) {
			continue;
		}
		builder.Append("[ConnectionStringAliases(");
		var parts = parameter.Split(',').Select(x=>x.Trim()).ToArray();
		for(var i = 0; i < parts.Length; i++) {
			var part = parts[i];
			builder.Append(@"""");
			builder.Append(part);
			builder.Append(@"""");
			if(i != parts.Length -1) {
				builder.Append(@", ");
			}
		}
		builder.AppendLine(")]");
		
		builder.Append("public string ");
		builder.Append(parts[0]);
		builder.AppendLine(" { get; set; }");
		builder.AppendLine();
	}
	
	Console.WriteLine(builder.ToString());
}

// Define other methods and classes here