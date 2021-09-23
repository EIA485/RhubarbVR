﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using RhubarbEngine.PlatformInfo;
using RNumerics;
using System.Numerics;

namespace RhubarbEngine.Components.Users
{
	[Category(new string[] { "Users" })]
	public class LocalUserPlatform : Component
	{
		public Driver<Platform> plat;

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			if (plat.Linked)
			{
				plat.Drivevalue = Engine.PlatformInfo.platform;
			}
		}

		public override void BuildSyncObjs(bool newRefIds)
		{
			plat = new Driver<Platform>(this, newRefIds);
		}

		public LocalUserPlatform(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public LocalUserPlatform()
		{
		}
	}
}