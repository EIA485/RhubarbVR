﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.Managers;
using CommandLine;
using RhubarbEngine.PlatformInfo;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace RhubarbEngine
{
	public class EngineInitializer
	{
		private readonly Engine _engine;

		public string intphase;

		public bool Initialised = false;

		public EngineInitializer(Engine _engine)
		{
			this._engine = _engine;
		}

		public void InitializeManagers()
		{
			//This is to make finding memory problems easier
			//System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.LowLatency;
			try
			{
				_engine.Logger.Log("Starting Managers");

				intphase = "Platform Info Manager";
				_engine.Logger.Log("Starting Platform Info Manager:");
				_engine.platformInfo = new PlatformInfoManager();
				_engine.PlatformInfo.Initialize(_engine);

				if (_engine.PlatformInfo.platform != Platform.Android)
				{
					intphase = "Window Manager";
					_engine.Logger.Log("Starting Window Manager:");
					_engine.windowManager = new Managers.WindowManager();
					_engine.WindowManager.Initialize(_engine);
				}
				else
				{

				}

				intphase = "Input Manager";
				_engine.Logger.Log("Starting Input Manager:");
				_engine.inputManager = new Managers.InputManager();
				_engine.InputManager.Initialize(_engine);

				intphase = "Render Manager";
				_engine.Logger.Log("Starting Render Manager:");
				_engine.renderManager = new Managers.RenderManager();
				_engine.RenderManager.Initialize(_engine);


				intphase = "Audio Manager";
				_engine.Logger.Log("Starting Audio Manager:");
				_engine.audioManager = new Managers.AudioManager();
				_engine.AudioManager.Initialize(_engine);

				intphase = "Net Api Manager";
				_engine.Logger.Log("Starting Net Api Manager:");
				_engine.netApiManager = new Managers.NetApiManager();
				if (token != null)
				{
					_engine.NetApiManager.token = token;
				}
				_engine.NetApiManager.Initialize(_engine);

				intphase = "World Manager";
				_engine.Logger.Log("Starting World Manager:");
				_engine.worldManager = new WorldManager();
				_engine.WorldManager.Initialize(_engine);

				_engine.AudioManager.task.Start();
				Initialised = true;
			}
			catch (Exception _e)
			{
				_engine.Logger.Log("Failed at " + intphase + " Error: " + _e);
			}

		}

		public string token;

		public string session;

		public IEnumerable<string> settings = Array.Empty<string>();

		public void LoadArguments(string[] _args)
		{
			foreach (var arg in _args)
			{
				_engine.Logger.Log(arg, true);
			}
			Parser.Default.ParseArguments<CommandLineOptions>(_args)
				.WithParsed<CommandLineOptions>(o =>
				{
					if (o.Verbose)
					{
						_engine.Verbose = true;
					}
					if (o.Datapath != null)
					{
						_engine.dataPath = o.Datapath;
					}
				    _engine.backend = o.GraphicsBackend;
				    _engine.outputType = o.OutputType;
					if (o.Settings != null)
					{
						settings = o.Settings;
					}
					if (o.Token != null)
					{
						token = o.Token;
					}
					if (o.SessionID != null)
					{
						session = o.SessionID;
					}
				});
		}
	}
}
