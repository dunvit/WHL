﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using log4net;
using Newtonsoft.Json.Linq;

namespace WHLocator.Infrastructure
{
    public class CrestAuthorization
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CrestAuthorization));

        private const string CLIENT_ID = "8f1e2ac9d4aa467c88b12674926dc5e6";
        private const string CLIENT_SECRET = "GZyvG71OxmfHzcDrTMreHw6CV7sDUwiBMiPSpbPn";

        public string AccessToken { get; set; }

        public string TokenType { get; set; }

        public string RefreshToken { get; set; }

        public int ExpiresIn { get; set; }

        public CrestAuthorization(string token)
        {
            Log.DebugFormat("[CrestAuthorization.CrestAuthorization] started for token = {0}", token);

            VerifyAuthorizationCode(token);

            Refresh();
        }

        private void VerifyAuthorizationCode(string token)
        {
            Log.DebugFormat("[CrestAuthorization.VerifyAuthorizationCode] started for token = {0}", token);

            var url = "https://login.eveonline.com/oauth/token";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            var encoded = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(CLIENT_ID + ":" + CLIENT_SECRET));

            Log.DebugFormat("[CrestAuthorization.VerifyAuthorizationCode] encoded is {0}", encoded);

            httpWebRequest.Headers.Add("Authorization", "Basic " + encoded);
            httpWebRequest.Host = "login.eveonline.com";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"grant_type\":\"authorization_code\",\"code\":\"" + token + "\"}";

                streamWriter.Write(json);
                streamWriter.Flush();
            }

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    Log.DebugFormat("[CrestAuthorization.VerifyAuthorizationCode] result = {0}", result);

                    dynamic data = JObject.Parse(result);

                    AccessToken = data.access_token;
                    RefreshToken = data.refresh_token;
                    TokenType = data.token_type;
                    ExpiresIn = data.expires_in;

                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("[CrestAuthorization.VerifyAuthorizationCode] Critical error. Exception is {0}", ex);
            }

        }

        public void Refresh()
        {
            Log.DebugFormat("[CrestAuthorization.Refresh] started for refresh_token = {0}", RefreshToken);

            var url = "https://login.eveonline.com/oauth/token";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            var encoded = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(CLIENT_ID + ":" + CLIENT_SECRET));
            httpWebRequest.Headers.Add("Authorization", "Basic " + encoded);
            httpWebRequest.Host = "login.eveonline.com";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"grant_type\":\"refresh_token\",\"refresh_token\":\"" + RefreshToken + "\"}";

                streamWriter.Write(json);
                streamWriter.Flush();
            }

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    Log.DebugFormat("[CrestAuthorization.Refresh] result = {0}", result);

                    dynamic data = JObject.Parse(result);

                    AccessToken = data.access_token;
                    RefreshToken = data.refresh_token;
                    TokenType = data.token_type;
                    ExpiresIn = data.expires_in;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Critical error in [CrestAuthorization.Refresh] Exception is {0}", ex);
            }

        }

        public dynamic ObtainingCharacterData()
        {
            Log.DebugFormat("[CrestAuthorization.ObtainingCharacterData] AccessToken = {0}", AccessToken);

            var url = "https://login.eveonline.com/oauth/verify";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = "GET";
            httpWebRequest.Headers.Add("Authorization", "Bearer " + AccessToken);
            httpWebRequest.Host = "login.eveonline.com";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                Log.DebugFormat("[CrestAuthorization.ObtainingCharacterData] result = {0}", result);

                return JObject.Parse(result);

            }

        }

        public dynamic GetLocation(long pilotId)
        {
            Log.DebugFormat("[CrestAuthorization.GetLocation] started. pilotId = {0}", pilotId);

            var url = "https://crest-tq.eveonline.com//characters/" + pilotId + "/location/";

            Trace.TraceInformation(DateTime.Now.ToLongTimeString() + " Start Get location. " + url);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            httpWebRequest.Method = "GET";
            httpWebRequest.Headers.Add("Authorization", "Bearer " + AccessToken);
            httpWebRequest.Host = "crest-tq.eveonline.com";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                Log.DebugFormat("[CrestAuthorization.GetLocation] result = {0}", result);

                return JObject.Parse(result);

            }
        }

        public dynamic GetCharacterInfo(long pilotId)
        {
            Log.DebugFormat("[CrestAuthorization.GetCharacterInfo] started. pilotId = {0}", pilotId);

            var url = "https://crest-tq.eveonline.com//characters/" + pilotId + "/";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            httpWebRequest.Method = "GET";
            httpWebRequest.Headers.Add("Authorization", "Bearer " + AccessToken);
            httpWebRequest.Host = "crest-tq.eveonline.com";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                Log.DebugFormat("[CrestAuthorization.GetCharacterInfo] result = {0}", result);

                return JObject.Parse(result);

            }
        }


    }
}
