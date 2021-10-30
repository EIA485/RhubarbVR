﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Newtonsoft.Json.Linq;

namespace RhubarbEngine.Managers
{

    public interface INetApiManager : IManager
    {
        bool Islogin { get; }
        
        string Token { get; set; }

        public string UserID { get; }

        public string AvatarUrl { get; }

        public string DisplayName { get;  }

        public string DeviceID { get;  }

        Task<bool> CheckForSession(string roomID);
        Task<IReadOnlyList<JToken>> GetRooms();
        Task<string> JoinSession(string roomID);
        Task LoadUserData(string Token);
        Task<string> SendRhubarbAPIPost(string Location, JObject objects);
        Task<string> UpdateSession(string sToken);
    }

    public class NetApiManager : INetApiManager
    {
        private const string RHUBARB_MATRIX_END_POINT = "https://matrix.rhubarbvr.net/";

        private const string RHUBARB_API_END_POINT = "https://api.rhubarbvr.net/";
        //private const string RHUBARB_API_END_POINT = "http://localhost:5000/";

        private IEngine _engine;

		public string Token { get; set; } = "";

        public string UserID { get; set; }

        public string AvatarUrl { get; set; }

        public string DisplayName { get; set; }

        public string DeviceID { get; set; }

        public bool Islogin { get; private set; } = false;

		public IManager Initialize(IEngine _engine)
		{
			this._engine = _engine;
			this._engine.Logger.Log("Starting Cloud Interface");
            if (_engine.LoginToken != null)
            {
                _engine.Logger.Log("trying login");
                LoadUserData(_engine.LoginToken).ConfigureAwait(true);
            }
			return this;
		}

        public async Task<string> UpdateSession(string sToken)
        {
            var obj = new JObject
            {
                ["AToken"] = sToken,
                ["MToken"] = Token
            };
            return await SendRhubarbAPIPost("Session/updateUser", obj);
        }

        public async Task<string> JoinSession(string roomID)
        {
            var obj = new JObject
            {
                ["RoomID"] = roomID,
                ["Token"] = Token
            };
            return await SendRhubarbAPIPost("Session/joinSession", obj);
        }


        public async Task<bool> CheckForSession(string roomID)
        {
            var obj = new JObject
            {
                ["RoomID"] = roomID,
                ["Token"] = Token
            };
            return await SendRhubarbAPIPost("Session/checkForSession", obj) == "True";
        }

        public async Task<IReadOnlyList<JToken>> GetRooms()
        {
            var data = await SendAuthenticatedGet("sync");
            return data["rooms"]["join"].ToArray();
        } 

        public async Task<string> SendRhubarbAPIPost(string Location,JObject objects)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(RHUBARB_API_END_POINT + Location);
                request.Method = "POST";
                request.ContentType = "application/json";
                var dataStream = request.GetRequestStream();
                var byteArray = Encoding.UTF8.GetBytes(objects.ToString());
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                var response = await request.GetResponseAsync();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return responseString;
            } catch (Exception e)
            {
                _engine.Logger.Log(e.ToString());
                return "";
            }
        }

        public async Task<JObject> SendAuthenticatedGet(string Location,string Front = "_matrix/client/r0/")
        {
                var request = (HttpWebRequest)WebRequest.Create(RHUBARB_MATRIX_END_POINT + Front + Location);
                request.Headers.Add("Authorization", "Bearer " + Token);
                request.Method = "GET";
                request.ContentType = "application/json";
                var response = await request.GetResponseAsync();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return JObject.Parse(responseString);
        }

        public async Task LoadUserData(string Token)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(RHUBARB_MATRIX_END_POINT + "_matrix/client/r0/account/whoami");
                request.Headers.Add("Authorization", "Bearer " + Token);
                request.Method = "GET";
                request.ContentType = "application/json";
                var response = request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                if (responseString.Contains("user_id"))
                {
                    _engine.Logger.Log("Login", true);
                    Islogin = true;
                    var juser = JObject.Parse(responseString);
                    UserID = (string)juser["user_id"];
                    DeviceID = (string)juser["device_id"];
                    _engine.Logger.Log("UserId: " + UserID, true);
                    this.Token = Token;
                    DisplayName = (string)(await SendAuthenticatedGet($"profile/{HttpUtility.UrlEncode(UserID)}/displayname"))["displayname"];
                    _engine.Logger.Log("DisplayName: " + DisplayName, true);
                    AvatarUrl = (string)(await SendAuthenticatedGet($"profile/{HttpUtility.UrlEncode(UserID)}/avatar_url"))["avatar_url"];

                }
                else
                {
                    _engine.Logger.Log("Failed to login", true);
                    ClearLoginData();
                }
            }
            catch(Exception e)
            {
                _engine.Logger.Log("Failed to login error"+e.ToString(), true);
                ClearLoginData();
            }
        }

        public void ClearLoginData()
        {
            Islogin = false;
            UserID = null;
            AvatarUrl = null;
            DisplayName = "Not Login";
            DeviceID = null;
            Token = "";
        }

        public void Update()
        {

        }
	}
}
