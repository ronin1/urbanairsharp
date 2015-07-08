﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UrbanAirSharp
{
	/// <summary>
	/// Configured http client and serializer
	/// </summary>
	public sealed class ServiceModelConfig
	{
		class Inner { internal static readonly ServiceModelConfig SINGLETON = Create(); }
		/// <summary>
		/// Lazy singleton instance handle
		/// </summary>
		public static ServiceModelConfig Instance { get { return Inner.SINGLETON; } } //lazy singleton pattern
		
		internal readonly String Host = "https://go.urbanairship.com/";
		internal readonly HttpClient HttpClient = new HttpClient();
		internal readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings();

		private ServiceModelConfig()
		{
			Host = GetConfigValue("host") ?? Host;
        }

		/// <summary>
		/// Load the keys from config file or environment variables
		/// </summary>
		public static ServiceModelConfig Create()
		{
			string key = GetConfigValue("uaAppKey");
			string secret = GetConfigValue("uaAppMAsterSecret");
			return Create(key, secret);
        }

		static string GetConfigValue(string key)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("key can not be null or blank");

			key = "UrbanAirSharp." + key;
            string v = ConfigurationManager.AppSettings[key];
			if(string.IsNullOrEmpty(v))
			{
				v = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
				if (string.IsNullOrEmpty(v))
				{
					v = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User);
					if (string.IsNullOrEmpty(v))
						v = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Machine);
				}
			}
			return string.IsNullOrEmpty(v) ? null : v;
		}

		/// <summary>
		/// Explicitly provides keys
		/// </summary>
		/// <param name="uaAppKey">Required UA provided app key</param>
		/// <param name="uaAppMAsterSecret">Required UA provided app secret</param>
		public static ServiceModelConfig Create(String uaAppKey, String uaAppMAsterSecret)
		{
			if (string.IsNullOrEmpty(uaAppKey))
				throw new ArgumentException("uaAppKey is required");
			if (string.IsNullOrEmpty(uaAppMAsterSecret))
				throw new ArgumentException("uaAppMAsterSecret is required");

			var auth = String.Format("{0}:{1}", uaAppKey, uaAppMAsterSecret);
			auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));

			var cf = new ServiceModelConfig();

			cf.SerializerSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
			cf.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
			cf.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
			cf.SerializerSettings.DateParseHandling = DateParseHandling.DateTime;
			cf.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
			cf.SerializerSettings.DateFormatString = "yyyy-MM-ddTH:mm:ss";

			cf.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
			cf.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/vnd.urbanairship+json; version=3;");

			return cf;
		}
	}
}
